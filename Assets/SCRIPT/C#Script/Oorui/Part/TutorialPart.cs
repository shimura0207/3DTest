/*
 *  @file   TutorialPart.cs
 *  @author oorui
 */

using Cysharp.Threading.Tasks;

public class TutorialPart : PartBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // メニューの初期化


    }

    /// <summary>
    /// 開始前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 実行中処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        // メインメニュー画面に遷移
        UniTask task = PartManager.Instance.TransitionPart(eGamePart.MainMenu);
        await UniTask.CompletedTask;
    }
}
