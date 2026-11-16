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
    public static Dictionary<int, int> GrantRewards(BattleState result) {

        // 所持カードデータを読み込む
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // マスターに登録されている全カードIDを取得する
        List<int> allCardIDs = CardDataMasterUtility.GetAllCardIDs();

        // 使用可能なカードIDだけを格納するリスト
        List<int> validCardIDs = new List<int>();

        // 全カードIDを確認する
        foreach (int cardID in allCardIDs) {

            // カードマスターを取得する
            Entity_CardData.Param cardData =
                CardDataMasterUtility.GetCardMaster(cardID);

            // マスターが存在しないカードは除外する
            if (cardData == null) {
                Debug.LogWarning($"カードマスターが存在しません。ID:{cardID}");
                continue;
            }

            // PlayIDが1のカードは抽選対象から除外する
            if (cardData.PlayID == 1) {
                Debug.LogWarning($"使用不可カードを抽選対象から除外しました。ID:{cardID}");
                continue;
            }

            // 使用可能なカードだけを追加する
            validCardIDs.Add(cardID);
        }

        // 抽選対象を使用可能なカードだけに置き換える
        allCardIDs = validCardIDs;

        // 抽選対象が存在しなければ終了する
        if (allCardIDs.Count == 0) {
            Debug.LogWarning("報酬対象の使用可能なカードが存在しません");
            return new Dictionary<int, int>();
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

        // 未所持カードの抽選候補
        List<int> unownedCards = new List<int>();

        // 所持3枚以下のカードの抽選候補
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
            // 3枚以下所持しているカードを候補に追加する
            else if (count <= 3) {
                ownedCards.Add(cardID);
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
                Debug.LogWarning("対戦結果が正しくありません");
                return new Dictionary<int, int>();
        }

        // 獲得カードIDと枚数を管理する
        Dictionary<int, int> acquiredCards = new Dictionary<int, int>();

        // 未所持枠を抽選する
        DrawWithFallback(
            unownedCards,
            ownedCards,
            allCardIDs,
            unownedCount,
            acquiredCards
        );

        // 所持カード枠を抽選する
        DrawWithFallback(
            ownedCards,
            null,
            allCardIDs,
            ownedCount,
            acquiredCards
        );

        // ランダム枠を抽選する
        DrawRandomCards(
            allCardIDs,
            randomCount,
            acquiredCards
        );

        // 獲得したカードをまとめてログに表示する
        ShowRewardLog(result, acquiredCards);

        // 獲得カード情報を呼び出し元に返す
        return acquiredCards;
    }

    /// <summary>
    /// 指定候補から抽選し、不足分を別候補で補填する
    /// </summary>
    private static void DrawWithFallback(
        List<int> candidates,
        List<int> fallbackCandidates,
        List<int> allCardIDs,
        int drawCount,
        Dictionary<int, int> acquiredCards) {

        // 最初の候補から抽選する
        int drawnCount = DrawCards(
            candidates,
            drawCount,
            acquiredCards
        );

        // 抽選できなかった枚数を取得する
        int remainingCount = drawCount - drawnCount;

        // 不足分がなければ終了する
        if (remainingCount <= 0) {
            return;
        }

        // 補填候補が存在する場合はそこから抽選する
        if (fallbackCandidates != null) {

            drawnCount = DrawCards(
                fallbackCandidates,
                remainingCount,
                acquiredCards
            );

            remainingCount -= drawnCount;
        }

        // それでも不足している場合は全カードから抽選する
        if (remainingCount > 0) {
            DrawRandomCards(
                allCardIDs,
                remainingCount,
                acquiredCards
            );
        }
    }

    /// <summary>
    /// 指定候補リストから重複なしでカードを抽選する
    /// </summary>
    private static int DrawCards(
        List<int> candidates,
        int drawCount,
        Dictionary<int, int> acquiredCards) {

        // 候補が存在しない場合は終了する
        if (candidates == null || candidates.Count == 0) {
            return 0;
        }

        // 実際に抽選した枚数
        int drawnCount = 0;

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

            // 報酬用としてカードを一時的に所持データへ追加する
            SaveManager.Instance.AcquireCardForReward(cardID);

            // 獲得カードの枚数を記録する
            AddAcquiredCard(acquiredCards, cardID);

            drawnCount++;
        }

        // 抽選した枚数を返す
        return drawnCount;
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

            // 報酬用としてカードを一時的に所持データへ追加する
            SaveManager.Instance.AcquireCardForReward(cardID);

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
    private static int GetTotalCount(
        Dictionary<int, int> acquiredCards) {

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