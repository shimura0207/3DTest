/*
 *  @file   FirstPlayerManager
 *  @author oorui
 */

/// <summary>
/// 先攻プレイヤーの情報を管理するクラス
/// </summary>
public static class FirstPlayerManager {

    // 先攻プレイヤーが決定しているかどうか。
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