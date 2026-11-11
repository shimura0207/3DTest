/*
 * @file    StartPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターン開始フェイズ
/// </summary>
public class StartPhase : PhaseBase {
    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        // ターン開始時効果があるか確認
        foreach (var monster in CardObjectManager.instance.fieldObjectList[PlayerType.Self]) {
            if (!monster.Value) continue;
            foreach (var ability in monster.Value.abilityList) {
               if (ability.abilityTiming != AbilityTiming.TurnStart) continue;
               // 見つかったターン開始時効果を積む
               AbilityManager.instance.SetAblityQueue(monster.Value, ability);
            }
        }

        // 積まれた効果があれば発動
        await AbilityManager.instance.AbilityExecute();

        // ターン開始時ドロー
        BattleSystemManager.instance.DrawCard(PlayerType.Self);

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
        // ターン開始時効果があるか確認
        foreach (var monster in CardObjectManager.instance.fieldObjectList[PlayerType.Opponent]) {
            if (!monster.Value) continue;
            foreach (var ability in monster.Value.abilityList) {
                if (ability.abilityTiming != AbilityTiming.TurnStart) continue;
                // 見つかったターン開始時効果を積む
                AbilityManager.instance.SetAblityQueue(monster.Value, ability);
            }
        }

        // 積まれた効果があれば発動
        await AbilityManager.instance.AbilityExecute();

        // ターン開始時ドロー
        BattleSystemManager.instance.DrawCard(PlayerType.Opponent);

        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }
}
