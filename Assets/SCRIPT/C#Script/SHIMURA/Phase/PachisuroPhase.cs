/*
 * @file    SlotPhase.cs
 * @author  Shimura
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PachisuroPhase :PhaseBase
{
    

    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {

        GameObject hand = GameObject.Find("HANDCanvas"); 
        GameObject deck = GameObject.Find("DeckContlol");

        Canvas rend= hand.GetComponent<Canvas>();
        SlotReelController slot; //呼ぶスクリプトにあだなつける
        GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
        slot = obj.GetComponent<SlotReelController>(); //付いているスクリプトを取得


        //Canvas rend = hand.GetComponent<Canvas>();
        rend.enabled = false;

        slot.SlotUpdate();
        //5回転が終わっており、リールが回っていない場合にNEXTする
        if (slot.IsAnyReelRotating() == false&&slot.slotIndex==slot.SLOT_TEARN_MAX_G) {
            slot.slotIndex = 0;//ターンごとのゲーム数リセット
            nextPhase = true;
        }

        await UniTask.CompletedTask;
        
    }



    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask OpponentExecute() {
        //SlotReelController slot; //呼ぶスクリプトにあだなつける
        //GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
        //slot = obj.GetComponent<SlotReelController>(); //付いているスクリプトを取得
        //
        //
        //slot.EnemySlotUpdate();

        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }
}
