/*
 *  @file   MainMenuPart
 *  @author oorui
 */
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;

/// <summary>
/// メインメニューパート
/// </summary>
public class MainMenuPart : PartBase {

    private const string _MENUWINDOW_MATCHMAKINGMENU = "Prefab/Part/MenuWindow/MatchMakingMenu";    // マッチング画面の階層パス
    private const string _MENUWINDOW_OPTIONMENU = "Prefab/Part/MenuWindow/OptionMenu";              // オプション画面の階層パス

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // メニューの初期化
        await MenuInitialize();
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // メインパートへ遷移
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューウィンドウの初期化処理
    /// </summary>
    private async UniTask MenuInitialize() {
        // メニューの初期化
        await MenuWindowManager.instance.Get<MatchmakingMenu>(_MENUWINDOW_MATCHMAKINGMENU).Initialize();   // マッチング画面
        await MenuWindowManager.instance.Get<OptionMenu>(_MENUWINDOW_OPTIONMENU).Initialize();             // オプション画面
    }
}
