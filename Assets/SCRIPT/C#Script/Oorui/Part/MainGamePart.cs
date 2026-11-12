/*
 *  @file   MainGamePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
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
    //private BattleSettingsMenu buttleSettings;

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
    // 生成したカードオブジェクト管理のインスタンス
    private CardObjectManager cardObjectManager = null;

    // カード効果管理のPrefab
    [SerializeField]
    private GameObject abilityManagerPrefab = null;
    // 生成したカード効果管理のインスタンス
    private AbilityManager abilityManager = null;

    [SerializeField]
    private GameObject patisuro = null;
    // 生成したPrefabを保持する
    private GameObject pachisuroInstance = null;

    //使用デッキのID
    public static int deckID { get; private set; } = 0;

    BattleState result = BattleState.None;
    // メインゲームで遷移するパート
    MainGamePartType type = MainGamePartType.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
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
        result = BattleState.None;
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ゲーム中処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // オブジェクトを表示する
        pachisuroInstance.SetActive(true);

        // ゲームの種類によって処理を分岐
        switch (type) {

            // マッチング対戦
            case MainGamePartType.Matching:
                // 対戦用更新処理
                await MatchingBattleExecute();
                break;

            // チュートリアル
            case MainGamePartType.Tutotrial:
                // チュートリアル用更新処理
                await TutorialGameExecute();
                break;
        }
    }

    /// <summary>
    /// マッチングのExecute処理
    /// </summary>
    /// <returns></returns>
    private async UniTask MatchingBattleExecute() {
        // バトル開始前の準備を行う
        await MenuWindowManager.Instance.BattleSettingsMenuOpen(deckID);
        // UIの進行処理
        UniTask task = UIExecute();
        // バトルの進行処理
        await BattleExecute();
        // バトル終了後の結果を取得する
        result = battleSystemManager.battleState;
        BattleSystemManager.instance.BattleTeardown();
        // リザルトパートに遷移する
        await PartManager.Instance.TransitionPart(GamePart.EndGame, result);
    }

    /// <summary>
    /// チュートリアル用更新処理
    /// </summary>
    /// <returns></returns>
    private async UniTask TutorialGameExecute() {
        // チュートリアルデッキを取得
        List<int> deck = DeckDataMasterUtility.GetDeckMaster(1);
        // レンタルデッキが取得できなければ処理を終了する
        if (deck == null) return;




        // メインメニューパートに遷移
        await PartManager.Instance.TransitionPart(GamePart.MainMenu);
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
        // カード効果管理のPrefabを作成
        if (abilityManagerPrefab != null) {
            // 自身の傘下に生成
            GameObject abilityObject = Instantiate(abilityManagerPrefab, transform);
            // 生成したオブジェクトから管理コンポーネントを取得
            abilityManager = abilityObject.GetComponent<AbilityManager>();
            // 管理コンポーネントが取得できたか確認
            if (abilityManager == null) return;
            // カード効果管理を初期化
            abilityManager.Initialize();
        }
    }

    /// <summary>
    /// 使用デッキの登録
    /// </summary>
    /// <param name="id"></param>
    public void SetDeckID(int id, MainGamePartType type) {

        // マッチングであればデッキIDと種類を渡す
        if (type == MainGamePartType.Matching) {
            deckID = id;
            this.type = type;
        }
        else if (type == MainGamePartType.Tutotrial) {
            this.type = type;
        }
    }
    /// <summary>
    /// バトルの進行処理
    /// </summary>
    private async UniTask BattleExecute() {
        // バトルが進行中の間、フェイズを進行する
        while (battleSystemManager.battleState == BattleState.InProgress) {
            await battleSystemManager.Execute();

            // 次のフレームまで待機する
            await UniTask.Yield();
        }
    }

    /// <summary>
    /// UIManagerの更新処理を回す
    /// </summary>
    private async UniTask UIExecute() {
        // バトルが進行中の間、UIを更新する
        while (battleSystemManager.battleState == BattleState.InProgress) {
            // UIの入力判定と更新を行う
            UIManager.instance.Execute();

            // 次のフレームまで待機する
            await UniTask.Yield();
        }
    }
}
