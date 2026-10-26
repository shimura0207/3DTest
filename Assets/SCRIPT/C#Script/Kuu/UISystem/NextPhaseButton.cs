/*
 * @file    NextPhaseButton.cs
 * @autor   Kuu
 */
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NextPhaseButton : MonoBehaviour
{
    // パチスロフェイズ移行確認メッセージ
    private const string PACHISURO_CONFIRM = "パチスロを回しますか？";
    // ターンエンド移行確認メッセージ
    private const string TURNEND_CONFIRM = "ターンを終了しますか？";

    /// <summary>
    /// パチスロフェイズ移行処理呼び出し
    /// </summary>
    public void OnClickPachisuroGo() {
        UniTask task = PachisuroGo();
    }

    /// <summary>
    /// エンドフェイズ移行処理呼び出し
    /// </summary>
    public void OnClickTurnEnd() {
        UniTask task = TurnEnd();
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
