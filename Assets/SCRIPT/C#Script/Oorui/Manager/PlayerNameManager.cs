/*
 *  @file   PlayerNameManager
 *  @author oorui
 */

using UnityEngine;

/// <summary>
/// プレイヤー名の送受信と管理を行うクラス
/// </summary>
public static class PlayerNameManager {

    // 自分のプレイヤー名
    public static string ownName { get; private set; } = "";

    // 対戦相手のプレイヤー名
    public static string opponentName { get; private set; } = "";

    // 対戦相手の名前を受信したかどうか
    public static bool isOpponentNameReceived { get; private set; } = false;

    /// <summary>
    /// 自分のプレイヤー名を対戦相手へ送信する
    /// </summary>
    /// <param name="playerName">自分のプレイヤー名</param>
    public static void SendPlayerName(string playerName) {

        // 自分の名前を保存する
        ownName = playerName;

        // 通信管理クラスが存在しない場合は終了する
        if (CustomNetworkManager.instance == null) return;

        // 名前が空の場合は送信しない
        if (string.IsNullOrWhiteSpace(playerName)) return;

        // 通信IDとプレイヤー名をまとめる
        string message = NetworkValueConverter.Pack(
            NetworkUseID.OpponentName,
            playerName
        );

        // 対戦相手へ名前を送信する
        CustomNetworkManager.instance.SendStringToOpponent(message);
    }

    /// <summary>
    /// 対戦相手のプレイヤー名を受信する
    /// </summary>
    /// <param name="values">受信した通信データ</param>
    public static void ReceiveOpponentName(string[] values) {

        // 通信データが不正なら終了する
        if (values == null || values.Length < 2) return;

        // 対戦相手の名前を保存する
        opponentName = values[1];

        // 名前を受信済みにする
        isOpponentNameReceived = true;

        // 受信した名前をログに表示する
        Debug.Log($"対戦相手の名前：{opponentName}");
    }

    /// <summary>
    /// プレイヤー名の情報を初期化する
    /// </summary>
    public static void Reset() {

        // 自分と対戦相手の名前を初期化する
        ownName = "";
        opponentName = "";

        // 受信状態を未受信に戻す
        isOpponentNameReceived = false;
    }
}