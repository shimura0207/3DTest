/*
 * @file    AbilityProcessData.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードの効果処理
/// </summary>
public class AbilityProcessData {

    /// <summary>
    /// ドロー効果
    /// </summary>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="value">ドロー枚数</param>
    /// <returns></returns>
    public async UniTask DrawAbility(PlayerType player, int value) {
        // 枚数に指定がない場合は1枚
        if (value <= 0) value = 1;
        for (int i = 0; i < value; i++ ) {
            BattleSystemManager.instance.DrawCard(player);
        }
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ダメージ付与効果
    /// </summary>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">ダメージ量</param>
    /// <returns></returns>
    public async UniTask GiveDamagerAbility(PlayerType player, AbilityTarget target, int value) {
        // 発動プレイヤーの相手
        PlayerType reversePlayer = PlayerType.None;
        if (player == PlayerType.Self) {
            reversePlayer = PlayerType.Opponent;
        }
        else if (player == PlayerType.Opponent) {
            reversePlayer = PlayerType.Self;
        }
        // 枚数に指定がない場合は1ダメージ
        if (value <= 0) value = 1;
        switch (target) {
            case AbilityTarget.SelfPlayer:
                BattleSystemManager.instance.PlayerGiveDamage(player, value);
                break;
            case AbilityTarget.OpponentPlayer:
                BattleSystemManager.instance.PlayerGiveDamage(reversePlayer, value);
                break;
            case AbilityTarget.SelfRandomMonster:
                break;
            case AbilityTarget.OponentRandomMonster:
                break;
            case AbilityTarget.SelfSelectMonster:
                break;
            case AbilityTarget.OpponentSelectMonster:
                break;
            case AbilityTarget.SelfAllMonster:
                break;
            case AbilityTarget.OpponentAllMonster:
                break;
            case AbilityTarget.ThisMonster:
                break;
            case AbilityTarget.OtherMonster:
                break;
        }

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// HP増加効果
    /// </summary>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="value">増加量</param>
    /// <returns></returns>
    public async UniTask GiveHealthAbility(PlayerType player, int value) {

    }
}
