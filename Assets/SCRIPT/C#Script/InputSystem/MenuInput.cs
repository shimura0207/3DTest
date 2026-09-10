/*
 *  @file   MenuInput.cs
 *  @author oorui
 */

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// メニューの入力を共通で処理するクラス
/// </summary>
public class MenuInput : MonoBehaviour, IPointerClickHandler {

    // 親階層に存在するメニュー
    private MenuWindowBase menuWindow;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Awake() {

        // 親階層から対応するMenuWindowBaseを取得する
        menuWindow = GetComponentInParent<MenuWindowBase>();
    }

    /// <summary>
    /// このUIがクリックされた時の処理
    /// </summary>
    /// <param name="eventData">クリック情報</param>
    public void OnPointerClick(PointerEventData eventData) {

        // 対応するメニューが取得できている場合のみ処理する
        if (menuWindow != null) {

            // このUIが所属するメニューだけに選択を通知する
            menuWindow.Select();
        }
    }
}