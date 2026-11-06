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
        // ターン終了時効果があるか確認
        foreach (var monster in CardObjectManager.instance.fieldObjectList[PlayerType.Self]) {
            if (!monster.Value) continue;
            foreach (var ability in monster.Value.abilityList) {
                if (ability.abilityTiming != AbilityTiming.TurnEnd) continue;
                // 見つかったターン開始時効果を積む
                AbilityManager.instance.SetAblityQueue(monster.Value, ability);
            }
        }

        // 積まれた効果があれば発動
        await AbilityManager.instance.AbilityExecute();

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
        // ターン終了時効果があるか確認
        foreach (var monster in CardObjectManager.instance.fieldObjectList[PlayerType.Opponent]) {
            if (!monster.Value) continue;
            foreach (var ability in monster.Value.abilityList) {
                if (ability.abilityTiming != AbilityTiming.TurnEnd) continue;
                // 見つかったターン開始時効果を積む
                AbilityManager.instance.SetAblityQueue(monster.Value, ability);
            }
        }

        // 積まれた効果があれば発動
        await AbilityManager.instance.AbilityExecute();

        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }
}
