/*
 *  @file   ClientConectionMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// 入力画面
/// </summary>
public class ClientConectionMenu : MenuWindowBase {
    [SerializeField]
    private NetworkUI networkUI;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    /// <summary>
    /// メニューウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        await base.Open();
        // クライアント処理の実行
        networkUI.StartClient();
    }

    /// <summary>
    /// メニューウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        await base.Close();
    }
}
