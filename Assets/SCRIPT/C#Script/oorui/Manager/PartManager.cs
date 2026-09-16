/*
 *  @file   PartManager
 *  @author oorui
 */
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
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
    /// パートオブジェクトのオリジナル
    /// </summary>
    [SerializeField]
    private PartBase[] partOriginList = null;

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
    /// <returns></returns>
    public override async UniTask Initialize() {
        Instance = this;
        // パートの生成
        int partMax = (int)GamePart.Max;
        // リストに生成
        partList = new PartBase[partMax];

        // UniTaskをまとめて管理するためのリストを用意
        List<UniTask> taskList = new List<UniTask>(partMax);
        for (int i = 0; i < partMax; i++) {
            // パートオブジェクトの生成
            partList[i] = Instantiate(partOriginList[i], transform);
            // 初期化処理を実行
            taskList.Add(partList[i].Initialize());
        }

        // すべてのパートの初期化処理を待つ
        await CommonModule.WaitTask(taskList);
    }

    /// <summary>
    /// パートの切り替え
    /// </summary>
    /// <param name="nextPart"></param>
    /// <returns></returns>
    public async UniTask TransitionPart(GamePart nextPart) {
        // 現在のパートの切り替え
        if (currentPart != null) await currentPart.Teardown();
        // パートの切り替え
        currentPart = partList[(int)nextPart];
        await currentPart.Setup();

        // 次のパートの実行
        await currentPart.Execute();
    }
}
