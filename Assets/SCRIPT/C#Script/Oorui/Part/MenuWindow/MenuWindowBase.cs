/*
 *  @file   MenuBase.cs
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// メニューウィンドウの基底クラス
/// </summary>
public class MenuWindowBase : MonoBehaviour {
    // ウィンドウの配置位置
    [SerializeField]
    private GameObject menuRoot = null;

    // メニューが選択された時に通知するイベント
    public event Action<MenuWindowBase> OnSelected;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public virtual async UniTask Initialize() {
        // すべてのウィンドウを非表示にする
        menuRoot?.SetActive(false);
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

    /// <summary>
    /// メニューが選択されたことを通知する
    /// </summary>
    public void Select() {
        // 自分自身を選択されたメニューとして通知する
        OnSelected?.Invoke(this);
    }
}
