/*
 *  @file   MenuBase.cs
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// メニューウィンドウの基底クラス
/// </summary>
public class MenuWindowBase : MonoBehaviour {
    [SerializeField]
    private GameObject menuRoot = null;     // ウィンドウの配置位置

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public virtual async UniTask Initialize() {
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// Window画面を表示する
    /// </summary>
    /// <returns></returns>
    public virtual async UniTask Open() {
        // メニューウィンドウを表示する
        menuRoot?.SetActive(true);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューウィンドウを閉じる
    /// </summary>
    /// <returns></returns>
    public virtual async UniTask Close() {
        // メニューウィンドウを非表示にする
        menuRoot?.SetActive(false);
        await UniTask.CompletedTask;
    }
}
