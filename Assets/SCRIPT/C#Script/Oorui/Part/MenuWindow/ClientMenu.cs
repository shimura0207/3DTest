/*
 *  @file   ClientMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// クライアント側で表示するメニュー
/// </summary>
public class ClientMenu : MenuWindowBase {
    // Clientとしてルームに参加するためのNetworkUI
    [SerializeField]
    private NetworkUI networkUI;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    public override async UniTask Open() {
        await base.Open();
        // 
    }
}
