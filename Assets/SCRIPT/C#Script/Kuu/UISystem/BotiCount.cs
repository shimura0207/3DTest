/*
 * @file    BotiCount.cs
 * @author  kuu
 */
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BotiCount : MonoBehaviour
{
    // 自分の墓地枚数テキスト
    [SerializeField] private TMP_Text selfText = null;

    // 相手の墓地枚数テキスト
    [SerializeField] private TMP_Text opponentText = null;

    // Update is called once per frame
    void Update()
    {
        // 自分の墓地枚数を表示
        selfText.text = "" + AreaCardManager.instance.outDeckCards[PlayerType.Self].graveCards.Count;

        // 相手の墓地枚数を表示
        opponentText.text = "" + AreaCardManager.instance.outDeckCards[PlayerType.Opponent].graveCards.Count;
    }
}
