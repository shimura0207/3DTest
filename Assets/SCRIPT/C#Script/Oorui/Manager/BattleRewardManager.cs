/*
 * @file    BattleRewardManager
 * @author  oorui
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 対戦終了時のカード報酬を管理するクラス
/// </summary>
public static class BattleRewardManager {

    // 勝利時の報酬枚数
    private const int WIN_UNOWNED_COUNT = 1;
    private const int WIN_OWNED_COUNT = 4;
    private const int WIN_RANDOM_COUNT = 5;

    // 敗北時の報酬枚数
    private const int LOSE_UNOWNED_COUNT = 1;
    private const int LOSE_OWNED_COUNT = 1;
    private const int LOSE_RANDOM_COUNT = 3;

    // レアリティ抽選確率
    private const int LR_RATE = 10;
    private const int SR_RATE = 20;
    private const int R_RATE = 30;
    private const int N_RATE = 40;

    // レアリティごとの抽選確率を管理する
    private static readonly Dictionary<CardRearity, int> rarityRates =
        new Dictionary<CardRearity, int>() {
            { CardRearity.LR, LR_RATE },
            { CardRearity.SR, SR_RATE },
            { CardRearity.R, R_RATE },
            { CardRearity.N, N_RATE }
        };

    /// <summary>
    /// 対戦結果に応じてカード報酬を付与する
    /// </summary>
    public static Dictionary<int, int> GrantRewards(BattleState result) {

        // 所持カードデータを読み込む
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // マスターに登録されている全カードIDを取得する
        List<int> allCardIDs = CardDataMasterUtility.GetAllCardIDs();

        // レアリティごとのカード一覧を用意する
        Dictionary<CardRearity, List<int>> allCardsByRarity =
            new Dictionary<CardRearity, List<int>>();

        // 各レアリティのリストを初期化する
        foreach (CardRearity rarity in rarityRates.Keys) {
            allCardsByRarity.Add(rarity, new List<int>());
        }

        // 使用可能なカードをレアリティごとに分類する
        foreach (int cardID in allCardIDs) {

            // カードマスターを取得する
            Entity_CardData.Param cardData =
                CardDataMasterUtility.GetCardMaster(cardID);

            // マスターが存在しないカードは除外する
            if (cardData == null) continue;

            // PlayIDが1のカードは抽選対象から除外する
            if (cardData.PlayID == 1) continue;

            // レアリティを取得する
            CardRearity rarity = (CardRearity)cardData.rarityID;

            // Noneなどの抽選対象外レアリティを除外する
            if (!allCardsByRarity.ContainsKey(rarity)) continue;

            // レアリティごとのリストに追加する
            allCardsByRarity[rarity].Add(cardID);
        }

        // 所持カードの枚数をカードIDごとに集計する
        Dictionary<int, int> ownedCounts = new Dictionary<int, int>();

        if (saveData != null && saveData.cardID != null) {

            foreach (int cardID in saveData.cardID) {

                // 同じカードIDの所持枚数を加算する
                if (ownedCounts.ContainsKey(cardID)) {
                    ownedCounts[cardID]++;
                }
                else {
                    ownedCounts.Add(cardID, 1);
                }
            }
        }

        // レアリティごとの未所持カード一覧
        Dictionary<CardRearity, List<int>> unownedByRarity =
            CreateCardListByRarity();

        // レアリティごとの所持カード一覧
        Dictionary<CardRearity, List<int>> ownedByRarity =
            CreateCardListByRarity();

        // 全カードを確認する
        foreach (KeyValuePair<CardRearity, List<int>> entry
            in allCardsByRarity) {

            foreach (int cardID in entry.Value) {

                // 所持枚数を取得する
                int count = ownedCounts.ContainsKey(cardID)
                    ? ownedCounts[cardID]
                    : 0;

                // 未所持カードを登録する
                if (count == 0) {
                    unownedByRarity[entry.Key].Add(cardID);
                }
                // 3枚以下所持しているカードを登録する
                else if (count <= 3) {
                    ownedByRarity[entry.Key].Add(cardID);
                }
            }
        }

        // 対戦結果に応じて報酬枚数を決定する
        int unownedCount;
        int ownedCount;
        int randomCount;

        switch (result) {

            case BattleState.Win:
                unownedCount = WIN_UNOWNED_COUNT;
                ownedCount = WIN_OWNED_COUNT;
                randomCount = WIN_RANDOM_COUNT;
                break;

            case BattleState.Lose:
                unownedCount = LOSE_UNOWNED_COUNT;
                ownedCount = LOSE_OWNED_COUNT;
                randomCount = LOSE_RANDOM_COUNT;
                break;

            default:
                return new Dictionary<int, int>();
        }

        // 獲得カードIDと枚数を管理する
        Dictionary<int, int> acquiredCards = new Dictionary<int, int>();

        // 未所持枠を抽選する
        DrawRewards(
            unownedByRarity,
            ownedByRarity,
            allCardsByRarity,
            unownedCount,
            acquiredCards,
            0
        );

        // 所持カード枠を抽選する
        DrawRewards(
            ownedByRarity,
            unownedByRarity,
            allCardsByRarity,
            ownedCount,
            acquiredCards,
            1
        );

        // ランダム枠を抽選する
        DrawRewards(
            allCardsByRarity,
            null,
            allCardsByRarity,
            randomCount,
            acquiredCards,
            2
        );

        // 獲得カード情報を呼び出し元に返す
        return acquiredCards;
    }

    /// <summary>
    /// レアリティごとのカードリストを作成する
    /// </summary>
    private static Dictionary<CardRearity, List<int>>
        CreateCardListByRarity() {

        // レアリティごとのカード一覧を用意する
        Dictionary<CardRearity, List<int>> result =
            new Dictionary<CardRearity, List<int>>();

        // 各レアリティのリストを初期化する
        foreach (CardRearity rarity in rarityRates.Keys) {
            result.Add(rarity, new List<int>());
        }

        return result;
    }

    /// <summary>
    /// 指定された枚数の報酬を抽選する
    /// </summary>
    /// <param name="candidates">最初に抽選する候補</param>
    /// <param name="fallbackCandidates">不足時の補填候補</param>
    /// <param name="allCards">レアリティごとの全カード</param>
    /// <param name="drawCount">抽選枚数</param>
    /// <param name="acquiredCards">獲得カード一覧</param>
    /// <param name="drawType">0:未所持 1:所持 2:ランダム</param>
    private static void DrawRewards(
        Dictionary<CardRearity, List<int>> candidates,
        Dictionary<CardRearity, List<int>> fallbackCandidates,
        Dictionary<CardRearity, List<int>> allCards,
        int drawCount,
        Dictionary<int, int> acquiredCards,
        int drawType) {

        // 指定された枚数だけ抽選する
        for (int i = 0; i < drawCount; i++) {

            // 抽選可能なレアリティを選択する
            CardRearity rarity = DrawRarity(
                candidates,
                fallbackCandidates,
                allCards
            );

            // 抽選できるカードが存在しない場合は終了する
            if (rarity == CardRearity.None) return;

            // 最初の候補からカードを取得する
            List<int> targetList = candidates[rarity];

            // 候補が存在しない場合は補填候補を取得する
            if (targetList.Count == 0 &&
                fallbackCandidates != null) {

                targetList = fallbackCandidates[rarity];
            }

            // それでも候補がない場合は全カードから抽選する
            if (targetList.Count == 0) {
                targetList = allCards[rarity];
            }

            // 抽選対象が存在しない場合は終了する
            if (targetList.Count == 0) continue;

            // ランダムにカードを選択する
            int index = Random.Range(0, targetList.Count);

            // 抽選したカードIDを取得する
            int cardID = targetList[index];

            // 未所持・所持枠は同じカードの重複抽選を防ぐ
            if (drawType != 2) {
                targetList.RemoveAt(index);
            }

            // 報酬用としてカードを一時的に所持データへ追加する
            SaveManager.Instance.AcquireCardForReward(cardID);

            // 獲得カードの枚数を記録する
            AddAcquiredCard(acquiredCards, cardID);
        }
    }

    /// <summary>
    /// 抽選可能なカードが存在するレアリティを抽選する
    /// </summary>
    private static CardRearity DrawRarity(
        Dictionary<CardRearity, List<int>> candidates,
        Dictionary<CardRearity, List<int>> fallbackCandidates,
        Dictionary<CardRearity, List<int>> allCards) {

        // 抽選可能なレアリティと確率を格納する
        List<CardRearity> availableRarities =
            new List<CardRearity>();

        int totalRate = 0;

        // 各レアリティの抽選可能枚数を確認する
        foreach (KeyValuePair<CardRearity, int> entry in rarityRates) {

            // 最初の候補を確認する
            bool available = candidates[entry.Key].Count > 0;

            // 補填候補を確認する
            if (!available && fallbackCandidates != null) {
                available =
                    fallbackCandidates[entry.Key].Count > 0;
            }

            // 全カードを確認する
            if (!available) {
                available = allCards[entry.Key].Count > 0;
            }

            // 抽選できるレアリティだけを追加する
            if (available) {
                availableRarities.Add(entry.Key);
                totalRate += entry.Value;
            }
        }

        // 抽選可能なレアリティがない場合
        if (availableRarities.Count == 0) {
            return CardRearity.None;
        }

        // 確率の合計から抽選する
        int randomValue = Random.Range(0, totalRate);

        // 確率に応じてレアリティを決定する
        foreach (CardRearity rarity in availableRarities) {

            randomValue -= rarityRates[rarity];

            if (randomValue < 0) {
                return rarity;
            }
        }

        // 通常ここには到達しない
        return availableRarities[0];
    }

    /// <summary>
    /// 獲得したカードIDと枚数を記録する
    /// </summary>
    private static void AddAcquiredCard(
        Dictionary<int, int> acquiredCards,
        int cardID) {

        // 既に記録されているカードなら枚数を加算する
        if (acquiredCards.ContainsKey(cardID)) {
            acquiredCards[cardID]++;
        }
        else {
            // 初めて獲得したカードなら新しく登録する
            acquiredCards.Add(cardID, 1);
        }
    }
}