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

        // デバッグログを表示する
        Debug.Log("StandbyPart通過");

        // タイトルパートへ遷移し、遷移処理が完了するまで待機する
        await PartManager.Instance.TransitionPart(eGamePart.Title);
    }
}
