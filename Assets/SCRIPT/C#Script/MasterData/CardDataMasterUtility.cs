/*
 * @file    CardDataMasterUtility
 * @author  oorui
 */

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
        for (int i = 0, max = cardMasterList.Count; i < max; i++) {
            if (cardMasterList[i].cardID != masterID) continue;
            return cardMasterList[i];
        }
        return null;
    }

    /// <summary>
    /// マスターデータに登録されているすべてのカードIDを取得
    /// </summary>
    /// <returns>カードID一覧</returns>
    public static List<int> GetAllCardIDs() {

        // 取得結果を格納するリスト
        List<int> cardIDList = new List<int>();

        // マスターデータが存在するか確認
        if (MasterDataManager.cardData == null ||
            MasterDataManager.cardData.Count == 0) return cardIDList;

        // カードのマスターデータ取得
        var cardMasterList = MasterDataManager.cardData[0];

        // マスターデータに登録されているIDをすべて取得
        for (int i = 0, max = cardMasterList.Count; i < max; i++) {

            // カードIDを取得
            int cardID = cardMasterList[i].cardID;

            // 重複しているIDは追加しない
            if (cardIDList.Contains(cardID)) continue;

            // IDをリストに追加
            cardIDList.Add(cardID);
        }

        return cardIDList;
    }

    /// <summary>
    /// テーマ別カードデータ取得
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static List<int> GetRaceCardData(CardTheme id) {
        // 取得結果を格納するリスト
        List<int> cardIDList = new List<int>();

        // マスターデータが存在するか確認
        if (MasterDataManager.cardData == null ||
            MasterDataManager.cardData.Count == 0) return cardIDList;

        // カードのマスターデータ取得
        var cardMasterList = MasterDataManager.cardData[0];

        // マスターデータから指定テーマのカードを取得
        for (int i = 0, max = cardMasterList.Count; i < max; i++) {

            // テーマIDが一致しなければスキップ
            if (cardMasterList[i].themeID != (int)id) continue;

            // カードIDを追加
            cardIDList.Add(cardMasterList[i].cardID);
        }

        // カードのリストを返す
        return cardIDList;
    }

    /// <summary>
    /// カード名が完全一致するカードのマスターデータを取得
    /// </summary>
    /// <param name="cardName">検索するカード名</param>
    /// <returns>一致したカードのマスターデータ</returns>
    public static Entity_CardData.Param GetCardNameData(string cardName) {

        // マスターデータが存在するか確認
        if (MasterDataManager.cardData == null ||
            MasterDataManager.cardData.Count == 0) return null;

        // カードのマスターデータ取得
        var cardMasterList = MasterDataManager.cardData[0];

        // カード名が完全一致するものを検索
        for (int i = 0, max = cardMasterList.Count; i < max; i++) {

            // 名前が一致しなければスキップ
            if (cardMasterList[i].name != cardName) continue;

            // 一致したカードのマスターデータを返す
            return cardMasterList[i];
        }

        // 該当するカードが存在しない場合
        return null;
    }
}
