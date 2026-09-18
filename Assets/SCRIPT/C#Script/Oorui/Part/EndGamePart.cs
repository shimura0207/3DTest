/*
 *  @file   EndGamePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;

/// <summary>
/// リザルト画面
/// </summary>
public class EndGamePart : PartBase {
    // 自分視点の対戦結果を保持する
    private BattleState battleResult = BattleState.None;

    // リザルト画面の階層パス
    private const string _MENUWINDOW_ENDGAME = "Prefab/Part/MenuWindow/EndGameMenu";

    // リザルトメニュー
    private EndGameMenu endGame;

    // 選択された遷移先
    private MainMenuSelect selectMenu = MainMenuSelect.EndGame;

    public override async UniTask Initialize() {
        // 基底側の処理を呼ぶ
        await base.Initialize();
        // リザルトメニューを取得
        endGame = MenuWindowManager.instance.Get<EndGameMenu>(_MENUWINDOW_ENDGAME);
        // リザルトメニューを初期化
        await endGame.Initialize();

        // リザルトメニューの選択イベントを登録する
        RegisterMenuEvent();
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        // 基底側の処理を呼ぶ
        await base.Setup();
    }

    /// <summary>
    /// メニューイベントを登録する
    /// </summary>
    private void RegisterMenuEvent() {
        // タイトルメニューが選択された時の処理を登録
        endGame.OnSelected += OnEndGameSelected;
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnEndGameSelected(MenuWindowBase menu) {

        // 遷移先をタイトルに設定する
        selectMenu = MainMenuSelect.EndGame;
    }

    public override async UniTask Execute() {
        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;

        // 保存した対戦結果をリザルトメニューに設定する
        endGame.SetBattleResult(battleResult);

        // リザルトメニューを表示する
        await endGame.Open();

        // メニューが選択されるまで待機する
        while (selectMenu == MainMenuSelect.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }

        // リザルトメニューを閉じる
        await endGame.Close();

        // メインメニュー画面に遷移する
        await PartManager.Instance.TransitionPart(GamePart.MainMenu);
    }

    /// <summary>
    /// 対戦結果を設定する
    /// </summary>
    /// <param name="result">自分視点の対戦結果</param>
    public void SetBattleResult(BattleState result) {
        // 受け取った対戦結果を保存する
        battleResult = result;
    }
}
