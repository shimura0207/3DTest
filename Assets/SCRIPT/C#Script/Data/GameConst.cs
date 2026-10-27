/*
 *  @file   GameConst
 *  @author oorui
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConst {

    // プレイヤーの体力
    public static readonly int PLAYER_HP = 50;

    // デッキの最大枚数
    public static readonly int DECK_CARD_MAX = 30;
    // 初手札枚数
    public static readonly int START_HAND_COUNT = 5;

    // 最小サイズ上限
    public static readonly int SIZE_LIMIT_MIN = 2;
    // 最大サイズ上限
    public static readonly int SIZE_LIMIT_MAX = 10;
    // サイズ上限上昇値
    public static readonly int SIZE_LIMIT_UP = 2;
}