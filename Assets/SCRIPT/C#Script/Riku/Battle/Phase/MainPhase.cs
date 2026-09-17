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
        nextPhase = RGDeckController.i;
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
