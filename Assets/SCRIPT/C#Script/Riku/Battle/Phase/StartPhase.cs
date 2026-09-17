/*
 * @file    StartPhase.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターン開始フェイズ
/// </summary>
public class StartPhase : PhaseBase {
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
    public override bool SelfExecute() {
        // ターン開始時効果確認(今後実装予定)

        // ターン開始時ドロー
        RGDeckController slot; //呼ぶスクリプトにあだなつける
        GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
        slot = obj.GetComponent<RGDeckController>(); //付いているスクリプトを取得
        slot.DrawCardToHand();
        //AreaCardManager.instance.DrawCard(1);
        // 次のフェイズへ
        nextPhase = true;

        return base.SelfExecute();
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override bool OpponentExecute() {
        // ターン開始時効果確認(今後実装予定)

        // ターン開始時ドロー(今後実装予定)

        // 次のフェイズへ
        nextPhase = true;

        return base.OpponentExecute();
    }
}
