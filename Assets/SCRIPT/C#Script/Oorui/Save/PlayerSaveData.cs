
/*
 *  @file   SaveData
 *  @author oorui
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// セーブファイルに保存するデータをまとめるクラス
/// </summary>
[Serializable]
public class PlayerSaveData {
    // プレイヤー名
    public string playerName = "";

    // 所持ジェム
    public int ownedGem = 0;

    // 所持ポイント
    public int ownedPoint = 0;
}