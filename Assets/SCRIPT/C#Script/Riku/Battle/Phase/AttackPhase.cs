/*
 * @file    AttackPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// アタックフェイズ
/// </summary>
public class AttackPhase : PhaseBase {
    // 液晶に寄ったカメラの位置
    private readonly Vector3 CAMERA_POSITION = new Vector3(0, 1.27f, -0.8f);

    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        // 当選役取得
        SlotReelController slot; //呼ぶスクリプトにあだなつける
        GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
        slot = obj.GetComponent<SlotReelController>(); //付いているスクリプトを取得
        List<PachisuroSymbolKoyakuEnum> koyakuList = slot.GetRoles();

        // フィールドのカード取得
        var fieldCards = CardObjectManager.instance.fieldObjectList[PlayerType.Self];

        // 当選役の数分の対応役チェック
        foreach (var koyaku in koyakuList) {
            // フィールドのカードをひとつずつチェック
            foreach (var card in fieldCards) {
                // フィールドにカードがなければスキップ
                if (!card.Value) continue;
                // 対応役によって攻撃
                CardAttack(PlayerType.Opponent, koyaku, card.Value, slot.AttackRush, slot.AttackNextRush);
                
                // UIに反映
                PlayerHPUIControllere hp; //呼ぶスクリプトにあだなつける
                GameObject player = GameObject.Find("Player2HP");
                hp = player.GetComponent<PlayerHPUIControllere>(); //付いているスクリプトを取得
                hp.SetCurrentHP(BattleSystemManager.instance.playerHP[PlayerType.Opponent]);

                await UniTask.DelayFrame(10);
            }
        }


        // 次のフェイズへ
        nextPhase = true;
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask OpponentExecute() {
        // 当選役取得
        SlotReelController slot; //呼ぶスクリプトにあだなつける
        GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
        slot = obj.GetComponent<SlotReelController>(); //付いているスクリプトを取得
        List<PachisuroSymbolKoyakuEnum> koyakuList = slot.GetRoles();

        // フィールドのカード取得
        var fieldCards = CardObjectManager.instance.fieldObjectList[PlayerType.Opponent];

        // 当選役の数分の対応役チェック
        foreach (var koyaku in koyakuList) {
            // フィールドのカードをひとつずつチェック
            foreach (var card in fieldCards) {
                // フィールドにカードがなければスキップ
                if (!card.Value) continue;
                // 対応役によって攻撃
                CardAttack(PlayerType.Self, koyaku, card.Value, slot.AttackRush, slot.AttackNextRush);

                // UIに反映
                PlayerHPUIControllere hp;
                GameObject player = GameObject.Find("Player1HP");
                hp = player.GetComponent<PlayerHPUIControllere>();
                hp.SetCurrentHP(BattleSystemManager.instance.playerHP[PlayerType.Self]);

                await UniTask.DelayFrame(10);
            }
        }

        // 次のフェイズへ
        nextPhase = true;
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 実行処理が呼ばれる前の準備
    /// </summary>
    public override void Setup() {
        // カメラを液晶の方に寄せる
        Camera.main.transform.position = CAMERA_POSITION;
        Camera.main.transform.eulerAngles = Vector3.zero;
    }

    /// <summary>
    /// カードの攻撃処理
    /// </summary>
    /// <param name="player">攻撃対象</param>>
    /// <param name="koyaku">当選役</param>
    /// <param name="card">確認するカード</param>
    /// <param name="rush">ラッシュ中か否か</param>
    /// <param name="nextRush">ラッシュ当選中かどうか</param>
    private void CardAttack(PlayerType player, PachisuroSymbolKoyakuEnum koyaku, CardObject card, bool rush, bool nextRush) {
        // カードの対応役の型を変換
        List<PachisuroSymbolKoyakuEnum> suportRole = card.SuportRoleToKoyaku();
        if (suportRole == null) return; 
        // 子役をチェックしてダメージ設定
        int damage = 0;
        // ラッシュ中の7当選、ラッシュ当選中の7当選もしくは全対応なら確定攻撃
        if (rush && koyaku == PachisuroSymbolKoyakuEnum.Seven ||
            nextRush && koyaku == PachisuroSymbolKoyakuEnum.Seven ||
            card.suportRole == CardSuportRole.ALL) {
            damage = card.currentStatus[MonsterStatus.ATK];
        }
        else {
            // 対応役をすべてチェックしてダメージ設定
            foreach (var role in suportRole) {
                if (koyaku == role) {
                    damage = card.currentStatus[MonsterStatus.ATK];
                }
            }
        }
        // 強チェリーが当選している場合はダメージ2倍
        if (koyaku == PachisuroSymbolKoyakuEnum.StrongCherry) {
            damage *= 2;
        }

        // ダメージ0なら攻撃はしない
        if (damage <= 0) return;

        FieldLane frontLane = FieldLane.None;
        // 0除算回避
        if (card.laneID == 0) {
            frontLane = FieldLane.Lane5;
        }
        else {
            frontLane = (FieldLane)((int)FieldLane.Max % card.laneID + 1);
        }
        // 正面のモンスターを取得
        CardObject frontMonster = CardObjectManager.instance.fieldObjectList[player][frontLane];

        // 正面にモンスターがいる場合はモンスターに攻撃
        if (frontMonster) {
            // モンスターにダメージを与える
            frontMonster.GiveStatus(MonsterStatus.HP, -damage);
        }
        else {
            // プレイヤーにダメージを与える
            BattleSystemManager.instance.PlayerGiveDamage(player, damage);
        }
    }
}
