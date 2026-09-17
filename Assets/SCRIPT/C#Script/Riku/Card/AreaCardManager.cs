/*
 * @file    AreaCardManager.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
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
    // 山札(相手側の山札は管理しなくていいので個別)
    private List<int> deckCards = null;
    // 山札外のカード
    struct OutDeckCards { 
        // 手札
        public List<int> handCards;
        // フィールド
        public List<int> fieldCards;
        // 墓地
        public List<int> graveCards;
    }
    // プレイヤーごとの山札外カード
    private Dictionary<PlayerType, OutDeckCards> outDeckCards;

    /// <summary>
    /// 初期化処理
    /// @author oorui
    /// </summary>
    public void Initialize() {
        instance = this;
    }

    /// <summary>
    /// デッキのセット
    /// </summary>
    /// <param name="setDeck">デッキのIDリスト</param>
    public void SetDeck(List<int> setDeck) {
        deckCards = setDeck;
    }

    /// <summary>
    /// 任意の枚数山札からドロー(自分側)
    /// </summary>
    /// <param name="drawCount">ドロー枚数</param>
    public void DrawCard(int drawCount) {
        if (IsEmpty(deckCards)) return;

        for (int i = 0; i < drawCount; i++) {
            // 山札の一番上のカードを自分の手札に追加
            outDeckCards[PlayerType.Self].handCards.Add(deckCards[0]);
            // 山札の一番上のカードを削除
            deckCards.RemoveAt(0);
        }
    }

    /// <summary>
    /// 任意のカードを手札から特定のレーンに出す
    /// </summary>
    /// <param name="player">自分か相手か</param>
    /// <param name="handNumber">手札の番号</param>
    /// <param name="fieldNumber">フィールドのレーン番号</param>
    public void HandToField(PlayerType player, int handNumber, int fieldNumber) {
        // 指定されたレーンに指定されたカードを追加
        outDeckCards[player].fieldCards.Insert(fieldNumber, outDeckCards[player].handCards[handNumber]);
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
    /// <param name="fieldNumber">フィールドのレーン番号</param>
    public void FieldToGrave(PlayerType player, int fieldNumber) {
        // 墓地に指定されたカードを追加
        outDeckCards[player].graveCards.Add(outDeckCards[player].fieldCards[fieldNumber]);
        // 指定されたカードをフィールドから削除
        outDeckCards[player].fieldCards.RemoveAt(fieldNumber);
    }

    /// <summary>
    /// 任意のカードをゲーム外から特定のレーンに出す
    /// </summary>
    /// <param name="player">自分か相手か</param>
    /// <param name="cardID">カードID</param>
    /// <param name="fieldID">フィールドのレーン番号</param>
    public void OutGameToField(PlayerType player, int cardID, int fieldID) {
        // 指定されたレーンに指定されたカードを追加
        outDeckCards[player].fieldCards.Insert(fieldID, cardID);
    }
}
