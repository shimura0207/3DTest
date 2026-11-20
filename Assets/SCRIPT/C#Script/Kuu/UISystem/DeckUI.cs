/*
 * @file    TableUI.cs
 * @autor   Kuu
 */
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckUI : MonoBehaviour
{
    // 自分のデッキ総数
    [SerializeField] private TMP_Text selfDeck = null;

    // 相手のデッキ総数
    [SerializeField] private TMP_Text opponentDeck = null;

    // デッキ最大枚数
    private const int MAX_TABLE = 15;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        selfDeck.text = "山札:" + AreaCardManager.instance.deckCards[PlayerType.Self].Count;
        opponentDeck.text = "山札:" + AreaCardManager.instance.deckCards[PlayerType.Opponent].Count;
    }
}
