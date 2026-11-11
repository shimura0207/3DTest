/*
 * @file    BattleRewardManager
 * @author  oorui
 */

using System.Collections.Generic;
using System.Text;
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

    /// <summary>
    /// 対戦結果に応じてカード報酬を付与する
    /// </summary>
    public static void GrantRewards(BattleState result) {

        // 所持カードデータを読み込む
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // マスターに登録されている全カードIDを取得する
        List<int> allCardIDs = CardDataMasterUtility.GetAllCardIDs();

        // 抽選対象が存在しなければ終了する
        if (allCardIDs == null || allCardIDs.Count == 0) {
            Debug.LogWarning("報酬対象のカードが存在しません");
            return;
        }

        // 所持カードの枚数をカードIDごとに集計する
        Dictionary<int, int> ownedCounts = new Dictionary<int, int>();

        foreach (int cardID in saveData.cardID) {

            // 同じカードIDの所持枚数を加算する
            if (ownedCounts.ContainsKey(cardID)) {
                ownedCounts[cardID]++;
            }
            else {
                ownedCounts.Add(cardID, 1);
            }
        }

        // 未所持カードの抽選候補を作成する
        List<int> unownedCards = new List<int>();

        // 所持3枚未満のカードの抽選候補を作成する
        List<int> ownedCards = new List<int>();

        foreach (int cardID in allCardIDs) {

            // 所持枚数を取得する
            int count = ownedCounts.ContainsKey(cardID)
                ? ownedCounts[cardID]
                : 0;

            // 未所持カードを候補に追加する
            if (count == 0) {
                unownedCards.Add(cardID);
            }
            // 1～2枚所持しているカードを候補に追加する
            else if (count < 3) {
                ownedCards.Add(cardID);
            }
        }

        // 対戦結果に応じて報酬枚数を決定する
        int unownedCount = 0;
        int ownedCount = 0;
        int randomCount = 0;

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
                Debug.LogWarning("対戦結果が正しくありません");
                return;
        }

        // 獲得したカードIDと抽選回数を管理する
        Dictionary<int, int> acquiredCards = new Dictionary<int, int>();

        // 未所持カードから抽選する
        DrawCards(unownedCards, unownedCount, acquiredCards);

        // 所持3枚未満のカードから抽選する
        DrawCards(ownedCards, ownedCount, acquiredCards);

        // 全カードから完全ランダムで抽選する
        DrawRandomCards(allCardIDs, randomCount, acquiredCards);

        // 獲得したカードをまとめてログに表示する
        ShowRewardLog(result, acquiredCards);
    }

    /// <summary>
    /// 指定された候補リストから重複なしでカードを抽選する
    /// </summary>
    private static void DrawCards(
        List<int> candidates,
        int drawCount,
        Dictionary<int, int> acquiredCards) {

        // 候補が存在しない場合は終了する
        if (candidates == null || candidates.Count == 0) {
            return;
        }

        // 指定枚数だけ抽選する
        for (int i = 0; i < drawCount; i++) {

            // 候補がなくなった場合は終了する
            if (candidates.Count == 0) {
                break;
            }

            // ランダムに抽選する
            int index = Random.Range(0, candidates.Count);

            // 抽選したカードIDを取得する
            int cardID = candidates[index];

            // 同じ候補から再度抽選されないように削除する
            candidates.RemoveAt(index);

            // カードを入手する
            SaveManager.Instance.AcquireCard(cardID);

            // 獲得カードの枚数を記録する
            AddAcquiredCard(acquiredCards, cardID);
        }
    }

    /// <summary>
    /// 全カードから完全ランダムで抽選する
    /// </summary>
    private static void DrawRandomCards(
        List<int> allCardIDs,
        int drawCount,
        Dictionary<int, int> acquiredCards) {

        // カードが存在しない場合は終了する
        if (allCardIDs == null || allCardIDs.Count == 0) {
            return;
        }

        // 指定枚数だけ抽選する
        for (int i = 0; i < drawCount; i++) {

            // 全カードからランダムに選択する
            int index = Random.Range(0, allCardIDs.Count);

            // 抽選したカードIDを取得する
            int cardID = allCardIDs[index];

            // カードを入手する
            SaveManager.Instance.AcquireCard(cardID);

            // 獲得カードの枚数を記録する
            AddAcquiredCard(acquiredCards, cardID);
        }
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

    /// <summary>
    /// 獲得したカードをまとめてログに表示する
    /// </summary>
    private static void ShowRewardLog(
        BattleState result,
        Dictionary<int, int> acquiredCards) {

        // ログを作成する
        StringBuilder log = new StringBuilder();

        // 対戦結果を表示する
        log.AppendLine("========== 対戦報酬 ==========");
        log.AppendLine($"対戦結果: {result}");
        log.AppendLine($"獲得カード合計: {GetTotalCount(acquiredCards)}枚");
        log.AppendLine("------------------------------");

        // カードIDごとに獲得枚数を表示する
        foreach (var card in acquiredCards) {

            // カードIDと獲得枚数を表示する
            log.AppendLine(
                $"カードID: {card.Key} / 獲得枚数: {card.Value}枚"
            );
        }

        // ログを終了する
        log.Append("==============================");

        // まとめて1回だけログを出力する
        Debug.Log(log.ToString());
    }

    /// <summary>
    /// 獲得カードの合計枚数を取得する
    /// </summary>
    private static int GetTotalCount(Dictionary<int, int> acquiredCards) {

        // 合計枚数
        int totalCount = 0;

        // 全カードの獲得枚数を加算する
        foreach (var card in acquiredCards) {
            totalCount += card.Value;
        }

        // 合計枚数を返す
        return totalCount;
    }
}