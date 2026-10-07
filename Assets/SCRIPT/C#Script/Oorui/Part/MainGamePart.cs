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

    // カードオブジェクト管理のPrefab
    [SerializeField]
    private GameObject cardObjectManagerPrefab = null;
    // 生成したカード管理のインスタンス
    private CardObjectManager cardObjectManager = null;

    [SerializeField]
    private GameObject patisuro = null;
    // 生成したPrefabを保持する
    private GameObject pachisuroInstance = null;

    //使用デッキのID
    public static int deckID { get; private set; } = 0;
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
        Vector3 pos = new Vector3(0, 0, 0);
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
        // オブジェクトを表示する
        pachisuroInstance.SetActive(true);

        // バトル準備を行う
        // 使用デッキIDを渡す
        await buttleSettings.Open(deckID);

        // ループ
        while (battleSystemManager.battleState == BattleState.InProgress) {
            // フェイズの進行
            await battleSystemManager.PhaseExecute();
            // フレーム待機
            await UniTask.Yield();
        }

        // バトル終了後の結果を取得する
        BattleState result = battleSystemManager.battleState;

        // 勝敗が決まったらリザルトパートに遷移
        Debug.Log("試合終了");
        // 勝敗を渡して、リザルトパートに遷移
        await PartManager.Instance.TransitionPart(GamePart.EndGame, result);
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
        // カードオブジェクト管理のPrefabを作成
        if (cardObjectManagerPrefab != null) {
            // 自身の傘下に生成
            GameObject cardObjectObject = Instantiate(cardObjectManagerPrefab, transform);
            // 生成したオブジェクトから管理コンポーネントを取得
            cardObjectManager = cardObjectObject.GetComponent<CardObjectManager>();
            // 管理コンポーネントが取得できたか確認
            if (cardObjectManager == null) return;
            // カード管理を初期化
            cardObjectManager.Initialize();
        }
    }

    /// <summary>
    /// 使用デッキの登録
    /// </summary>
    /// <param name="id"></param>
    public void SetDeckID(int id) {
        deckID = id;
    }
}
