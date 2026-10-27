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
    // レーンID(フィールド以外の場所にある場合は使わない)
    public int laneID { get; private set;  } = -1;

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
    public CardSuportRole suportRole { get; private set; } = CardSuportRole.None;
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
        if (data == null) return; 
        // データから各情報を保存
        defaultStatus = new CardStates() { HP = data.HP, ATK = data.ATK, SIZE = data.SIZE };
        currentStatus = defaultStatus;
        suportRole = (CardSuportRole)data.suportRoleID;
        cardType = (CardType)data.CardType;
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
        laneID = -1;
        defaultStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        currentStatus = new CardStates() { HP = -1, ATK = -1, SIZE = -1 };
        suportRole = CardSuportRole.None;
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

    /// <summary>
    /// レーンIDのセット
    /// </summary>
    /// <param name="setID">セットするID</param>
    public void SetLaneID(int setID) {
        laneID = setID;
    }

    /// <summary>
    /// 対応役列挙体を子役列挙体のリストに変換する
    /// </summary>
    /// <returns></returns>
    public List<PachisuroSymbolKoyakuEnum> SuportRoleToKoyaku() {
        // 変換した対応役を保存
        List<PachisuroSymbolKoyakuEnum> suportRoleList = new List<PachisuroSymbolKoyakuEnum>();
        switch (suportRole) {
            case CardSuportRole.None:
            case CardSuportRole.NULL:
                return null;
                // 全対応は変換の必要がないのでNONEを入れておく
            case CardSuportRole.ALL:
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.NONE);
                break;
            case CardSuportRole.Miss:
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.Miss);
                break;
            case CardSuportRole.Replay:
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.Replay);
                break;
            case CardSuportRole.Cherry:
                // チェリーは強チェリーと弱チェリーの両対応として登録
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.WeakCherry);
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.StrongCherry);
                break;
            case CardSuportRole.Bell:
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.Bell);
                break;
            case CardSuportRole.Suika:
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.Suika);
                break;
            case CardSuportRole.Cherry_Suika:
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.WeakCherry);
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.StrongCherry);
                suportRoleList.Add(PachisuroSymbolKoyakuEnum.Suika);
                break;
        }
        return suportRoleList;
    }
}
