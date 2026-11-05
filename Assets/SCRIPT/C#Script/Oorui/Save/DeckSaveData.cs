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

    // デッキの名前
    public string deckName;

    // デッキに登録されているカードID
    public List<int> cardIDs = new List<int>();
}
