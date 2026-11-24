/*
 * @file    BattleSettingsMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;

using System.Collections.Generic;

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
    public async UniTask Open(int id) {

        await base.Open();

        // 先攻情報の確認
        if (!FirstPlayerManager.isFirstPlayerDecided) {

            UnityEngine.Debug.LogError(
                "[BattleSettingsMenu] 先攻情報が未決定です。"
            );

            await Close();
            return;
        }

        PlayerType firstPlayer =
            FirstPlayerManager.firstPlayer;

        if (firstPlayer != PlayerType.Self &&
            firstPlayer != PlayerType.Opponent) {

            UnityEngine.Debug.LogError(
                $"[BattleSettingsMenu] 不正な先攻：{firstPlayer}"
            );

            await Close();
            return;
        }

        // デッキ取得
        List<int> useDeckList = GetUseDeckList(id);

        // デッキが存在するか確認
        if (useDeckList == null || useDeckList.Count == 0) {

            UnityEngine.Debug.LogError(
                "[BattleSettingsMenu] デッキが存在しません。"
            );

            await Close();
            return;
        }

        UnityEngine.Debug.Log(
            $"[BattleSettingsMenu] BattleSetup開始" +
            $" 先攻={firstPlayer}" +
            $" デッキ枚数={useDeckList.Count}"
        );

        // バトル初期化
        await BattleSystemManager.instance.BattleSetup(
            useDeckList,
            firstPlayer
        );

        UnityEngine.Debug.Log(
            "[BattleSettingsMenu] BattleSetup完了"
        );

        await Close();
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
    private List<int> GetUseDeckList(int id) {
        // セーブデータから指定されたデッキのカードID一覧を取得する
        List<int> deck = SaveManager.Instance.GetDeckCardIDs(id);

        // デッキが存在しない場合
        if (deck == null) {
            return new List<int>();
        }

        // カードID一覧を返す
        return deck;
    }

    private List<int> GetUseTutorialDeckList() {
        // レンタルデッキを取得
        List<int> rental = DeckDataMasterUtility.GetDeckMaster(1);

        // レンタルデッキが取得できなければ処理を終了する
        if (rental == null) return new List<int>();

        return rental;
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
