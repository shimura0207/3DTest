/*
 * @file    MainPhase.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メインフェイズ
/// </summary>
public class MainPhase : PhaseBase {
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
        GameObject hand = GameObject.Find("HANDCanvas"); //Mangerっていうオブジェクトを探す
        GameObject deck = GameObject.Find("DeckContlol"); //Mangerっていうオブジェクトを探す


        hand.SetActive(true);
        deck.SetActive(true);

        //nextPhase = RGDeckController.i;
        nextPhase = Input.GetKeyDown(KeyCode.Space);
        return base.SelfExecute();
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override bool OpponentExecute() {
        // 次のフェイズへ
        nextPhase = true;
        return base.OpponentExecute();
    }

    
}
