/*
 *  @file   CardListPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カード関連の処理をするパート
/// </summary>
public class CardListPart : PartBase {
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
    public override async UniTask Setup() {
        await base.Setup();
        Debug.Log("カード関連画面を表示中");
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
  

    }

}
