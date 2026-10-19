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
            cardItem.SetCard(cardSprite, entry.Value);

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

    }
}