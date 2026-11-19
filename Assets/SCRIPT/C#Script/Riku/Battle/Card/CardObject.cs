/*
 * @file    CardObject.cs
 * @autor   Riku
 */

using Cysharp.Threading.Tasks;
using Mirror.BouncyCastle.Math.Field;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

/// <summary>
/// カードオブジェクトクラス
/// </summary>
public class CardObject : MonoBehaviour {
    // 未使用エフェクトの親オブジェクト
    [SerializeField]
    private Transform unuseEffectRoot = null;
    // 使用エフェクトの親オブジェクト
    [SerializeField]
    private Transform useEffectRoot = null;
    // 疲労しないエフェクト
    [SerializeField]
    private Transform noFatigueEffect = null;
    // ダメージ軽減エフェクト
    [SerializeField]
    private Transform attackReductionEffeect = null;
    // 疲労エフェクト
    [SerializeField]
    private Transform fatigueEffect = null;
    // プレイ可能エフェクト
    [SerializeField]
    private Transform canPlayEffect = null;
    // 役昇格エフェクト
    [SerializeField]
    private Transform upperEffect = null;
    // G数上昇エフェクト
    [SerializeField]
    private Transform gOverEffect = null;

    // オブジェクトを判別する個別ID
    public int objectID { get; private set; } = -1;
    // カードの種類を判別するID
    public int cardID { get; private set; } = -1;
    // 手札ID(手札以外の場所にある場合は使わない)
    public int handID { get; private set; } = -1;
    // レーンID(フィールド以外の場所にある場合は使わない)
    public int laneID { get; private set; } = -1;

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
    // カードの効果
    public List<CardAbilityDatabase.AbilityData> abilityList { get; private set; }
    // 疲労しているかどうか
    public bool isFatigue { get; private set; } = false;

    // SpriteRendererコンポーネント
    [SerializeField]
    private SpriteRenderer spriteRenderer = null;
    // 読み取り専用プロパティ
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize() {
        defaultStatus = new Dictionary<MonsterStatus, int>();
        currentStatus = new Dictionary<MonsterStatus, int>();
        abilityList = new List<CardAbilityDatabase.AbilityData>();
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
            spriteRenderer.sprite = ScriptableObjectManager.instance.GetCardImage(GameConst.BACK_IMAGE_ID);
        }
        // 自分のカードはIDから取得
        else if (playerType == PlayerType.Self) {
            // IDからイラストを取得
            spriteRenderer.sprite = ScriptableObjectManager.instance.GetCardImage(cardID);
        }

        // IDから効果を取得
        abilityList = ScriptableObjectManager.instance.GetCardAbility(cardID);
        // nullだったら空を入れとく
        if (abilityList == null) abilityList = new List<CardAbilityDatabase.AbilityData>();

        // フィールドのカードの場合はエフェクトをつける
        if (laneID < 0) return;
        foreach (var ability in abilityList) {
            if (ability.abilityType != AbilityType.Passive) continue;
            switch (ability.passiveAbility) {
                case PassiveAbility.NoFatigue:
                    noFatigueEffect.SetParent(useEffectRoot);
                    break;
                case PassiveAbility.AttackReduction:
                    attackReductionEffeect.SetParent(useEffectRoot);
                    break;
                case PassiveAbility.Upper:
                    upperEffect.SetParent(useEffectRoot);
                    break;
                case PassiveAbility.GOver:
                    gOverEffect.SetParent(useEffectRoot);
                    break;
            }
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
        abilityList = new List<CardAbilityDatabase.AbilityData>();
        isFatigue = false;
        spriteRenderer.sprite = null;
        noFatigueEffect.SetParent(unuseEffectRoot);
        attackReductionEffeect.SetParent(unuseEffectRoot);
        fatigueEffect.SetParent(unuseEffectRoot);
        canPlayEffect.SetParent(unuseEffectRoot);
        upperEffect.SetParent(unuseEffectRoot);
        gOverEffect.SetParent(unuseEffectRoot);
    }

    /// <summary>
    /// モンスターとして出ているこのオブジェクトを破壊する
    /// </summary>
    /// <param name="isAbility">効果発動中かどうか</param>
    public void DestroyMonster(bool isAbility = false) {
        // フィールドから墓地へIDの移動
        AreaCardManager.instance.FieldToGrave(playerType, laneID);
        // オブジェクトのフィールドを離れる処理
        CardObjectManager.instance.LeaveFieldSetup(playerType, (FieldLane)laneID);
        // 離れた時効果発動確認
        foreach (var ability in abilityList) {
            if (ability.abilityTiming != AbilityTiming.Leave) continue;
            // 見つかった効果を積む
            AbilityManager.instance.SetAblityQueue(this, ability);
        }
        // 自分自身の未使用化
        CardObjectManager.instance.UnuseCardObject(this);
        
        if (isAbility) return;
        // 積まれた効果があれば発動
        UniTask task = AbilityManager.instance.AbilityExecute();
    }

    /// <summary>
    /// モンスターとして出ているこのオブジェクトを手札に戻す
    /// <param name="isAbility">効果発動中かどうか</param>
    /// </summary>
    public void ReturnMonster(bool isAbility = false) {
        // フィールドから手札へIDの移動
        AreaCardManager.instance.FieldToHand(playerType, laneID);
        // オブジェクトのフィールドを離れる処理
        CardObjectManager.instance.LeaveFieldSetup(playerType, (FieldLane)laneID);
        // オブジェクトを手札へ移動
        CardObjectManager.instance.MoveToHand(playerType, objectID);
        // 離れた時効果発動確認
        foreach (var ability in abilityList) {
            if (ability.abilityTiming != AbilityTiming.Leave) continue;
            // 見つかった効果を積む
            AbilityManager.instance.SetAblityQueue(this, ability);
        }

        if (isAbility) return;
        // 積まれた効果があれば発動
        UniTask task = AbilityManager.instance.AbilityExecute();
    }

    /// <summary>
    /// 手札IDのセット
    /// </summary>
    /// <param name="setID">セットするID</param>
    public void SetHandID(int setID) {
        handID = setID;

        
        if (playerType != PlayerType.Self) return;
        // 手札のカードではなくなったのならエフェクトは消す
        if (handID < 0) {
            canPlayEffect.SetParent(unuseEffectRoot);
            return;
        }
        // プレイ可能なカードはエフェクトを出す
        switch (cardType) {
            case CardType.Monster:
                // フィールドに空きがあるか
                bool fieldBlank = false;
                foreach (var card in CardObjectManager.instance.fieldObjectList[PlayerType.Self]) {
                    if (card.Value != null) continue;
                    fieldBlank = true;
                    break;
                }
                // サイズが超過しないか
                bool sizeBlank = false;
                if (currentStatus[MonsterStatus.SIZE] <= BattleSystemManager.instance.GetRemainingSize(PlayerType.Self)) {
                    sizeBlank = true;
                }
                // フィールドに空きがあり、サイズが超過しないならプレイ可能
                if (fieldBlank && sizeBlank) {
                    canPlayEffect.SetParent(useEffectRoot);
                }
                break;
            case CardType.Spell:
                // スペル使用回数が3回以下ならプレイ可能
                if (BattleSystemManager.instance.spellCastCount < GameConst.SPELL_CAST_MAX) {
                    canPlayEffect.SetParent(useEffectRoot);
                }
                else {
                    canPlayEffect.SetParent(unuseEffectRoot);
                }
                break;
        }
    }

    /// <summary>
    /// 手札のプレイ可能エフェクトの表示非表示
    /// </summary>
    /// <param name="isActive"></param>
    public void SetCanPlayEffectActive(bool isActive) {
        if (isActive) {
            canPlayEffect.SetParent(useEffectRoot);
        }
        else {
            canPlayEffect.SetParent(unuseEffectRoot);
        }
    }

    /// <summary>
    /// レーンIDのセット
    /// </summary>
    /// <param name="setID">セットするID</param>
    public void SetLaneID(int setID) {
        laneID = setID;

        // フィールドのカードではなくなったのならエフェクトは消す
        if (laneID < 0) {
            noFatigueEffect.SetParent(unuseEffectRoot);
            attackReductionEffeect.SetParent(unuseEffectRoot);
            upperEffect.SetParent(unuseEffectRoot);
            gOverEffect.SetParent(unuseEffectRoot);
            return;
        }
        // 常在効果を持っている場合はエフェクトを出す
        foreach (var ability in abilityList) {
            if (ability.abilityType != AbilityType.Passive) continue;
            switch (ability.passiveAbility) {
                case PassiveAbility.NoFatigue:
                    noFatigueEffect.SetParent(useEffectRoot);
                    break;
                case PassiveAbility.AttackReduction:
                    attackReductionEffeect.SetParent(useEffectRoot);
                    break;
                case PassiveAbility.Upper:
                    upperEffect.SetParent(useEffectRoot);
                    break;
                case PassiveAbility.GOver:
                    gOverEffect.SetParent(useEffectRoot);
                    break;
            }
        }
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
        spriteRenderer.sprite = ScriptableObjectManager.instance.GetCardImage(imageID);
    }

    /// <summary>
    /// 指定したステータスをセット
    /// </summary>
    /// <param name="setStatus">セットするステータス</param>
    /// <param name="setValue">セットする値</param>
    public void SetStatus(MonsterStatus setStatus, int setValue) {
        if (cardType != CardType.Monster) return;
        currentStatus[setStatus] = setValue;

        // HPが0になった場合は破壊
        if (currentStatus[MonsterStatus.HP] <= 0) {
            DestroyMonster();
        }
    }

    /// <summary>
    /// 指定したステータスに与える
    /// ※破壊処理を内包しているのでHPを減らす場合はforeach厳禁
    /// </summary>
    /// <param name="giveStatus">与えるステータス</param>
    /// <param name="giveValue">与える値</param>
    public void GiveStatus(MonsterStatus giveStatus, int giveValue) {
        if (cardType != CardType.Monster) return;
        currentStatus[giveStatus] += giveValue;
        
        // ダメージをくらったら表示
        if (giveStatus == MonsterStatus.HP && giveValue < 0) {
            UIManager.instance.GenerateDamageUI(giveValue, this);
        }

        // ATKが0以下の場合は0にする
        if (currentStatus[MonsterStatus.ATK] <= 0) {
            currentStatus[MonsterStatus.ATK] = 0;
        }
        // SIZEが0以下の場合は0にする
        if (currentStatus[MonsterStatus.SIZE] <= 0) {
            currentStatus[MonsterStatus.SIZE] = 0;
        }
        // HPが0になった場合は破壊
        if (currentStatus[MonsterStatus.HP] <= 0) {
            DestroyMonster();
        }
    }

    /// <summary>
    /// 疲労状態の変更
    /// </summary>
    /// <param name="setFatigue"></param>
    public void SetIsFatigue(bool setFatigue) {
        // 疲労しない効果を持っている場合は変更できない
        foreach (var ability in abilityList) {
            if (ability.passiveAbility == PassiveAbility.NoFatigue) return;
        }
        // 疲労
        isFatigue = setFatigue;
        // 疲労エフェクトの表示
        if (isFatigue) {
            fatigueEffect.SetParent(useEffectRoot);
        }
        // 非表示
        else {
            fatigueEffect.SetParent(unuseEffectRoot);
        }
    }
}
