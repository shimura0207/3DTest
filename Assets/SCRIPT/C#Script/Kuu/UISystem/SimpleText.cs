/*
 * @file    SimpleText.cs
 * @autor   Kuu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleText : MonoBehaviour
{
    [SerializeField] private Canvas cardInfo = null;
    // Start is called before the first frame update
    void Start()
    {
        // IDからステータスを取得
        var data = CardDataMasterUtility.GetCardMaster(101);
        cardInfo.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            cardInfo.enabled = true;
        }
    }
}
