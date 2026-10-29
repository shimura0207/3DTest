/*
 * @file    CardObject.cs
 * @autor   Riku
 */

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
    //public struct CardStates {
    //    // HP
    //    public int HP;
    //    // 攻撃力
    //    public int ATK;
    //    // サイズ
    //    public int SIZE;
    //}

    // デフォルトのステータス
    public Dictionary<MonsterStatus, int> defaultStatus { get; private set; }
    // 現在のステータス
    public Dictionary<MonsterStatus, int> currentStatus { get; private set; }
    // 対応役
    public CardSuportRole suportRole { get; private set; } = CardSuportRole.None;
    // カードタイプ
    public CardType cardType { get; private set; } = CardType.None;
    // 所有者
    public PlayerType playerType { get; private set; } = PlayerType.None;

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
        defaultStatus = new Dictionary<MonsterStatus, int>();
        currentStatus = new Dictionary<MonsterStatus, int>();
    }

    /// <summary>
    /// 準備
    /// </summary>
    /// <param name="setObjectID">オブジェクトの個別ID</param>
    /// <param name="setCardID">カードの識別ID</param>
    /// <param name="player">どちらのプレイヤーか</param>
    public void Setup(int setObjectID, int setCardID, PlayerType player) {
        // IDのセット
        objectID = setObjectID;
        cardID = setCardID;
        // IDからステータスを取得
        var data = CardDataMasterUtility.GetCardMaster(cardID);
        if (data == null) return;
        // データから各情報を保存
        defaultStatus[MonsterStatus.HP] = data.HP;
        defaultStatus[MonsterStatus.ATK] = data.ATK;
        defaultStatus[MonsterStatus.SIZE] = data.SIZE;
        currentStatus = defaultStatus;
        suportRole = (CardSuportRole)data.suportRoleID;
        cardType = (CardType)data.CardType;
        // 所有者をセット
        playerType = player;

        // 相手のカードの場合はとりあえず裏面画像に設定
        if (playerType == PlayerType.Opponent) {
            spriteRenderer.sprite = cardImageDatabase.GetCardImage(GameConst.BACK_IMAGE_ID);
        }
        // 自分のカードはIDから取得
        else if (playerType == PlayerType.Self) {
            // IDからイラストを取得
            spriteRenderer.sprite = cardImageDatabase.GetCardImage(cardID);
        }
    }

    /// <summary>
    /// 片付け
    /// </summary>
    public void Teardown() {
        objectID = -1;
        cardID = -1;
        handID = -1;
        laneID = -1;
        defaultStatus = new Dictionary<MonsterStatus, int>();
        currentStatus = new Dictionary<MonsterStatus, int>();
        suportRole = CardSuportRole.None;
        cardType = CardType.None;
        playerType = PlayerType.None;
        spriteRenderer.sprite = null;
    }

    /// <summary>
    /// モンスターとして出ているこのオブジェクトを破壊する
    /// </summary>
    public void DestroyMonster() {
        // フィールドから墓地へIDの移動
        AreaCardManager.instance.FieldToGrave(playerType, laneID);
        // オブジェクトのフィールドを離れる処理
        CardObjectManager.instance.LeaveFieldSetup(playerType, (FieldLane)laneID);
        // 自分自身の未使用化
        CardObjectManager.instance.UnuseCardObject(this);
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

    /// <summary>
    /// カードの画像変更
    /// </summary>
    /// <param name="imageID">画像のID(背面画像以外はカードIDと同じ)</param>
    public void ChangeCardImage(int imageID) {
        spriteRenderer.sprite = cardImageDatabase.GetCardImage(imageID);
    }

    /// <summary>
    /// 指定したステータスをセット
    /// </summary>
    /// <param name="setStatus">セットするステータス</param>
    /// <param name="setValue">セットする値</param>
    public void SetStatus(MonsterStatus setStatus, int setValue) {
        currentStatus[setStatus] = setValue;

        // HPが0になった場合は破壊
        if (currentStatus[MonsterStatus.HP] <= 0) {
            DestroyMonster();
        }
    }

    /// <summary>
    /// 指定したステータスに与える
    /// </summary>
    /// <param name="giveStatus">与えるステータス</param>
    /// <param name="giveValue">与える値</param>
    public void GiveStatus(MonsterStatus giveStatus, int giveValue) {
        currentStatus[giveStatus] += giveValue;

        // HPが0になった場合は破壊
        if (currentStatus[MonsterStatus.HP] <= 0) {
            DestroyMonster();
        }
    }
}
