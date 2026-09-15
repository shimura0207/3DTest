/*
 * @file    PhaseBase.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各フェイズの基底クラス
/// </summary>
public class PhaseBase {
    // 次のターンへ行くかどうか
    private bool nextPhase = false;

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
