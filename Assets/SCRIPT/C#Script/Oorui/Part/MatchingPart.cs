/*
 * @file MatchingPart.cs
 * @author oorui
 * ROOM選択・自動生成UIへの接続を追加。
 */
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>デッキ選択、ランダム／ROOM選択、成立後の対戦準備を行う。</summary>
public class MatchingPart : PartBase {
    private bool isRandomMatch;
    private bool isPrivateMatch;
    private bool isReturn;
    private bool isDeckSelect;
    private bool busy;

    // 既存と同じフィールド名を維持し、Inspectorの登録を引き継ぐ。
    [SerializeField] private Button randomButton;
    [SerializeField] private Button privateButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button deckSelectButton;
    [SerializeField] private TMP_Text selectedDeckText;
    [SerializeField] private Image deckImage;

    [Header("ROOM自動生成画面")]
    [Tooltip("任意。未設定ならデッキ名テキストのフォント、次にTMPの標準フォントを使用。日本語対応フォントを推奨。")]
    [SerializeField] private TMP_FontAsset roomUIFont;
    private CancellationTokenSource partLifetime;

    public override async UniTask Initialize() {
        await base.Initialize();
    }

    public override async UniTask Setup() {
        await base.Setup();
        // 再入場時は前回の待機を終了し、このパート用の中断通知を作り直す。
        partLifetime?.Cancel();
        partLifetime?.Dispose();
        partLifetime = new CancellationTokenSource();
        ResetSelections();
        busy = false;
        SetButtonsInteractable(true);
        RefreshSelectedDeck();
        RegisterButtons();
        Debug.Log("マッチング画面表示中");
        await FadeManager.Instance.FadeIn(FadeType.Black, 0.5f);
    }

    public override async UniTask Execute() {
        if (partLifetime == null) return;
        CancellationToken token = partLifetime.Token;
        try {
            while (!token.IsCancellationRequested) {
                await UniTask.WaitUntil(
                    () => isRandomMatch || isPrivateMatch || isReturn || isDeckSelect,
                    cancellationToken: token);
                busy = true;
                SetButtonsInteractable(false);
                try {
                    if (isReturn) {
                        isReturn = false;
                        await PartManager.Instance.TransitionPart(GamePart.MainMenu);
                        // 遷移後にこのパートの待機ループへ戻らない。
                        return;
                    }

                    if (isDeckSelect) {
                        isDeckSelect = false;
                        int? result = await MenuWindowManager.Instance.DeckSelectMenuOpen(
                            DeckSelectionManager.SelectedDeckID);
                        token.ThrowIfCancellationRequested();
                        if (result.HasValue) {
                            DeckSelectionManager.SetSelectedDeckID(result.Value);
                            RefreshSelectedDeck();
                        }
                        continue;
                    }

                    // 選択を保存してから解除。キャンセル後に勝手に再度開かないようにする。
                    bool roomSelected = isPrivateMatch;
                    isPrivateMatch = false;
                    isRandomMatch = false;
                    int deckID = DeckSelectionManager.SelectedDeckID;
                    if (deckID < 0 || SaveManager.Instance.GetDeckData(deckID) == null) {
                        RefreshSelectedDeck();
                        if (selectedDeckText != null) selectedDeckText.text = "先にデッキを選択してください";
                        continue;
                    }

                    if (roomSelected) {

                        // 前回の対戦情報をリセットする
                        // 必ず通信接続前に実行する
                        FirstPlayerManager.Reset();
                        PlayerNameManager.Reset();
                        PlayerDeckInfoManager.Reset();

                        TMP_FontAsset font = roomUIFont != null
                            ? roomUIFont
                            : (selectedDeckText != null
                                ? selectedDeckText.font
                                : null);

                        bool matched = await RoomMatchingUI.OpenAsync(
                            font,
                            token
                        );

                        token.ThrowIfCancellationRequested();

                        if (!matched) continue;

                        Debug.Log("[ROOM] マッチング成立");
                    }
                    else {
                        // ランダムマッチは従来のメニューとキャンセル判定を使用。
                        await MenuWindowManager.Instance.Open<RandomMatchingMenu>();
                        token.ThrowIfCancellationRequested();
                        if (RandomMatchingMenu.IsMatchingCancelled) continue;
                    }

                    // ROOMもランダムも、成立後は同じ既存処理へ合流する。
                    PlayerSaveData saveData = SaveManager.Instance.Load();
                    PlayerNameManager.SendPlayerName(saveData.playerName);
                    int keyCardID = SaveManager.Instance.GetDeckKeyCardID(deckID);
                    PlayerDeckInfoManager.SendOwnKeyCardID(keyCardID);

                    // 元の受信フラグを利用。切断・パート終了で永久待機しないようにする。
                    await UniTask.WaitUntil(() =>
                        (PlayerNameManager.isOpponentNameReceived &&
                         PlayerDeckInfoManager.isOpponentKeyCardReceived) ||
                        !Mirror.NetworkClient.isConnected, cancellationToken: token);
                    if (!Mirror.NetworkClient.isConnected) {
                        MatchingNetMatch.instance?.OnLeaveRoomButton();
                        Debug.LogWarning("対戦相手の情報交換中に接続が終了しました。");
                        continue;
                    }
                    // ROOMマッチングの場合は先攻・後攻を決定する
                    // ランダムマッチングでは既に決定済み
                    if (roomSelected) {

                        bool firstPlayerReady =
                            await SetupRoomFirstPlayerAsync(token);

                        if (!firstPlayerReady) {

                            Debug.LogError(
                                "[ROOM] 先攻情報の同期に失敗しました。"
                            );

                            MatchingNetMatch.instance?.OnLeaveRoomButton();

                            continue;
                        }
                    }

                    // 両方式で先攻情報が確定していることを確認
                    if (!FirstPlayerManager.isFirstPlayerDecided) {

                        Debug.LogError("[MATCH] 先攻プレイヤーが未決定です。");

                        MatchingNetMatch.instance?.OnLeaveRoomButton();

                        continue;
                    }

                    Debug.Log(
                        $"[MATCH] 先攻情報確定：" +
                        $"{FirstPlayerManager.firstPlayer}"
                    );
                    await MenuWindowManager.Instance.Open<OpponentConfirmMenu>();
                    token.ThrowIfCancellationRequested();
                    await FadeManager.Instance.FadeOut(FadeType.White, 0.5f);
                    token.ThrowIfCancellationRequested();
                    AudioManager.instance.FadeOutBGM();
                    await PartManager.Instance.TransitionPart(
                        GamePart.MainGame, deckID, MainGamePartType.Matching);
                    return;
                }
                finally {
                    busy = false;
                    SetButtonsInteractable(true);
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) {
            // Teardownによる終了は正常な中断として扱う。
        }
    }

    private void RefreshSelectedDeck() {
        DeckSaveData data = DeckSelectionManager.SelectedDeckID >= 0
            ? SaveManager.Instance.GetDeckData(DeckSelectionManager.SelectedDeckID) : null;
        if (data == null) DeckSelectionManager.SetSelectedDeckID(-1);
        if (selectedDeckText != null) selectedDeckText.text = data != null ? data.deckName : "未選択";
        if (deckImage != null) {
            deckImage.sprite = data != null
                ? ScriptableObjectManager.instance.GetCardImage(data.keyCardID) : null;
            deckImage.enabled = deckImage.sprite != null;
            deckImage.preserveAspect = true;
        }
    }

    private void RegisterButtons() {
        UnregisterButtons();
        if (randomButton != null) randomButton.onClick.AddListener(OnClickRandomButton);
        if (privateButton != null) privateButton.onClick.AddListener(OnClickMatchingButton);
        if (returnButton != null) returnButton.onClick.AddListener(OnClickReturnButton);
        if (deckSelectButton != null) deckSelectButton.onClick.AddListener(OnClickDeckSelectButton);
    }
    private void UnregisterButtons() {
        if (randomButton != null) randomButton.onClick.RemoveListener(OnClickRandomButton);
        if (privateButton != null) privateButton.onClick.RemoveListener(OnClickMatchingButton);
        if (returnButton != null) returnButton.onClick.RemoveListener(OnClickReturnButton);
        if (deckSelectButton != null) deckSelectButton.onClick.RemoveListener(OnClickDeckSelectButton);
    }
    private void SetButtonsInteractable(bool value) {
        if (randomButton != null) randomButton.interactable = value;
        if (privateButton != null) privateButton.interactable = value;
        if (returnButton != null) returnButton.interactable = value;
        if (deckSelectButton != null) deckSelectButton.interactable = value;
    }
    private void ResetSelections() {
        isRandomMatch = isPrivateMatch = isReturn = isDeckSelect = false;
    }
    // 同フレームに連打されても選択を重ねない。
    private bool CanSelect => !busy && !isRandomMatch && !isPrivateMatch && !isReturn && !isDeckSelect;
    private void OnClickRandomButton() { if (CanSelect) isRandomMatch = true; }
    private void OnClickMatchingButton() { if (CanSelect) isPrivateMatch = true; }
    private void OnClickReturnButton() { if (CanSelect) isReturn = true; }
    private void OnClickDeckSelectButton() { if (CanSelect) isDeckSelect = true; }

    public override async UniTask Teardown() {
        // ROOM画面待機中でも中断を通知し、生成UIと通信を片付ける。
        partLifetime?.Cancel();
        partLifetime?.Dispose();
        partLifetime = null;
        UnregisterButtons();
        ResetSelections();
        busy = false;
        await base.Teardown();
    }

    /// <summary>
    /// ROOMマッチング時の先攻・後攻を決定する。
    /// ホストが抽選し、クライアントへ送信する。
    /// </summary>
    private async UniTask<bool> SetupRoomFirstPlayerAsync(
        CancellationToken token
    ) {
        token.ThrowIfCancellationRequested();

        var match = MatchingNetMatch.instance;
        var network = CustomNetworkManager.instance;

        // 通信管理クラスの確認
        if (match == null || network == null) {

            Debug.LogError(
                "[ROOM] 通信管理クラスが存在しません。"
            );

            return false;
        }

        // 接続状態の確認
        if (!Mirror.NetworkClient.isConnected) {

            Debug.LogError("[ROOM] 通信が切断されています。");

            return false;
        }

        Debug.Log(
            $"[ROOM] 先攻同期開始 isHost={match.isHost}"
        );

        // ホスト側のみ先攻プレイヤーを決定する
        if (match.isHost) {

            // 先攻情報が未決定の場合のみ抽選する
            if (!FirstPlayerManager.isFirstPlayerDecided) {

                int randomValue = UnityEngine.Random.Range(0, 2);

                PlayerType firstPlayer =
                    randomValue == 0
                        ? PlayerType.Self
                        : PlayerType.Opponent;

                FirstPlayerManager.SetFirstPlayer(firstPlayer);
            }

            // ホスト自身が先攻かどうか
            bool hostIsFirst =
                FirstPlayerManager.firstPlayer == PlayerType.Self;

            // 既存の通信形式に変換
            string message = NetworkValueConverter.Pack(
                NetworkUseID.FirstPlayer,
                hostIsFirst
            );

            // クライアントへ先攻情報を送信
            if (!network.SendStringToOpponent(message)) {

                Debug.LogError(
                    "[ROOM] 先攻情報の送信に失敗しました。"
                );

                return false;
            }

            Debug.Log(
                $"[ROOM][HOST] 先攻決定：" +
                $"{FirstPlayerManager.firstPlayer}"
            );
        }

        // クライアントはホストからの受信を待つ
        // 永久に待機しないようにタイムアウトを設定
        float startTime = Time.realtimeSinceStartup;
        const float timeoutSeconds = 10f;

        while (!FirstPlayerManager.isFirstPlayerDecided) {

            token.ThrowIfCancellationRequested();

            if (!Mirror.NetworkClient.isConnected) {

                Debug.LogWarning(
                    "[ROOM] 先攻情報の待機中に切断されました。"
                );

                return false;
            }

            if (CustomNetworkManager.instance == null) {
                return false;
            }

            if (Time.realtimeSinceStartup - startTime
                >= timeoutSeconds) {

                Debug.LogError(
                    "[ROOM] 先攻情報の受信タイムアウト"
                );

                return false;
            }

            await UniTask.Yield(
                cancellationToken: token
            );
        }

        // 先攻情報が正しい値か確認
        PlayerType decidedPlayer =
            FirstPlayerManager.firstPlayer;

        if (decidedPlayer != PlayerType.Self &&
            decidedPlayer != PlayerType.Opponent) {

            Debug.LogError(
                $"[ROOM] 不正な先攻情報：{decidedPlayer}"
            );

            return false;
        }

        Debug.Log(
            $"[ROOM] 先攻情報確定：{decidedPlayer}"
        );

        return Mirror.NetworkClient.isConnected;
    }
}
