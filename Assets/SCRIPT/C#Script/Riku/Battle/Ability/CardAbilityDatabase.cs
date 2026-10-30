/*
 * @file    CardAbilityDatabase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// カード効果データベース
/// </summary>
[CreateAssetMenu(fileName = "CardAbilityDatabase", menuName = "CardData/CardAbilityDatabase")]
public class CardAbilityDatabase : ScriptableObject {
    [System.Serializable]
    public struct AbilityData {
        public int abilityID;
        // 効果の種類
        public AbilityType abilityType;

        // 通常効果用
        public AbilityTiming abilityTiming;
        public AbilityTarget abilityTarget;
        public AbilityCondition abilityCondition;
        public AbilityProcess abilityProcess;

        // 特殊効果用
        public SpecialAbility specialAbility;

        // キーワード効果用
        public KeywordAbility keywordAbility;
    }
    [System.Serializable]
    public struct CardAbilityData {
        public int cardID;
        public List<AbilityData> abilityList;
    }

    // カードの効果リスト
    [SerializeField]
    private List<CardAbilityData> cardAbilityDataList = new List<CardAbilityData>();
    // リストの読み取り専用プロパティ
    public List<CardAbilityData> CardAbilityDataList => cardAbilityDataList;

    /// <summary>
    /// リストにデータ追加
    /// </summary>
    /// <param name="addCardID"></param>
    /// <param name="abilityData"></param>
    public void AddCardAbilityDataList(int addCardID, int addAbilityID, AbilityData abilityData) {
        // カードデータを構造体にして追加
        CardAbilityData data = new CardAbilityData() {
            cardID = addCardID,
            abilityList = new List<AbilityData>()
        };
        
        // リストに何もなければそのまま追加
        if (cardAbilityDataList.Count == 0) {
            // 効果リストに追加
            data.abilityList.Add(abilityData);
            // リストに追加
            cardAbilityDataList.Add(data);
            return;
        }

        // 同じカードIDに他の効果が入っている場合
        foreach (var card in cardAbilityDataList) {
            if (card.cardID != addCardID) continue;

            // 効果リストに追加
            card.abilityList.Add(abilityData);
            return;
        }

        // 同じカードIDに他の効果がない場合
        int insertIndex = 0;
        // 追加データのIDより大きいIDが出てくるまでカウント
        while (cardAbilityDataList.Count > insertIndex &&
               cardAbilityDataList[insertIndex].cardID < addCardID) {
            insertIndex++;
        }
        // 効果リストに追加
        data.abilityList.Add(abilityData);
        // 適正な位置に追加
        cardAbilityDataList.Insert(insertIndex, data);
        return;
    }

    /// <summary>
    /// リストからデータ削除
    /// </summary>
    /// <param name="cardIndex"></param>
    /// <param name="abilityIndex"></param>
    public void DeleteCardAbilityData (int cardIndex, int abilityIndex) {
        // カードの効果削除
        cardAbilityDataList[cardIndex].abilityList.RemoveAt(abilityIndex);

        // カードの効果がなくなったらそのIDのリストも削除 
        if (cardAbilityDataList[cardIndex].abilityList.Count > 0) return;
        cardAbilityDataList.RemoveAt(cardIndex);
    }
}
