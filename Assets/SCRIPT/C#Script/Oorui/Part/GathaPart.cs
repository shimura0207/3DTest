/*
 *  @file   GathaPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ガチャパート
/// </summary>
public class GathaPart : PartBase {
    public bool isreturnPart = false;   // 一つ前のシーンに戻る

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();
        Debug.Log("ガチャ画面表示中");
        isreturnPart = false;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが押されたらひとつ前のパートに戻る
        if (isreturnPart) {
            await PartManager.Instance.TransitionPart(GamePart.MainMenu);
        }
    }

}
