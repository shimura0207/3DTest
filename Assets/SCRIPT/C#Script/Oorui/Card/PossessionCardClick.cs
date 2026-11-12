/*
 *  @file   PossessionCardClick
 *  @author oorui
 */

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 所持カードのクリック処理を管理するクラス
/// </summary>
public class PossessionCardClick : MonoBehaviour, IPointerClickHandler {

    // クリックされたときに実行する処理
    private Action<int, Image> onCardClicked = null;

    // カード情報を管理するクラス
    private PossessionCardItem possessionCardItem = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Awake() {

        // 同じオブジェクトにあるカード情報を取得する
        possessionCardItem = GetComponent<PossessionCardItem>();
    }

    /// <summary>
    /// クリック時の処理を登録する
    /// </summary>
    public void SetClickAction(Action<int, Image> action) {

        // クリック時の処理を設定する
        onCardClicked = action;
    }

    /// <summary>
    /// カードがクリックされたときの処理
    /// </summary>
    public void OnPointerClick(PointerEventData eventData) {
        // カード情報が取得できなければ終了する
        if (possessionCardItem == null) return;

        // 左クリックの場合
        if (eventData.button == PointerEventData.InputButton.Left) {

            // カードIDとImageを渡す
            onCardClicked?.Invoke(
                possessionCardItem.CardID,
                possessionCardItem.CardImage
            );
        }

        // 右クリックの場合
        else if (eventData.button == PointerEventData.InputButton.Right) {

            // カード詳細メニューを開く
            possessionCardItem.OnRightClick();
        }
    }
}