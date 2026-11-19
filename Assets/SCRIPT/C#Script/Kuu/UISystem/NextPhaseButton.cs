/*
 * @file    NextPhaseButton.cs
 * @autor   Kuu
 */
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class NextPhaseButton : MonoBehaviour
{
    // エンドフェイズ移行ボタン
    [SerializeField] private Image endButton = null;

    // パチスロフェイズ移行ボタン
    [SerializeField] private Image pachisuroGoButton = null;

    // ターンエンド画像
    [SerializeField] private Sprite turnEnd = null;

    // エネミーターン画像
    [SerializeField] private Sprite enemyTurn = null;

    // PACHISUROGO!!画像
    [SerializeField] private Sprite pachisurogo = null;

    // PACHISUROGO!!モノクロ画像
    [SerializeField] private Sprite pachisurogoMono = null;

    // パチスロフェイズ移行確認メッセージ
    private const string PACHISURO_CONFIRM = "パチスロを回しますか？";
    // ターンエンド移行確認メッセージ
    private const string TURNEND_CONFIRM = "ターンを終了しますか？";

    private void Update() {
        // 現在のターンプレイヤーによってターンエンドボタンの画像を変える
        if (BattleSystemManager.instance.turnPlayer == PlayerType.Self) {
            endButton.sprite = turnEnd;
        } else if (BattleSystemManager.instance.turnPlayer == PlayerType.Opponent) {
            endButton.sprite = enemyTurn;
        }

        // 現在のターンプレイヤーによってPachisuroGo!!ボタンの画像を変える
        if (BattleSystemManager.instance.turnPlayer == PlayerType.Self) {
            pachisuroGoButton.sprite = pachisurogo;
        }
        else if (BattleSystemManager.instance.turnPlayer == PlayerType.Opponent) {
            pachisuroGoButton.sprite = pachisurogoMono;
        }
    }

    /// <summary>
    /// パチスロフェイズ移行処理呼び出し
    /// </summary>
    public void OnClickPachisuroGo() {
        if (BattleSystemManager.instance.pachisuroGoActive) {
            UniTask task = PachisuroGo();
            AudioManager.instance.PlaySE("Button");
        }
    }

    /// <summary>
    /// エンドフェイズ移行処理呼び出し
    /// </summary>
    public void OnClickTurnEnd() {
        if (BattleSystemManager.instance.turnEndActive) {
            UniTask task = TurnEnd();
            AudioManager.instance.PlaySE("Button");
        }
    }

    /// <summary>
    /// パチスロフェイズ移行処理
    /// </summary>
    /// <returns></returns>
    public async UniTask PachisuroGo() {
        // 確認ウィンドウ表示
        bool confirmation = await MenuWindowManager.Instance.Confirm(PACHISURO_CONFIRM);

        // "はい"ならパチスロフェイズ
        if (confirmation) {
            BattleSystemManager.instance.PachisuroGo();

            // 相手に送信
            var pack = NetworkValueConverter.Pack(NetworkUseID.PachisuroGoButton);
            CustomNetworkManager.instance.SendStringToOpponent(pack);
            return;
        }

        // "いいえ"なら戻る
        return;
    }

    /// <summary>
    /// エンドフェイズ移行処理
    /// </summary>
    /// <returns></returns>
    public async UniTask TurnEnd() {
        // 確認ウィンドウ表示
        bool confirmation = await MenuWindowManager.Instance.Confirm(TURNEND_CONFIRM);

        // "はい"ならターンエンドフェイズ
        if (confirmation) {
            BattleSystemManager.instance.TurnEnd();

            // 相手に送信
            var pack = NetworkValueConverter.Pack(NetworkUseID.TurnEndButton);
            CustomNetworkManager.instance.SendStringToOpponent(pack);
            return;
        }

        // "いいえ"なら戻る
        return;
    }
}
