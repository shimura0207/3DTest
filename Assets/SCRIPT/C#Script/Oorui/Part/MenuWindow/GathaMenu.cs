/*
 * @file    GathaMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// ガチャメニューウィンドウ
/// </summary>
public class GathaMenu : MenuWindowBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        //  基底側でメニューウィンドウを表示
        await base.Open();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
        await UniTask.CompletedTask;
    }
}
