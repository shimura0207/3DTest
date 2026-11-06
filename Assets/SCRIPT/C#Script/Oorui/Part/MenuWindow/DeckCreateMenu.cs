using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 保存済みデッキの内容を確認するメニュー
/// </summary>
public class DeckCreateMenu : MenuWindowBase {

    // デッキ名を表示するテキスト
    [SerializeField] private TMP_Text deckNameText;

    // 戻るボタン
    [SerializeField] private Button returnButton;

    // カード一覧を表示するContent
    [SerializeField] private RectTransform content;

    // カード表示用Prefab
    [SerializeField] private PossessionCardItem cardPrefab;

    // デッキ削除ボタン
    [SerializeField] private Button deleteButton;

    // 選択されたデッキ情報
    private DeckSaveData deckData = null;

    // 生成したカード表示オブジェクト
    private List<PossessionCardItem> cardItems = new List<PossessionCardItem>();

    // メニューの操作状態
    private DeckBuildMenuSelect selectMenu = DeckBuildMenuSelect.None;

    private const string _DELETE_DECK = "デッキを削除しますか？";


    /// <summary>
    /// 表示するデッキ情報を設定する
    /// </summary>
    public void SetDeckData(DeckSaveData data) {

        // 選択されたデッキ情報を保持する
        deckData = data;

        // デッキ名を表示する
        if (deckNameText != null && deckData != null) {
            deckNameText.text = deckData.deckName;
        }
    }


    /// <summary>
    /// メニューを開く
    /// </summary>
    public override async UniTask Open() {

        // 操作状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;

        // メニューを表示する
        await base.Open();

        // ボタンイベントを登録する
        AllListner();

        // デッキのカード一覧を更新する
        RefreshDeckCards();

        // 戻るボタンが押されるまで待機する
        await UniTask.WaitUntil(() => selectMenu == DeckBuildMenuSelect.DeckBuild);

        // メニューを閉じる
        await Close();
    }


    /// <summary>
    /// メニューを閉じる
    /// </summary>
    public override async UniTask Close() {

        // メニューを非表示にする
        await base.Close();

        // 操作状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;
    }


    /// <summary>
    /// デッキに登録されているカードを表示する
    /// </summary>
    private void RefreshDeckCards() {

        // 表示先とPrefabが設定されているか確認する
        if (content == null || cardPrefab == null) return;

        // デッキ情報が存在するか確認する
        if (deckData == null || deckData.cardIDs == null) {
            ClearCards();
            return;
        }

        // マスターデータ管理クラスが存在するか確認する
        if (ScriptableObjectManager.instance == null) return;

        // 前回生成したカード表示を削除する
        ClearCards();

        // カードIDごとに登録枚数を集計する
        Dictionary<int, int> cardCounts = OwnedCardUtility.GetCardCounts(deckData.cardIDs);

        // 集計したカード情報を1種類ずつ表示する
        foreach (var entry in cardCounts) {

            // カードIDから画像を取得する
            Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(entry.Key);

            // 画像が取得できなければ次のカードへ進む
            if (cardSprite == null) continue;

            // カード表示オブジェクトを生成する
            PossessionCardItem cardItem = Instantiate(cardPrefab, content);

            // カードID・画像・デッキ内の枚数を設定する
            cardItem.SetCard(entry.Key, cardSprite, entry.Value);

            // 生成したオブジェクトをリストに登録する
            cardItems.Add(cardItem);
        }

        // レイアウトを更新する
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // スクロール位置を先頭に戻す
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();

        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }


    /// <summary>
    /// 生成したカード表示をすべて削除する
    /// </summary>
    private void ClearCards() {

        // 生成済みのカード表示を削除する
        foreach (PossessionCardItem item in cardItems) {
            if (item != null) {
                Destroy(item.gameObject);
            }
        }

        // リストを空にする
        cardItems.Clear();
    }


    /// <summary>
    /// ボタンイベントを登録する
    /// </summary>
    private void AllListner() {
        // 戻るボタンのイベントを一度解除する
        returnButton.onClick.RemoveListener(OnReturnButton);

        // 削除ボタンのイベントを一度解除する
        deleteButton.onClick.RemoveListener(OnDeleteButton);

        // 戻るボタンのイベントを登録する
        returnButton.onClick.AddListener(OnReturnButton);

        // 削除ボタンのイベントを登録する
        deleteButton.onClick.AddListener(OnDeleteButton);
    }


    /// <summary>
    /// 戻るボタンが押されたときの処理
    /// </summary>
    private void OnReturnButton() {

        // メニューを閉じる操作に設定する
        selectMenu = DeckBuildMenuSelect.DeckBuild;
    }

    /// <summary>
    /// デッキ削除ボタンが押された時の処理
    /// </summary>
    private async void OnDeleteButton() {
        // デッキ情報が存在しなければ終了する
        if (deckData == null) return;

        // SaveManagerが存在するか確認する
        if (SaveManager.Instance == null) return;

        // デッキ削除の確認画面を表示する
        bool isDeleteDeck = await MenuWindowManager.Instance.Confirm(_DELETE_DECK);

        // いいえが選択された場合は削除せずに終了する
        if (!isDeleteDeck) return;

        // 選択中のデッキをセーブデータから削除する
        bool isDeleted = SaveManager.Instance.DeleteDeck(deckData.deckID);

        // 削除に失敗した場合はメニューを閉じない
        if (!isDeleted) return;

        // デッキ情報を初期化する
        deckData = null;

        // デッキ一覧に戻る操作に設定する
        selectMenu = DeckBuildMenuSelect.DeckBuild;
    }
}