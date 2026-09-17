/*
 * @file    AttackPhase.cs
 * @author  Riku
 */

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
    public override bool SelfExecute() {
        // 当選役取得
        { 
            RGDeckController slot; //呼ぶスクリプトにあだなつける
            GameObject obj = GameObject.Find("Manager"); //Mangerっていうオブジェクトを探す
            slot = obj.GetComponent<RGDeckController>(); //付いているスクリプトを取得

        }

        return base.SelfExecute();
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override bool OpponentExecute() {
        return base.OpponentExecute();
    }
}
