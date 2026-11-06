/*
 * @file    AbilityManager.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カード効果管理クラス
/// </summary>
public class AbilityManager : MonoBehaviour {
    // 自身への参照
    public static AbilityManager instance { get; private set; } = null;

    // 効果データ
    private AbilityProcessData abilityProcessData = null;

    // 発動待機効果
    private Queue<Func<UniTask>> abilityQueue = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        instance = this;
        abilityProcessData = new AbilityProcessData();

        abilityQueue = new Queue<Func<UniTask>>();
    }

    /// <summary>
    /// 効果実行処理
    /// </summary>
    /// <returns></returns>
    public async UniTask AbilityExecute() {
        // 積まれた効果をひとつずつ発動
        while (true) {
            if (abilityQueue.Count <= 0) break;
            var ability = abilityQueue.Dequeue();
            if (ability == null) break; 

            // 効果発動
            await ability();
            
            // 次のフレームまで待機
            await UniTask.Yield();
        }
    }

    /// <summary>
    /// 効果をキューに積む
    /// </summary>
    /// <param name="ability">発動する効果</param>
    /// <param name="player">発動させたプレイヤー</param>
    public void SetAblityQueue(PlayerType player, CardAbilityDatabase.AbilityData ability) {
        Func<UniTask> func = null;
        switch (ability.abilityType) {
            case AbilityType.Normal:
                func = GetNormalAbility(player, ability.abilityTiming, ability.abilityTarget, ability.abilityProcess, ability.abilityValue);
                break;
            case AbilityType.Special:
                func = GetSpecialAbility(player, ability.specialAbility);
                break;
        }
        abilityQueue.Enqueue(func);
    }

    /// <summary>
    /// 通常効果の取得
    /// </summary>
    /// <param name="player">発動させたプレイヤー</param>
    /// <param name="timig">効果タイミング</param>
    /// <param name="target">効果対象</param>
    /// <param name="process">効果内容</param>
    /// <param name="value">効果値</param>
    /// <returns></returns>
    private Func<UniTask> GetNormalAbility(PlayerType player, AbilityTiming timig, AbilityTarget target, AbilityProcess process, int value) {
        Func<UniTask> func = null;
        // ラムダ式で引数を固定して代入する
        switch (process) {
            case AbilityProcess.Draw:
                func = () => abilityProcessData.DrawAbility(player, value);
                break;
            case AbilityProcess.Search:
                break;
            case AbilityProcess.GiveDamage:
                break;
            case AbilityProcess.HeelHP:
                break;
            case AbilityProcess.DiscardHand:
                break;
            case AbilityProcess.UpATK:
                break;
            case AbilityProcess.DownATK:
                break;
            case AbilityProcess.SummonMonster:
                break;
            case AbilityProcess.TransformationMonster:
                break;
            case AbilityProcess.GiveState:
                break;
            case AbilityProcess.DeleteMonster:
                break;
        }
        return func;
    }

    /// <summary>
    /// 特殊効果の取得
    /// </summary>
    /// <param name="player">発動させたプレイヤー</param>
    /// <param name="ability">効果</param>
    /// <returns></returns>
    private Func<UniTask> GetSpecialAbility(PlayerType player, SpecialAbility ability) {
        Func<UniTask> func = null;
        switch (ability) {
            case SpecialAbility.Isuka1:
                break;
            case SpecialAbility.Isuka2:
                break;
        }
        return func;
    }
}
