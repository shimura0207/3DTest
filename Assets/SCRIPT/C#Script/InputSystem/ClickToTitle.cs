/*
 *  @file   ClickToTitle
 *  @author oorui
 */

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// タイトル画面のクリック用クラス
/// </summary>
public class ClickToTitle : MonoBehaviour {
    // 入力を受け取るInputAction
    [SerializeField]
    private InputActionReference click;

    // 判定を置く親ウィンドウ
    private TitleMenu title;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start() {
        // 親オブジェクトのScriptを取得
        title = GetComponentInParent<TitleMenu>();
        if(click != null) {
            click.action.performed += OnClick;

            // 有効化
            click.action.Enable();
        }
    }

    /// <summary>
    /// クリック時の処理
    /// </summary>
    /// <param name="context"></param>
    private void OnClick(InputAction.CallbackContext context) {
        // メニューを閉じる用のフラグをtrueにする
        title.isCloseMenu = true;
    }

}
