/*
 * @file    PhaseBase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

/// <summary>
/// 各フェイズの基底クラス
/// </summary>
public class PhaseBase : MonoBehaviour{
    // 次のフェイズへ行くかどうか
    public bool nextPhase { get; protected set; } = false;

    /// <summary>
    /// 自身のターン実行処理
    /// </summary>
    /// <returns></returns>
    public virtual async UniTask SelfExecute() {
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 相手のターン実行処理
    /// </summary>
    /// <returns></returns>
    public virtual async UniTask OpponentExecute() {
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 実行処理が呼ばれる前の準備
    /// </summary>
    public virtual void Setup() {}

    /// <summary>
    /// 片付け処理
    /// </summary>
    public virtual void Teardown() {
        nextPhase = false;
    }
}
