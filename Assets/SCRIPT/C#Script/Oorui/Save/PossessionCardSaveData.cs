/*
 *  @file   PossessionCardSaveData
 *  @author oorui
 */

using System;
using System.Collections.Generic;

/// <summary>
/// 所持カードのセーブデータを管理するクラス
/// </summary>
[Serializable]
public class PossessionCardSaveData {

    /// <summary>
    /// プレイヤーが所持しているカードのセーブデータ
    /// @tips   カードIDの重複はあり[104,104,105,106]
    /// </summary>
    public List<int> cardID = new List<int>();
}