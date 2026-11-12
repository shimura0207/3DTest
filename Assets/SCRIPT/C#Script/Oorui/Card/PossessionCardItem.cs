using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 所持カード1種類分の表示を管理するクラス
/// </summary>
public class PossessionCardItem : MonoBehaviour {

    // カード画像を表示するImage
    [SerializeField] private Image cardImage = null;

    // 所持枚数を表示するテキスト
    [SerializeField] private TMP_Text countText = null;

    // カードのID
    public int CardID { get; private set; }

    // 所持枚数を保持する変数
    private int ownedCount = 0;

    // 所持枚数を外部から取得するためのプロパティ
    public int OwnedCount => ownedCount;

    // カード画像を外部から取得するためのプロパティ
    public Image CardImage => cardImage;

    // 詳細メニューを閉じた後に実行する処理
    private Action onDetailClosed = null;

    /// <summary>
    /// 詳細メニュー終了後の処理を設定する
    /// </summary>
    public void SetDetailClosedCallback(Action callback) {

        // 呼び出し元から渡された処理を登録する
        onDetailClosed = callback;
    }

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
    public void SetOwnedState(bool isOwned) {

        // 所持しているカードは通常色、未所持カードは灰色にする
        if (cardImage != null) {
            cardImage.color = isOwned ? Color.white : Color.gray;
        }
    }

    /// <summary>
    /// 右クリックされたときの処理
    /// </summary>
    public void OnRightClick() {

        // 詳細メニューを開き、閉じるまで待機する
        OpenDetailMenuAsync().Forget();
    }

    /// <summary>
    /// 詳細メニュー終了後に一覧を更新する
    /// </summary>
    private async UniTask OpenDetailMenuAsync() {

        // カード詳細メニューが閉じるまで待機する
        await MenuWindowManager.Instance.CardDetailMenuOpen(CardID);

        // 詳細メニュー終了後の処理を実行する
        onDetailClosed?.Invoke();
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