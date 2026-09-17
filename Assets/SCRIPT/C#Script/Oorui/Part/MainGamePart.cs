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

    [SerializeField]
    private GameObject patisuro = null;
    // 生成したPrefabを保持する
    private GameObject patisuroInstance = null;
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
        Vector3 pos = new Vector3(-0.06109436f, 1.215314f, -7.767032f);
        // パチスロを出す
        patisuroInstance = Instantiate(patisuro, pos, Quaternion.identity,transform);
        // 生成したオブジェクトを非表示にする
        patisuroInstance.SetActive(false);
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

        // オブジェクトを表示する
        patisuroInstance.SetActive(true);

        // 勝敗が決まったらリザルトパートに遷移
        Debug.Log("試合終了");
        //await PartManager.Instance.TransitionPart(GamePart.EndGame);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 終了時の片付け実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
        // 生成したオブジェクトを削除する
        if (patisuroInstance != null) {
            Destroy(patisuroInstance);
            patisuroInstance = null;
        }
        await UniTask.CompletedTask;
    }
}
