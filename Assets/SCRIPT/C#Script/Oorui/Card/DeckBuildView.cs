/*
 *  @file   DeckBuildView
 *  @author oorui
 */

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキに編成されたカードの表示を管理するクラス
/// </summary>
public class DeckBuildView : MonoBehaviour {

    // カードを並べるContent
    [SerializeField] private RectTransform content = null;

    // カードの表示用Prefab
    [SerializeField] private PossessionCardItem cardPrefab = null;

    // 1種類のカードをデッキに登録できる最大枚数
    private const int MAX_CARD_COUNT = 3;
    // 現在のデッキテーマ
    private CardTheme deckTheme = CardTheme.None;


    // カードIDごとの表示オブジェクト
    private readonly Dictionary<int, PossessionCardItem> cardItems = new Dictionary<int, PossessionCardItem>();

    // カードIDごとのデッキ登録枚数
    private readonly Dictionary<int, int> cardCounts = new Dictionary<int, int>();

    // カードIDごとの所持枚数
    private readonly Dictionary<int, int> ownedCardCounts = new Dictionary<int, int>();

    // デッキに登録されているカードID
    private readonly List<int> deckCardIDs = new List<int>();

    // カードクリック時に実行する処理
    private Action<int, Image> onCardClicked = null;

    // 所持カード情報を読み込み済みかどうか
    private bool isOwnedCardCountsLoaded = false;
    /// <summary>
    /// カードクリック時の処理を設定する
    /// </summary>
    public void SetCardClickAction(Action<int, Image> action) {

        // クリック時の処理を登録する
        onCardClicked = action;
    }
    /// <summary>
    /// 所持カード情報を読み込む
    /// </summary>
    public void RefreshOwnedCardCounts() {

        // 所持枚数を初期化する
        ownedCardCounts.Clear();

        // SaveManagerが存在しなければ終了する
        if (SaveManager.Instance == null) {
            isOwnedCardCountsLoaded = false;
            return;
        }

        // 所持カードのセーブデータを取得する
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // セーブデータが存在しない場合は終了する
        if (saveData == null || saveData.cardID == null) {
            isOwnedCardCountsLoaded = true;
            return;
        }

        // カードIDごとの所持枚数を集計する
        Dictionary<int, int> cardCounts = OwnedCardUtility.GetCardCounts(saveData.cardID);

        // 所持枚数を管理用Dictionaryに登録する
        foreach (KeyValuePair<int, int> entry in cardCounts) {
            ownedCardCounts.Add(entry.Key, entry.Value);
        }

        // 所持カード情報の読み込み完了
        isOwnedCardCountsLoaded = true;
    }

    /// <summary>
    /// デッキにカードを追加する
    /// </summary>
    public void AddCard(int cardID) {
        // テーマが合わないカードは追加しない
        if (!CanAddCard(cardID)) return;

        // 必要な参照が設定されているか確認する
        if (content == null || cardPrefab == null) return;

        // 最新の所持カード情報を読み込む
        RefreshOwnedCardCounts();

        // 所持カード情報が取得できなければ終了する
        if (!isOwnedCardCountsLoaded) return;

        // 対象カードの所持枚数を取得する
        if (!ownedCardCounts.TryGetValue(cardID, out int ownedCount)) {
            return;
        }

        // 現在のデッキ登録枚数を取得する
        cardCounts.TryGetValue(cardID, out int currentCount);

        // 最大3枚と所持枚数のうち、小さい方を追加上限にする
        int maxCount = Mathf.Min(MAX_CARD_COUNT, ownedCount);

        // 上限枚数に達している場合は追加しない
        if (currentCount >= maxCount) return;

        // マスターデータ管理クラスの存在を確認する
        if (ScriptableObjectManager.instance == null) return;

        // カード画像を取得する
        Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(cardID);

        // 画像が取得できなければ追加しない
        if (cardSprite == null) return;

        // すでに同じカードが登録されているか確認する
        if (cardCounts.ContainsKey(cardID)) {

            // 登録枚数を1枚増やす
            cardCounts[cardID]++;

            // 表示中のカードの枚数を更新する
            cardItems[cardID].SetCard(
                cardID,
                cardSprite,
                cardCounts[cardID]
            );
        }
        else {

            // カード表示Prefabを生成する
            PossessionCardItem cardItem = Instantiate(cardPrefab, content);

            // カード画像と枚数を設定する
            cardItem.SetCard(cardID, cardSprite, 1);

            // クリック処理クラスを取得する
            PossessionCardClick cardClick =
                cardItem.GetComponent<PossessionCardClick>();

            // クリック時の処理を登録する
            if (cardClick != null) {
                cardClick.SetClickAction(onCardClicked);
            }

            // カード表示を管理する
            cardItems.Add(cardID, cardItem);

            // 登録枚数を管理する
            cardCounts.Add(cardID, 1);
        }

        // カードIDをデッキに追加する
        deckCardIDs.Add(cardID);

        // 登録カードからデッキテーマを再判定する
        UpdateDeckTheme();

        // レイアウトを更新する
        UpdateLayout();
    }

    /// <summary>
    /// デッキに登録されているカードからテーマを再判定する
    /// </summary>
    private void UpdateDeckTheme() {

        // 一度テーマを未決定に戻す
        deckTheme = CardTheme.None;

        // 登録されているカードを順番に確認する
        foreach (int cardID in deckCardIDs) {

            // マスターデータを取得する
            var cardData = CardDataMasterUtility.GetCardMaster(cardID);

            // マスターデータが存在しなければスキップする
            if (cardData == null) continue;

            // カードのテーマを取得する
            CardTheme cardTheme = (CardTheme)cardData.themeID;

            // 汎用カードはテーマ決定の対象外
            if (cardTheme == CardTheme.Neutral) continue;

            // 最初に見つかった汎用以外のテーマを採用する
            deckTheme = cardTheme;

            // テーマが決定したので終了する
            return;
        }
    }

    /// <summary>
    /// デッキから指定したカードを1枚削除する
    /// </summary>
    public void RemoveCard(int cardID) {

        // デッキに存在しないカードなら終了する
        if (!cardCounts.ContainsKey(cardID)) return;

        // カードIDをリストから1つ削除する
        if (!deckCardIDs.Remove(cardID)) return;

        // 登録枚数を1枚減らす
        cardCounts[cardID]--;

        // 登録枚数が0枚になった場合
        if (cardCounts[cardID] <= 0) {

            // カード表示を削除する
            Destroy(cardItems[cardID].gameObject);

            // 管理データからカードを削除する
            cardItems.Remove(cardID);
            cardCounts.Remove(cardID);

        }
        else {

            // カード画像を取得する
            Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(cardID);

            // 残り枚数に合わせて表示を更新する
            cardItems[cardID].SetCard(
                cardID,
                cardSprite,
                cardCounts[cardID]
            );
        }
        // 登録カードからデッキテーマを再判定する
        UpdateDeckTheme();
        // レイアウトを更新する
        UpdateLayout();
    }

    /// <summary>
    /// 保存済みのカード一覧をデッキに読み込む
    /// </summary>
    /// <param name="cardIDs">読み込むカードID一覧</param>
    public void SetCardIDs(List<int> cardIDs) {

        // 現在のカード表示を初期化する
        ClearCards();

        // 読み込むカード一覧が存在しなければ終了する
        if (cardIDs == null) {
            return;
        }

        // 保存されているカードを1枚ずつデッキに追加する
        foreach (int cardID in cardIDs) {

            // 既存の追加処理を利用してカードを登録する
            AddCard(cardID);
        }
    }

    /// <summary>
    /// デッキに登録されているカードIDを取得する
    /// </summary>
    public List<int> GetCardIDs() {

        // 重複を含めたカードID一覧のコピーを返す
        return new List<int>(deckCardIDs);
    }

    /// <summary>
    /// デッキのカード表示をすべて削除する
    /// </summary>
    public void ClearCards() {

        // 生成済みのカード表示をすべて削除する
        foreach (PossessionCardItem cardItem in cardItems.Values) {
            if (cardItem != null) {
                Destroy(cardItem.gameObject);
            }
        }

        // 管理データを初期化する
        cardItems.Clear();
        cardCounts.Clear();
        deckCardIDs.Clear();
        // デッキテーマを初期化する
        deckTheme = CardTheme.None;
        // レイアウトを更新する
        UpdateLayout();
    }

    /// <summary>
    /// Contentのレイアウトを更新する
    /// </summary>
    private void UpdateLayout() {

        // Canvasの更新を反映する
        Canvas.ForceUpdateCanvases();

        // Contentのレイアウトを再構築する
        if (content != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }

    /// <summary>
    /// 指定したカードのデッキ登録枚数を取得する
    /// </summary>
    public int GetCardCount(int cardID) {

        // 登録されているカードの枚数を返す
        if (cardCounts.TryGetValue(cardID, out int count)) {
            return count;
        }

        // まだ1枚も登録されていなければ0を返す
        return 0;
    }

    /// <summary>
    /// KEYカードの表示を更新する
    /// </summary>
    /// <param name="keyCardID">KEYカードのID</param>
    public void RefreshKeyCardLabel(int keyCardID) {

        // Content内に存在するカードをすべて取得する
        PossessionCardItem[] cardItems = content.GetComponentsInChildren<PossessionCardItem>(true);

        // 各カードのKEY表示を更新する
        foreach (PossessionCardItem item in cardItems) {

            // KEYカードかどうか確認する
            bool isKeyCard = item.CardID == keyCardID && keyCardID >= 0;

            // 既存のKEY表示を探す
            Transform labelTransform = item.transform.Find("KeyLabel");

            // KEYカードではない場合
            if (!isKeyCard) {

                // 既存のKEY表示があれば削除する
                if (labelTransform != null) {
                    Destroy(labelTransform.gameObject);
                }

                continue;
            }

            // すでにKEY表示が存在する場合は再生成しない
            if (labelTransform != null) {
                continue;
            }

            // KEY表示用のオブジェクトを生成する
            GameObject labelObject = new GameObject(
                "KeyLabel",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            // カードの子オブジェクトにする
            labelObject.transform.SetParent(item.transform, false);

            // RectTransformを取得する
            RectTransform rect = labelObject.GetComponent<RectTransform>();

            // 左下に配置する
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);

            // 左下からの位置を設定する
            rect.anchoredPosition = new Vector2(-10f, -30f);

            // 表示サイズを設定する
            rect.sizeDelta = new Vector2(55f, 25f);

            // TextMeshProを取得する
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();

            // 表示する文字列を設定する
            label.text = "KEY";

            // フォントサイズを設定する
            label.fontSize = 20;

            // 太字にする
            label.fontStyle = FontStyles.Bold;

            // 右上に文字を配置する
            label.alignment = TextAlignmentOptions.TopRight;

            // 文字色を設定する
            label.color = Color.yellow;

            // カードのクリック操作を妨げないようにする
            label.raycastTarget = false;
        }
    }

    /// <summary>
    /// 現在のデッキテーマを取得する
    /// </summary>
    public CardTheme GetDeckTheme() {
        return deckTheme;
    }

    /// <summary>
    /// 指定されたカードがデッキに追加可能か判定する
    /// </summary>
    public bool CanAddCard(int cardID) {
        // マスターデータからカード情報を取得する
        var cardData = CardDataMasterUtility.GetCardMaster(cardID);

        // マスターデータが存在しなければ追加できない
        if (cardData == null) return false;

        // カードのテーマを取得する
        CardTheme cardTheme = (CardTheme)cardData.themeID;

        // デッキテーマが未決定なら追加可能
        if (deckTheme == CardTheme.None) return true;

        // デッキテーマと同じテーマ、または汎用なら追加可能
        return cardTheme == deckTheme || cardTheme == CardTheme.Neutral;
    }
}