/*
 *  @file   PossessionCardListView
 *  @author oorui
 */

using System.Collections.Generic;
using TMPro;
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

    // 全カードを表示するかどうか
    public bool isShowAllCards = false;

    /// <summary>
    /// 所持カード一覧を更新する
    /// </summary>
    public void RefreshCards() {

        // 必要なオブジェクトが設定されているか確認
        if (!CheckInstance()) return;

        // ScrollRectを取得
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();

        // 現在のスクロール位置を保存
        float currentScrollPosition = scrollRect != null
            ? scrollRect.verticalNormalizedPosition
            : 1f;

        // 所持カードをセーブデータから取得
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // 前回生成したカードを削除
        ClearCards();

        // 所持カードIDを取得
        List<int> ownedCardIDs = saveData != null && saveData.cardID != null
            ? saveData.cardID
            : new List<int>();

        // カードIDごとの所持枚数を集計
        Dictionary<int, int> cardCounts =
            OwnedCardUtility.GetCardCounts(ownedCardIDs);

        // マスターデータからすべてのカードIDを取得
        List<int> allCardIDs = CardDataMasterUtility.GetAllCardIDs();

        // 全カードIDを確認
        foreach (int cardID in allCardIDs) {

            // 所持枚数を取得
            int ownedCount = 0;
            cardCounts.TryGetValue(cardID, out ownedCount);

            // 所持カードのみ表示する場合、未所持カードを除外
            if (!isShowAllCards && ownedCount <= 0) continue;

            // カード画像を取得
            Sprite cardSprite =
                ScriptableObjectManager.instance.GetCardImage(cardID);

            // 画像が取得できなければスキップ
            if (cardSprite == null) continue;

            // カード表示Prefabを生成
            PossessionCardItem cardItem = Instantiate(cardPrefab, content);

            // カード画像と所持枚数を設定
            cardItem.SetCard(cardID, cardSprite, ownedCount);

            // 所持状況に応じて画像の色を変更
            cardItem.SetOwnedState(ownedCount > 0);

            // 詳細メニュー終了後に既存カードの表示を更新する
            cardItem.SetDetailClosedCallback(UpdateCardDisplay);

            // 生成したカードを管理リストに登録
            cardItems.Add(cardItem);
        }

        // Canvasの更新
        Canvas.ForceUpdateCanvases();

        // Contentのレイアウトを再構築
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // ScrollRectのレイアウトを更新
        if (scrollRect != null) {

            // スクロール位置を復元
            scrollRect.verticalNormalizedPosition = currentScrollPosition;

            // スクロール位置の変更を即時反映
            scrollRect.velocity = Vector2.zero;

            // ScrollRectの表示を更新
            scrollRect.StopMovement();
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

    /// <summary>
    /// カード表示を切り替える
    /// </summary>
    public void ToggleCardDisplay() {

        // 表示状態を反転
        isShowAllCards = !isShowAllCards;

        // カード一覧を再生成
        RefreshCards();
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
            rect.anchoredPosition = new Vector2(11f, -19.8f);

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
    /// スクロール位置を一番上に戻す
    /// </summary>
    public void ResetScrollPosition() {

        // ScrollRectを取得
        ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();

        // スクロール位置を一番上に戻す
        if (scrollRect != null) {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    /// <summary>
    /// 既存カードの所持状況を更新する
    /// </summary>
    public void UpdateCardDisplay() {

        // 必要なオブジェクトが設定されているか確認
        if (!CheckInstance()) return;

        // 所持カードをセーブデータから取得
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // 所持カードIDを取得
        List<int> ownedCardIDs = saveData != null && saveData.cardID != null
            ? saveData.cardID
            : new List<int>();

        // カードIDごとの所持枚数を集計
        Dictionary<int, int> cardCounts =
            OwnedCardUtility.GetCardCounts(ownedCardIDs);

        // 既存のカード表示を確認
        foreach (PossessionCardItem cardItem in cardItems) {

            // カードが削除されていないか確認
            if (cardItem == null) continue;

            // 所持枚数を取得
            int ownedCount = 0;
            cardCounts.TryGetValue(cardItem.CardID, out ownedCount);

            // 所持枚数と画像の色を更新
            cardItem.UpdateOwnedCount(ownedCount);
        }
    }
}