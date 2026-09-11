/*
 * @file    CardObject.cs
 * @autor   Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードオブジェクトクラス
/// </summary>
public class CardObject : MonoBehaviour {
    // オブジェクトを判別する個別ID
    public int objectID { get; private set; } = -1;
    // 使用中かどうか
    private bool isActive = false;
    // カードの種類を判別するID
    public int cardID { get; private set; } = -1;

    // カードのステータス
    private struct CardStates {
        // HP
        public int HP;
        // 攻撃力
        public int ATK;
        // サイズ
        public int SIZE;
    }

    // デフォルトのステータス
    private CardStates defaultStatus;
    // 現在のステータス
    private CardStates currentStatus;


    // Start is called before the first frame update
    void Start() {
        defaultStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        currentStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
    }

    // Update is called once per frame
    void Update() {

    }

    public void Initialize() {

    }

    /// <summary>
    /// 片付け
    /// </summary>
    public void Teardown() {
        objectID = -1;
        isActive = false;
        cardID = -1;
        defaultStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        currentStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
    }

    /// <summary>
    /// オブジェクトIDをセット
    /// </summary>
    public void SetObjectID(int setID) {
        objectID = setID;
    }
}
