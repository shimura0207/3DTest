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

    // カード1枚分の表示用Prefab
    [SerializeField]
    private Image cardPrefab = null;

    // 生成したカードのImageを管理するリスト
    private readonly List<Image> cardImages = new List<Image>();

    /// <summary>
    /// 所持カード一覧を更新する
    /// </summary>
    public void RefreshCards() {
        // nullチェックを行う
        CheckInstance();

        // 所持カードをセーブデータから取得
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // セーブデータが取得できなければ処理を終了する
        if (saveData == null || saveData.cardID == null) return;

        // 前回生成したカードをすべて削除する
        foreach (Image cardImage in cardImages) {
            if (cardImage != null) {
                Destroy(cardImage.gameObject);
            }
        }

        // 管理リストを空にする
        cardImages.Clear();

        // 所持カードIDを1つずつ処理する
        foreach (int cardID in saveData.cardID) {

            // カードIDに対応するスプライトを取得する
            Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(cardID);

            // 画像を取得できなければ、このカードの生成をスキップする
            if (cardSprite == null) continue;

            // カードPrefabを生成
            Image cardImage = Instantiate(cardPrefab, content);

            // 取得したスプライトを設定する
            cardImage.sprite = cardSprite;

            // スプライトの縦横比を維持して表示する
            cardImage.preserveAspect = true;

            // 生成したカードを管理リストに登録する
            cardImages.Add(cardImage);
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
    private void CheckInstance() {
        // SaveManagerが初期化されているか
        if (SaveManager.Instance == null) return;
        // ScriptableObjectManagerが初期化されているか
        if (ScriptableObjectManager.instance == null) return;
        // Inspectorで必要なオブジェクトが設定されているか確認する
        if (content == null || cardPrefab == null) return;
    }
}