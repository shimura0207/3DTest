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
        //GameObject hand = GameObject.Find("HANDCanvas");
        //
        //RGDeckController slot; //呼ぶスクリプトにあだなつける
        //GameObject obj = GameObject.Find("DeckContlol"); //Mangerっていうオブジェクトを探す
        //Canvas rend = hand.GetComponent<Canvas>();
        //rend.enabled = true;
        //slot = obj.GetComponent<RGDeckController>(); //付いているスクリプトを取得
        //RGDeckController.NotIchenge();
        //slot.DrawCardToHand();
        //AreaCardManager.instance.DrawCard(1);

        // ターン開始時効果確認(今後実装予定)

        // ターン開始時ドロー
        int card = AreaCardManager.instance.DrawCard();
        CardObject cardObject = CardObjectManager.instance.UseCardObject(card);
        CardObjectManager.instance.MoveToHand(PlayerType.Self, cardObject.objectID);
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
        // ターン開始時効果確認(今後実装予定)

        // ターン開始時ドロー(今後実装予定)

        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }
}
