/*
 * @file    OptionMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

/// <summary>
/// オプション画面を開くメニューウィンドウ
/// </summary>
public class OptionMenu : MenuWindowBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    /// <summary>
    /// ウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        await base.Open();


        // 処理が終わったあとに↓
        // メニューウィンドウを閉じる
        await Close();
    }

    /// <summary>
    /// ウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        await base.Close();

        await UniTask.CompletedTask;
    }
}
