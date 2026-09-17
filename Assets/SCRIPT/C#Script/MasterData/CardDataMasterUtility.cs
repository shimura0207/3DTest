using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ID指定のマスターデータ取得
/// </summary>
public class CardDataMasterUtility {
    
    /// <summary>
    /// ID指定のカードデータのマスターデータ取得
    /// </summary>
    /// <param name="masterID"></param>
    /// <returns></returns>
    public static Entity_CardData.Param GetCardMaster(int masterID) {
        // カードのマスターデータ取得
        var cardMasterList = MasterDataManager.cardData[0];
        // IDが一致するものを返す
        for(int i = 0, max = cardMasterList.Count; i < max; i++) {
            if (cardMasterList[i].CardID != masterID) continue;
            return cardMasterList[i];
        }
        return null;
    }
}
