/*
 *  @file   CardImageDatabase.cs
 *  @author Riku
 */
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カードのイラストデータベース
/// </summary>
[CreateAssetMenu(fileName = "CardImageDatabase", menuName = "CardData/CardImageDatabase")]
public class CardImageDatabase : ScriptableObject {
    [System.Serializable]
    public struct CardImageData {
        public int cardID;
        public Sprite cardImage;
    }

    // カードのイラストデータリスト
    public List<CardImageData> cardImageDataList { get; private set; } = new List<CardImageData>();

    /// <summary>
    /// リストにデータを追加
    /// </summary>
    /// <param name="addID">追加するカードのID</param>
    /// <param name="addImage">追加するカードのイラスト</param>
    public void AddCardImageDataList(int addID, Sprite addImage) {
        // カードデータを構造体にして追加
        CardImageData data = new CardImageData() {
            cardID = addID,
            cardImage = addImage
        };

        // リストに何もなければそのまま追加
        if (cardImageDataList.Count == 0) {
            cardImageDataList.Add(data);
            return;
        }

        int insertIndex = 0;
        // 追加データのIDより大きいIDが出てくるまでカウント
        while (cardImageDataList.Count > insertIndex &&
               cardImageDataList[insertIndex].cardID < addID) {
            insertIndex++;
        }
        
        // IDが被っていなければ追加
        if (cardImageDataList.Count > insertIndex &&
            cardImageDataList[insertIndex].cardID == addID) {
            // ID被りなので警告
            Debug.LogWarning("このIDはすでに登録されています。");
        }
        else {
            // 適正な位置に追加
            cardImageDataList.Insert(insertIndex, data);
        }
        
    }

    /// <summary>
    /// リストからデータ削除
    /// </summary>
    /// <param name="deleteDataIndex">削除するデータの番号</param>
    public void DeleteCardImageData(int deleteDataIndex) {
        cardImageDataList.RemoveAt(deleteDataIndex);
    }
}
