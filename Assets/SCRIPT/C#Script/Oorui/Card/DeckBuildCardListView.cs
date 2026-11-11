/*
 *  @file   DeckBuildCardListView
 *  @author oorui
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ編成画面専用の所持カード一覧を管理するクラス
/// </summary>
public class DeckBuildCardListView : MonoBehaviour {

    // カードを並べるContent
    [SerializeField]
    private RectTransform content = null;

    // カード1種類分の表示用Prefab
    [SerializeField]
    private PossessionCardItem cardPrefab = null;

    // 生成したカード表示を管理するリスト
    private readonly List<PossessionCardItem> cardItems =
        new List<PossessionCardItem>();

    // カードクリック時に実行する処理
    private Action<int, Image> onCardClicked = null;

    /// <summary>
    /// カードクリック時の処理を設定する
    /// </summary>
    public void SetCardClickAction(Action<int, Image> action) {
        // クリック時の処理を登録する
        onCardClicked = action;
    }

    /// <summary>
    /// デッキ編成用の所持カード一覧を更新する
    /// </summary>
    /// <param name="deckCardIDs">現在デッキに登録されているカードID</param>
    public void RefreshCards(List<int> deckCardIDs = null) {

        // 必要なオブジェクトが設定されているか確認する
        if (!CheckInstance()) return;

        // 所持カードをセーブデータから取得する
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // 前回生成したカードを削除する
        ClearCards();

        // セーブデータが存在しない場合は終了する
        if (saveData == null || saveData.cardID == null) return;

        // 所持カードIDごとの枚数を集計する
        Dictionary<int, int> cardCounts = OwnedCardUtility.GetCardCounts(saveData.cardID);

        // デッキ登録済みカードIDごとの枚数を集計する
        Dictionary<int, int> deckCounts =
            deckCardIDs != null
            ? OwnedCardUtility.GetCardCounts(deckCardIDs)
            : new Dictionary<int, int>();

        // カードIDごとに表示を生成する
        foreach (KeyValuePair<int, int> entry in cardCounts) {

            // カード画像を取得する
            Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(entry.Key);

            // 画像が取得できなければスキップする
            if (cardSprite == null) continue;

            // デッキ登録済み枚数を取得する
            int registeredCount = deckCounts.ContainsKey(entry.Key)
                ? deckCounts[entry.Key]
                : 0;

            // 残り編成可能枚数を計算する
            int remainingCount = Mathf.Max(0, entry.Value - registeredCount);

            // カード表示Prefabを生成する
            PossessionCardItem cardItem = Instantiate(cardPrefab, content);

            // 残り枚数を設定する
            cardItem.SetCard(entry.Key, cardSprite, remainingCount);

            // クリック処理クラスを取得する
            PossessionCardClick cardClick = cardItem.GetComponent<PossessionCardClick>();

            // クリック時の処理を登録する
            if (cardClick != null) {
                cardClick.SetClickAction(onCardClicked);
            }

            // 管理リストに登録する
            cardItems.Add(cardItem);
        }

        // レイアウトを更新する
        UpdateLayout();

        // スクロール位置を先頭に戻す
        ResetScrollPosition();
    }

    /// <summary>
    /// 必要な参照が設定されているか確認する
    /// </summary>
    private bool CheckInstance() {

        // セーブ管理クラスの確認
        if (SaveManager.Instance == null) return false;

        // マスターデータ管理クラスの確認
        if (ScriptableObjectManager.instance == null) return false;

        // Inspector参照の確認
        if (content == null || cardPrefab == null) return false;

        return true;
    }

    /// <summary>
    /// 生成済みカードをすべて削除する
    /// </summary>
    private void ClearCards() {

        // 生成済みのカードを削除する
        foreach (PossessionCardItem cardItem in cardItems) {
            if (cardItem != null) {
                Destroy(cardItem.gameObject);
            }
        }

        // 管理リストを初期化する
        cardItems.Clear();
    }

    /// <summary>
    /// カードのレアリティ順に並べ替える
    /// </summary>
    public void RealitySort() {

        // レアリティが高い順に並べ替える
        cardItems.Sort((a, b) => {

            // マスターデータを取得する
            var masterA = CardDataMasterUtility.GetCardMaster(a.CardID);

            var masterB = CardDataMasterUtility.GetCardMaster(b.CardID);

            // マスターデータがないカードは最後に配置する
            if (masterA == null && masterB == null) return 0;
            if (masterA == null) return 1;
            if (masterB == null) return -1;

            // レアリティを比較する
            return ((int)masterB.rarityID).CompareTo((int)masterA.rarityID);
        });

        // 表示順を更新する
        UpdateSiblingIndex();
    }

    /// <summary>
    /// カードID順に並べ替える
    /// </summary>
    public void ResetSort() {

        // カードIDが小さい順に並べ替える
        cardItems.Sort((a, b) => a.CardID.CompareTo(b.CardID));

        // 表示順を更新する
        UpdateSiblingIndex();

        // スクロール位置を先頭に戻す
        ResetScrollPosition();
    }

    /// <summary>
    /// 所持枚数が多い順に並べ替える
    /// </summary>
    public void OwnerCountSort() {

        // 所持枚数が多い順に並べ替える
        cardItems.Sort((a, b) => b.OwnedCount.CompareTo(a.OwnedCount));

        // 表示順を更新する
        UpdateSiblingIndex();

        // スクロール位置を先頭に戻す
        ResetScrollPosition();
    }

    /// <summary>
    /// Contentの表示順を更新する
    /// </summary>
    private void UpdateSiblingIndex() {

        // リストの順番をHierarchyに反映する
        for (int i = 0; i < cardItems.Count; i++) {
            cardItems[i].transform.SetSiblingIndex(i);
        }

        // レイアウトを更新する
        UpdateLayout();
    }

    /// <summary>
    /// レイアウトを更新する
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
    /// スクロール位置を一覧の先頭に戻す
    /// </summary>
    private void ResetScrollPosition() {

        // ScrollRectを取得する
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();

        // スクロール位置を先頭に戻す
        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    /// <summary>
    /// 指定したカードの残り所持枚数を更新する
    /// </summary>
    public void UpdateCardOwnedCount(int cardID, int remainingCount) {

        // 一覧にあるカードを検索する
        foreach (PossessionCardItem cardItem in cardItems) {

            // カードIDが一致するか確認する
            if (cardItem.CardID != cardID) continue;

            // 残り所持枚数を更新する
            cardItem.UpdateOwnedCount(remainingCount);

            // 対象カードが見つかったので終了する
            return;
        }
    }
}