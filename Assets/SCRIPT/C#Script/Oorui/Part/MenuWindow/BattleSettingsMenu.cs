/*
 * @file    BattleSettingsMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

/// <summary>
/// バトル開始前の準備を行う
/// </summary>
public class BattleSettingsMenu : MenuWindowBase {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // 基底側でメニューウィンドウを表示
        await base.Open();
        // 先行プレイヤー
        PlayerType firstPlayer = DecideFirstPlayer();

        // バトルで使用するデッキを取得する
        List<int> useDeckList = GetUseDeckList();

        // バトル開始前の準備をする
        BattleSystemManager.instance.BattleSetup(useDeckList,firstPlayer);

        // メニューを閉じる
        await Close();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 先攻プレイヤーを決定する
    /// </summary>
    /// <returns></returns>
    private PlayerType DecideFirstPlayer() {
        // ランダムで決める
        int randomValue = UnityEngine.Random.Range(0, 2);

        // 0の場合は自分を先攻にする
        if (randomValue == 0) {
            return PlayerType.Self;
        }

        // 1の場合は相手を先攻にする
        return PlayerType.Opponent;

    }

    /// <summary>
    /// デッキを登録
    /// </summary>
    /// <returns></returns>
    private List<int> GetUseDeckList() {
        return new List<int>() {
            1, 2, 3, 4, 5, 6, 7
        };
    }

    /// <summary>
    /// ウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        await base.Close();
        await UniTask.CompletedTask;
    }
}
