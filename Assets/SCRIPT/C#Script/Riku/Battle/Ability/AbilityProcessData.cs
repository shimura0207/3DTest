/*
 * @file    AbilityProcessData.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カードの効果処理
/// </summary>
public class AbilityProcessData {
    // ステータス付与関連効果
    private Dictionary<AbilityTarget, Func<MonsterStatus, CardObject, PlayerType, int, UniTask>> giveStatus = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        // ステータス付与関連効果の登録
        RegisterGiveStatus();
    }

    /// <summary>
    /// ダメージ付与効果の登録
    /// </summary>
    private void RegisterGiveStatus() {
        giveStatus = new Dictionary<AbilityTarget, Func<MonsterStatus, CardObject, PlayerType, int, UniTask>>();
        // 自身プレイヤー
        giveStatus[AbilityTarget.SelfPlayer] = (status, card, player, value) => {
            if (status != MonsterStatus.HP) return UniTask.CompletedTask;
            // 自身プレイヤーのHP付与
            BattleSystemManager.instance.PlayerGiveHP(player, value);
            return UniTask.CompletedTask;
        };
        // 相手プレイヤー
        giveStatus[AbilityTarget.SelfPlayer] = (status, card, player, value) => {
            if (status != MonsterStatus.HP) return UniTask.CompletedTask;
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // 相手プレイヤーのHP付与
            BattleSystemManager.instance.PlayerGiveHP(revPla, value);
            return UniTask.CompletedTask;
        };
        // 自身のランダムなモンスター
        giveStatus[AbilityTarget.SelfRandomMonster] = (status, card, player, value) => {
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(player);
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
            return UniTask.CompletedTask;
        };
        // 相手のランダムなモンスター
        giveStatus[AbilityTarget.OpponentRandomMonster] = (status, card, player, value) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(revPla);
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
            return UniTask.CompletedTask;
        };
        // 自身の選択したモンスター
        giveStatus[AbilityTarget.SelfSelectMonster] = async (status, card, player, value) => {
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(player);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[player][lane];
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
        };
        // 相手の選択したモンスター
        giveStatus[AbilityTarget.OpponentSelectMonster] = async (status, card, player, value) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(revPla);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[revPla][lane];
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
        };
        // 自身の全てのモンスター
        giveStatus[AbilityTarget.SelfAllMonster] = (status, card, player, value) => {
            var fieldMonster = CardObjectManager.instance.fieldObjectList[player];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // モンスターに指定ステータスを付与
                fieldMonster[i].GiveStatus(status, value);
            }
            return UniTask.CompletedTask;
        };
        // 相手の全てのモンスター
        giveStatus[AbilityTarget.OpponentAllMonster] = (status, card, player, value) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            var fieldMonster = CardObjectManager.instance.fieldObjectList[revPla];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // モンスターに指定ステータスを付与
                fieldMonster[i].GiveStatus(status, value);
            }
            return UniTask.CompletedTask;
        };
        // このモンスター
        giveStatus[AbilityTarget.ThisMonster] = (status, card, player, value) => {
            // このモンスターに指定ステータスを付与
            card.GiveStatus(status, value);
            return UniTask.CompletedTask;
        };
        giveStatus[AbilityTarget.OtherMonster] = (status, card, player, value) => {
            // 発動モンスター以外の全てのモンスターを取得
            List<CardObject> otherMonster = new List<CardObject>();
            for (PlayerType i = 0; i < PlayerType.Max; i++) {
                foreach (var obj in CardObjectManager.instance.fieldObjectList[i]) {
                    if (!obj.Value) continue;
                    otherMonster.Add(obj.Value);
                }
            }
            // 取得した全てに指定ステータスを付与
            for (int i = 0; i < otherMonster.Count; i++) {
                otherMonster[i].GiveStatus(status, value);
            }
            return UniTask.CompletedTask;
        };
    }

    /// <summary>
    /// 対のプレイヤーを返す
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private PlayerType GetReversePlayer(PlayerType player) {
        PlayerType reversePlayer = PlayerType.None;
        if (player == PlayerType.Self) {
            reversePlayer = PlayerType.Opponent;
        }
        else if (player == PlayerType.Opponent) {
            reversePlayer = PlayerType.Self;
        }
        return reversePlayer;
    }

    /// <summary>
    /// ランダムなモンスターの取得
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private CardObject GetRandomMonster(PlayerType player) {
        List<CardObject> fieldMonster = new List<CardObject>();
        // 指定したプレイヤーのフィールドのモンスターを積む
        foreach (var obj in CardObjectManager.instance.fieldObjectList[player]) {
            if (!obj.Value) continue;
            fieldMonster.Add(obj.Value);
        }
        // 抽選
        int randIndex = UnityEngine.Random.Range(0, fieldMonster.Count);
        return fieldMonster[randIndex];
    }

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
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">ダメージ量</param>
    /// <returns></returns>
    public async UniTask GiveDamagerAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // ダメージ量に指定がない場合は1ダメージ
        if (value <= 0) value = 1;
        // ダメージ付与
        await giveStatus[target](MonsterStatus.HP, card, player, -value);
    }

    /// <summary>
    /// HP増加効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">増加量</param>
    /// <returns></returns>
    public async UniTask GiveHealthAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // 増加量に指定がない場合は1増加
        if (value <= 0) value = 1;
        // HP付与
        await giveStatus[target](MonsterStatus.HP, card, player, value);
    }

    /// <summary>
    /// ATK増加効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">増加量</param>
    /// <returns></returns>
    public async UniTask GiveAttackAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // 増加量に指定がない場合は1増加
        if (value <= 0) value = 1;
        // ATK増加
        await giveStatus[target](MonsterStatus.ATK, card, player, value);
    }

    /// <summary>
    /// ATK減少効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">減少量</param>
    /// <returns></returns>
    public async UniTask ReduceAttackAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // 減少量に指定がない場合は1減少
        if (value <= 0) value = 1;
        // ATK減少
        await giveStatus[target](MonsterStatus.ATK, card, player, -value);
    }

    /// <summary>
    /// 手札を捨てる効果
    /// </summary>
    /// <param name="player"></param>
    /// <param name="target"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async UniTask HandDestructionAbility(PlayerType player, AbilityTarget target, int value) {

    }
}
