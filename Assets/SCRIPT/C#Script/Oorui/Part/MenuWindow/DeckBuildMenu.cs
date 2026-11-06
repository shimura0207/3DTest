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

    // 選択された操作
    private DeckBuildMenuSelect selectMenu = DeckBuildMenuSelect.None;

    // 編集対象のデッキ情報
    private DeckSaveData editingDeckData = null;

    // 現在選択されているカードID
    private int selectedCardID = -1;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {

        await base.Initialize();

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

        // 所持カード一覧を更新する
        deckBuildCardListView.RefreshCards();

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

        // ボタンイベントを登録する
        saveButton.onClick.AddListener(OnSaveButton);
        addCardButton.onClick.AddListener(OnAddCardButton);
        removeCardButton.onClick.AddListener(OnRemoveCardButton);
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

        // デッキにカードを追加する
        deckBuildView.AddCard(selectedCardID);

        // 登録枚数を更新する
        UpdateSelectedCardCount();
    }

    /// <summary>
    /// －ボタンが押されたときの処理
    /// </summary>
    private void OnRemoveCardButton() {

        // カードが選択されていなければ終了する
        if (selectedCardID < 0) return;

        // デッキからカードを1枚削除する
        deckBuildView.RemoveCard(selectedCardID);

        // 登録枚数を更新する
        UpdateSelectedCardCount();
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

}