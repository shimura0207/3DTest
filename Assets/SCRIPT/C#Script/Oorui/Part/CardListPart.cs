/*
 *  @file   CardListPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カード関連の処理をするパート
/// </summary>
public class CardListPart : PartBase {
    public bool isreturnPart = false;   // 一つ前のシーンに戻る
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 基底側の処理を行う
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 使用前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SetUp() {
        await base.SetUp();
        Debug.Log("カード関連画面を表示中");

        // フラグを初期化する
        isreturnPart = false;
    }
    
    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが押されたらひとつ前のパートに戻る
        if (isreturnPart) {
            // メインメニューパートに戻る
            Debug.Log("メインメニューに戻りました");
            await PartManager.Instance.TransitionPart(eGamePart.MainMenu);
        }
    }



}
