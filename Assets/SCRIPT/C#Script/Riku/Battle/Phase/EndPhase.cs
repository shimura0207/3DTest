/*
 * @file    EndPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターン終了フェイズ
/// </summary>
public class EndPhase : PhaseBase {
    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        // ターン終了時処理(今後実装予定)

        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask OpponentExecute() {
        // ターン終了時処理(今後実装予定)

        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }
}
