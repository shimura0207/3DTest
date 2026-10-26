/*
 * @file    FieldSIZEUIControllere.cs
 * @autor   Kuu
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FieldSIZEUIControllere : MonoBehaviour
{
    // 自分のサイズテキスト
    [SerializeField] private TextMeshProUGUI selfSizeText;

    // 相手のサイズテキスト
    [SerializeField] private TextMeshProUGUI opponentSizeText;

    // 現在の最大サイズ
    private int currentMaxSize = 0;

    // 対戦相手の最大サイズ
    private int opponentMaxSize = 0;

    // 現在のサイズ
    private int currentSize = 0;

    // 対戦相手の現在サイズ
    private int opponentCurrentSize = 0;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        UpdateSizeText();
    }

    /// <summary>
    /// サイズテキスト更新
    /// </summary>
    private void UpdateSizeText() {
        currentMaxSize = UIManager.instance.currentMaxSize;
        opponentMaxSize = UIManager.instance.opponentMaxSize;
        currentSize = currentMaxSize - UIManager.instance.restSize;
        opponentCurrentSize = opponentMaxSize - UIManager.instance.opponentRestSize;

        selfSizeText.text = "SIZE:" + currentSize.ToString() + "/" + currentMaxSize.ToString();
        opponentSizeText.text = "SIZE:" + opponentCurrentSize.ToString() + "/" + opponentMaxSize.ToString();
    }
}
