/*
 *  @file   FirstPlayerManager
 *  @author oorui
 */

using UnityEngine;

/// <summary>
/// 先攻プレイヤーの情報を管理するクラス
/// </summary>
public static class FirstPlayerManager {

    /// 先攻プレイヤーが決定しているかどうか。
    public static bool isFirstPlayerDecided { get; private set; } = false;

    // 決定した先攻プレイヤーを保持する。
    public static PlayerType firstPlayer { get; private set; } = PlayerType.None;

    /// <summary>
    /// 先攻プレイヤーを設定する。
    /// </summary>
    /// <param name="firstPlayer">先攻プレイヤー</param>
    public static void SetFirstPlayer(PlayerType firstPlayer) {
        // 不正な値や未決定の値は登録しない。
        if (firstPlayer != PlayerType.Self &&
            firstPlayer != PlayerType.Opponent) {
            return;
        }

        // 先攻プレイヤーを保存する。
        FirstPlayerManager.firstPlayer = firstPlayer;

        // 決定済みとして記録する。
        isFirstPlayerDecided = true;
    }

    /// <summary>
    /// ホストから受信した先攻情報を保存する。
    /// 通信形式：firstPlayer,ホストが先攻かどうか
    /// </summary>
    /// <param name="values">受信した通信データ</param>
    public static void ReceiveFirstPlayer(string[] values) {
        // 通信データがnull、または項目数が不正なら処理を終了する。
        if (values == null || values.Length != 2) return;

        // ホストが先攻かどうかをboolに変換する。
        if (!bool.TryParse(values[1], out bool hostIsFirst)) return;

        // クライアント視点で先攻プレイヤーを決定する。
        PlayerType clientFirstPlayer = hostIsFirst ? PlayerType.Opponent : PlayerType.Self;

        // クライアント側に先攻情報を保存する。
        SetFirstPlayer(clientFirstPlayer);

        // クライアント視点で先攻が誰なのかをログに出力する。
        Debug.Log($"[先攻決定][クライアント] 先攻プレイヤー：{clientFirstPlayer}");
    }

    /// <summary>
    /// 先攻情報を初期状態に戻す。
    /// 新しい対戦を開始するときなどに使用する。
    /// </summary>
    public static void Reset() {
        // 先攻情報を未決定に戻す。
        firstPlayer = PlayerType.None;

        // 決定済みフラグを解除する。
        isFirstPlayerDecided = false;
    }
}