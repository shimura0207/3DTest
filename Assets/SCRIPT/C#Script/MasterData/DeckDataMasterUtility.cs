/*
 *  @file   DeckDataMasterUtility
 *  @author oorui
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ID指定のマスターデータ取得
/// </summary>
public class DeckDataMasterUtility {

    /// <summary>
    /// デッキIDを指定して、デッキに含まれるカードIDを取得する
    /// </summary>
    /// <param name="deckID">取得するデッキのID</param>
    /// <returns>デッキに含まれるカードIDのリスト</returns>
    public static List<int> GetDeckMaster(int deckID) {

        // デッキに含まれるカードIDを格納するリスト
        List<int> cardIDList = new List<int>();

        // デッキのマスターデータを取得
        var deckMasterList = MasterDataManager.rentalDeck[0];

        // デッキマスターデータを確認
        for (int i = 0, max = deckMasterList.Count; i < max; i++) {

            // 指定したデッキIDではない場合は次のデータへ
            if (deckMasterList[i].DeckID != deckID) continue;

            // カードのIDを取得
            int cardID = deckMasterList[i].CardID;

            // Countの数だけカードIDをリストに追加
            for (int j = 0; j < deckMasterList[i].Count; j++) {
                cardIDList.Add(cardID);
            }
        }

        // デッキに含まれるカードIDのリストを返す
        return cardIDList;
    }

}
