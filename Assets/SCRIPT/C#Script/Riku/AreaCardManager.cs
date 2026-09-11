/*
 * @file    AreaCardManager.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各エリアのカード管理クラス
/// </summary>
public class AreaCardManager : MonoBehaviour {
    // 自身への参照
    public static AreaCardManager instance { get; private set; } = null;

    // 各エリアのリストにカードIDを積んで管理
    // 山札
    private List<int> deckCard = null;
    // 手札
    private List<int> handCard = null;
    // フィールド
    private List<int> fieldCard = null;
    // 墓地
    private List<int> graveCard = null;

    // Start is called before the first frame update
    void Start() {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        
    }

    /// <summary>
    /// デッキのセット
    /// </summary>
    /// <param name="setDeck"></param>
    public void SetDeck(List<int> setDeck) {
        deckCard = setDeck;
    }

    /// <summary>
    /// 任意の枚数山札からドロー
    /// </summary>
    /// <param name="drawCount"></param>
    public void DrawCard(int drawCount) {
        for (int i = 0; i < drawCount; i++) {
            // 山札の一番上のカードを手札に追加
            handCard.Add(deckCard[0]);
            // 山札の一番上のカードを削除
            deckCard.RemoveAt(0);
        }
    }

    /// <summary>
    /// 任意のカードを手札から特定のレーンに出す
    /// </summary>
    /// <param name="handNumber"></param>
    /// <param name="fieldNumber"></param>
    public void HandToField(int handNumber, int fieldNumber) {
        // 指定されたレーンに指定されたカードを追加
        fieldCard.Insert(fieldNumber, handCard[handNumber]);
        // 指定されたカードを手札から削除
        handCard.RemoveAt(handNumber);
    }

    /// <summary>
    /// 任意のカードを手札から墓地へ送る
    /// </summary>
    /// <param name="handNumber"></param>
    public void HandToGrave(int handNumber) {
        // 墓地に指定されたカードを追加
        graveCard.Add(handCard[handNumber]);
        // 指定されたカードを手札から削除
        handCard.RemoveAt(handNumber);
    }

    /// <summary>
    /// 任意のカードをフィールドから墓地へ送る
    /// </summary>
    /// <param name="fieldNumber"></param>
    public void FieldToGrave(int fieldNumber) {
        // 墓地に指定されたカードを追加
        graveCard.Add(fieldCard[fieldNumber]);
        // 指定されたカードをフィールドから削除
        fieldCard.RemoveAt(fieldNumber);
    }

    /// <summary>
    /// 任意のカードをゲーム外から特定のレーンに出す
    /// </summary>
    /// <param name="cardID"></param>
    /// <param name="fieldID"></param>
    public void OutGameToField(int cardID, int fieldID) {
        // 指定されたレーンに指定されたカードを追加
        fieldCard.Insert(fieldID, cardID);
    }
}
