/*
 * @file    NextPhaseButton.cs
 * @autor   Kuu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextPhaseButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PACHISUROGO() {
        bool confirmation = PartManager.Instance.

        //BattleSystemManager.instance.GoPachisuro();
    }

    public void TURNEND() {
        BattleSystemManager.instance.TurnEnd();
    }
}
