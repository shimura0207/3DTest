
/*
 *  @file   GameGemManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ゲーム内通貨を管理するクラス
/// </summary>
public class GameGemManager : SystemObject {
    /// <summary>
    /// 自身への参照
    /// </summary>
    public static GameGemManager Instance { get; private set; }

    private const int INITIAL_GEM = 0;      // 初期通貨所持量
    private const int MAX_GEM = 999999;     // 通貨の最大所持量
    private int currentGem;                 // 現在所持している通貨



    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 自身をシングルトンとして登録する
        Instance = this;

        // 初期所持通貨を設定する
        currentGem = INITIAL_GEM;

        await UniTask.CompletedTask;
    }


    /// <summary>
    /// 現在の所持通貨を取得する
    /// </summary>
    /// <returns>現在の所持通貨</returns>
    public int GetGem() {
        // 現在の所持通貨を返す
        return currentGem;
    }

    /// <summary>
    /// 通貨を追加する
    /// </summary>
    /// <param name="amount">追加する通貨の量</param>
    public void AddGem(int amount) {
        // 0以下の値は追加しない
        if (amount <= 0) return;

        // 最大所持数を超えないように通貨を追加する
        currentGem = Mathf.Min(currentGem + amount, MAX_GEM);
    }

    /// <summary>
    /// 通貨を消費できるか確認する
    /// </summary>
    /// <param name="amount">消費する通貨の量</param>
    /// <returns>消費可能ならtrue</returns>
    public bool CanSpendGem(int amount) {
        // 0以下の値は消費不可とする
        if (amount <= 0) return false;

        // 所持通貨が消費量以上なら消費可能
        return currentGem >= amount;
    }

    /// <summary>
    /// 通貨を消費する
    /// </summary>
    /// <param name="amount">消費する通貨量</param>
    /// <returns>消費出来たらtrue</returns>
    public bool SpendGem(int amount) {
        // 0以下は減らせない
        if (amount <= 0) return false;

        // 通貨を消費できるかチェック、所持通貨が消費量以下なら減らせない
        if (!CanSpendGem(amount)) return false;

        // 所持通貨から指定分減らす
        currentGem -= amount;

        // 消費できたことを通知
        return true;
    }


}
