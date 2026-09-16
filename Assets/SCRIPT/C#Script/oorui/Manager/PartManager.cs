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
    private PartBase[] _partOriginList = null;

    /// <summary>
    /// 管理中のパートオブジェクト
    /// </summary>
    private PartBase[] _partList = null;

    /// <summary>
    /// 現在のパート
    /// </summary>
    private PartBase _currentPart = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        Instance = this;
        // パートの生成
        int partMax = (int)eGamePart.Max;
        // リストに生成
        _partList = new PartBase[partMax];

        // UniTaskをまとめて管理するためのリストを用意
        List<UniTask> taskList = new List<UniTask>(partMax);
        for (int i = 0; i < partMax; i++) {
            // パートオブジェクトの生成
            _partList[i] = Instantiate(_partOriginList[i], transform);
            // 初期化処理を実行
            taskList.Add(_partList[i].Initialize());
        }

        // すべてのパートの初期化処理を待つ
        await CommonModule.WaitTask(taskList);
    }

    /// <summary>
    /// パートの切り替え
    /// </summary>
    /// <param name="nextPart"></param>
    /// <returns></returns>
    public async UniTask TransitionPart(eGamePart nextPart) {
        // 現在のパートの切り替え
        if (_currentPart != null) await _currentPart.Teardown();
        // パートの切り替え
        _currentPart = _partList[(int)nextPart];
        await _currentPart.Setup();

        // 次のパートの実行
        await _currentPart.Execute();
    }
}
