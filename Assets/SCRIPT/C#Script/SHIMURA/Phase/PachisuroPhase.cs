/*
 * @file    SlotPhase.cs
 * @author  Shimura
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PachisuroPhase : PhaseBase {


    public static PachisuroPhase Instance { get; private set; }

    private readonly Vector3 CAMERA_POSITION = new Vector3(0, 1.16f, -1.27f);
    private readonly Vector3 CAMERA_EULER_ANGLES = new Vector3(3.0f, 0.0f, 0.0f);

    public override void Setup() {
        SlotReelController slot; //呼ぶスクリプトにあだなつける
        GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
        slot = obj.GetComponent<SlotReelController>(); //付いているスクリプトを取得

        slot.SOUND.Play();

        // カメラを液晶の方に寄せる
        Camera.main.transform.position = CAMERA_POSITION;
        Camera.main.transform.eulerAngles = CAMERA_EULER_ANGLES;

        base.Setup();
    }


    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        Instance = this;

        GameObject hand = GameObject.Find("HANDCanvas");
        GameObject deck = GameObject.Find("DeckContlol");

        Canvas rend = hand.GetComponent<Canvas>();
        SlotReelController slot; //呼ぶスクリプトにあだなつける
        GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
        slot = obj.GetComponent<SlotReelController>(); //付いているスクリプトを取得
        Instance = this;

        //Canvas rend = hand.GetComponent<Canvas>();
        rend.enabled = false;

        slot.SlotUpdate();
        if (slot.IsAnyReelRotating() == false && slot.slotIndex == slot.SLOT_TEARN_MAX_G) {
            slot.stoppedReelCount = 0;
            slot.glassFlash.ResetAllGlass();
            slot.efectManager.HideTrumps();
            slot.HideStopEffectImage();
            slot.DebugAAA();
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
