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
    [SerializeField] private TMP_Text selfTable = null;

    // 相手のデッキ総数
    [SerializeField] private TMP_Text opponentTable = null;

    // デッキ最大枚数
    private const int MAX_TABLE = 15;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        
    }
}
