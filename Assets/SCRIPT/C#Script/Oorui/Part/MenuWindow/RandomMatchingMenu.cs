/*
 *  @file   RandomMatchingMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using Mirror;
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
    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();
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
        // メニューウィンドウを表示する
        await base.Open();
        // 新しいマッチングのキャンセル状態を初期化する。
        isCancel = false;
        // 新しい対戦の先攻情報を初期化する。
        FirstPlayerManager.Reset();
        // メニューを開いている状態にする
        isMenuOpen = true;

        // マッチング中のアニメーションを別タスクで開始する。
        AnimateMatchingText().Forget();

        // ランダムマッチを開始する
        MatchingNetMatch.instance.OnRandomMatchButton();

        // 対戦開始通知を受け取るまで待機する
        await UniTask.WaitUntil(() =>
            !isMenuOpen ||
            CustomNetworkManager.instance == null ||
            CustomNetworkManager.instance.IsMatchStarted
        );

        // キャンセルされた場合はメニューを閉じて処理を終了する。
        if (isCancel) {
            // MatchingNetMatch.instance.
            await Close();
            return;
        }

        // メニューが閉じられた場合は後続処理を行わない
        if (!isMenuOpen) return;

        // NetworkManagerがなくなった場合はエラー表示にする
        if (CustomNetworkManager.instance == null) {
            matchingText.text = "通信が終了しました。";
            return;
        }

        // 対戦相手を確認したら表示を更新する
        matchingText.text = "対戦相手が見つかりました。";

        // 先行プレイヤーを決定する
        DecideFirstPlayer();

        // 少し表示してから閉じる処理
        await UniTask.Delay(1000);

        // マッチング後にメニューを閉じる
        await Close();
    }

    /// <summary>
    /// ウィンドウを閉じるときの処理
    /// </summary>
    public override async UniTask Close() {
        // 待機中の処理を終了させる
        isMenuOpen = false;

        // メニューウィンドウを非表示にする
        await base.Close();

        // 表示内容を初期化する
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
        Debug.Log($"[先攻決定] {firstPlayer}");

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
        string message = NetworkValueConverter.Pack(
            NetworkUseID.FirstPlayer,
            hostIsFirst
        );

        // 対戦相手のクライアントへ先攻情報を送信する。
        if (CustomNetworkManager.instance == null ||
            !CustomNetworkManager.instance.SendStringToOpponent(message)) {
            Debug.LogWarning("[先攻情報] クライアントへの送信に失敗しました。");
        }
    }

    /// <summary>
    /// CustomNetworkManagerから呼ぶ、FirstPlayer選考処理
    /// </summary>
    public static void ReceiveFirstPlayer(string[] values) {
        // 通信データの項目数が足りなければ抜ける。
        if (values.Length != 2) return;

        // ホストが先攻かどうかをboolに変換する
        if (!bool.TryParse(values[1], out bool hostIsFirst)) return;

        // クライアント自身の視点で先攻プレイヤーを決定する。
        PlayerType clientFirstPlayer = hostIsFirst ? PlayerType.Opponent : PlayerType.Self;

        // クライアント側に先攻情報を保存する。
        FirstPlayerManager.SetFirstPlayer(clientFirstPlayer);
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
        isCancel = flag;
    }

    /// <summary>
    /// マッチング中の「...」を1つずつ表示する処理
    /// </summary>
    private async UniTask AnimateMatchingText() {
        // 表示するドットの数を管理する。
        int dotCount = 0;

        // メニューが開いている間、アニメーションを繰り返す。
        while (isMenuOpen) {
            // ドットの数に応じて表示する文字列を変更する。
            matchingText.text = "対戦相手を探しています" + new string('.', dotCount);

            // 次の表示まで0.5秒待機する。
            await UniTask.Delay(500);

            // 文字数管理
            dotCount = (dotCount + 1) % 4;
        }
    }

}