/*
 *  @file   CardListPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カード一覧パート
/// </summary>
public class CardListPart : PartBase {

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
        Debug.Log("カード一覧画面表示中");
        
    }
    

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {

    }
}
