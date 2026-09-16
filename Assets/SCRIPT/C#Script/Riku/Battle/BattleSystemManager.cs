/*
 * @file    BattleSystemManager.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start() {
        instance = this;

        // リスト生成
        int phaseMax = (int)BattlePhase.Max;
        phaseList = new PhaseBase[phaseMax];
    }

    // Update is called once per frame
    void Update() {

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
}
