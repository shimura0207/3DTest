/*
 * @file    EndPhase.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターン終了フェイズ
/// </summary>
public class EndPhase : PhaseBase {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override bool SelfExecute() {
        // ターン終了時処理(今後実装予定)

        // ターンプレイヤーを交代
        BattleSystemManager.instance.ChangeTurnPlayer();
        // 次のフェイズへ
        nextPhase = true;

        return base.SelfExecute();
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override bool OpponentExecute() {
        // ターン終了時処理(今後実装予定)

        // ターンプレイヤーを交代
        BattleSystemManager.instance.ChangeTurnPlayer();
        // 次のフェイズへ
        nextPhase = true;

        return base.OpponentExecute();
    }
}
