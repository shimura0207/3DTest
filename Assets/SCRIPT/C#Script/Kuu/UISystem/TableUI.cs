/*
 * @file    TableUI.cs
 * @autor   Kuu
 */
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class TableUI : MonoBehaviour
{
    // 自分のテーブル数
    [SerializeField] private TMP_Text selfTable = null;

    // 相手のテーブル数
    [SerializeField] private TMP_Text opponentTable = null;

    // 現在の相手のテーブル数
    private int currentOpTable = -1;

    // テーブル最大数
    private const int MAX_TABLE = 15;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var slotReelController = SlotReelController.Instance;
        if (slotReelController == null) {
            return;
        }

        // 自分のテーブル数を表示
        selfTable.text = "Table:" + slotReelController.currentTable + "/" + MAX_TABLE;

        // 相手のテーブル数を表示
        if (slotReelController.OpponentTable.HasValue) {
            currentOpTable = slotReelController.OpponentTable.Value;
        }
        opponentTable.text = "Table:" + currentOpTable + "/" + MAX_TABLE;
    }
}
