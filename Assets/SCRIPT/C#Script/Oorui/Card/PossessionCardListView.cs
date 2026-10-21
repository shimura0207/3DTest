/*
 *  @file   PossessionCardListView
 *  @author oorui
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 所持カード一覧の表示を管理するクラス
/// </summary>
public class PossessionCardListView : MonoBehaviour {

    // カードを並べるContent
    [SerializeField]
    private RectTransform content = null;

    // カード1種類分の表示用Prefab
    [SerializeField]
    private PossessionCardItem cardPrefab = null;


    // 生成したカード表示を管理するリスト
    private readonly List<PossessionCardItem> cardItems = new List<PossessionCardItem>();

    /// <summary>
    /// 所持カード一覧を更新する
    /// </summary>
    public void RefreshCards() {
        // 必要なオブジェクトが設定されているか確認する
        if (!CheckInstance()) return;

        // 所持カードをセーブデータから取得する
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // セーブデータが存在しない場合は一覧を空にする
        if (saveData == null || saveData.cardID == null) {
            ClearCards();
            return;
        }

        // 前回生成したカードを削除する
        ClearCards();

        // カードIDごとの所持枚数を集計する
        Dictionary<int, int> cardCounts = OwnedCardUtility.GetCardCounts(saveData.cardID);

        // カードIDごとに1つずつ表示を生成する
        foreach (KeyValuePair<int, int> entry in cardCounts) {
            // カードIDに対応するスプライトを取得する
            Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(entry.Key);

            // 画像が取得できなければ、このカードをスキップする
            if (cardSprite == null) continue;

            // カード表示Prefabを生成する
            PossessionCardItem cardItem = Instantiate(cardPrefab, content);

            // カード画像と所持枚数を設定する
            cardItem.SetCard(entry.Key, cardSprite, entry.Value);

            // 生成したカードを管理リストに登録する
            cardItems.Add(cardItem);
        }

        // Contentのレイアウトを即時更新する
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // スクロール位置を一覧の先頭に戻す
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();

        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    /// <summary>
    /// nullチェックをまとめて行う
    /// </summary>
    private bool CheckInstance() {
        // セーブ管理クラスが初期化されているか確認する
        if (SaveManager.Instance == null) return false;
        // マスターデータ管理クラスが初期化されているか確認する
        if (ScriptableObjectManager.instance == null) return false;
        // Inspectorで必要なオブジェクトが設定されているか確認する
        if (content == null || cardPrefab == null) return false;

        // 必要な参照がすべて存在する
        return true;
    }

    /// <summary>
    /// 生成済みのカード表示をすべて削除する
    /// </summary>
    private void ClearCards() {
        // 生成済みのカードを削除する
        foreach (PossessionCardItem cardItem in cardItems) {
            if (cardItem != null) {
                Destroy(cardItem.gameObject);
            }
        }

        // 管理リストを空にする
        cardItems.Clear();
    }

    /// <summary>
    /// カードのレアリティ順に並べ替える
    /// </summary>
    public void RealitySort() {
        // レアリティが高い順にカードを並べ替える
        cardItems.Sort((a, b) => {
            // カードIDからマスターデータを取得する
            var masterA = CardDataMasterUtility.GetCardMaster(a.CardID);
            var masterB = CardDataMasterUtility.GetCardMaster(b.CardID);

            // マスターデータがないカードは最後に配置する
            if (masterA == null && masterB == null) return 0;
            if (masterA == null) return 1;
            if (masterB == null) return -1;

            // Enumの値を比較し、高いレアリティを先にする
            return ((int)masterB.rarityID).CompareTo((int)masterA.rarityID);
        });

        // 並べ替えた順番をContentの表示に反映する
        for (int i = 0; i < cardItems.Count; i++) {
            cardItems[i].transform.SetSiblingIndex(i);
        }

        // レイアウトを即時更新する
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    /// <summary>
    /// 並び替えの初期化
    /// </summary>
    public void ResetSort() {
        // カードIDが小さい順に並べ替える
        cardItems.Sort((a, b) => a.CardID.CompareTo(b.CardID));

        // 並べ替えた順番をContentの表示に反映する
        for (int i = 0; i < cardItems.Count; i++) {
            cardItems[i].transform.SetSiblingIndex(i);
        }

        // レイアウトを即時更新する
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // スクロール位置を一覧の先頭に戻す
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();
        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 1f;
        }

    }

    /// <summary>
    /// カードを所持枚数順に並べる
    /// </summary>
    public void OwnerCountSort() {
        // 所持枚数が多い順にカードを並べ替える
        cardItems.Sort((a, b) => b.OwnedCount.CompareTo(a.OwnedCount));

        // 並べ替えた順番をContentの表示に反映する
        for (int i = 0; i < cardItems.Count; i++) {
            cardItems[i].transform.SetSiblingIndex(i);
        }

        // レイアウトを即時更新する
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // スクロール位置を一覧の先頭に戻す
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();
        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}