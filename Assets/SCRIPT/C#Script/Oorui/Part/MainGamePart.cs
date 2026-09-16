/*
 *  @file   MainGamePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// メインゲームパート
/// ※対戦
/// </summary>
public class MainGamePart : PartBase {
    // バトル開始前準備メニューの階層パス
    //private const string _MENUWINDOW_BUTTLESETTINGSMENU = "Prefab/Part/MenuWindow/ButtleSettingsMenu";
    // バトル開始前準備メニュー
    private BattleSettingsMenu buttleSettings;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // メニューを取得
        //buttleSettings = MenuWindowManager.instance.Get<BattleSettingsMenu>(_MENUWINDOW_BUTTLESETTINGSMENU);
        // メニューを初期化
        //await buttleSettings.Initialize();
    }

    /// <summary>
    /// 開始前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ゲーム中処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {

        // バトル準備を行う
        //await buttleSettings.Open();

        ///            ///
        /// バトル処理 ///
        ///            ///


        // 勝敗が決まったらリザルトパートに遷移
        Debug.Log("試合終了");
        await PartManager.Instance.TransitionPart(GamePart.EndGame);
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
