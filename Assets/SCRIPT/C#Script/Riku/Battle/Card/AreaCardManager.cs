/*
 * @file    AreaCardManager.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

using static CommonModule;

/// <summary>
/// 各エリアのカード管理クラス
/// </summary>
public class AreaCardManager : MonoBehaviour {
    // 自身への参照
    public static AreaCardManager instance { get; private set; } = null;

    // 各エリアのリストにカードIDを積んで管理
    private Dictionary<PlayerType, List<int>> deckCards = null;
    // 山札外のカード
    public struct OutDeckCards { 
        // 手札
        public List<int> handCards;
        // フィールド
        public List<int> fieldCards;
        // 墓地
        public List<int> graveCards;
    }
    // プレイヤーごとの山札外カード
    public Dictionary<PlayerType, OutDeckCards> outDeckCards { get; private set; }

    // 相手のデッキを受け取ったか
    public bool catchOpponentDeck { get; private set; } = false;

    /// <summary>
    /// 初期化処理
    /// @author oorui
    /// </summary>
    public void Initialize() {
        instance = this;

        deckCards = new Dictionary<PlayerType, List<int>>();
        outDeckCards = new Dictionary<PlayerType, OutDeckCards>();
        for (PlayerType i = 0; i < PlayerType.Max; i++) {
            deckCards[i] = new List<int>();
            outDeckCards[i] = new OutDeckCards() {
                handCards = new List<int>(),
                fieldCards = new List<int>(),
                graveCards = new List<int>()
            };
            // フィールドは5つ固定なのであらかじめ無効数を入れておく
            for (int j = 0; j < (int)FieldLane.Max; j++) {
                outDeckCards[i].fieldCards.Add((int)FieldLane.None);
            }
        }
    }

    /// <summary>
    /// デッキのセット
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="setDeck">デッキの中身</param>
    public void SetDeck(PlayerType player, List<int> setDeck) {
        deckCards[player] = setDeck;
    }

    /// <summary>
    /// デッキシャッフル(自分のデッキ)
    /// </summary>
    public void ShuffleDeck() {
        // Fisher-Yatesシャッフル
        // リストの上から順にランダムな位置のカードと入れ替えていく
        int listMax = deckCards[PlayerType.Self].Count;
        for (int i = 0; i < listMax - 1; i++) {
            // 順番で選ばれたカード以降の範囲から抽選
            int randomIndex = Random.Range(i, listMax);
            // 入れ替え
            int cardID = deckCards[PlayerType.Self][i];
            deckCards[PlayerType.Self][i] = deckCards[PlayerType.Self][randomIndex];
            deckCards[PlayerType.Self][randomIndex] = cardID;
        }
    }

    /// <summary>
    /// 山札からのドロー
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <returns></returns>
    public int DrawCard(PlayerType player) {
        if (IsEmpty(deckCards[player])) return -1;

        // 山札の一番上のカードを自分の手札に追加
        int card = deckCards[player][0];
        outDeckCards[player].handCards.Add(card);
        // 山札の一番上のカードを削除
        deckCards[player].RemoveAt(0);

        return card;
    }

    /// <summary>
    /// 任意のカードを手札から特定のレーンに出す
    /// </summary>
    /// <param name="player">自分か相手か</param>
    /// <param name="handNumber">手札の番号</param>
    /// <param name="laneID">フィールドのレーン番号</param>
    public void HandToField(PlayerType player, int handNumber, int laneID) {
        // 指定されたレーンに指定されたカードを追加
        outDeckCards[player].fieldCards[laneID] = outDeckCards[player].handCards[handNumber];
        // 指定されたカードを手札から削除
        outDeckCards[player].handCards.RemoveAt(handNumber);
    }

    /// <summary>
    /// 任意のカードを手札から墓地へ送る
    /// </summary>
    /// <param name="player">自分か相手か</param>
    /// <param name="handNumber">手札の番号</param>
    public void HandToGrave(PlayerType player, int handNumber) {
        // 墓地に指定されたカードを追加
        outDeckCards[player].graveCards.Add(outDeckCards[player].handCards[handNumber]);
        // 指定されたカードを手札から削除
        outDeckCards[player].handCards.RemoveAt(handNumber);
    }

    /// <summary>
    /// 任意のカードをフィールドから墓地へ送る
    /// </summary>
    /// <param name="player">自分か相手か</param>
    /// <param name="laneID">フィールドのレーン番号</param>
    public void FieldToGrave(PlayerType player, int laneID) {
        // 墓地に指定されたカードを追加
        outDeckCards[player].graveCards.Add(outDeckCards[player].fieldCards[laneID]);
        // 指定されたカードをフィールドから削除
        outDeckCards[player].fieldCards.RemoveAt(laneID);
    }

    /// <summary>
    /// 任意のカードをゲーム外から特定のレーンに出す
    /// </summary>
    /// <param name="player">自分か相手か</param>
    /// <param name="cardID">カードID</param>
    /// <param name="laneID">フィールドのレーン番号</param>
    public void OutGameToField(PlayerType player, int cardID, int laneID) {
        // 指定されたレーンに指定されたカードを追加
        outDeckCards[player].fieldCards[laneID] = cardID;
    }

    /// <summary>
    /// レーンの交換
    /// </summary>
    /// <param name="player">自分か相手か</param>
    /// <param name="aLaneID">レーンを交換するカードA</param>
    /// <param name="bLaneID">レーンを交換するカードB</param>
    public void LaneChange(PlayerType player, int aLaneID, int bLaneID) {
        // 指定されたレーンのカードを保持
        int a = outDeckCards[player].fieldCards[aLaneID];
        // aをbに置き換え
        outDeckCards[player].fieldCards[aLaneID] = outDeckCards[player].fieldCards[bLaneID];
        // bを保持したaに置き換え
        outDeckCards[player].fieldCards[bLaneID] = a;
    }

    /// <summary>
    /// 送信用にデッキをstringに纏める
    /// </summary>
    /// <returns></returns>
    public string DeckPack() {
        var deck = deckCards[PlayerType.Self];

        return NetworkValueConverter.Pack(NetworkUseID.deck, deck);
    }

    /// <summary>
    /// 相手のデッキを受け取る
    /// </summary>
    /// <param name="opponentDeck"></param>
    public void SetOpponentDeck(string[] opponentDeck) {
        catchOpponentDeck = true;
        List<int> deck = new List<int>();
        for (int i = 0; i < opponentDeck.Length; i++) {
            // 配列の頭は識別IDなのでスキップ
            if (i == 0) continue;
            // 一つずつint型のIDに変換して入れていく
            int cardID = -1;
            if (!NetworkValueConverter.TryGetInt(opponentDeck[i], out cardID)) continue;
            deck.Add(cardID);
        }
        // 変換したデッキをセット
        if (deck.Count != GameConst.DECK_CARD_MAX) {
            Debug.Log("相手のデッキが正しくセットされていません。");
        }
        deckCards[PlayerType.Opponent] = deck;
    }

    /// <summary>
    /// デッキ受け取りフラグのセット
    /// </summary>
    /// <param name="setFlag"></param>
    public void SetCatchOpponentDeck(bool setFlag) {
        catchOpponentDeck = setFlag;
    }
}
