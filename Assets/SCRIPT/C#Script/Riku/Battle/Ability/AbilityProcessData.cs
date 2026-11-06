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
}
