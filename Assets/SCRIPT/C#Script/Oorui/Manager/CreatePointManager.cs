/*
 *  @file   CreatePointManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 生成ポイントを管理するクラス
/// </summary>

public class CreatePointManager : SystemObject {
    /// <summary>
    /// 自身への参照
    /// </summary>
    public static CreatePointManager Instance { get; private set; }

    private const int INITIAL_POINT = 0;    // 初期生成ポイント所持量
    private const int MAX_POINT = 999999;   // ポイントの最大所持量
    private int currentPoint;               // 現在処理しているポイント

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 自身をシングルトンとして登録する
        Instance = this;

        // 初期所持ポイントを設定
        currentPoint = INITIAL_POINT;

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 現在の所持ポイントを取得
    /// </summary>
    /// <returns></returns>
    public int GetPoint() {
        return currentPoint;
    }

    /// <summary>
    /// ポイントを追加する
    /// </summary>
    /// <param name="amount">追加するポイント量</param>
    public void AddPoint(int amount) {
        // 0以下の値は追加しない
        if (amount <= 0) return;

        // 最大数を超えないようにポイントを追加する
        currentPoint = Mathf.Min(currentPoint + amount, MAX_POINT);
    }

    /// <summary>
    /// ポイントを消費できるか確認する
    /// </summary>
    /// <param name="amount">消費するポイントの量</param>
    /// <returns>消費可能ならtrue</returns>
    public bool CanSpendPoint(int amount) {
        // 0以下の値は消費不可とする
        if (amount <= 0) return false;

        // 所持ポイントが消費量以上なら消費可能
        return currentPoint >= amount;
    }

    /// <summary>
    /// ポイントを消費する
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool SpendPoint(int amount) {
        // 0以下は減らせない
        if (amount <= 0) return false;

        // ポイントを消費できるかチェック、所持ポイントが消費量以下なら減らせない
        if (!CanSpendPoint(amount)) return false;

        // 所持ポイントから指定分減らす
        currentPoint -= amount;
        // 消費できたことを通知
        return true;
    }

}
