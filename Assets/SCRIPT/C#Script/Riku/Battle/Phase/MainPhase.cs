/*
 * @file    MainPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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
    public override async UniTask SelfExecute() {
        await UniTask.DelayFrame(30);
        GameObject hand = GameObject.Find("HANDCanvas"); //Mangerっていうオブジェクトを探す
        GameObject deck = GameObject.Find("DeckContlol"); //Mangerっていうオブジェクトを探す


        hand.SetActive(true);
        deck.SetActive(true);

        nextPhase = RGDeckController.i;
        //nextPhase = Input.GetKeyDown(KeyCode.Space);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask OpponentExecute() {
        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    } 
}
