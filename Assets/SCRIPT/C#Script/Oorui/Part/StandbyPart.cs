/*
 *  @file   StandbyPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 準備パート
/// </summary>
public class StandbyPart : PartBase {

    public override async UniTask Execute() {

        // マスターデータを読み込む
        MasterDataManager.LoadAllData();

        // 初期デッキを登録する
        RegisterStarterDecks();

        // デバッグログを表示する
        Debug.Log("StandbyPart通過");

        // タイトルパートへ遷移する
        await PartManager.Instance.TransitionPart(GamePart.Title);
    }

    /// <summary>
    /// 初期デッキと所持カードを登録する
    /// </summary>
    private void RegisterStarterDecks() {

        // プレイヤーデータが存在するか確認する
        bool hasPlayerSave = SaveManager.Instance.HasPlayerSaveFile();

        // 所持カードデータが存在するか確認する
        bool hasCardSave = SaveManager.Instance.HasCardSaveFile();

        // 両方存在する場合は登録を終了する
        if (hasPlayerSave && hasCardSave) {
            Debug.Log("初期データが登録済みのためスキップします");
            return;
        }

        // 初期デッキのマスターデータを取得する
        SortedDictionary<int, List<int>> starterDecks =
            StarterDeckDataMasterUtility.GetStarterDecks();

        // 初期デッキデータが存在しなければ終了する
        if (starterDecks == null || starterDecks.Count == 0) {
            Debug.LogWarning("初期デッキのマスターデータが存在しません");
            return;
        }

        // 所持カードデータが存在しない場合のみ登録する
        if (!hasCardSave) {

            // 新規の所持カードセーブデータを取得する
            PossessionCardSaveData cardSaveData =
                SaveManager.Instance.LoadCards();

            // カードIDごとの所持枚数を管理する
            Dictionary<int, int> cardCount = new Dictionary<int, int>();

            // 所持カードの最大枚数
            const int MAX_CARD_COUNT = 3;

            // すべての初期デッキを確認する
            foreach (var deck in starterDecks) {

                // デッキ内のカードIDを確認する
                foreach (int cardID in deck.Value) {

                    // カードIDが辞書に存在しなければ追加する
                    if (!cardCount.ContainsKey(cardID)) {
                        cardCount.Add(cardID, 0);
                    }

                    // すでに3枚所持している場合は追加しない
                    if (cardCount[cardID] >= MAX_CARD_COUNT) {
                        continue;
                    }

                    // 所持カードにカードIDを追加する
                    cardSaveData.cardID.Add(cardID);

                    // 所持枚数を加算する
                    cardCount[cardID]++;
                }
            }

            // 所持カードデータを保存する
            SaveManager.Instance.SaveCards(cardSaveData);

            Debug.Log("初期カードの登録が完了しました");
        }

        // プレイヤーデータが存在しない場合のみデッキを登録する
        if (!hasPlayerSave) {

            // DeckIDごとにデッキを作成する
            foreach (var deck in starterDecks) {

                // マスターデータのDeckIDを取得する
                int masterDeckID = deck.Key;

                // デッキに含まれるカードID一覧を取得する
                List<int> cardIDs = deck.Value;

                // 空のデッキは登録しない
                if (cardIDs == null || cardIDs.Count == 0) {
                    continue;
                }

                // 初期デッキ名を設定する
                string deckName = $"レンタルデッキ{masterDeckID}";

                // デッキをセーブデータに登録する
                int savedDeckID = SaveManager.Instance.SaveDeck(
                    deckName,
                    cardIDs
                );

                // 登録結果を表示する
                Debug.Log(
                    $"初期デッキ登録完了 MasterDeckID:{masterDeckID} " +
                    $"SaveDeckID:{savedDeckID} カード枚数:{cardIDs.Count}"
                );
            }
        }
    }
}