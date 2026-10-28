/*
 * @file    PlayerHPUIControllere.cs
 * @autor   Kuu
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// プレイヤーのHPをUIに反映させる処理
/// </summary>
public class PlayerHPUIControllere : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private Slider HPBar;
    public int playerHP = 50;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "HP:" + playerHP.ToString();
        HPBar.value = playerHP;
    }

    /// <summary>
    /// HPを更新する
    /// </summary>
    /// <param name="currentHP"></param>
    public void SetCurrentHP(int currentHP) {
        playerHP = currentHP;
    }
}
