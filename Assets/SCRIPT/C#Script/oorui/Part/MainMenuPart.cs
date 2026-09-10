/*
 *  @file   MainMenuPart
 *  @author oorui
 */
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;

/// <summary>
/// メインメニューパート
/// </summary>
public class MainMenuPart : PartBase {

    // マッチング画面の階層パス
    private const string _MENUWINDOW_MATCHMAKINGMENU =
        "Prefab/Part/MenuWindow/MatchingMenu";

    // オプション画面の階層パス
    private const string _MENUWINDOW_OPTIONMENU =
        "Prefab/Part/MenuWindow/OptionMenu";

    // マッチングメニュー
    private MatchmakingMenu matchmakingMenu;

    // オプションメニュー
    private OptionMenu optionMenu;

    // 選択された遷移先
    private eMainMenuSelect selectMenu = eMainMenuSelect.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();

        // 各メニューを取得する
        matchmakingMenu = MenuWindowManager.instance.Get<MatchmakingMenu>(_MENUWINDOW_MATCHMAKINGMENU);

        optionMenu = MenuWindowManager.instance.Get<OptionMenu>(_MENUWINDOW_OPTIONMENU);

        // 各メニューを初期化する
        await matchmakingMenu.Initialize();
        await optionMenu.Initialize();

        // 各メニューの選択イベントを登録する
        RegisterMenuEvent();
    }

    /// <summary>
    /// メニューイベントを登録する
    /// </summary>
    private void RegisterMenuEvent() {

        // マッチングメニューが選択された時の処理を登録する
        matchmakingMenu.OnSelected += OnMatchmakingSelected;

        // オプションメニューが選択された時の処理を登録する
        optionMenu.OnSelected += OnOptionSelected;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // BGM再生

        // 選択状態を初期化する
        selectMenu = eMainMenuSelect.None;

        // マッチングメニューを表示する
        await matchmakingMenu.Open();

        // オプションメニューを表示する
        await optionMenu.Open();

        // どれかのメニューが選択されるまで待機する
        while (selectMenu == eMainMenuSelect.None) {

            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }

        // すべてのメニューを閉じる
        await CloseAllMenu();

        // 選択されたメニューに応じて遷移する
        await TransitionSelectedPart();
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnMatchmakingSelected(MenuWindowBase menu) {

        // 遷移先をマッチングに設定する
        selectMenu = eMainMenuSelect.Matchmaking;
    }

    /// <summary>
    /// オプションメニューが選択された時の処理
    /// </summary>
    private void OnOptionSelected(MenuWindowBase menu) {

        // 遷移先をオプションに設定する
        selectMenu = eMainMenuSelect.Option;
    }

    /// <summary>
    /// すべてのメニューを閉じる
    /// </summary>
    /// <returns></returns>
    private async UniTask CloseAllMenu() {

        // マッチングメニューを閉じる
        await matchmakingMenu.Close();

        // オプションメニューを閉じる
        await optionMenu.Close();
    }

    /// <summary>
    /// 選択されたメニューに対応したパートへ遷移する
    /// </summary>
    /// <returns></returns>
    private async UniTask TransitionSelectedPart() {

        // 選択されたメニューによって遷移先を変更する
        switch (selectMenu) {

            case eMainMenuSelect.Matchmaking:

                // マッチングパートへ遷移する
                await PartManager.Instance
                    .TransitionPart(eGamePart.Matchmaking);

                break;

            case eMainMenuSelect.Option:

                // オプションパートへ遷移する
                await PartManager.Instance
                    .TransitionPart(eGamePart.Option);

                break;

            case eMainMenuSelect.CardList:

                // カード一覧パートへ遷移する
                await PartManager.Instance
                    .TransitionPart(eGamePart.CardList);

                break;

            case eMainMenuSelect.Gacha:

                // ガチャパートへ遷移する
                await PartManager.Instance
                    .TransitionPart(eGamePart.Gacha);

                break;
        }
    }
}
