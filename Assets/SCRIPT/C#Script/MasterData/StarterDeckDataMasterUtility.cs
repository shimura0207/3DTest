/*
 *  @file   StarterDeckDataMasterUtility
 *  @author oorui
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初期デッキのマスターデータを取得するクラス
/// </summary>
public static class StarterDeckDataMasterUtility {

    /// <summary>
    /// DeckIDごとに初期デッキのカードID一覧を取得する
    /// </summary>
    /// <returns>DeckIDとカードID一覧の組み合わせ</returns>
    public static SortedDictionary<int, List<int>> GetStarterDecks() {

        // DeckIDごとのカードIDを格納する辞書
        SortedDictionary<int, List<int>> deckDictionary =
            new SortedDictionary<int, List<int>>();

        // 初期デッキのマスターデータを取得
        var deckMasterList = MasterDataManager.starterDeck[0];

        // マスターデータを順番に確認する
        foreach (var data in deckMasterList) {

            // DeckIDを取得する
            int deckID = data.DeckID;

            // カードIDを取得する
            int cardID = data.CardID;

            // カードの登録枚数を取得する
            int count = data.Count;

            // DeckIDがまだ登録されていなければリストを作成する
            if (!deckDictionary.ContainsKey(deckID)) {
                deckDictionary.Add(deckID, new List<int>());
            }

            // Countの数だけカードIDを追加する
            for (int i = 0; i < count; i++) {
                deckDictionary[deckID].Add(cardID);
            }
        }

        // DeckIDごとに整理したデータを返す
        return deckDictionary;
    }
}