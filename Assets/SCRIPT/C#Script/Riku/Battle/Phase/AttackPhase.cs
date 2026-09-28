/*
 * @file    AttackPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アタックフェイズ
/// </summary>
public class AttackPhase : PhaseBase {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        // 当選役取得
        {
            // 当選役を保存
            SlotReelController slot; //呼ぶスクリプトにあだなつける
            GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
            slot = obj.GetComponent<SlotReelController>(); //付いているスクリプトを取得
            List<PachisuroSymbolKoyakuEnum> koyakuList = slot.GetRoles();

            // フィールドのカード取得
            RGCardData[] fieldCards = new RGCardData[5];
            // ひとつずつオブジェクトを走査してデータを保存していく
            RGFieldCard card;
            obj = GameObject.Find("PlayerField/PlayerPanel1/FieldCard_");
            if (obj) {
                card = obj.GetComponent<RGFieldCard>();
                fieldCards[0] = card.GetCardDate();
            }
            else {
                fieldCards[0] = null;
            }
            obj = GameObject.Find("PlayerField/PlayerPanel2/FieldCard_");
            if (obj) {
                card = obj.GetComponent<RGFieldCard>();
                fieldCards[1] = card.GetCardDate();
            }
            else {
                fieldCards[1] = null;
            }
            obj = GameObject.Find("PlayerField/PlayerPanel3/FieldCard_");
            if (obj) {
                card = obj.GetComponent<RGFieldCard>();
                fieldCards[2] = card.GetCardDate();
            }
            else {
                fieldCards[2] = null;
            }
            obj = GameObject.Find("PlayerField/PlayerPanel4/FieldCard_");
            if (obj) {
                card = obj.GetComponent<RGFieldCard>();
                fieldCards[3] = card.GetCardDate();
            }
            else {
                fieldCards[3] = null;
            }
            obj = GameObject.Find("PlayerField/PlayerPanel5/FieldCard_");
            if (obj) {
                card = obj.GetComponent<RGFieldCard>();
                fieldCards[4] = card.GetCardDate();
            }
            else {
                fieldCards[4] = null;
            }

            // 当選役の数分の対応役チェック
            for (int i = 0; i < koyakuList.Count; i++) {
                // フィールドのカードをひとつずつチェック
                for (int j = 0; j < fieldCards.Length; j++) {
                    // フィールドにカードがなければスキップ
                    if (!fieldCards[j]) continue;
                    // 対応役が成立していれば攻撃
                    if (koyakuList[i] != fieldCards[j].SupportRole) continue;
                    
                    // カードの攻撃力分のダメージを与える
                    BattleSystemManager.instance.PlayerGiveDamage(PlayerType.Opponent, fieldCards[j].Atk);

                    // UIに反映
                    PlayerHPUIControllere hp; //呼ぶスクリプトにあだなつける
                    GameObject player = GameObject.Find("Player2HP");
                    hp = player.GetComponent<PlayerHPUIControllere>(); //付いているスクリプトを取得
                    hp.SetCurrentHP(BattleSystemManager.instance.playerHP[PlayerType.Opponent]);
                    
                    Debug.Log("相手プレイヤーに" + fieldCards[j].Atk + "のダメージ");

                    await UniTask.DelayFrame(10);
                }
            }

            // 次のフェイズへ
            nextPhase = true;
        }
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask OpponentExecute() {
        PlayerHPUIControllere hp;
        GameObject player = GameObject.Find("Player1HP");
        hp = player.GetComponent<PlayerHPUIControllere>();
        hp.SetCurrentHP(BattleSystemManager.instance.playerHP[PlayerType.Self]);

        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }
}
