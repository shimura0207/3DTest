/*
 * @file    CardTextUI.cs
 * @autor   Kuu
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// プレイヤーのHPをUIに反映させる処理
/// </summary>
public class PlayerHPUIControllere : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public int playerHP = 50;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = playerHP.ToString();
    }

    /// <summary>
    /// HPを更新する
    /// </summary>
    /// <param name="currentHP"></param>
    public void SetCurrentHP(int currentHP) {
        playerHP = currentHP;
    }
}
