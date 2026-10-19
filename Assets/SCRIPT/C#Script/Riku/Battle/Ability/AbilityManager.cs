/*
 * @file    AbilityManager.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カード効果管理クラス
/// </summary>
public class AbilityManager : MonoBehaviour {
    // 自身への参照
    public static AbilityManager instance { get; private set; } = null;

    // 発動待機効果
    private Queue<Func<UniTask>> abilityQueue = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        instance = this;

        abilityQueue = new Queue<Func<UniTask>>();
    }

    /// <summary>
    /// 効果をキューに積む
    /// </summary>
    /// <param name="setFunc">発動効果</param>
    public void SetAblityQueue(Func<UniTask> setFunc) {
        abilityQueue.Enqueue(setFunc);
    }


}
