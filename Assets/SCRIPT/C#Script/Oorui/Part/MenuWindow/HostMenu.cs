/*
 *  @file   HostMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// ホスト側で表示するメニューウィンドウ
/// </summary>
public class HostMenu : MenuWindowBase {

    // Hostとしてネットワークを開始するためのNetworkUI
    [SerializeField]
    private NetworkUI networkUI;

    /// <summary>
    /// 初期化
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
        // ホスト処理を開始する
        networkUI.StartHost();
    }

    /// <summary>
    /// メニューウィンドウが閉じたときの処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
        await UniTask.CompletedTask;
    }
}
