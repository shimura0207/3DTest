using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 * @file  RGDeckController.h
 * @author simura
 */

/// <summary>
/// RGカードゲームのデッキと手札を管理するスクリプト。
/// 
/// 役割:
/// ・Inspectorでデッキ内容を設定する
/// ・関数を呼ぶとデッキから1枚引く
/// ・引いたカードをHandCard1に追加する
/// ・既存の手札を右にずらす
/// 
/// 手札の並び:
/// handSlots[0] = HandCard1
/// handSlots[1] = HandCard2
/// handSlots[2] = HandCard3
/// handSlots[3] = HandCard4
/// handSlots[4] = HandCard5
/// </summary>
public class RGDeckController : MonoBehaviour
{

    [Header("UI表示")]

    [Tooltip("残りデッキ枚数を表示するTextMeshPro。UI側で用意したTextを入れる。")]
    [SerializeField] private TMP_Text deckCountText;

    [Tooltip("デッキ枚数表示の前につける文字。")]
    [SerializeField] private string deckCountPrefix = "Deck: ";
    //============================================================
    // デッキ設定
    //============================================================

    [Header("デッキ設定")]

    [Tooltip("Inspectorで設定するデッキ内容。何枚でもOK。上から順番に引きます。")]
    [SerializeField] private List<RGCardData> deckCards = new List<RGCardData>();

    [Tooltip("ゲーム開始時にデッキをシャッフルするか。")]
    [SerializeField] private bool shuffleOnStart = true;

    [Tooltip("Start時に手札を空にするか。")]
    [SerializeField] private bool clearHandOnStart = true;

    //============================================================
    // 手札設定
    //============================================================

    [Header("手札スロット")]

    [Tooltip("HandCard1～5を左から順番に入れる。Element0がHandCard1。")]
    [SerializeField] private RGHandCardSlot[] handSlots = new RGHandCardSlot[5];

    //============================================================
    // 実行中データ
    //============================================================

    /// <summary>
    /// 実際にゲーム中に使うデッキ。
    /// InspectorのdeckCardsを直接減らすと編集データが変わってしまうので、
    /// Start時にruntimeDeckへコピーして使います。
    /// </summary>
    private List<RGCardData> runtimeDeck = new List<RGCardData>();

    /// <summary>
    /// 現在の手札データ。
    /// handCards[0] = HandCard1
    /// handCards[1] = HandCard2
    /// のように対応します。
    /// </summary>
    private RGCardData[] handCards = new RGCardData[5];

    //============================================================
    // Unityイベント
    //============================================================

    public void Initialize()
    {
        ResetDeck();

        if (shuffleOnStart)
        {
            ShuffleDeck();
        }

        if (clearHandOnStart)
        {
            ClearHand();
        }
        else
        {
            RefreshAllHandViews();
        }
    }

    //============================================================
    // デッキ操作
    //============================================================

    /// <summary>
    /// Inspectorで設定したdeckCardsを、実行用デッキにコピーします。
    /// 
    /// どこをいじる？
    /// ・デッキ内容を変更したい → InspectorのDeck Cardsを変更
    /// ・デッキを初期状態に戻したい → この関数を呼ぶ
    /// </summary>
    public void ResetDeck()
    {
        runtimeDeck.Clear();

        for (int i = 0; i < deckCards.Count; i++)
        {
            if (deckCards[i] == null)
            {
                continue;
            }

            runtimeDeck.Add(deckCards[i]);
        }

       // Debug.Log("デッキを初期化しました。枚数: " + runtimeDeck.Count);
        RefreshDeckCountText();
    }

    /// <summary>
    /// 実行用デッキをシャッフルします。
    /// 
    /// Fisher-Yates方式のシャッフルです。
    /// </summary>
    public void ShuffleDeck()
    {
        for (int i = runtimeDeck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            RGCardData temp = runtimeDeck[i];
            runtimeDeck[i] = runtimeDeck[randomIndex];
            runtimeDeck[randomIndex] = temp;
        }

        Debug.Log("デッキをシャッフルしました。");
    }

    /// <summary>
    /// デッキから1枚引いて手札に追加します。
    /// 
    /// 外部から呼びたいメイン関数はこれです。
    /// 例:
    /// deckController.DrawCardToHand();
    /// 
    /// 処理:
    /// 1. デッキの一番上を取得
    /// 2. デッキからそのカードを削除
    /// 3. 手札を右にずらす
    /// 4. HandCard1に新しいカードを入れる
    /// </summary>
    public void DrawCardToHand()
    {

        
        if (runtimeDeck.Count <= 0)
        {
            Debug.LogWarning("デッキが空なのでカードを引けません。");
            return;
        }

        RGCardData drawnCard = runtimeDeck[0];
        runtimeDeck.RemoveAt(0);

        AddCardToHandLeft(drawnCard);

        //Debug.Log("カードを引きました: " + drawnCard.CardName + " / 残りデッキ枚数: " + runtimeDeck.Count);
        RefreshDeckCountText();
        
    }

    /// <summary>
    /// 指定枚数分カードを引きます。
    /// 初期手札で4枚引きたい場合などに使えます。
    /// </summary>
    public void DrawCardsToHand(int drawCount)
    {
        for (int i = 0; i < drawCount; i++)
        {
            DrawCardToHand();
        }
    }

    /// <summary>
    /// 現在のデッキ残り枚数を返します。
    /// UI表示などに使えます。
    /// </summary>
    public int GetDeckCount()
    {
        return runtimeDeck.Count;
    }

    //============================================================
    // 手札操作
    //============================================================

    /// <summary>
    /// 手札の一番左、つまりHandCard1にカードを追加します。
    /// 
    /// 追加前に、既存の手札を右へずらします。
    /// 
    /// 例:
    /// 追加前:
    /// HandCard1 = A
    /// HandCard2 = B
    /// HandCard3 = C
    /// HandCard4 = D
    /// HandCard5 = E
    /// 
    /// 新しくXを引くと:
    /// HandCard1 = X
    /// HandCard2 = A
    /// HandCard3 = B
    /// HandCard4 = C
    /// HandCard5 = D
    /// 
    /// Eは押し出されて消えます。
    /// </summary>
    public void AddCardToHandLeft(RGCardData newCard)
    {
        if (newCard == null)
        {
            Debug.LogWarning("追加しようとしたカードがnullです。");
            return;
        }

        ShiftHandRight();

        handCards[0] = newCard;

        RefreshAllHandViews();
    }

    /// <summary>
    /// 手札を右に1つずらします。
    /// 
    /// 4 → 5
    /// 3 → 4
    /// 2 → 3
    /// 1 → 2
    /// の順に処理します。
    /// 
    /// 左から処理すると上書き事故が起きるので、
    /// 必ず右端から逆順で処理します。
    /// </summary>
    private void ShiftHandRight()
    {
        for (int i = handCards.Length - 1; i >= 1; i--)
        {
            handCards[i] = handCards[i - 1];
        }

        handCards[0] = null;
    }

    /// <summary>
    /// 手札を全部空にします。
    /// </summary>
    public void ClearHand()
    {
        for (int i = 0; i < handCards.Length; i++)
        {
            handCards[i] = null;
        }

        RefreshAllHandViews();
    }

    /// <summary>
    /// 現在の手札配列を取得します。
    /// デバッグや効果処理で使えます。
    /// </summary>
    public RGCardData[] GetHandCards()
    {
        return handCards;
    }

    //============================================================
    // 表示更新
    //============================================================

    /// <summary>
    /// handCardsの中身を、Scene上のHandCard1～5へ反映します。
    /// </summary>
    private void RefreshAllHandViews()
    {
        for (int i = 0; i < handSlots.Length; i++)
        {
            if (handSlots[i] == null)
            {
                continue;
            }

            if (i >= handCards.Length)
            {
                handSlots[i].ClearCard();
                continue;
            }

            if (handCards[i] == null)
            {
                handSlots[i].ClearCard();
            }
            else
            {
                handSlots[i].SetCard(handCards[i]);
            }
        }
    }


    /// <summary>
    /// 指定した番号の手札カードを取得します。
    /// handIndex は 0=HandCard1, 1=HandCard2, 2=HandCard3...
    /// </summary>
    public RGCardData GetHandCardAt(int handIndex)
    {
        if (handIndex < 0 || handIndex >= handCards.Length)
        {
            return null;
        }

        return handCards[handIndex];
    }

    /// <summary>
    /// 指定した番号の手札を消費し、その後ろの手札を左詰めします。
    /// 
    /// 例:
    /// HandCard1 = A
    /// HandCard2 = B
    /// HandCard3 = C
    /// の状態で HandCard2 を消費すると、
    /// 
    /// HandCard1 = A
    /// HandCard2 = C
    /// HandCard3 = 空
    /// になります。
    /// </summary>
    public void RemoveHandCardAtAndCompact(int removeIndex)
    {
        if (removeIndex < 0 || removeIndex >= handCards.Length)
        {
            Debug.LogWarning("手札消費に失敗しました。範囲外です: " + removeIndex);
            return;
        }

        if (handCards[removeIndex] == null)
        {
            Debug.LogWarning("消費しようとした手札が空です: HandCard" + (removeIndex + 1));
            return;
        }

       // Debug.Log("手札を消費しました: " + handCards[removeIndex].CardName);

        for (int i = removeIndex; i < handCards.Length - 1; i++)
        {
            handCards[i] = handCards[i + 1];
        }

        handCards[handCards.Length - 1] = null;

        RefreshAllHandViews();
    }

    /// <summary>
    /// 残りデッキ枚数をUIに表示します。
    /// </summary>
    private void RefreshDeckCountText()
    {
        if (deckCountText == null)
        {
            return;
        }

        deckCountText.text = deckCountPrefix + runtimeDeck.Count;
    }


    public static bool i;
    public static void Ichenge() {
        i = true;
    }
    public static void NotIchenge() {
        i = false;
    }
}