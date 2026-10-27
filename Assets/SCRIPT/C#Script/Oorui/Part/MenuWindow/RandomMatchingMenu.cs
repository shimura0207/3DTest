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
    // メニューが開いているかどうか
    private bool isMenuOpen = false;
    // 接続中に表示するテキスト
    [SerializeField]
    private TMP_Text matchingText = null;

    // マッチング中からキャンセルする処理
    [SerializeField] private Button cancelButton;
    private bool isCancel = false;

    // ランダムマッチがキャンセルされたかどうかを管理する。
    public static bool IsMatchingCancelled { get; private set; } = false;
    // マッチング中のアニメーションを実行するかどうか。
    private bool isAnimatingMatchingText = false;

    // Open処理ごとに異なる番号を付け、古い処理を識別する。
    private int openSessionId = 0;
    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();
        // ボタンの処理登録
        cancelButton.onClick.RemoveListener(OnClickCancelButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);
        // 初期状態を設定する
        isMenuOpen = false;
        matchingText.text = "";
    }

    /// <summary>
    /// メニューが開かれたときの処理
    /// </summary>
    public override async UniTask Open() {
        // 今回のOpen処理に識別番号を割り当てる。
        int sessionId = ++openSessionId;

        // 新しいマッチングの状態を初期化する。
        isCancel = false;
        IsMatchingCancelled = false;
        FirstPlayerManager.Reset();

        // メニューを開いている状態にする。
        isMenuOpen = true;
        isAnimatingMatchingText = true;

        // メニューウィンドウを表示する。
        await base.Open();

        // 待機中に別のOpenやCloseが実行された場合は終了する。
        if (sessionId != openSessionId) return;

        // マッチング中のアニメーションを開始する。
        AnimateMatchingText(sessionId).Forget();

        // MatchingNetMatchが存在しない場合はエラー表示にする。
        if (MatchingNetMatch.instance == null) {
            matchingText.text = "マッチングを開始できません。";
            isAnimatingMatchingText = false;
            return;
        }

        // ランダムマッチを開始する。
        MatchingNetMatch.instance.OnRandomMatchButton();

        // キャンセル、メニュー終了、通信終了、対戦開始のいずれかを待機する。
        await UniTask.WaitUntil(() =>
            sessionId != openSessionId ||
            isCancel ||
            !isMenuOpen ||
            CustomNetworkManager.instance == null ||
            CustomNetworkManager.instance.IsMatchStarted
        );

        // 古いOpen処理なら、以降の表示やメニュー操作を行わない。
        if (sessionId != openSessionId) return;

        // キャンセルされた場合はメニューを閉じて終了する。
        if (isCancel) {
            await Close();
            return;
        }

        // メニューが閉じられていた場合は終了する。
        if (!isMenuOpen) return;

        // 通信が終了していた場合はエラーを表示する。
        if (CustomNetworkManager.instance == null) {
            isAnimatingMatchingText = false;
            matchingText.text = "通信が終了しました。";
            return;
        }

        // マッチング中のアニメーションを停止する。
        isAnimatingMatchingText = false;

        // 対戦相手が見つかったことを表示する。
        matchingText.text = "対戦相手が見つかりました。";

        // ホストは先攻を決定して送信し、クライアントは受信を待つ。
        DecideFirstPlayer();

        // 先攻情報が確定するまで待機する。
        await UniTask.WaitUntil(() =>
            sessionId != openSessionId ||
            !isMenuOpen ||
            FirstPlayerManager.isFirstPlayerDecided
        );

        // 古いOpen処理なら終了する。
        if (sessionId != openSessionId) return;

        // メニューが閉じられていた場合は終了する。
        if (!isMenuOpen) return;

        // 対戦相手が見つかった表示を少し維持する。
        await UniTask.Delay(1000);

        // 待機中に別の処理へ切り替わっていたら終了する。
        if (sessionId != openSessionId) return;

        // 現在のマッチングメニューだけを閉じる。
        await Close();
    }

    /// <summary>
    /// ウィンドウを閉じるときの処理
    /// </summary>
    public override async UniTask Close() {
        // 実行中のOpen処理とアニメーションを無効化する。
        ++openSessionId;

        // メニューとアニメーションを停止状態にする。
        isMenuOpen = false;
        isAnimatingMatchingText = false;

        // メニューウィンドウを非表示にする。
        await base.Close();

        // Closeの待機中に新しいOpenが始まった場合は表示を変更しない。
        if (isMenuOpen) return;

        // 表示内容を初期化する。
        matchingText.text = "";
    }


    /// <summary>
    /// ホスト側で先攻プレイヤーをランダムに決定する
    /// </summary>
    private void DecideFirstPlayer() {
        // すでに先攻が決まっている場合は再抽選しない。
        if (FirstPlayerManager.isFirstPlayerDecided) return;

        // MatchingNetMatchが存在しない場合は処理を行わない。
        if (MatchingNetMatch.instance == null) return;

        // ホスト以外では先攻を決定しない。
        if (!MatchingNetMatch.instance.isHost) return;

        // 0ならホスト自身、1なら対戦相手を先攻にする。
        int result = UnityEngine.Random.Range(0, 2);

        // firstPlayerに結果を入れる
        PlayerType firstPlayer = result == 0 ? PlayerType.Self : PlayerType.Opponent;

        // 決定した先攻プレイヤーを保存する。
        FirstPlayerManager.SetFirstPlayer(firstPlayer);

        // 決定結果をログに出力する。
        Debug.Log($"[先攻決定][ホスト] 先攻プレイヤー：{firstPlayer}");

        // 決定した先攻情報をクライアントへ送信する。
        SendFirstPlayerToClient();
    }

    /// <summary>
    /// ホストが決定した先攻情報をクライアントへ送信する。
    /// </summary>
    private void SendFirstPlayerToClient() {
        // MatchingNetMatchが存在しない場合は送信しない。
        if (MatchingNetMatch.instance == null) return;

        // ホスト以外からは送信しない。
        if (!MatchingNetMatch.instance.isHost) return;

        // 先攻情報が未決定なら送信しない。
        if (!FirstPlayerManager.isFirstPlayerDecided) return;

        // ホスト自身が先攻ならtrue、相手が先攻ならfalseにする。
        bool hostIsFirst = FirstPlayerManager.firstPlayer == PlayerType.Self;

        // 先攻情報を既存の通信形式に変換する。
        string message = NetworkValueConverter.Pack(NetworkUseID.FirstPlayer, hostIsFirst);

        // 対戦相手のクライアントへ先攻情報を送信する。
        if (CustomNetworkManager.instance == null ||
            !CustomNetworkManager.instance.SendStringToOpponent(message)) {
            Debug.LogWarning("[先攻情報] クライアントへの送信に失敗しました。");
        }
    }

    /// <summary>
    /// キャンセルボタンが押された時の処理
    /// </summary>
    private void OnClickCancelButton() {
        SetIsCanncel(true);
    }
    /// <summary>
    /// フラグの変更
    /// </summary>
    /// <param name="flag"></param>
    public void SetIsCanncel(bool flag) {
        // 内部のキャンセル状態を更新する。
        isCancel = flag;

        // MatchingPartから参照できるキャンセル状態を更新する。
        IsMatchingCancelled = flag;
    }

    /// <summary>
    /// マッチング中の「...」を1つずつ表示する処理
    /// </summary>
    private async UniTask AnimateMatchingText(int sessionId) {
        // 表示するドットの数を管理する。
        int dotCount = 0;

        // 今回のOpen処理が有効な間だけアニメーションを続ける。
        while (sessionId == openSessionId && isMenuOpen && isAnimatingMatchingText) {
            // マッチング中のテキストを更新する。
            matchingText.text = "対戦相手を探しています" + new string('.', dotCount);

            // 次の表示まで0.5秒待機する。
            await UniTask.Delay(500);

            // ドットの数を0～3で繰り返す。
            dotCount = (dotCount + 1) % 4;
        }
    }

}