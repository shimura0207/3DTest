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

    // 現在のターンプレイヤー
    private PlayerType turnPlayer = PlayerType.None;

    // Start is called before the first frame update
    void Start() {
        instance = this;
    }

    // Update is called once per frame
    void Update() {

    }
}
