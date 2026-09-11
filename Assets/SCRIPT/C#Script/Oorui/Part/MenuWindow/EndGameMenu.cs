/*
 * @file    EndGameMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
/// <summary>
/// リザルトで表示するメニューウィンドウ
/// </summary>
public class EndGameMenu : MenuWindowBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    /// <summary>
    /// リザルトウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // 基底側でリザルトメニューウィンドウを表示
        await base.Open();
    }

    /// <summary>
    /// リザルトウィンドウが非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
    }
}
