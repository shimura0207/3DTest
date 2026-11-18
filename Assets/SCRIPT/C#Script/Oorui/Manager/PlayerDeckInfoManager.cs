/*
 *  @file   PlayerDeckInfoManager
 *  @author oorui
 */

using System;
using UnityEngine;

/// <summary>
/// 自分と対戦相手のデッキ情報を管理するクラス
/// </summary>
public static class PlayerDeckInfoManager {

    // 自分のKEYカードID
    public static int ownKeyCardID { get; private set; } = -1;

    // 対戦相手のKEYカードID
    public static int opponentKeyCardID { get; private set; } = -1;

    // 自分のKEYカードのテーマ
    public static CardTheme ownTheme { get; private set; } = CardTheme.None;

    // 対戦相手のKEYカードのテーマ
    public static CardTheme opponentTheme { get; private set; } = CardTheme.None;

    // 対戦相手のKEYカードIDを受信したかどうか
    public static bool isOpponentKeyCardReceived { get; private set; } = false;

    /// <summary>
    /// 自分のKEYカードIDを設定して対戦相手へ送信する
    /// </summary>
    /// <param name="keyCardID">自分のKEYカードID</param>
    public static void SendOwnKeyCardID(int keyCardID) {

        // 自分のKEYカードIDを保存する
        ownKeyCardID = keyCardID;

        // KEYカードのテーマを取得する
        ownTheme = GetCardTheme(keyCardID);

        // 通信管理クラスが存在しない場合は終了する
        if (CustomNetworkManager.instance == null) return;

        // 有効なKEYカードIDでない場合は送信しない
        if (keyCardID < 0) return;

        // 通信IDとKEYカードIDを文字列にまとめる
        string message = NetworkValueConverter.Pack(
            NetworkUseID.OpponentKeyCardID,
            keyCardID.ToString()
        );

        // 対戦相手へKEYカードIDを送信する
        CustomNetworkManager.instance.SendStringToOpponent(message);
    }

    /// <summary>
    /// 対戦相手のKEYカードIDを受信する
    /// </summary>
    /// <param name="values">受信した通信データ</param>
    public static void ReceiveOpponentKeyCardID(string[] values) {

        // 通信データが不正なら終了する
        if (values == null || values.Length < 2) return;

        // 受信したKEYカードIDを数値に変換する
        if (!int.TryParse(values[1], out int keyCardID)) return;

        // 不正なKEYカードIDなら終了する
        if (keyCardID < 0) return;

        // 対戦相手のKEYカードIDを保存する
        opponentKeyCardID = keyCardID;

        // 対戦相手のKEYカードのテーマを取得する
        opponentTheme = GetCardTheme(keyCardID);

        // 受信済みにする
        isOpponentKeyCardReceived = true;
    }

    /// <summary>
    /// カードIDからテーマを取得する
    /// </summary>
    /// <param name="cardID">テーマを取得するカードID</param>
    /// <returns>カードのテーマ</returns>
    private static CardTheme GetCardTheme(int cardID) {

        // 無効なカードIDの場合はNoneを返す
        if (cardID < 0) return CardTheme.None;

        // カードマスターデータを取得する
        var cardData = CardDataMasterUtility.GetCardMaster(cardID);

        // カードデータが存在しない場合はNoneを返す
        if (cardData == null) return CardTheme.None;

        // マスターデータのThemeIDをCardThemeに変換して返す
        return (CardTheme)cardData.themeID;
    }

    /// <summary>
    /// デッキ情報を初期化する
    /// </summary>
    public static void Reset() {

        // KEYカードIDを初期化する
        ownKeyCardID = -1;
        opponentKeyCardID = -1;

        // テーマを初期化する
        ownTheme = CardTheme.None;
        opponentTheme = CardTheme.None;

        // 受信状態を未受信に戻す
        isOpponentKeyCardReceived = false;
    }
}