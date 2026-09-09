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

    private const string _MENUWINDOW_TITLE = "Prefab/Part/MenuWindow/TitleMenu";


    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // メニューの初期化
        await MenuWindowManager.instance.Get<TitleMenu>(_MENUWINDOW_TITLE).Initialize();
    }
    
    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // BGM再生

        // タイトルメニューウィンドウ表示
        await MenuWindowManager.instance.Get<TitleMenu>().Open();

        // メインパートへ遷移
        Debug.Log("TitlePart通過");
        UniTask task = PartManager.Instance.TransitionPart(eGamePart.MainMenu);
        await UniTask.CompletedTask;
    }
}
