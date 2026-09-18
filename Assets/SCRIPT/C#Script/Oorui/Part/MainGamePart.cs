/*
 *  @file   MainGamePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// メインゲームパート
/// ※対戦
/// </summary>
public class MainGamePart : PartBase {
    // バトル開始前準備メニューの階層パス
    private const string _MENUWINDOW_BUTTLESETTINGSMENU = "Prefab/Part/MenuWindow/BattleSettingsMenu";
    // バトル開始前準備メニュー
    private BattleSettingsMenu buttleSettings;

    // バトルシステム管理のPrefab
    [SerializeField]
    private GameObject battleSystemManagerPrefab = null;
    // 生成したバトルシステム管理のインスタンス
    private BattleSystemManager battleSystemManager = null;

    // カード管理のPrefab
    [SerializeField]
    private GameObject areaCardManagerPrefab = null;
    // 生成したカード管理のインスタンス
    private AreaCardManager areaCardManager = null;

    // ゲームの状態
    private BattleState battleState = BattleState.None;

    [SerializeField]
    private GameObject patisuro = null;
    // 生成したPrefabを保持する
    private GameObject pachisuroInstance = null;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // メニューを取得
        buttleSettings = MenuWindowManager.instance.Get<BattleSettingsMenu>(_MENUWINDOW_BUTTLESETTINGSMENU);
        // メニューを初期化
        await buttleSettings.Initialize();
        // バトル関連のマネージャーの生成、初期化
        AllManagerInitialize();
        
    }

    /// <summary>
    /// 開始前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();
        Vector3 pos = new Vector3(-0.06109436f, 1.215314f, -7.767032f);
        // パチスロを出す
        pachisuroInstance = Instantiate(patisuro, pos, Quaternion.identity, transform);
        // 生成したオブジェクトを非表示にする
        pachisuroInstance.SetActive(false);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ゲーム中処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // バトル状態を進行中に初期化する
        battleState = BattleState.InProgress;
        // バトル準備を行う
        await buttleSettings.Open();

        // オブジェクトを表示する
        pachisuroInstance.SetActive(true);

        // ループ
        while (battleState == BattleState.InProgress) {
            // フェイズの進行
            battleSystemManager.PhaseExecute();
            if (Input.GetKeyDown(KeyCode.Q)) {
                battleState = BattleState.Win;
            }
            else if(Input.GetKeyDown(KeyCode.E)){
                battleState = BattleState.Lose;
            }
            // フレーム待機
            await UniTask.Yield();
        }
        // 勝敗が決まったらリザルトパートに遷移
        Debug.Log("試合終了");
        // 勝敗を渡して、リザルトパートに遷移
        await PartManager.Instance.TransitionPart(GamePart.EndGame, battleState);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 終了時の片付け実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
        // 生成したオブジェクトを削除する
        if (pachisuroInstance != null) {
            Destroy(pachisuroInstance);
            pachisuroInstance = null;
        }
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// バトル関連のManagerを生成、初期化を行う
    /// </summary>
    private void AllManagerInitialize() {
        // バトルシステム管理のPrefabを生成
        if (battleSystemManagerPrefab != null) {
            // 自身の傘下に生成
            GameObject battleSystemObjct = Instantiate(battleSystemManagerPrefab, transform);

            // 生成したオブジェクトから管理コンポーネントを取得
            battleSystemManager = battleSystemObjct.GetComponent<BattleSystemManager>();

            // 管理コンポーネントが取得できたか確認
            if (battleSystemManager == null) return;

            // バトルシステム管理を初期化
            battleSystemManager.Initialize();
        }
        // 各エリアのカード管理のPrefabを作成
        if (areaCardManagerPrefab != null) {
            // 自身の傘下に生成
            GameObject areacardObject = Instantiate(areaCardManagerPrefab, transform);
            // 生成したオブジェクトから管理コンポーネントを取得
            areaCardManager = areacardObject.GetComponent<AreaCardManager>();
            // 管理コンポーネントが取得できたか確認
            if (areaCardManager == null) return;
            // カード管理を初期化
            areaCardManager.Initialize();
        }
    }
}
