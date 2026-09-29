
/*
 *  @file   PartManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// パート管理
/// </summary>
public class PartManager : SystemObject {

    /// <summary>
    /// 自身への参照
    /// </summary>
    public static PartManager Instance { get; private set; } = null;

    /// <summary>
    /// パートの参照設定
    /// </summary>
    [SerializeField]
    private PartManagerConfig partManagerConfig = null;

    /// <summary>
    /// 管理中のパートオブジェクト
    /// </summary>
    private PartBase[] partList = null;

    /// <summary>
    /// 現在のパート
    /// </summary>
    private PartBase currentPart = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns>初期化処理のUniTask</returns>
    public override async UniTask Initialize() {

        // 自身をシングルトンのインスタンスに設定する
        Instance = this;

        // PartManagerConfigが設定されていなければ抜ける
        if (partManagerConfig == null) return;

        // ScriptableObjectからパートのPrefab一覧を取得する
        PartBase[] partOriginList = partManagerConfig.PartOriginList;

        // パートの参照一覧が設定されていなければ抜ける
        if (partOriginList == null) return;

        // GamePart.Maxを基準に管理するパート数を設定する
        int partMax = (int)GamePart.Max;

        // パート数の一致確認
        if (partOriginList.Length != partMax) return;

        // 管理するパートの配列を生成する
        partList = new PartBase[partMax];

        // UniTaskをまとめて管理するためのリストを用意する
        List<UniTask> taskList = new List<UniTask>(partMax);

        // 全パートを生成して初期化する
        for (int i = 0; i < partMax; i++) {

            // 生成元のパートPrefabを取得する
            PartBase origin = partOriginList[i];

            // パートの参照が設定されていなければ抜ける
            if (origin == null) return;

            // パートPrefabを自身の子オブジェクトとして生成する
            partList[i] = Instantiate(origin, transform);

            // 生成したパートの初期化処理を登録する
            taskList.Add(partList[i].Initialize());
        }

        // すべてのパートの初期化処理が完了するまで待機する
        await CommonModule.WaitTask(taskList);
    }

    /// <summary>
    /// パートの切り替え
    /// </summary>
    /// <param name="nextPart">切り替え先のパート</param>
    /// <returns>切り替え処理のUniTask</returns>
    public async UniTask TransitionPart(GamePart nextPart) {

        // パートの管理配列が初期化されていなければ抜ける
        if (partList == null) return;

        // パートの列挙値が有効な範囲か確認する
        int nextPartIndex = (int)nextPart;
        if (nextPartIndex < 0 || nextPartIndex >= partList.Length) return;

        // 現在のパートが存在する場合は終了処理を実行する
        if (currentPart != null) {
            await currentPart.Teardown();
        }

        // 切り替え先のパートを取得する
        currentPart = partList[nextPartIndex];

        // 切り替え先のパートが存在しなければ抜ける
        if (currentPart == null) return;

        // 切り替え先のパートをセットアップする
        await currentPart.Setup();

        // 切り替え先のパートを実行する
        await currentPart.Execute();
    }

    /// <summary>
    /// パートの切り替え
    /// 勝敗を入力
    /// </summary>
    /// <param name="nextPart">切り替え先のパート</param>
    /// <param name="state">対戦結果</param>
    /// <returns>切り替え処理のUniTask</returns>
    public async UniTask TransitionPart(GamePart nextPart, BattleState state) {
        // パートの管理配列が初期化されていなければ抜ける
        if (partList == null) return;

        // パートの列挙値が有効な範囲か確認する
        int nextPartIndex = (int)nextPart;
        if (nextPartIndex < 0 || nextPartIndex >= partList.Length) return;

        // 現在のパートが存在する場合は終了処理を実行する
        if (currentPart != null) {
            await currentPart.Teardown();
        }

        // 切り替え先のパートを取得する
        currentPart = partList[nextPartIndex];

        // 切り替え先のパートが存在しなければ抜ける
        if (currentPart == null) return;

        // 切り替え先がEndGamePartか確認する
        if (currentPart is EndGamePart endGamePart) {
            // リザルト画面に表示する対戦結果を設定する
            endGamePart.SetBattleResult(state);
        }

        // 切り替え先のパートのセットアップを行う
        await currentPart.Setup();

        // 切り替え先のパートを実行する
        await currentPart.Execute();
    }

    /// <summary>
    /// パートの切り替え
    /// 接続状況を入力
    /// </summary>
    /// <param name="nextPart">切り替え先のパート</param>
    /// <param name="state">接続状況</param>
    /// <returns>切り替え処理のUniTask</returns>
    public async UniTask TransitionPart(GamePart nextPart, ConnectionState state) {
        // パートの管理配列が初期化されていなければ抜ける
        if (partList == null) return;

        // パートの列挙値が有効な範囲か確認する
        int nextPartIndex = (int)nextPart;
        if (nextPartIndex < 0 || nextPartIndex >= partList.Length) return;

        // 現在のパートが存在する場合は終了処理を実行する
        if (currentPart != null) {
            await currentPart.Teardown();
        }

        // 切り替え先のパートを取得する
        currentPart = partList[nextPartIndex];

        // 切り替え先のパートが存在しなければ抜ける
        if (currentPart == null) return;

        // 切り替え先がPrivateMatchingPartか確認する
        if (currentPart is PrivateMatchingPart privateMatching) {
            // リザルト画面に表示する対戦結果を設定する
            // endGamePart.SetBattleResult(state);
        }

        // 切り替え先のパートのセットアップを行う
        await currentPart.Setup();

        // 切り替え先のパートを実行する
        await currentPart.Execute();
    }
}

