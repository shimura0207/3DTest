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
    /// <param name="player"></param>
    /// <returns></returns>
    public async UniTask DrawAbility(PlayerType player) {
        BattleSystemManager.instance.DrawCard(player);
        await UniTask.CompletedTask;
    }
}
