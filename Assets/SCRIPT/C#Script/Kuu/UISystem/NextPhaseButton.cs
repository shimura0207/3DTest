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
    public void OnClickPACHISUROGO() {
        UniTask task = PACHISUROGO();
    }

    /// <summary>
    /// エンドフェイズ移行処理呼び出し
    /// </summary>
    public void OnClickTURNEND() {
        UniTask task = TURNEND();
    }

    /// <summary>
    /// パチスロフェイズ移行処理
    /// </summary>
    /// <returns></returns>
    public async UniTask PACHISUROGO() {
        // 確認ウィンドウ表示
        bool confirmation = await MenuWindowManager.Instance.Confirm(PACHISURO_CONFIRM);

        // "はい"ならパチスロフェイズ
        if (confirmation) {
            BattleSystemManager.instance.PachisuroGo();

            return;
        }

        // "いいえ"なら戻る
        return;
    }

    /// <summary>
    /// エンドフェイズ移行処理
    /// </summary>
    /// <returns></returns>
    public async UniTask TURNEND() {
        // 確認ウィンドウ表示
        bool confirmation = await MenuWindowManager.Instance.Confirm(TURNEND_CONFIRM);

        // "はい"ならターンエンドフェイズ
        if (confirmation) {
            BattleSystemManager.instance.TurnEnd();

            return;
        }

        // "いいえ"なら戻る
        return;
    }
}
