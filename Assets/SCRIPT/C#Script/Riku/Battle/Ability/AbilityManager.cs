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

    // 発動待機効果
    private Queue<Func<UniTask>> abilityQueue = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        instance = this;

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
    public void SetAblityQueue(CardAbilityDatabase.AbilityData ability) {
        Func<UniTask> func = null;
        switch (ability.abilityType) {
            case AbilityType.Normal:
                func = GetNormalAbility(ability.abilityTiming, ability.abilityTarget, ability.abilityProcess);
                break;
            case AbilityType.Special:
                func = GetSpecialAbility(ability.specialAbility);
                break;
        }
        abilityQueue.Enqueue(func);
    }

    /// <summary>
    /// 通常効果の取得
    /// </summary>
    /// <param name="timig">効果タイミング</param>
    /// <param name="target">効果対象</param>
    /// <param name="process">効果内容</param>
    /// <returns></returns>
    private Func<UniTask> GetNormalAbility(AbilityTiming timig, AbilityTarget target, AbilityProcess process) {
        Func<UniTask> func = null;
        switch (process) {
            case AbilityProcess.Draw:
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
    /// <param name="ability">効果</param>
    /// <returns></returns>
    private Func<UniTask> GetSpecialAbility(SpecialAbility ability) {
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
