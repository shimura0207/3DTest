/*
 *  @file   TitlePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// タイトルパート
/// </summary>
public class TitlePart : PartBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // メニューの初期化
    }
    
    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // メインパートへ遷移
        Debug.Log("TitlePart通過");
        UniTask task = PartManager.Instance.TransitionPart(eGamePart.MainMenu);
        await UniTask.CompletedTask;
    }
}
