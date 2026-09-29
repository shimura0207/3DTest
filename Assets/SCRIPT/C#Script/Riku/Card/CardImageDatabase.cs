/*
 *  @file   CardImageDatabase.cs
 *  @author Riku
 */
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// カードの見た目データベース
/// </summary>
public class CardImageDatabase : ScriptableObject {
    // カードの見た目データリスト
    private Dictionary<int, Sprite> cardImageDataList;

    /// <summary>
    /// リストにデータを追加
    /// </summary>
    /// <param name="addID">追加するカードのID</param>
    /// <param name="addImage">追加するカードの見た目</param>
    public void AddCardImageDataList(int addID, Sprite addImage) {
        cardImageDataList.Add(addID, addImage);
    }
}
