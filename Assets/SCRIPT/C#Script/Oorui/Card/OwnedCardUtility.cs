/*
 *  @file   OwnerdCardUtility
 *  @author oorui
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードに関する処理の呼び出しクラス
/// </summary>
public class OwnedCardUtility {

    /// <summary>
    /// カードID毎の枚数を集計する
    /// </summary>
    /// <param name="ownedCardIDs"></param>
    /// <returns></returns>
    public static Dictionary<int, int> GetCardCounts(List<int> ownedCardIDs) {
        Dictionary<int, int> cardCounts = new Dictionary<int, int>();

        // 所持カードIDを1枚ずつ確認する
        foreach (int cardID in ownedCardIDs) {
            // IDが同じカードであれば重ねる
            if (cardCounts.ContainsKey(cardID)) {
                cardCounts[cardID]++;
            }
            // 初登録IDなら初期カードとして登録
            else {
                cardCounts.Add(cardID, 1);
            }
        }

        // 集計結果を返す
        return cardCounts;
    }

}
