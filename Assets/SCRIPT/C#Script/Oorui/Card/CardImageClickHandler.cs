/*
 *  @file   CardImageClickHandler
 *  @author oorui
 */

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// カード画像のクリックイベントを親に伝える
/// </summary>
public class CardImageClickHandler : MonoBehaviour, IPointerClickHandler {

    // 親のPossessionCardItem
    private PossessionCardItem possessionCardItem;

    private void Awake() {

        // 親オブジェクトからPossessionCardItemを取得
        possessionCardItem = GetComponentInParent<PossessionCardItem>();
    }

    /// <summary>
    /// 画像がクリックされたときの処理
    /// </summary>
    public void OnPointerClick(PointerEventData eventData) {

        // 右クリック以外は無視
        if (eventData.button != PointerEventData.InputButton.Right) return;

        // 親のカード詳細メニューを開く処理を呼び出す
        possessionCardItem?.OnRightClick();
    }
}