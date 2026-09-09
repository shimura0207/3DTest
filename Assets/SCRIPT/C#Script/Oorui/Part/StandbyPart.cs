/*
 *  @file   StandbyPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 準備パート
/// </summary>
public class StandbyPart : PartBase {
    public override async UniTask Execute() {
        // マスターデータの読み込み

        // タイトルパートへ遷移
        Debug.Log("StandbyPart通過");
        UniTask task = PartManager.Instance.TransitionPart(eGamePart.Title);
        await UniTask.CompletedTask;
    }
}
