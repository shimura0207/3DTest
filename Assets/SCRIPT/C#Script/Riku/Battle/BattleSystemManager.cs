/*
 * @file    BattleSystemManager.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

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
    private PlayerType turnPlayer = PlayerType.None;
    // 各フェイズのリスト
    [SerializeField]
    private PhaseBase[] phaseList = null;
    // 現在のフェイズ
    private BattlePhase currentPhase = BattlePhase.None;
    // 対戦の状態
    public BattleState battleState { get; private set; } = BattleState.None;
    // プレイヤーの体力
    public Dictionary<PlayerType, int> playerHP { get; private set; } = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        instance = this;

        // リスト生成
        int phaseMax = (int)BattlePhase.Max;
        phaseList = new PhaseBase[phaseMax];
        playerHP = new Dictionary<PlayerType, int>();
        // 対戦中に切り替え
        battleState = BattleState.InProgress;
    }

    /// <summary>
    /// フェイズの実行
    /// </summary>
    public void PhaseExecute() {
        // 各フェイズのターン処理
        switch (turnPlayer) {
            case PlayerType.Self:
                // 自分のターン
                phaseList[(int)currentPhase].SelfExecute();
                break;
            case PlayerType.Opponent:
                // 相手のターン
                phaseList[(int)currentPhase].OpponentExecute();
                break;
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
        }
        else {
            // フェイズを一つ進める
            currentPhase++;
        }
    }

    /// <summary>
    /// バトル準備
    /// </summary>
    /// <param name="setUseDeckList">使用デッキリスト</param>
    /// <param name="setFirsetPlayer">先攻プレイヤー</param>
    public void BattleSetup(List<int> setUseDeckList, PlayerType setFirsetPlayer) {
        // 自身の使用デッキ登録
        AreaCardManager.instance.SetDeck(setUseDeckList);
        // 先攻プレイヤー登録
        firstPlayer = setFirsetPlayer;
        // 現在のターンプレイヤーを先攻プレイヤーに
        turnPlayer = firstPlayer;
        // HPの初期化
        for (PlayerType i = 0; i < PlayerType.Max; i++) {
            playerHP[i] = PLAYER_HP;
        }
    }

    /// <summary>
    /// ターンプレイヤーを交代する
    /// </summary>
    private void ChangeTurnPlayer() {
        switch (turnPlayer) {
            case PlayerType.Self:
                turnPlayer = PlayerType.Opponent;
                break;
            case PlayerType.Opponent:
                turnPlayer = PlayerType.Self;
                break;
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
}
