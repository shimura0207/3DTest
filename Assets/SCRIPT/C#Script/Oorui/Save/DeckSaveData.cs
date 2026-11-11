/*
 *  @file   DeckSaveData
 *  @author oorui
 */

using System;
using System.Collections.Generic;

/// <summary>
/// 1つのデッキのセーブデータ
/// </summary>
[Serializable]
public class DeckSaveData {
    // デッキ識別用のID
    public int deckID;

    // デッキの名前
    public string deckName;

    // デッキに登録されているカードID
    public List<int> cardIDs = new List<int>();
}
