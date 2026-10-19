/*
 * @file    CardAbilityDatabase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CardImageDatabase;

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

    // カードの能力リスト
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

            cardAbilityDataList.Add(data);
            return;
        }



        int insertIndex = 0;
        // 追加データのIDより大きいIDが出てくるまでカウント
        while (cardAbilityDataList.Count > insertIndex &&
               cardAbilityDataList[insertIndex].cardID < addID) {
            insertIndex++;
        }

        // IDが被っていなければ追加
        if (cardAbilityDataList.Count > insertIndex &&
            cardAbilityDataList[insertIndex].cardID == addID) {
            // ID被りなので警告
            Debug.LogWarning("このIDはすでに登録されています。");
        }
        else {
            // 適正な位置に追加
            cardAbilityDataList.Insert(insertIndex, data);
        }
    }
}
