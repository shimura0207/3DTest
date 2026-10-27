/*
 * @file    BattleSystemManager.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

using static GameConst;

/// <summary>
/// バトルシステム管理クラス
/// </summary>
public class BattleSystemManager : MonoBehaviour {
    // 自身への参照
    public static BattleSystemManager instance { get; private set; } = null;
    // 先攻のプレイヤー
    private PlayerType firstPlayer = PlayerType.None;
    // 現在のターンプレイヤー
    public PlayerType turnPlayer { get; private set; } = PlayerType.None;
    // 各フェイズのリスト
    [SerializeField]
    private PhaseBase[] phaseList = null;
    // 現在のフェイズ
    public BattlePhase currentPhase { get; private set; } = BattlePhase.None;
    // 対戦の状態
    public BattleState battleState { get; private set; } = BattleState.None;
    // プレイヤーの体力
    public Dictionary<PlayerType, int> playerHP { get; private set; } = null;
    // プレイヤーのサイズ上限
    public Dictionary <PlayerType, int> playerSizeLimit { get; private set; } = null;
    // 現在のターン
    private int currentTurn = -1;
    // バチスロ移行フラグ
    private bool pachisuroGo = false;
    // ターンエンドフラグ
    private bool turnEnd = false;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        instance = this;

        playerHP = new Dictionary<PlayerType, int>();
        playerSizeLimit = new Dictionary<PlayerType, int>();
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    public async UniTask Execute() {
        // 各フェイズのターン処理
        switch (turnPlayer) {
            case PlayerType.Self:
                // 自分のターン
                await phaseList[(int)currentPhase].SelfExecute();
                break;
            case PlayerType.Opponent:
                // 相手のターン
                await phaseList[(int)currentPhase].OpponentExecute();
                break;
        }

        // パチスロフェイズへの移行
        if (pachisuroGo) {
            // メインフェイズを次のフェイズへ
            phaseList[(int)currentPhase].NextPhase();
            pachisuroGo = false;
        }
        // ターンエンド
        if (turnEnd) {
            // フェイズの片付け
            phaseList[(int)currentPhase].Teardown();
            // フェイズをエンドフェイズに
            currentPhase = BattlePhase.EndPhase;
            // フェイズの準備
            phaseList[(int)currentPhase].Setup();
            turnEnd = false;
        }

        // 次のフェイズへの移行
        if (phaseList[(int)currentPhase].nextPhase) {
            // 移行処理
            NextPhaseSetup();
        }
    }

    /// <summary>
    /// 次のフェイズへの移行
    /// </summary>
    private void NextPhaseSetup() {
        // フェイズの片付け
        phaseList[(int)currentPhase].Teardown();
        // 今回がエンドフェイズならターンを交代しスタートフェイズへ
        if (currentPhase == BattlePhase.EndPhase) {
            // ターンプレイヤー交代
            ChangeTurnPlayer();
            // スタートフェイズに戻る
            currentPhase = BattlePhase.StartPhase;
            
            Debug.Log("スタートフェイズ");
        }
        else {
            // フェイズを一つ進める
            currentPhase++;
            
            switch (currentPhase) {
                case BattlePhase.StartPhase:
                    Debug.Log("スタートフェイズ");
                    break;
                case BattlePhase.MainPhase:
                    Debug.Log("メインフェイズ");
                    break;
                case BattlePhase.PachisuroPhase:
                    Debug.Log("パチスロフェイズ");
                    break;
                case BattlePhase.AttackPhase:
                    Debug.Log("アタックフェイズ");
                    break;
                case BattlePhase.EndPhase:
                    Debug.Log("エンドフェイズ");
                    break;
            }
        }
        // フェイズの準備
        phaseList[(int)currentPhase].Setup();
    }

    /// <summary>
    /// バトル準備
    /// </summary>
    /// <param name="setUseDeckList">使用デッキリスト</param>
    /// <param name="setFirsetPlayer">先攻プレイヤー</param>
    /// <returns></returns>
    public async UniTask BattleSetup(List<int> setUseDeckList, PlayerType setFirsetPlayer) {
        // 自身の使用デッキ登録
        AreaCardManager.instance.SetDeck(PlayerType.Self, setUseDeckList);
        // デッキのシャッフル
        AreaCardManager.instance.ShuffleDeck();
        // 相手にデッキの内容を送る
        var deck = AreaCardManager.instance.DeckPack();
        CustomNetworkManager.instance.SendStringToOpponent(deck);
        // 先攻プレイヤー登録
        firstPlayer = setFirsetPlayer;
        // 現在のターンプレイヤーを先攻プレイヤーに
        turnPlayer = firstPlayer;
        // フェイズをスタートに
        currentPhase = BattlePhase.StartPhase;
        // 対戦中状態に切り替え
        battleState = BattleState.InProgress;
        // 現在のターンを1に
        currentTurn = 1;

        // 相手のデッキを受け取るまで待つ
        while (!AreaCardManager.instance.catchOpponentDeck) {
            await UniTask.Yield();
        }
        AreaCardManager.instance.SetCatchOpponentDeck(false);
        
        // 各プレイヤーの初期化
        for (PlayerType i = 0; i < PlayerType.Max; i++) {
            // HPの初期化
            playerHP[i] = PLAYER_HP;
            // サイズ上限を最小値に
            playerSizeLimit[i] = SIZE_LIMIT_MIN;
            //playerSizeLimit[i] = 10;// デバッグ用仮

            // 手札5枚用意
            for (int f = 0; f < START_HAND_COUNT; f++) {
                DrawCard(i);
            }
        }

        // ログ
        switch (turnPlayer) {
            case PlayerType.Self:
                Debug.Log("先攻：自分のターン");
                break;
            case PlayerType.Opponent:
                Debug.Log("先攻：相手のターン");
                break;
        }
    }

    /// <summary>
    /// ターンプレイヤーを交代する
    /// </summary>
    private void ChangeTurnPlayer() {
        switch (turnPlayer) {
            case PlayerType.Self:
                turnPlayer = PlayerType.Opponent;
                Debug.Log("相手のターン");
                break;
            case PlayerType.Opponent:
                turnPlayer = PlayerType.Self;
                Debug.Log("自分のターン");
                break;
        }
        // 交代したプレイヤーが先攻プレイヤーならターンを進める
        if (firstPlayer == turnPlayer) {
            currentTurn++;
        }
        // プレイヤーのサイズ上限を上げる
        if (playerSizeLimit[turnPlayer] + SIZE_LIMIT_UP >= SIZE_LIMIT_MAX) {
            currentTurn = SIZE_LIMIT_MAX;
        }
        else {
            playerSizeLimit[turnPlayer] += SIZE_LIMIT_UP;
        }
    }

    /// <summary>
    /// プレイヤーに指定のダメージを与える
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="giveDamage">与えるダメージ量</param>
    public void PlayerGiveDamage(PlayerType player, int giveDamage) {
        // ダメージ分HPを減らす
        playerHP[player] -= giveDamage;

        // HPが0以下なら勝敗がつく
        if (playerHP[player] > 0) return;

        playerHP[player] = 0;
        switch (player) {
            case PlayerType.Self:
                // 自身のHPが0なら敗北
                battleState = BattleState.Lose;
                break;
            case PlayerType.Opponent:
                // 相手のHPが0なら勝利
                battleState = BattleState.Win;
                break;
        }
    }

    /// <summary>
    /// カードのドロー(オブジェクト操作含め)
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    public void DrawCard(PlayerType player) {
        // 山札からIDを抜く
        int card = AreaCardManager.instance.DrawCard(player);
        // オブジェクトの使用化
        CardObject cardObject = CardObjectManager.instance.UseCardObject(card, player);
        // オブジェクトを手札の位置に移動
        CardObjectManager.instance.MoveToHand(player, cardObject.objectID);
    }

    /// <summary>
    /// モンスター召喚
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="card">召喚するカードのオブジェクト</param>
    /// <param name="lane">召喚するカードのレーン</param>
    public async UniTask SummonMonster(PlayerType player, CardObject card, FieldLane lane) {
        // 手札から指定されたレーンにカードを移動
        AreaCardManager.instance.HandToField(player, card.handID, (int)lane);
        // 指定されたレーンにカードを設置
        CardObjectManager.instance.MoveToField(player, card.objectID, lane);
        // 効果発動確認(今後実装予定)

        // 自分の処理なら相手に送る
        if (player != PlayerType.Self) return;
        var pack = NetworkValueConverter.Pack(NetworkUseID.playCard, card.objectID, lane);
        CustomNetworkManager.instance.SendStringToOpponent(pack);
    }

    /// <summary>
    /// スペルを唱える
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="card">唱えるカードのオブジェクト</param>
    public void CastSpell(PlayerType player, CardObject card) {
        // 効果発動(今後実装予定)

        // カードを手札から墓地に移動
        AreaCardManager.instance.HandToGrave(player, card.handID);
        // カードオブジェクトを未使用状態に
        CardObjectManager.instance.UnuseCardObject(card);
        // 手札の整列
        CardObjectManager.instance.ArrangeHand(player);

        // 自身の処理なら相手に送る
        if (player != PlayerType.Self) return;
        var pack = NetworkValueConverter.Pack(NetworkUseID.playCard, card.objectID, FieldLane.None);
        CustomNetworkManager.instance.SendStringToOpponent(pack);
    }


    /// <summary>
    /// 残りのサイズ取得
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <returns></returns>
    public int GetRemainingSize(PlayerType player) {
        var fieldObject = CardObjectManager.instance.fieldObjectList[player];
        int size = 0;
        // フィールドの合計サイズ計算
        foreach (var obj in fieldObject) {
            if (!obj.Value) continue;
            size += obj.Value.currentStatus.SIZE;
        }
        // サイズ上限から合計サイズを引いた値を返す
        return playerSizeLimit[player] - size;
    }

    /// <summary>
    /// レーン交換
    /// </summary>
    /// <param name="player"></param>
    /// <param name="laneA"></param>
    /// <param name="laneB"></param>
    public void LaneChange(PlayerType player, FieldLane laneA, FieldLane laneB) {
        // IDリストを交換
        AreaCardManager.instance.LaneChange(player, (int)laneA, (int)laneB);
        // オブジェクトを交換
        CardObjectManager.instance.LaneChange(player, laneA, laneB);

        // 自身の処理なら相手に送る
        if (player != PlayerType.Self) return;
        var pack = NetworkValueConverter.Pack(NetworkUseID.laneCange, laneA, laneB);
        CustomNetworkManager.instance.SendStringToOpponent(pack);
    }

    /// <summary>
    /// パチスロフェイズへ移行
    /// </summary>
    public void PachisuroGo() {
        if (currentPhase != BattlePhase.MainPhase) return;
        pachisuroGo = true;
    }

    /// <summary>
    /// ターンエンド
    /// </summary>
    public void TurnEnd() {
        if (currentPhase != BattlePhase.MainPhase) return;
        turnEnd = true;
    }

    /// <summary>
    /// 相手のプレイカード受け取り
    /// </summary>
    /// <param name="playCard"></param>
    public void ReceiveOpponentPlayCard(string[] playCard) {
        // 2つ目の情報がカードのオブジェクトIDなのでintに変換
        int objectID = -1;
        if (!NetworkValueConverter.TryGetInt(playCard[1], out objectID)) return;
        // 3つ目の情報がレーンIDなので一度intに変換
        int laneID = -1;
        if (!NetworkValueConverter.TryGetInt(playCard[2], out objectID)) return;

        // カード使用
        CardObject obj = CardObjectManager.instance.GetUseCardObject(objectID);
        switch (obj.cardType) {
            case CardType.Monster:
                UniTask task = SummonMonster(PlayerType.Opponent, obj, (FieldLane)laneID);
                break;
            case CardType.Spell:
                CastSpell(PlayerType.Opponent, obj);
                break;
        }
    }

    /// <summary>
    /// 相手のレーン交換受け取り
    /// </summary>
    /// <param name="changeLane"></param>
    public void ReceiveOpponentLaneChange(string[] changeLane) {
        // 2,3つ目の情報が交換するレーンなので一度intに変換
        int laneA = -1;
        if (!NetworkValueConverter.TryGetInt(changeLane[1], out laneA)) return;
        int laneB = -1;
        if (!NetworkValueConverter.TryGetInt(changeLane[2], out laneB)) return;

        // 受け取った2つのレーンを交換
        LaneChange(PlayerType.Opponent, (FieldLane)laneA, (FieldLane)laneB);
    }
}
