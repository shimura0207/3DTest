/*
 * @file    AbilityManager.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
        abilityProcessData.Initialize();

        abilityQueue = new Queue<Func<UniTask>>();
    }

    /// <summary>
    /// 効果実行処理
    /// </summary>
    /// <returns></returns>
    public async UniTask AbilityExecute() {
        // ボタンの無効化
        BattleSystemManager.instance.SetPachisuroGoActive(false);
        BattleSystemManager.instance.SetTurnEndActive(false);
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
        // ボタンの有効化
        if (BattleSystemManager.instance.turnPlayer == PlayerType.Self ||
            BattleSystemManager.instance.currentPhase == BattlePhase.MainPhase) {
            BattleSystemManager.instance.SetTurnEndActive(true);
            BattleSystemManager.instance.SetPachisuroGoActive(true);
        }
    }

    /// <summary>
    /// 効果をキューに積む
    /// </summary>
    /// <param name="card">発動させたカード</param>
    /// <param name="ability">発動する効果</param>
    public void SetAblityQueue(CardObject card, CardAbilityDatabase.AbilityData ability) {
        Func<UniTask> func = null;
        if (ability.abilityType != AbilityType.Normal) return;
        func = GetNormalAbility(card, ability.abilityTarget, ability.abilityProcess, ability.abilityValue);
        abilityQueue.Enqueue(func);
    }

    /// <summary>
    /// 通常効果の取得
    /// </summary>
    /// <param name="card">発動させたカード</param>
    /// <param name="target">効果対象</param>
    /// <param name="process">効果内容</param>
    /// <param name="value">効果値</param>
    /// <returns></returns>
    private Func<UniTask> GetNormalAbility(CardObject card, AbilityTarget target, AbilityProcess process, int value) {
        Func<UniTask> func = null;
        PlayerType player = card.playerType;
        // ラムダ式で引数を固定して代入する
        switch (process) {
            case AbilityProcess.Draw:
                func = () => abilityProcessData.DrawAbility(card, player, value);
                break;
            case AbilityProcess.GiveDamage:
                func = () => abilityProcessData.GiveDamageAbility(card, player, target, value);
                break;
            case AbilityProcess.GiveHealth:
                func = () => abilityProcessData.GiveHealthAbility(card, player, target, value);
                break;
            case AbilityProcess.DiscardHand:
                func = () => abilityProcessData.HandDestructionAbility(card, player, target);
                break;
            case AbilityProcess.UpATK:
                func = () => abilityProcessData.GiveAttackAbility(card, player, target, value);
                break;
            case AbilityProcess.DownATK:
                func = () => abilityProcessData.ReduceAttackAbility(card, player, target, value);
                break;
            case AbilityProcess.DeleteMonster:
                func = () => abilityProcessData.DeleteMonsterAbility(card, player, target);
                break;
            case AbilityProcess.ReturnMonster:
                func = () => abilityProcessData.ReturnMonsterAbility(card, player, target);
                break;
            case AbilityProcess.HealFatigue:
                func = () => abilityProcessData.HealFatigueAbility(card, player, target);
                break;
            case AbilityProcess.GiveFatigue:
                func = () => abilityProcessData.GiveFatigueAbility(card, player, target);
                break;
            case AbilityProcess.SetRoleBell:
                func = () => abilityProcessData.SetRoleBellAbility(card);
                break;
            case AbilityProcess.SetRoleSuika:
                func = () => abilityProcessData.SetRoleSuikaAbility(card);
                break;
            case AbilityProcess.SetRoleReplay:
                func = () => abilityProcessData.SetRoleReplayAbility(card);
                break;
            case AbilityProcess.SetRoleBlank:
                func = () => abilityProcessData.SetRoleBlankAbility(card);
                break;
            case AbilityProcess.SetRoleCherry:
                func = () => abilityProcessData.SetRoleCherryAbility(card);
                break;
            case AbilityProcess.Uoooo:
                func = () => abilityProcessData.UooooAbility(card);
                break;
            case AbilityProcess.Nakamayobi:
                func = () => abilityProcessData.NakamayobiAbility(card, player);
                break;
            case AbilityProcess.LevelUP:
                func = () => abilityProcessData.LevelUPAbility(card);
                break;
            case AbilityProcess.Soukougeki:
                func = () => abilityProcessData.SoukougekiAbility(card, player);
                break;
        }
        return func;
    }

    /// <summary>
    /// 相手の効果対象カード受け取り
    /// </summary>
    /// <param name="abilityTargetCard"></param>
    public void ReceiveOpponentAbilityTargetCard(string[] abilityTargetCard) {
        // 送られたオブジェクトIDをintに変換
        int objectID = -1;
        NetworkValueConverter.TryGetInt(abilityTargetCard[1], out objectID);
        // オブジェクトIDからオブジェクト取得
        abilityProcessData.receiveCard = CardObjectManager.instance.GetUseCardObject(objectID);
        abilityProcessData.isReceiveCard = true;
    }

    /// <summary>
    /// 相手の発動したなかまよびの情報を受け取り
    /// </summary>
    /// <param name="nakamayobi"></param>
    public void ReceiveOpponentNakamayobi(string[] nakamayobi) {
        // 2つ目の情報がカードIDなのでint変換
        NetworkValueConverter.TryGetInt(nakamayobi[1], out abilityProcessData.receiveCardID);
        abilityProcessData.isReceiveCardID = true;
        // 3つ目の情報がレーンなのでenum変換
        NetworkValueConverter.TryGetEnum(nakamayobi[2], out abilityProcessData.receiveLane);
        abilityProcessData.isReceiveLane = true;
    }
}
