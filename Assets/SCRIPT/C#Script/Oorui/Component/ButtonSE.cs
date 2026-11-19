/*
 *  @file   ButtonSE
 *  @author oorui
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// ボタン押下時のSEを管理するクラス
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSE : MonoBehaviour, IPointerDownHandler {

    /// <summary>
    /// ボタンが押された瞬間に呼び出される
    /// </summary>
    public void OnPointerDown(PointerEventData eventData) {

        // ボタン押下時のSEを再生する
        AudioManager.instance.PlaySE("Button");
    }
}