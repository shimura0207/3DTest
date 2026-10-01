/*
 * @file    CardObject.cs
 * @autor   Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードオブジェクトクラス
/// </summary>
public class CardObject : MonoBehaviour {
    // オブジェクトを判別する個別ID
    public int objectID { get; private set; } = -1;
    // カードの種類を判別するID
    public int cardID { get; private set; } = -1;

    // カードのステータス
    private struct CardStates {
        // HP
        public int HP;
        // 攻撃力
        public int ATK;
        // サイズ
        public int SIZE;
    }

    // デフォルトのステータス
    private CardStates defaultStatus;
    // 現在のステータス
    private CardStates currentStatus;

    // カードのイラストデータ
    [SerializeField]
    private CardImageDatabase cardImageDatabase = null;
    // SpriteRendererコンポーネント
    [SerializeField]
    private SpriteRenderer spriteRenderer = null;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize() {
        defaultStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        currentStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
    }

    /// <summary>
    /// 準備
    /// </summary>
    /// <param name="setObjectID">オブジェクトの個別ID</param>
    /// <param name="setCardID">カードの識別ID</param>
    public void Setup(int setObjectID, int setCardID) {
        // IDのセット
        objectID = setObjectID;
        cardID = setCardID;
        // IDからステータスを取得

        // IDからイラストを取得
        spriteRenderer.sprite = cardImageDatabase.GetCardImage(cardID);
    }

    /// <summary>
    /// 片付け
    /// </summary>
    public void Teardown() {
        objectID = -1;
        cardID = -1;
        defaultStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        currentStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        spriteRenderer.sprite = null;
    }
}
