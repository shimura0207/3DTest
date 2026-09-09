/*
 *  @file   MainGamePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

/// <summary>
/// メインゲームパート
/// ※対戦
/// </summary>
public class MainGamePart : PartBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    } 

    /// <summary>
    /// 開始前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SetUp() {
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ゲーム中処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        
    }

    /// <summary>
    /// 終了時の片付け実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
        await UniTask.CompletedTask;
    }
}
