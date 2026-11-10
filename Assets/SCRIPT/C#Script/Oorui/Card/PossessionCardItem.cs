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

    // カード画像を外部から取得するためのプロパティ
    public Image CardImage => cardImage;

    /// <summary>
    /// カード画像と所持枚数を設定する
    /// </summary>
    /// <param name="cardID">カードID</param>
    /// <param name="cardSprite">カード画像</param>
    /// <param name="count">所持枚数</param>
    /// <param name="isShowCount">枚数を表示するかどうか</param>
    public void SetCard(int cardID, Sprite cardSprite, int count, bool isShowCount = true) {

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

        // 所持枚数を保持する
        ownedCount = count;

        // 枚数表示用テキストが設定されている場合
        if (countText != null) {

            // 枚数表示の有無を切り替える
            countText.gameObject.SetActive(isShowCount);

            // 枚数表示が有効な場合のみテキストを設定する
            if (isShowCount) {
                countText.text = $"×{count}";
            }
        }
    }

    /// <summary>
    /// カードの所持状況に応じて画像の色を変更
    /// </summary>
    /// <param name="isOwned">所持しているか</param>
    public void SetOwnedState(bool isOwned) {

        // 所持しているカードは通常色で表示
        if (isOwned) {
            cardImage.color = Color.white;
        }
        // 未所持カードは灰色で表示
        else {
            cardImage.color = Color.gray;
        }
    }
}