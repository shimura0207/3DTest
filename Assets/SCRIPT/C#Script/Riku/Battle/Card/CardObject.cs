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
    // 手札ID(手札以外の場所にある場合は使わない)
    public int handID { get; private set; } = -1;

    // カードのステータス
    public struct CardStates {
        // HP
        public int HP;
        // 攻撃力
        public int ATK;
        // サイズ
        public int SIZE;
    }

    // デフォルトのステータス
    public CardStates defaultStatus { get; private set; }
    // 現在のステータス
    public CardStates currentStatus { get; private set; }
    // 対応役
    public CardKoyaku suportRole { get; private set; } = CardKoyaku.None;
    // カードタイプ
    public CardType cardType { get; private set; } = CardType.None;

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
        var data = CardDataMasterUtility.GetCardMaster(cardID);
        // データから各情報を保存
        defaultStatus = new CardStates() { HP = data.hp, ATK = data.atk, SIZE = data.size };
        currentStatus = defaultStatus;
        suportRole = (CardKoyaku)data.KoyakuID;
        cardType = (CardType)data.Type;
        // IDからイラストを取得
        spriteRenderer.sprite = cardImageDatabase.GetCardImage(cardID);
    }

    /// <summary>
    /// 片付け
    /// </summary>
    public void Teardown() {
        objectID = -1;
        cardID = -1;
        handID = -1;
        defaultStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        currentStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        suportRole = CardKoyaku.None;
        cardType = CardType.None;
        spriteRenderer.sprite = null;
    }

    /// <summary>
    /// 手札IDのセット
    /// </summary>
    /// <param name="setID">セットするID</param>
    public void SetHandID(int setID) {
        handID = setID;
    }
}
