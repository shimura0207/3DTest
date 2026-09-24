/*
 * @file    PhaseBase.cs
 * @author  Riku
 */

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
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public virtual bool SelfExecute() {
        return nextPhase;
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public virtual bool OpponentExecute() {
        return nextPhase;
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    public virtual void Teardown() {
        nextPhase = false;
    }
}
