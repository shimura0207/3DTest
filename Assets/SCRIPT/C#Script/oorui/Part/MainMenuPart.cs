/*
 *  @file   MainMenuPart
 *  @author oorui
 */
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

/// <summary>
/// メインメニューパート
/// </summary>
public class MainMenuPart : PartBase {

    private const string _MENUWINDOW_MATCHMAKINGMENU = "Prefab/Part/MenuWindow/MatchMakingMenu";

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // メニューの初期化
        await MenuWindowManager.instance.Get<MatchmakingMenu>(_MENUWINDOW_MATCHMAKINGMENU).Initialize();   // マッチング画面
        
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // メインパートへ遷移
       await UniTask.CompletedTask;
    }
}
