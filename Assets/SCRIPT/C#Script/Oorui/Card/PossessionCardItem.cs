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

    /// <summary>
    /// カード画像と所持枚数を設定する
    /// </summary>
    public void SetCard(Sprite cardSprite, int count) {
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
            // カード枚数を表示
            countText.text = $"×{count}";
        }
    }
}