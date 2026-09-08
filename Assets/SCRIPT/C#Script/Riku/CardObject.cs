/*
 * @file    CardObject.cs
 * @autor   Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardObject : MonoBehaviour 
{
    // カードのID
    private int cardID;

    // カードのステータス
    struct CardStates {
        int HP;
        int ATK;
        int SIZE;
    }

    // デフォルトのステータス
    private CardStates defaultStates;
    // 現在のステータス
    private CardStates currentStates;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
