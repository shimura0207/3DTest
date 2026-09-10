/*
 *  @file   MattchingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// マッチングパート
/// </summary>
public class MatchingPart : PartBase {
    public bool isReturnPart = false;       // 一つ前のシーンに戻る
    public bool isMatchingClick = false;    // マッチングボタンを押したかどうか

   

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    public override async UniTask SetUp() {
        await base.SetUp();
        Debug.Log("マッチング画面表示中");
        // フラグを初期化する
        isReturnPart = false;
        isMatchingClick = false;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {



        // ←が押されたらひとつ前のパートに戻る
        if (isReturnPart) {
            // メインメニューパートに戻る
            Debug.Log("メインメニューに戻りました。");
            await PartManager.Instance.TransitionPart(eGamePart.MainMenu);
        }

        // マッチングボタンが押されたらマッチング
        if (isMatchingClick) {
            // マッチング開始

            // マッチしたらメインゲームパートに遷移
            Debug.Log("マッチングしました");
            UniTask task = PartManager.Instance.TransitionPart(eGamePart.MainGame);
            await UniTask.CompletedTask;
        }


    }
}
