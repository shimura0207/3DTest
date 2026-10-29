/*
 *  @file   RandomMatchingMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ランダムマッチの開始と進行状況の表示を管理するメニュー
/// </summary>
public class RandomMatchingMenu : MenuWindowBase {

    // 接続中に表示するテキスト
    [SerializeField] private TMP_Text matchingText = null;

    // マッチングをキャンセルするボタン
    [SerializeField] private Button cancelButton = null;

    // メニューが開いているかどうか
    private bool isMenuOpen = false;

    // キャンセルされたかどうか
    private bool isCancel = false;

    // マッチング中のアニメーションを実行するかどうか
    private bool isAnimatingMatchingText = false;

    // Open処理ごとの識別番号。古い非同期処理の誤動作を防ぐ
    private int openSessionId = 0;

    // 外部から参照できるキャンセル状態
    public static bool IsMatchingCancelled { get; private set; } = false;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();

        // ボタンのイベントを重複登録しないようにする
        cancelButton.onClick.RemoveListener(OnClickCancelButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);

        // 初期状態を設定する
        isMenuOpen = false;
        matchingText.text = "";
    }

    /// <summary>
    /// メニューを開き、マッチングを開始する
    /// </summary>
    public override async UniTask Open() {
        // 今回のOpen処理を識別する番号を発行する
        int sessionId = ++openSessionId;

        // マッチングの状態を初期化する
        ResetMatchingState();

        // メニューを表示する
        await base.Open();

        // 待機中に別のOpenやCloseが実行されていたら終了する
        if (!IsCurrentSession(sessionId)) return;

        // マッチング中のアニメーションを開始する
        StartMatchingAnimation(sessionId);

        // マッチング管理クラスが存在するか確認する
        if (MatchingNetMatch.instance == null) {
            ShowMessage("マッチングを開始できません。");
            return;
        }

        // ランダムマッチを開始する
        MatchingNetMatch.instance.OnRandomMatchButton();

        // マッチングの終了条件を満たすまで待機する
        await WaitForMatchingEnd(sessionId);

        // 古い処理なら以降の処理を行わない
        if (!IsCurrentSession(sessionId)) return;

        // キャンセルされた場合はメニューを閉じる
        if (isCancel) {
            await Close();
            return;
        }

        // メニューが閉じられていた場合は終了する
        if (!isMenuOpen) return;

        // 通信が終了していた場合はエラーを表示する
        if (CustomNetworkManager.instance == null) {
            ShowMessage("通信が終了しました");
            return;
        }

        // 対戦相手が見つかったことを表示する
        ShowMessage("対戦相手が見つかりました");

        // ホストが先攻を決定し、クライアントへ送信する
        DecideFirstPlayer();

        // 先攻情報が確定するまで待機する
        await WaitForFirstPlayerDecision(sessionId);

        // 古い処理や閉じられたメニューなら終了する
        if (!IsCurrentSession(sessionId) || !isMenuOpen) return;

        // 対戦相手が見つかった表示を1秒間維持する
        await UniTask.Delay(1000);

        // 待機中に別の処理へ切り替わっていたら終了する
        if (!IsCurrentSession(sessionId)) return;

        // マッチング完了後にメニューを閉じる
        await Close();
    }

    /// <summary>
    /// マッチングの状態を初期化する
    /// </summary>
    private void ResetMatchingState() {
        isCancel = false;
        IsMatchingCancelled = false;
        isMenuOpen = true;
        isAnimatingMatchingText = true;

        // 前回の先攻決定情報をリセットする
        FirstPlayerManager.Reset();
    }

    /// <summary>
    /// 指定されたOpen処理が現在も有効か確認する
    /// </summary>
    private bool IsCurrentSession(int sessionId) {
        return sessionId == openSessionId;
    }

    /// <summary>
    /// マッチングが終了するまで待機する
    /// </summary>
    private async UniTask WaitForMatchingEnd(int sessionId) {
        await UniTask.WaitUntil(() =>
            !IsCurrentSession(sessionId) ||
            isCancel ||
            !isMenuOpen ||
            CustomNetworkManager.instance == null ||
            CustomNetworkManager.instance.IsMatchStarted
        );
    }

    /// <summary>
    /// 先攻情報が確定するまで待機する
    /// </summary>
    private async UniTask WaitForFirstPlayerDecision(int sessionId) {
        await UniTask.WaitUntil(() =>
            !IsCurrentSession(sessionId) ||
            !isMenuOpen ||
            FirstPlayerManager.isFirstPlayerDecided
        );
    }

    /// <summary>
    /// ウィンドウを閉じるときの処理
    /// </summary>
    public override async UniTask Close() {
        // 実行中のOpen処理とアニメーションを無効化する
        ++openSessionId;
        isMenuOpen = false;
        isAnimatingMatchingText = false;

        // メニューを非表示にする
        await base.Close();

        // Closeの待機中に新しいOpenが始まった場合は表示を変更しない
        if (isMenuOpen) return;

        // 表示内容を初期化する
        matchingText.text = "";
    }

    /// <summary>
    /// ホスト側で先攻プレイヤーを決定する
    /// </summary>
    private void DecideFirstPlayer() {
        // 先攻決定済み、管理クラス不在、またはホスト以外なら何もしない
        if (FirstPlayerManager.isFirstPlayerDecided) return;
        if (MatchingNetMatch.instance == null) return;
        if (!MatchingNetMatch.instance.isHost) return;

        // ホスト自身か対戦相手のどちらかを先攻にする
        int result = UnityEngine.Random.Range(0, 2);
        PlayerType firstPlayer =
            result == 0 ? PlayerType.Self : PlayerType.Opponent;

        // 決定結果を保存する
        FirstPlayerManager.SetFirstPlayer(firstPlayer);

        Debug.Log($"[先攻決定][ホスト] 先攻プレイヤー：{firstPlayer}");

        // 決定した先攻情報をクライアントへ送信する
        SendFirstPlayerToClient();
    }

    /// <summary>
    /// ホストが決定した先攻情報をクライアントへ送信する
    /// </summary>
    private void SendFirstPlayerToClient() {
        // ホスト側で先攻情報が確定していることを確認する
        if (MatchingNetMatch.instance == null) return;
        if (!MatchingNetMatch.instance.isHost) return;
        if (!FirstPlayerManager.isFirstPlayerDecided) return;

        // ホストが先攻ならtrue、相手が先攻ならfalseに変換する
        bool hostIsFirst = FirstPlayerManager.firstPlayer == PlayerType.Self;

        // 既存の通信形式に変換する
        string message = NetworkValueConverter.Pack(
            NetworkUseID.FirstPlayer,
            hostIsFirst
        );

        // 対戦相手へ送信する
        if (CustomNetworkManager.instance == null ||
            !CustomNetworkManager.instance.SendStringToOpponent(message)) {
            Debug.LogWarning("[先攻情報] クライアントへの送信に失敗しました。");
        }
    }

    /// <summary>
    /// キャンセルボタンが押されたときの処理
    /// </summary>
    private void OnClickCancelButton() {
        // キャンセル状態を設定する
        SetIsCancelled(true);

        // マッチング探索と通信を停止する
        if (MatchingNetMatch.instance != null) {
            MatchingNetMatch.instance.CancelRandomMatch();
        }
    }

    /// <summary>
    /// キャンセル状態を変更する
    /// </summary>
    public void SetIsCancelled(bool flag) {
        isCancel = flag;
        IsMatchingCancelled = flag;
    }

    /// <summary>
    /// メッセージを表示し、マッチング中のアニメーションを停止する
    /// </summary>
    private void ShowMessage(string message) {
        isAnimatingMatchingText = false;
        matchingText.text = message;
    }

    /// <summary>
    /// マッチング中の「...」を1つずつ表示する
    /// </summary>
    private async UniTask AnimateMatchingText(int sessionId) {
        int dotCount = 0;

        // 今回のOpen処理が有効な間だけアニメーションを続ける
        while (IsCurrentSession(sessionId) &&
               isMenuOpen &&
               isAnimatingMatchingText) {

            // ドットの数を変えながらメッセージを更新する
            matchingText.text =
                "対戦相手を探しています" + new string('.', dotCount);

            // 次の表示まで0.5秒待機する
            await UniTask.Delay(500);

            // ドットの数を0～3で繰り返す
            dotCount = (dotCount + 1) % 4;
        }
    }

    /// <summary>
    /// マッチング中のアニメーションを開始する
    /// </summary>
    private void StartMatchingAnimation(int sessionId) {
        AnimateMatchingText(sessionId).Forget();
    }
}