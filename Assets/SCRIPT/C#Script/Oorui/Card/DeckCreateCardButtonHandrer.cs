using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckCreateCardButtonHandrer : MonoBehaviour
{

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
