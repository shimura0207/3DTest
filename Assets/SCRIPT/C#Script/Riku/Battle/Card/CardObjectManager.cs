/*
 * @file    CardObjectManafer.cs
 * @author  Riku
 */

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

using static CommonModule;

/// <summary>
/// カードオブジェクト管理クラス
/// </summary>
public class CardObjectManager : MonoBehaviour {
    // 自身への参照
    public static CardObjectManager instance { get; private set; } = null;

    // 使用中カードオブジェクトの親オブジェクト
    [SerializeField]
    private Transform useObjectRoot = null;
    // 未使用カードオブジェクトの親オブジェクト
    [SerializeField]
    private Transform unuseObjectRoot = null;
    // カードオブジェクトのオリジナル
    [SerializeField]
    private CardObject originObject = null;

    // 使用中のカードオブジェクトリスト
    private List<CardObject> useObjectList = null;
    // 未使用のカードオブジェクトリスト
    private List<CardObject> unuseObjectList = null;

    // フィールドのカードオブジェクトのコピーリスト
    public Dictionary<PlayerType, Dictionary<FieldLane, CardObject>> fieldObjectList { get; private set; } = null;

    // 自身の手札の親オブジェクト
    [SerializeField]
    private Transform selfHandRoot = null;
    // 相手の手札の親オブジェクト
    [SerializeField]
    private Transform opponentHandRoot = null;
    // 自身のフィールドの各レーン親オブジェクト
    [SerializeField]
    private List<Transform> selfFieldLaneRoot = null;
    // 相手のフィールドの各レーン親オブジェクト
    [SerializeField]
    private List<Transform> opponentFieldLaneRoot = null;

    // ある程度の生成数
    private const int CARD_OBJECT_MAX = 30;
    // 手札間隔
    private const float HAND_CARD_DISTANCE = 0.05f;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize() {
        instance = this;

        // カードオブジェクトをある程度生成して未使用状態にしておく
        useObjectList = new List<CardObject>();
        unuseObjectList = new List<CardObject>();
        for (int i = 0; i < CARD_OBJECT_MAX; i++) {
            unuseObjectList.Add(Instantiate(originObject, unuseObjectRoot));
            unuseObjectList[i].Initialize();
        }

        // フィールドのカードのコピーリストはあらかじめnullを積んでおく
        fieldObjectList = new Dictionary<PlayerType, Dictionary<FieldLane, CardObject>>();
        for (PlayerType i = 0; i < PlayerType.Max; i++) {
            fieldObjectList[i] = new Dictionary<FieldLane, CardObject>();
            for (FieldLane j = 0; j < FieldLane.Max; j++) {
                fieldObjectList[i][j] = null;
            }
        }
    }

    /// <summary>
    /// カードオブジェクトを使用状態にする
    /// </summary>
    /// <param name="cardID">使用カードのID</param>
    /// <returns></returns>
    public CardObject UseCardObject(int cardID) {
        // 使用可能なカードオブジェクトのインスタンスを取得
        CardObject useCard = GetUsableCardObject();
        // 使用可能なIDを取得して使用リストに追加
        int useID = -1;
        for (int i = 0, max = useObjectList.Count; i < max; i++) {
            if (useObjectList[i] != null) continue;
            // 使用可能な場所が見つかった
            useID = i;
            useObjectList[i] = useCard;
            break;
        }
        // リストに使用可能な場所が見つからなかったので末尾に追加
        if (useID < 0) {
            useID = useObjectList.Count;
            useObjectList.Add(useCard);
        }
        // 使用中親オブジェクトへ親を変更
        useCard.transform.SetParent(useObjectRoot, false);
        // オブジェクトの準備
        useCard.Setup(useID, cardID);
        return useCard;
    }

    /// <summary>
    /// カードオブジェクトを未使用状態にする
    /// </summary>
    /// <param name="unuseObject"></param>
    public void UnuseCardObject(CardObject unuseObject) {
        if (unuseObject == null) return;
        if (unuseObject.objectID < 0) return;
        
        // 使用中リストから除外
        useObjectList[unuseObject.objectID] = null;
        // オブジェクトの片付け処理
        unuseObject.Teardown();
        // 未使用リストに追加
        unuseObjectList.Add(unuseObject);
        // 未使用親オブジェクトへ親を変更
        unuseObject.transform.SetParent(unuseObjectRoot, false);
    }

    /// <summary>
    /// 未使用状態のカードオブジェクト取得
    /// </summary>
    /// <returns></returns>
    private CardObject GetUsableCardObject() {
        if (IsEmpty(unuseObjectList)) return Instantiate(originObject);

        // リストの0番を渡して削除
        CardObject result = unuseObjectList[0];
        unuseObjectList.RemoveAt(0);
        return result;
    }

    /// <summary>
    /// 手札への移動
    /// </summary>
    /// <param name="player">どっちのプレイヤーか</param>
    /// <param name="objectID">オブジェクトの識別ID</param>
    public void MoveToHand(PlayerType player, int objectID) {
        // 座標を初期化
        Transform objectTransform = useObjectList[objectID].transform;
        objectTransform.localPosition = Vector3.zero;
        // 手札を親にする
        switch (player) {
            case PlayerType.Self:
                objectTransform.SetParent(selfHandRoot, false);
                break;
            case PlayerType.Opponent:
                objectTransform.SetParent(opponentHandRoot, false);
                break;
        }
        // 手札IDを仮で振り分け
        useObjectList[objectID].SetHandID(0);
        // 手札の整列
        ArrangeHand(player);
    }

    /// <summary>
    /// フィールドへの移動
    /// </summary>
    /// <param name="player">どっちのプレイヤーか</param>
    /// <param name="objectID">オブジェクトの識別ID</param>
    /// <param name="setLane">置かれるレーン</param>
    public void MoveToField(PlayerType player, int objectID, FieldLane setLane) {
        CardObject card = useObjectList[objectID];
        // 手札IDを初期化
        card.SetHandID(-1);
        // フィールドのリストに追加
        fieldObjectList[player][setLane] = card;
        // 座標を初期化
        Transform objectTransform = card.transform;
        objectTransform.localPosition = Vector3.zero;
        // 指定したレーンを親にする
        switch (player) {
            case PlayerType.Self:
                objectTransform.SetParent(selfFieldLaneRoot[(int)setLane], false);
                break;
            case PlayerType.Opponent:
                objectTransform.SetParent(opponentFieldLaneRoot[(int)setLane], false);
                break;
        }
        // レーンIDセット
        card.SetLaneID((int)setLane);
        // 手札の整列
        ArrangeHand(player);
    }

    /// <summary>
    /// 手札の整列
    /// </summary>
    public void ArrangeHand(PlayerType player) {
        int handCount = AreaCardManager.instance.outDeckCards[player].handCards.Count;
        // 手札が0枚以下なら整列の必要なし
        if (handCount <= 0) return;
        
        // カード配置位置
        float setPos = 0.0f;
        // 手札の枚数が奇数なら
        if (handCount % 2 == 1) {
            // 中央の一枚分を抜いて手札の半分の枚数計算
            int halfHand = (handCount - 1) / 2;
            // 手札の半分枚数分左に寄せた位置をスタートにする
            setPos -= halfHand * HAND_CARD_DISTANCE;  
        }
        // 手札の枚数が偶数なら
        else {
            // 手札の半分の枚数計算
            int halfHand = handCount / 2;
            // 手札の半分枚数から1引いた数分プラス半分の間隔分左に寄せた位置をスタートにする
            setPos -= ((halfHand - 1) * HAND_CARD_DISTANCE) + (HAND_CARD_DISTANCE / 2);
        }

        // 決定された左端の位置から手札の枚数分右にずらしながら配置する
        int handID = 0;
        foreach (var card in useObjectList) {
            if (!card) continue;
            // 手札IDがないやつは手札じゃないのでスキップ
            if (card.handID < 0) continue;
            // 配置
            card.transform.localPosition = new Vector3(setPos, 0.0f, 0.0f);
            // 手札IDをセット
            card.SetHandID(handID);
            // 手札IDカウントアップ
            handID++;
            // 配置位置を一つ右にずらす
            setPos += HAND_CARD_DISTANCE;
        }
    }

    /// <summary>
    /// フィールドを離れる時に呼ぶ処理
    /// </summary>
    /// <param name="player"></param>
    /// <param name="lane"></param>
    public void LeaveFieldSetup(PlayerType player, FieldLane lane) {
        fieldObjectList[player][lane].SetLaneID(-1);
        fieldObjectList[player][lane] = null;
    }

    /// <summary>
    /// 指定したものと同じレーンの番号を取得(自身のレーン)
    /// </summary>
    /// <param name="getTransform"></param>
    /// <returns></returns>
    public FieldLane GetObjectLane(Transform getTransform) {
        // 同じレーンを走査
        for (FieldLane i = 0; i < FieldLane.Max; i++) {
            if (selfFieldLaneRoot[(int)i] != getTransform) continue;
            return i;
        }
        return FieldLane.None;
    }

    /// <summary>
    /// レーン交換
    /// </summary>
    /// <param name="player"></param>
    /// <param name="laneA"></param>
    /// <param name="laneB"></param>
    public void LaneChange(PlayerType player, FieldLane laneA, FieldLane laneB) {
        CardObject cardA = fieldObjectList[player][laneA];
        CardObject cardB = fieldObjectList[player][laneB];
        // 座標を初期化
        if (cardA)
            cardA.transform.localPosition = Vector3.zero;
        if (cardB)
            cardB.transform.localPosition = Vector3.zero;
        // レーンの親を交換
        switch (player) {
            case PlayerType.Self:
                if (cardA)
                    cardA.transform.SetParent(selfFieldLaneRoot[(int)laneB], false);
                if (cardB)
                    cardB.transform.SetParent(selfFieldLaneRoot[(int)laneA], false);
                break;
            case PlayerType.Opponent:
                if (cardA)
                    cardA.transform.SetParent(opponentFieldLaneRoot[(int)laneB], false);
                if (cardB)
                    cardB.transform.SetParent(opponentFieldLaneRoot[(int)laneA], false);
                break;
        }
        // フィールドのリストを交換
        fieldObjectList[player][laneA] = cardB;
        fieldObjectList[player][laneB] = cardA;
        // レーンIDを交換
        if (cardA)
            cardA.SetLaneID((int)laneB);
        if (cardB)
            cardB.SetLaneID((int)laneA);
    }
}
