/*
 * @file    EndGameMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
/// <summary>
/// リザルトで表示するメニューウィンドウ
/// </summary>
public class EndGameMenu : MenuWindowBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    /// <summary>
    /// リザルトウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // 基底側でリザルトメニューウィンドウを表示
        await base.Open();
    }

    /// <summary>
    /// リザルトウィンドウが非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
    }


    /// <summary>
    /// 対戦結果を設定する
    /// </summary>
    /// <param name="result">対戦結果</param>
    public void SetBattleResult(BattleState result) {
        // 対戦結果に応じて表示内容を切り替える
        switch (result) {
            case BattleState.Win:
                // 勝利画像表示
                Debug.Log("Win");
                break;

            case BattleState.Lose:
                // 敗北画像表示
                Debug.Log("Lose");
                break;
        }
    }
}
