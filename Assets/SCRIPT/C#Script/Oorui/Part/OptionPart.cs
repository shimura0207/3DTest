/*
 *  @file   OptionPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 設定パート
/// </summary>
public class OptionPart : PartBase {

    public bool isreturnPart = false;   // 一つ前のシーンに戻る

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    public override async UniTask Setup() {
        Debug.Log("設定画面表示中");
        // フラグを初期化する
        isreturnPart = false;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ←が押されたらひとつ前のパートに戻る
        if (isreturnPart) {
            Debug.Log("メインメニューに戻りました。");
            await PartManager.Instance.TransitionPart(eGamePart.MainMenu);
        }
    }
}
