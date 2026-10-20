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
    public struct CardAbilityData {
        public int cardID;
        public List<Func<UniTask>> abilityList;
    }

    // カードの効果リスト
    [SerializeField]
    private List<CardAbilityData> cardAbilityDataList = new List<CardAbilityData>();
    // リストの読み取り専用プロパティ
    public List<CardAbilityData> CardAbilityDataList => cardAbilityDataList;

    /// <summary>
    /// リストにデータ追加
    /// </summary>
    /// <param name="addID">追加するカードのID</param>
    /// <param name="abilityID">追加する効果の番号</param>
    /// <param name="abilityFunc">追加する効果</param>
    public void AddCardAbilityDataList(int addID, int abilityID, Func<UniTask> abilityFunc) {
        // カードデータを構造体にして追加
        CardAbilityData data = new CardAbilityData() {
            cardID = addID,
            abilityList = new List<Func<UniTask>>()
        };
        
        // リストに何もなければそのまま追加
        if (cardAbilityDataList.Count == 0) {
            // 効果の番号より小さい番号にはnullを入れておく
            for (int i = 0; i < abilityID; i++) {
                data.abilityList.Add(null);
            }
            // 効果番号の位置に効果が設定される
            data.abilityList.Add(abilityFunc);
            // 追加
            cardAbilityDataList.Add(data);
            return;
        }

        // 同じカードIDに他の効果が入っている場合
        foreach (var card in cardAbilityDataList) {
            if (card.cardID != addID) continue;

            // 追加したい効果番号がなかったらその番号までnullを詰めて追加
            if (card.abilityList.Count <= abilityID) {
                // nullの敷き詰め
                for (int i = 0; i < abilityID - card.abilityList.Count; i++) {
                    card.abilityList.Add(null);
                }
                // 効果番号の位置に効果が設定される
                card.abilityList.Add(abilityFunc);
            }
            else {
                // 追加したい効果番号に追加
                card.abilityList[abilityID] = abilityFunc;
            }
            return;
        }

        // 同じカードIDに他の効果がない場合
        int insertIndex = 0;
        // 追加データのIDより大きいIDが出てくるまでカウント
        while (cardAbilityDataList.Count > insertIndex &&
               cardAbilityDataList[insertIndex].cardID < addID) {
            insertIndex++;
        }
        // 効果の番号より小さい番号にはnullを入れておく
        for (int i = 0; i < abilityID; i++) {
            data.abilityList.Add(null);
        }
        // 効果番号の位置に効果が設定される
        data.abilityList.Add(abilityFunc);
        // 適正な位置に追加
        cardAbilityDataList.Insert(insertIndex, data);
        return;
    }
}
