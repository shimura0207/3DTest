/*
 * @file    TurnPlayerBanner.cs
 * @author  kuu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPlayerBanner : MonoBehaviour
{
    // ターン開始バナー
    [SerializeField] Image banner = null;

    // 自分のターンバナー画像
    [SerializeField] Sprite selfBanner = null;

    // 相手のターンバナー画像
    [SerializeField] Sprite opponentBanner = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ターンプレイヤーに合わせてバナーの画像を変える
        if (BattleSystemManager.instance.turnPlayer == PlayerType.Self) {
            banner.sprite = selfBanner;
        }
        else if (BattleSystemManager.instance.turnPlayer == PlayerType.Opponent) {
            banner.sprite = opponentBanner;
        }
    }


}
