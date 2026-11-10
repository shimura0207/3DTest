/*
 * @file    AbilityProcessData.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Mirror.Examples.CCU;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

/// <summary>
/// カードの効果処理
/// </summary>
public class AbilityProcessData {
    // ステータス付与関連効果
    private Dictionary<AbilityTarget, Func<MonsterStatus, CardObject, PlayerType, int, UniTask>> giveStatus = null;
    // 手札を捨てる効果
    private Dictionary<AbilityTarget, Func<PlayerType, UniTask>> handDestruction = null;
    // モンスター破壊効果
    private Dictionary<AbilityTarget, Func<CardObject, PlayerType, UniTask>> deleteMonster = null;
    // モンスターを手札に戻す効果
    private Dictionary<AbilityTarget, Func<CardObject, PlayerType, UniTask>> returnMonster = null;

    // リーチェLv.1の名前
    private const string REACHE_LV1_NAME = "RG:REACHE Lv.1";
    // リーチェLv.2の名前
    private const string REACHE_LV2_NAME = "RG:REACHE Lv.2";
    // リーチェLv.3の名前
    private const string REACHE_LV3_NAME = "RG:REACHE Lv.3";
    // イスカLv.1の名前
    private const string ISUKA_LV1_NAME = "RG:ISUKA LV1";
    // イスカLv.2の名前
    private const string ISUKA_LV2_NAME = "RG:ISUKA LV2";

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        // ステータス付与関連効果の登録
        RegisterGiveStatus();
        // 手札を捨てる効果の登録
        RegisterHandDestruction();
        // モンスター破壊効果の登録
        RegisterDeleteMonster();
        // モンスターを手札に戻す効果の登録
        RegisterReturnMonster();
    }

    /// <summary>
    /// ダメージ付与効果の登録
    /// </summary>
    private void RegisterGiveStatus() {
        giveStatus = new Dictionary<AbilityTarget, Func<MonsterStatus, CardObject, PlayerType, int, UniTask>>();
        // 自身プレイヤー
        giveStatus[AbilityTarget.SelfPlayer] = (status, card, player, value) => {
            if (status != MonsterStatus.HP) return UniTask.CompletedTask;
            // 自身プレイヤーのHP付与
            BattleSystemManager.instance.PlayerGiveHP(player, value);
            return UniTask.CompletedTask;
        };
        // 相手プレイヤー
        giveStatus[AbilityTarget.SelfPlayer] = (status, card, player, value) => {
            if (status != MonsterStatus.HP) return UniTask.CompletedTask;
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // 相手プレイヤーのHP付与
            BattleSystemManager.instance.PlayerGiveHP(revPla, value);
            return UniTask.CompletedTask;
        };
        // 自身のランダムなモンスター
        giveStatus[AbilityTarget.SelfRandom] = (status, card, player, value) => {
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(player);
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
            return UniTask.CompletedTask;
        };
        // 相手のランダムなモンスター
        giveStatus[AbilityTarget.OpponentRandom] = (status, card, player, value) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(revPla);
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
            return UniTask.CompletedTask;
        };
        // 自身の選択したモンスター
        giveStatus[AbilityTarget.SelfSelect] = async (status, card, player, value) => {
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(player);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[player][lane];
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
        };
        // 相手の選択したモンスター
        giveStatus[AbilityTarget.OpponentSelect] = async (status, card, player, value) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(revPla);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[revPla][lane];
            // 取得したモンスターに指定ステータスを付与
            monster.GiveStatus(status, value);
        };
        // 自身の全てのモンスター
        giveStatus[AbilityTarget.SelfAll] = (status, card, player, value) => {
            var fieldMonster = CardObjectManager.instance.fieldObjectList[player];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // モンスターに指定ステータスを付与
                fieldMonster[i].GiveStatus(status, value);
            }
            return UniTask.CompletedTask;
        };
        // 相手の全てのモンスター
        giveStatus[AbilityTarget.OpponentAll] = (status, card, player, value) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            var fieldMonster = CardObjectManager.instance.fieldObjectList[revPla];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // モンスターに指定ステータスを付与
                fieldMonster[i].GiveStatus(status, value);
            }
            return UniTask.CompletedTask;
        };
        // このモンスター
        giveStatus[AbilityTarget.ThisMonster] = (status, card, player, value) => {
            if (!card) return UniTask.CompletedTask;
            // このモンスターに指定ステータスを付与
            card.GiveStatus(status, value);
            return UniTask.CompletedTask;
        };
        // このモンスター以外のすべてのモンスター
        giveStatus[AbilityTarget.OtherMonster] = (status, card, player, value) => {
            // 発動モンスター以外の全てのモンスターを取得
            List<CardObject> otherMonster = new List<CardObject>();
            for (PlayerType i = 0; i < PlayerType.Max; i++) {
                foreach (var obj in CardObjectManager.instance.fieldObjectList[i]) {
                    if (!obj.Value) continue;
                    if (card && card == obj.Value) continue; 
                    otherMonster.Add(obj.Value);
                }
            }
            // 取得した全てに指定ステータスを付与
            for (int i = 0; i < otherMonster.Count; i++) {
                otherMonster[i].GiveStatus(status, value);
            }
            return UniTask.CompletedTask;
        };
    }

    /// <summary>
    /// 手札を捨てる効果の登録
    /// </summary>
    private void RegisterHandDestruction() {
        handDestruction = new Dictionary<AbilityTarget, Func<PlayerType, UniTask>>();
        // 自身プレイヤー
        handDestruction[AbilityTarget.SelfPlayer] = null;
        // 相手プレイヤー
        handDestruction[AbilityTarget.SelfPlayer] = null;
        // 自身のランダムなカード
        handDestruction[AbilityTarget.SelfRandom] = (player) => {
            // ランダムな手札取得
            CardObject hand = GetRandomHand(player);
            // ランダムに選ばれたカードを捨てる
            BattleSystemManager.instance.HandDestruction(player, hand);
            return UniTask.CompletedTask;
        };
        // 相手のランダムなカード
        handDestruction[AbilityTarget.OpponentRandom] = (player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // ランダムな手札取得
            CardObject hand = GetRandomHand(revPla);
            // ランダムに選ばれたカードを捨てる
            BattleSystemManager.instance.HandDestruction(revPla, hand);
            return UniTask.CompletedTask;
        };
        // 自分の選択したカード
        handDestruction[AbilityTarget.SelfSelect] = async (player) => {
            // 手札のカードを選択
            CardObject selectCard = await SelectHandCard();
            // 選択したカードを捨てる
            BattleSystemManager.instance.HandDestruction(player, selectCard);
        };
        // 相手の選択したカード
        handDestruction[AbilityTarget.OpponentSelect] = null;
        // 自身の全てのカード
        handDestruction[AbilityTarget.SelfAll] = (player) => {
            // 全ての手札を取得
            List<CardObject> handCards = CardObjectManager.instance.GetHandCardList(player);
            for (int i = 0; i < handCards.Count; i++) {
                // 捨てる
                BattleSystemManager.instance.HandDestruction(player, handCards[i]);
            }
            return UniTask.CompletedTask;
        };
        // 相手の全てのカード
        handDestruction[AbilityTarget.OpponentAll] = (player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // 全ての手札を取得
            List<CardObject> handCards = CardObjectManager.instance.GetHandCardList(player);
            for (int i = 0; i < handCards.Count; i++) {
                // 捨てる
                BattleSystemManager.instance.HandDestruction(revPla, handCards[i]);
            }
            return UniTask.CompletedTask;
        };
        // このモンスター
        handDestruction[AbilityTarget.ThisMonster] = null;
        // このモンスター以外のすべてのモンスター
        handDestruction[AbilityTarget.OtherMonster] = null;
    }

    /// <summary>
    /// モンスター破壊効果の登録
    /// </summary>
    private void RegisterDeleteMonster() {
        deleteMonster = new Dictionary<AbilityTarget, Func<CardObject, PlayerType, UniTask>>();
        // 自身プレイヤー
        deleteMonster[AbilityTarget.SelfPlayer] = null;
        // 相手プレイヤー
        deleteMonster[AbilityTarget.SelfPlayer] = null;
        // 自身のランダムなカード
        deleteMonster[AbilityTarget.SelfRandom] = (card, player) => {
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(player);
            // 取得したモンスターを破壊
            monster.DestroyMonster(true);
            return UniTask.CompletedTask;
        };
        // 相手のランダムなカード
        deleteMonster[AbilityTarget.OpponentRandom] = (card, player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(revPla);
            // 取得したモンスターを破壊
            monster.DestroyMonster(true);
            return UniTask.CompletedTask;
        };
        // 自分の選択したカード
        deleteMonster[AbilityTarget.SelfSelect] = async (card, player) => {
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(player);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[player][lane];
            // 取得したモンスターを破壊
            monster.DestroyMonster(true);
        };
        // 相手の選択したカード
        deleteMonster[AbilityTarget.OpponentSelect] = async (card, player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(revPla);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[revPla][lane];
            // 取得したモンスターを破壊
            monster.DestroyMonster(true);
        };
        // 自身の全てのモンスター
        deleteMonster[AbilityTarget.SelfAll] = (card, player) => {
            var fieldMonster = CardObjectManager.instance.fieldObjectList[player];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // 取得したモンスターを破壊
                fieldMonster[i].DestroyMonster(true);
            }
            return UniTask.CompletedTask;
        };
        // 相手の全てのモンスター
        deleteMonster[AbilityTarget.OpponentAll] = (card, player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            var fieldMonster = CardObjectManager.instance.fieldObjectList[revPla];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // 取得したモンスターを破壊
                fieldMonster[i].DestroyMonster(true);
            }
            return UniTask.CompletedTask;
        };
        // このモンスター
        deleteMonster[AbilityTarget.ThisMonster] = (card, player) => {
            if (!card) return UniTask.CompletedTask;
            // 取得したモンスターを破壊
            card.DestroyMonster(true);
            return UniTask.CompletedTask;
        };
        // このモンスター以外のすべてのモンスター
        deleteMonster[AbilityTarget.OtherMonster] = (card, player) => {
            // 発動モンスター以外の全てのモンスターを取得
            List<CardObject> otherMonster = new List<CardObject>();
            for (PlayerType i = 0; i < PlayerType.Max; i++) {
                foreach (var obj in CardObjectManager.instance.fieldObjectList[i]) {
                    if (!obj.Value) continue;
                    if (card && card == obj.Value) continue;
                    otherMonster.Add(obj.Value);
                }
            }
            // 取得したモンスターを破壊
            for (int i = 0; i < otherMonster.Count; i++) {
                otherMonster[i].DestroyMonster(true);
            }
            return UniTask.CompletedTask;
        };
    }

    /// <summary>
    /// モンスターを手札に戻す効果の登録
    /// </summary>
    private void RegisterReturnMonster() {
        returnMonster = new Dictionary<AbilityTarget, Func<CardObject, PlayerType, UniTask>>();
        // 自身プレイヤー
        returnMonster[AbilityTarget.SelfPlayer] = null;
        // 相手プレイヤー
        returnMonster[AbilityTarget.SelfPlayer] = null;
        // 自身のランダムなカード
        returnMonster[AbilityTarget.SelfRandom] = (card, player) => {
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(player);
            // 取得したモンスターを破壊
            monster.ReturnMonster(true);
            return UniTask.CompletedTask;
        };
        // 相手のランダムなカード
        returnMonster[AbilityTarget.OpponentRandom] = (card, player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // ランダムなモンスター取得
            CardObject monster = GetRandomMonster(revPla);
            // 取得したモンスターを破壊
            monster.ReturnMonster(true);
            return UniTask.CompletedTask;
        };
        // 自分の選択したカード
        returnMonster[AbilityTarget.SelfSelect] = async (card, player) => {
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(player);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[player][lane];
            // 取得したモンスターを破壊
            monster.ReturnMonster(true);
        };
        // 相手の選択したカード
        returnMonster[AbilityTarget.OpponentSelect] = async (card, player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            // レーン選択
            FieldLane lane = await MenuWindowManager.Instance.PlacedLaneMenuOpen(revPla);
            // 選択したレーンのモンスター取得
            CardObject monster = CardObjectManager.instance.fieldObjectList[revPla][lane];
            // 取得したモンスターを手札に戻す
            monster.ReturnMonster(true);
        };
        // 自身の全てのモンスター
        returnMonster[AbilityTarget.SelfAll] = (card, player) => {
            var fieldMonster = CardObjectManager.instance.fieldObjectList[player];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // 取得したモンスターを手札に戻す
                fieldMonster[i].ReturnMonster(true);
            }
            return UniTask.CompletedTask;
        };
        // 相手の全てのモンスター
        returnMonster[AbilityTarget.OpponentAll] = (card, player) => {
            // 相手プレイヤーを取得
            PlayerType revPla = GetReversePlayer(player);
            var fieldMonster = CardObjectManager.instance.fieldObjectList[revPla];
            for (FieldLane i = 0; i < FieldLane.Max; i++) {
                // 取得したモンスターを手札に戻す
                fieldMonster[i].ReturnMonster(true);
            }
            return UniTask.CompletedTask;
        };
        // このモンスター
        returnMonster[AbilityTarget.ThisMonster] = (card, player) => {
            // 取得したモンスターを手札に戻す
            card.ReturnMonster(true);
            return UniTask.CompletedTask;
        };
        // このモンスター以外のすべてのモンスター
        returnMonster[AbilityTarget.OtherMonster] = (card, player) => {
            // 発動モンスター以外の全てのモンスターを取得
            List<CardObject> otherMonster = new List<CardObject>();
            for (PlayerType i = 0; i < PlayerType.Max; i++) {
                foreach (var obj in CardObjectManager.instance.fieldObjectList[i]) {
                    if (!obj.Value) continue;
                    if (card && card == obj.Value) continue;
                    otherMonster.Add(obj.Value);
                }
            }
            // 取得したモンスターを手札に戻す
            for (int i = 0; i < otherMonster.Count; i++) {
                otherMonster[i].ReturnMonster(true);
            }
            return UniTask.CompletedTask;
        };
    }

    /// <summary>
    /// 対のプレイヤーを返す
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private PlayerType GetReversePlayer(PlayerType player) {
        PlayerType reversePlayer = PlayerType.None;
        if (player == PlayerType.Self) {
            reversePlayer = PlayerType.Opponent;
        }
        else if (player == PlayerType.Opponent) {
            reversePlayer = PlayerType.Self;
        }
        return reversePlayer;
    }

    /// <summary>
    /// ランダムなモンスターの取得
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private CardObject GetRandomMonster(PlayerType player) {
        List<CardObject> fieldMonster = new List<CardObject>();
        // 指定したプレイヤーのフィールドのモンスターを積む
        foreach (var obj in CardObjectManager.instance.fieldObjectList[player]) {
            if (!obj.Value) continue;
            fieldMonster.Add(obj.Value);
        }
        // 抽選
        int randIndex = UnityEngine.Random.Range(0, fieldMonster.Count);
        return fieldMonster[randIndex];
    }

    /// <summary>
    /// ランダムな手札のカード取得
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private CardObject GetRandomHand(PlayerType player) {
        List<CardObject> fieldMonster = CardObjectManager.instance.GetHandCardList(player);
        // 抽選
        int randIndex = UnityEngine.Random.Range(0, fieldMonster.Count);
        return fieldMonster[randIndex];
    }

    /// <summary>
    /// 手札のカード選択
    /// </summary>
    /// <returns></returns>
    private async UniTask<CardObject> SelectHandCard() {
        // 選択したカード
        CardObject selectCard = null;
        while (!selectCard) {
            // マウスカーソルのスクリーン座標取得
            Vector3 mousePos = Input.mousePosition;
            // スクリーン座標からレイを取得
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            // カードオブジェクトレイヤー取得
            int cardLayer = LayerMask.GetMask("CardObject");
            // レイキャストでカードを選ぶ
            if (Physics.Raycast(
                ray, out RaycastHit hit,
                GameConst.RAY_RANGE_MAX, cardLayer)
                ) {
                if (!hit.collider) continue;
                // 選んだオブジェクトを保存
                GameObject selectObject = hit.collider.gameObject;
                // オブジェクトのクラス取得
                selectCard = selectObject.GetComponent<CardObject>();
                // 相手のカードか手札以外のカードなら選択リセット
                if (selectCard.playerType != PlayerType.Self ||
                    selectCard.handID >= 0) {
                    selectObject = null;
                    selectCard = null;
                }
            }
            // 次のフレームまで待つ
            await UniTask.Yield();
        }
        return selectCard;
    }

    /// <summary>
    /// ドロー効果
    /// </summary>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="value">ドロー枚数</param>
    /// <returns></returns>
    public async UniTask DrawAbility(PlayerType player, int value) {
        // 枚数に指定がない場合は1枚
        if (value <= 0) value = 1;
        for (int i = 0; i < value; i++ ) {
            BattleSystemManager.instance.DrawCard(player);
        }
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ダメージ付与効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">ダメージ量</param>
    /// <returns></returns>
    public async UniTask GiveDamageAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // ダメージ量に指定がない場合は1ダメージ
        if (value <= 0) value = 1;
        // ダメージ付与
        await giveStatus[target](MonsterStatus.HP, card, player,  -value);
    }

    /// <summary>
    /// HP増加効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">増加量</param>
    /// <returns></returns>
    public async UniTask GiveHealthAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // 増加量に指定がない場合は1増加
        if (value <= 0) value = 1;
        // HP付与
        await giveStatus[target](MonsterStatus.HP, card, player, value);
    }

    /// <summary>
    /// ATK増加効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">増加量</param>
    /// <returns></returns>
    public async UniTask GiveAttackAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // 増加量に指定がない場合は1増加
        if (value <= 0) value = 1;
        // ATK増加
        await giveStatus[target](MonsterStatus.ATK, card, player, value);
    }

    /// <summary>
    /// ATK減少効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <param name="value">減少量</param>
    /// <returns></returns>
    public async UniTask ReduceAttackAbility(CardObject card, PlayerType player, AbilityTarget target, int value) {
        // 減少量に指定がない場合は1減少
        if (value <= 0) value = 1;
        // ATK減少
        await giveStatus[target](MonsterStatus.ATK, card, player, -value);
    }

    /// <summary>
    /// 手札を捨てる効果
    /// </summary>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <returns></returns>
    public async UniTask HandDestructionAbility(PlayerType player, AbilityTarget target) {
        // ターゲットが正常ではないなら処理しない
        if (handDestruction[target] == null) return;
        // 手札を捨てる
        await handDestruction[target](player);
    }

    /// <summary>
    /// モンスター破壊効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <returns></returns>
    public async UniTask DeleteMonsterAbility(CardObject card, PlayerType player, AbilityTarget target) {
        // ターゲットが正常ではないなら処理しない
        if (deleteMonster[target] == null) return;
        // モンスター破壊
        await deleteMonster[target](card, player);
    }

    /// <summary>
    /// モンスターを手札に戻す効果
    /// </summary>
    /// <param name="card">発動カード</param>
    /// <param name="player">発動プレイヤー</param>
    /// <param name="target">発動対象</param>
    /// <returns></returns>
    public async UniTask ReturnMonsterAbility(CardObject card, PlayerType player, AbilityTarget target) {
        // ターゲットが正常ではないなら処理しない
        if (returnMonster[target] == null) return;
        // モンスターを手札に戻す
        await returnMonster[target](card, player);
    }

    /// <summary>
    /// なかまよび効果(マジカルバードの特殊効果)
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public async UniTask NakamayobiAbility(PlayerType player) {
        // マジカルバードモンスターのカードID全取得
        List<int> bardIDs = new List<int>();
        foreach (var cardID in CardDataMasterUtility.GetRaceCardData(CardTheme.MagicalBard)) {
            var data = CardDataMasterUtility.GetCardMaster(cardID);
            if ((CardType)data.CardType != CardType.Monster) continue;
            bardIDs.Add(data.cardID);
        }
        // ランダムで1体を選ぶ
        int rand = UnityEngine.Random.Range(0, bardIDs.Count);
        // 出すレーンを選択
        FieldLane lane = await MenuWindowManager.Instance.EmptyLaneMenuOpen(PlayerType.Self);
        // 選ばれたモンスターのオブジェクト生成
        CardObject cardObject = CardObjectManager.instance.UseCardObject(bardIDs[rand], player);
        // サイズを0にして召喚
        cardObject.SetStatus(MonsterStatus.SIZE, 0);
        await BattleSystemManager.instance.SummonMonster(player, cardObject, lane, true);
    }

    /// <summary>
    /// レベルアップ効果(リールギアの特殊効果)
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public async UniTask LevelUPAbility(CardObject card) {
        // それぞれのIDを取得
        int reacheLv1ID = CardDataMasterUtility.GetCardNameData(REACHE_LV1_NAME).cardID;
        int reacheLv2ID = CardDataMasterUtility.GetCardNameData(REACHE_LV2_NAME).cardID;
        int reacheLv3ID = CardDataMasterUtility.GetCardNameData(REACHE_LV3_NAME).cardID;
        int isukaLv1ID = CardDataMasterUtility.GetCardNameData(ISUKA_LV1_NAME).cardID;
        int isukaLv2ID = CardDataMasterUtility.GetCardNameData(ISUKA_LV2_NAME).cardID;
        // それぞれのレベルアップ
        // リーチェレベル1
        if (card.cardID == reacheLv1ID) {
            // カードのID変更
            AreaCardManager.instance.OutGameToField(card.playerType, reacheLv1ID, card.laneID);
            // カードの情報更新
            card.Setup(card.objectID, reacheLv1ID, card.playerType);
            // イラスト設定
            card.ChangeCardImage(card.cardID);
        }
        // リーチェのレベル2
        else if (card.cardID == reacheLv2ID) {
            // カードのID変更
            AreaCardManager.instance.OutGameToField(card.playerType, reacheLv2ID, card.laneID);
            // カードの情報更新
            card.Setup(card.objectID, reacheLv2ID, card.playerType);
            // イラスト設定
            card.ChangeCardImage(card.cardID);
        }
        // イスカのレベル1
        else if (card.cardID == isukaLv1ID) {
            // カードのID変更
            AreaCardManager.instance.OutGameToField(card.playerType, isukaLv1ID, card.laneID);
            // カードの情報更新
            card.Setup(card.objectID, isukaLv1ID, card.playerType);
            // イラスト設定
            card.ChangeCardImage(card.cardID);
        }
    }
}
