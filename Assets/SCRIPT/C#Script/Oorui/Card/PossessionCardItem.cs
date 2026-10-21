/*
 *  @file   PossessionCardItem
 *  @author oorui
 */

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 所持カード1種類分の表示を管理するクラス
/// </summary>
public class PossessionCardItem : MonoBehaviour {
    // カード画像を表示するImage
    [SerializeField]
    private Image cardImage = null;

    // 所持枚数を表示するテキスト
    [SerializeField]
    private TMP_Text countText = null;

    // カードのID
    public int CardID { get; private set; }

    // 所持枚数を保持する変数
    private int ownedCount = 0;

    // 所持枚数を外部から取得するためのプロパティ
    public int OwnedCount => ownedCount;

    /// <summary>
    /// カード画像と所持枚数を設定する
    /// </summary>
    public void SetCard(int cardID,Sprite cardSprite, int count) {   
        // ソート時に使用するカードIDを保存する
        CardID = cardID;
        // カード画像を設定する
        if (cardImage != null) {
            cardImage.sprite = cardSprite;

            // カード画像の縦横比を維持する
            cardImage.preserveAspect = true;

            // 0枚の場合は灰色、1枚以上の場合は通常色にする
            cardImage.color = count == 0 ? Color.gray : Color.white;
        }

        // 所持枚数を設定する
        if (countText != null) {
            // 所持カード枚数を登録
            ownedCount = count;
            // カード枚数を表示
            countText.text = $"×{count}";
        }
    }
}