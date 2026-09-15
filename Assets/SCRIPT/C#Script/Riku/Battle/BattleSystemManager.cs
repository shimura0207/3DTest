/*
 * @file    BattleSystemManager.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start() {
        instance = this;
    }

    // Update is called once per frame
    void Update() {

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
}
