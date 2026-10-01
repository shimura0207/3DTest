/*
 * @file    CardObjectManafer.cs
 * @author  Riku
 */

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
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

    // ある程度の生成数
    private const int CARD_OBJECT_MAX = 30;

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
        useCard.transform.SetParent(useObjectRoot);
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
        unuseObject.transform.SetParent(unuseObjectRoot);
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
    /// 手札の整列
    /// </summary>
    public void ArrangeHand(PlayerType player) {
        int hand = AreaCardManager.instance.outDeckCards[player].handCards.Count;
    }
}
