/*
 * @file    TitleMenu.cs
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
/// <summary>
/// タイトルで表示するメニューウィンドウ
/// </summary>
public class TitleMenu : MenuWindowBase {

    /// <summary>
    /// 初期化処理　
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // 基底側でメニューウィンドウを表示
        await base.Open();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューウィンドウが非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
        await UniTask.CompletedTask;
    }

}
