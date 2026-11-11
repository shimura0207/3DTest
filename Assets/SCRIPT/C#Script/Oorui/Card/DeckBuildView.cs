/*
 *  @file   DeckBuildView
 *  @author oorui
 */

using System.Collections.Generic;
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

    // カードIDごとの表示オブジェクト
    private readonly Dictionary<int, PossessionCardItem> cardItems = new Dictionary<int, PossessionCardItem>();

    // カードIDごとのデッキ登録枚数
    private readonly Dictionary<int, int> cardCounts = new Dictionary<int, int>();

    // カードIDごとの所持枚数
    private readonly Dictionary<int, int> ownedCardCounts = new Dictionary<int, int>();

    // デッキに登録されているカードID
    private readonly List<int> deckCardIDs = new List<int>();

    // 所持カード情報を読み込み済みかどうか
    private bool isOwnedCardCountsLoaded = false;

    /// <summary>
    /// 所持カード情報を読み込む
    /// </summary>
    public void RefreshOwnedCardCounts() {

        // 所持枚数を初期化する
        ownedCardCounts.Clear();

        // SaveManagerが存在しなければ終了する
        if (SaveManager.Instance == null) {
            Debug.LogError("SaveManagerが存在しません");
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

        // 必要な参照が設定されているか確認する
        if (content == null || cardPrefab == null) return;

        // 所持カード情報が未読み込みなら読み込む
        if (!isOwnedCardCountsLoaded) {
            RefreshOwnedCardCounts();
        }

        // 所持カード情報が取得できなければ終了する
        if (!isOwnedCardCountsLoaded) return;

        // 対象カードの所持枚数を取得する
        if (!ownedCardCounts.TryGetValue(cardID, out int ownedCount)) {
            return;
        }

        // 現在のデッキ登録枚数を取得する
        cardCounts.TryGetValue(cardID, out int currentCount);

        // 追加可能な上限を決定する（最大3枚かつ所持枚数まで）
        int maxCount = Mathf.Min(MAX_CARD_COUNT, ownedCount);

        // 上限枚数に達している場合は追加しない
        if (currentCount >= maxCount) {
            Debug.Log($"カードID:{cardID} は追加上限の {maxCount} 枚に達しています");
            return;
        }

        // カード画像を取得する
        if (ScriptableObjectManager.instance == null) return;

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

            // カード表示を管理する
            cardItems.Add(cardID, cardItem);

            // 登録枚数を管理する
            cardCounts.Add(cardID, 1);
        }

        // カードIDをデッキに追加する（重複を許可）
        deckCardIDs.Add(cardID);

        // レイアウトを更新する
        UpdateLayout();
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
            LayoutRebuilder.ForceRebuildLayoutImmediate (content);
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
}