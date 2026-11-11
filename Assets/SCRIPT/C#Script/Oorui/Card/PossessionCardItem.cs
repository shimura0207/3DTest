/*
 *  @file   PossessionCardItem
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public void SetCard(int cardID, Sprite cardSprite, int count, bool isShowCount = true) {

        // カードIDを保存する
        CardID = cardID;

        // 所持枚数を保持する
        ownedCount = count;

        // カード画像を設定する
        if (cardImage != null) {
            cardImage.sprite = cardSprite;
            cardImage.preserveAspect = true;
        }

        // 所持枚数と画像の色を更新する
        UpdateOwnedCount(count, isShowCount);
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

    // 右クリックされたときの処理
    public void OnRightClick() {

        // カード詳細メニューを開く
        MenuWindowManager.Instance.CardDetailMenuOpen(CardID).Forget();
    }

    /// <summary>
    /// デッキ登録後の残り所持枚数を更新する
    /// </summary>
    public void UpdateOwnedCount(int count, bool isShowCount = true) {

        // 所持枚数を更新する
        ownedCount = Mathf.Max(0, count);

        // カード画像の色を更新する
        if (cardImage != null) {
            cardImage.color = ownedCount == 0 ? Color.gray : Color.white;
        }

        // 所持枚数テキストを更新する
        if (countText != null) {
            countText.gameObject.SetActive(isShowCount);
            if (isShowCount) {
                countText.text = $"×{ownedCount}";
            }
        }
    }
}