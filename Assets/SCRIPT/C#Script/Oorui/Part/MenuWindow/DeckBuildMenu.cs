/*
 *  @file   DeckBuildMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 新規デッキ編成を行う画面
/// </summary>
public class DeckBuildMenu : MenuWindowBase {

    // デッキ名入力欄
    [SerializeField] private TMP_InputField deckNameInputField;

    // 保存ボタン
    [SerializeField] private Button saveButton;

    // カード追加ボタン
    [SerializeField] private Button addCardButton;

    // カード削除ボタン
    [SerializeField] private Button removeCardButton;

    // 選択中カードの画像
    [SerializeField] private Image selectedCardImage;

    // デッキ編成専用の所持カード一覧
    [SerializeField] private DeckBuildCardListView deckBuildCardListView;

    // デッキに編成したカードの表示
    [SerializeField] private DeckBuildView deckBuildView;

    // 選択中カードのデッキ登録枚数表示
    [SerializeField] private TMP_Text selectedCardCountText;

    // 戻るボタン
    [SerializeField] private Button returnButton;

    // 選択された操作
    private DeckBuildMenuSelect selectMenu = DeckBuildMenuSelect.None;

    // 編集対象のデッキ情報
    private DeckSaveData editingDeckData = null;

    // 現在選択されているカードID
    private int selectedCardID = -1;

    // カードの最大枚数
    private const int _DECKCARD_MAXNUM = 30;
    // 保存ボタンの元の色設定
    private ColorBlock defaultSaveButtonColors;
    // 保存ボタン内のテキスト
    [SerializeField] private TMP_Text saveButtonText;

    // 保存ボタン内のテキストの元の色
    private Color defaultSaveButtonTextColor;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();
        // 保存ボタンの初期色を保存する
        defaultSaveButtonColors = saveButton.colors;

        // 保存ボタンのテキストの初期色を保存する
        defaultSaveButtonTextColor = saveButtonText.color;
    }

    /// <summary>
    /// 編集するデッキ情報を設定する
    /// </summary>
    public void SetEditDeckData(DeckSaveData data) {
        // 編集対象のデッキ情報を保持する
        editingDeckData = data;
    }

    /// <summary>
    /// メニューを開いたときの処理
    /// </summary>
    public override async UniTask Open() {

        // メニューを表示する
        await base.Open();
        // ボタンイベントを登録する
        ALLListner();

        // カードクリック時の処理を登録する
        deckBuildCardListView.SetCardClickAction(OnPossessionCardClicked);
        // 保存ボタンの色を更新する
        UpdateSaveButtonColor();
        // 選択状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;
        selectedCardID = -1;
        // 選択中カードの枚数表示を初期化する
        selectedCardCountText.text = "0";
        // デッキのカードを初期化する
        deckBuildView.ClearCards();

        // 所持カード情報を最新の状態に更新する
        deckBuildView.RefreshOwnedCardCounts();
        // 編集モードかどうか確認する
        if (editingDeckData != null) {
            // 保存されているデッキ名を入力欄に設定する
            deckNameInputField.text = editingDeckData.deckName;

            // 保存されているカード一覧を読み込む
            deckBuildView.SetCardIDs(editingDeckData.cardIDs);
        }
        else {
            // 新規作成の場合はデッキ名を初期化する
            deckNameInputField.text = "";
        }

        // 選択カード画像を初期化する
        selectedCardImage.sprite = null;
        selectedCardImage.enabled = false;

        // カード操作ボタンを無効にする
        addCardButton.interactable = false;
        removeCardButton.interactable = false;

        // 現在デッキに登録されているカードIDを取得する
        var currentDeckCardIDs = deckBuildView.GetCardIDs();

        // デッキ登録済み枚数を差し引いて所持カード一覧を更新する
        deckBuildCardListView.RefreshCards(currentDeckCardIDs);

        // 保存ボタンが押されるまで待機する
        await UniTask.WaitUntil(() => selectMenu != DeckBuildMenuSelect.None);

        // メニューを閉じる
        await Close();
    }

    /// <summary>
    /// メニューを閉じたときの処理
    /// </summary>
    public override async UniTask Close() {

        // メニューを非表示にする
        await base.Close();

        // 次回の入力待ちに備えて初期化する
        selectMenu = DeckBuildMenuSelect.None;

        // 編集対象のデッキ情報を初期化する
        editingDeckData = null;
    }

    /// <summary>
    /// ボタンイベントの登録、解除
    /// </summary>
    private void ALLListner() {

        // 二重登録を防止する
        saveButton.onClick.RemoveListener(OnSaveButton);
        addCardButton.onClick.RemoveListener(OnAddCardButton);
        removeCardButton.onClick.RemoveListener(OnRemoveCardButton);
        returnButton.onClick.RemoveListener(OnReturnButton);

        // ボタンイベントを登録する
        saveButton.onClick.AddListener(OnSaveButton);
        addCardButton.onClick.AddListener(OnAddCardButton);
        removeCardButton.onClick.AddListener(OnRemoveCardButton);
        returnButton.onClick.AddListener(OnReturnButton);
    }

    /// <summary>
    /// 所持カードがクリックされたときの処理
    /// </summary>
    private void OnPossessionCardClicked(int cardID, Image cardImage) {
        // 選択されたカードIDを保存する
        selectedCardID = cardID;

        // 選択されたカード画像を表示する
        selectedCardImage.sprite = cardImage.sprite;
        selectedCardImage.enabled = true;

        // カード操作ボタンを有効にする
        addCardButton.interactable = true;
        removeCardButton.interactable = true;

        // 選択中カードの登録枚数を表示する
        UpdateSelectedCardCount();
    }

    /// <summary>
    /// ＋ボタンが押されたときの処理
    /// </summary>
    private void OnAddCardButton() {

        // カードが選択されていなければ終了する
        if (selectedCardID < 0) return;

        // デッキに登録されているカードIDを取得する
        var cardIDs = deckBuildView.GetCardIDs();

        // 30枚に達していれば終了する
        if (cardIDs.Count >= _DECKCARD_MAXNUM) return;

        // 所持カード情報を取得する
        var saveData = SaveManager.Instance.LoadCards();

        // 所持枚数を取得する
        var cardCounts = OwnedCardUtility.GetCardCounts(saveData.cardID);

        // 選択カードの所持枚数を取得する
        int ownedCount = cardCounts.ContainsKey(selectedCardID)
            ? cardCounts[selectedCardID]
            : 0;

        // 現在の登録枚数を取得する
        int deckCount = deckBuildView.GetCardCount(selectedCardID);

        // 所持枚数を超える場合は追加しない
        if (deckCount >= ownedCount) return;

        // デッキにカードを追加する
        deckBuildView.AddCard(selectedCardID);

        // 残り編成可能枚数を更新する
        UpdateRemainingCardCount();

        // 登録枚数を更新する
        UpdateSelectedCardCount();

        // 保存ボタンの色を更新する
        UpdateSaveButtonColor();
    }

    /// <summary>
    /// －ボタンが押されたときの処理
    /// </summary>
    private void OnRemoveCardButton() {

        // カードが選択されていなければ終了する
        if (selectedCardID < 0) return;

        // デッキからカードを1枚削除する
        deckBuildView.RemoveCard(selectedCardID);

        // 残り編成可能枚数を更新する
        UpdateRemainingCardCount();

        // 登録枚数を更新する
        UpdateSelectedCardCount();

        // 保存ボタンの色を更新する
        UpdateSaveButtonColor();
    }

    /// <summary>
    /// 保存ボタンが押されたときの処理
    /// </summary>
    private void OnSaveButton() {
        // 入力されたデッキ名を取得する
        string deckName = deckNameInputField.text.Trim();

        // デッキ名が空の場合は仮の名前を設定する
        if (string.IsNullOrWhiteSpace(deckName)) {
            deckName = "NoName";
        }

        // デッキに登録されているカードIDを取得する
        var cardIDs = deckBuildView.GetCardIDs();

        if (cardIDs.Count < _DECKCARD_MAXNUM) {
            // カード枚数が少ないことを伝える
            Debug.Log("カード足りないデッキ作れない");
            return;
        }

        // 編集モードかどうか確認する
        if (editingDeckData != null) {

            // 既存デッキの内容を更新する
            bool isUpdated = SaveManager.Instance.UpdateDeck(
                editingDeckData.deckID,
                deckName,
                cardIDs
            );

            // 更新に失敗した場合は終了する
            if (!isUpdated) return;
        }
        else {
            // 新しいデッキとして保存する
            SaveManager.Instance.SaveDeck(deckName, cardIDs);
        }

        // 保存操作を設定する
        selectMenu = DeckBuildMenuSelect.DeckBuild;
    }

    /// <summary>
    /// 戻るボタンが押された時の処理
    /// </summary>
    private void OnReturnButton() {
        selectMenu = DeckBuildMenuSelect.DeckBuild;
    }

    /// <summary>
    /// 選択中カードのデッキ登録枚数を表示する
    /// </summary>
    private void UpdateSelectedCardCount() {

        // カードが選択されていなければ0を表示する
        if (selectedCardID < 0) {
            selectedCardCountText.text = "0";
            return;
        }

        // 選択中カードの登録枚数を取得する
        int count = deckBuildView.GetCardCount(selectedCardID);

        // 登録枚数をテキストに反映する
        selectedCardCountText.text = count.ToString();
    }

    /// <summary>
    /// デッキのカード枚数に応じて保存ボタンの色を変更する
    /// </summary>
    private void UpdateSaveButtonColor() {

        // 現在のデッキ登録枚数を取得する
        int cardCount = deckBuildView.GetCardIDs().Count;

        // 現在のボタンの色設定を取得する
        ColorBlock colors = defaultSaveButtonColors;

        // 30枚未満なら灰色にする
        if (cardCount < _DECKCARD_MAXNUM) {
            colors.normalColor = Color.gray;
            colors.highlightedColor = Color.gray;
            colors.selectedColor = Color.gray;
            colors.pressedColor = Color.gray;

            // テキストも灰色にする
            saveButtonText.color = Color.gray;
        }
        else {
            // 元のテキスト色に戻す
            saveButtonText.color = defaultSaveButtonTextColor;
        }

        // ボタンに色設定を反映する
        saveButton.colors = colors;
    }


    /// <summary>
    /// 選択中カードの残り編成可能枚数を更新する
    /// </summary>
    private void UpdateRemainingCardCount() {

        // 所持カード情報を取得する
        var saveData = SaveManager.Instance.LoadCards();

        // カードIDごとの所持枚数を取得する
        var cardCounts = OwnedCardUtility.GetCardCounts(saveData.cardID);

        // 所持枚数を取得する
        int ownedCount = cardCounts.ContainsKey(selectedCardID)
            ? cardCounts[selectedCardID]
            : 0;

        // デッキに登録されている枚数を取得する
        int deckCount = deckBuildView.GetCardCount(selectedCardID);

        // 残り編成可能枚数を計算する
        int remainingCount = Mathf.Max(0, ownedCount - deckCount);

        // 所持カード一覧の表示だけを更新する
        deckBuildCardListView.UpdateCardOwnedCount(
            selectedCardID,
            remainingCount
        );
    }
}