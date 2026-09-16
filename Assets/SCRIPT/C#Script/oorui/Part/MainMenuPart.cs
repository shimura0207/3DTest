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
    private const string _MENUWINDOW_MATCHMAKINGMENU = "Prefab/Part/MenuWindow/MatchingMenu";
    // オプション画面の階層パス
    private const string _MENUWINDOW_OPTIONMENU = "Prefab/Part/MenuWindow/OptionMenu";
    // ガチャ画面の階層パス
    private const string _MENUWINDOW_GATHAMENU = "Prefab/Part/MenuWindow/GachaMenu";
    // カード関連画面の階層パス
    private const string _MENUWINDOW_CARTMENU = "Prefab/Part/MenuWindow/CardMenu";

    // マッチングメニュー
    private MatchmakingMenu matchmakingMenu;

    // オプションメニュー
    private OptionMenu optionMenu;

    // ガチャメニュー
    private GathaMenu gachaMenu;

    // カード関連メニュー
    private CardRelationMenu cardRelation;

    // 選択された遷移先
    private MainMenuSelect selectMenu = MainMenuSelect.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();

        // 各メニューを取得する
        matchmakingMenu = MenuWindowManager.instance.Get<MatchmakingMenu>(_MENUWINDOW_MATCHMAKINGMENU);
        optionMenu = MenuWindowManager.instance.Get<OptionMenu>(_MENUWINDOW_OPTIONMENU);
        gachaMenu = MenuWindowManager.instance.Get<GathaMenu>(_MENUWINDOW_GATHAMENU);
        cardRelation = MenuWindowManager.instance.Get<CardRelationMenu>(_MENUWINDOW_CARTMENU);

        // 各メニューを初期化する
        await matchmakingMenu.Initialize();
        await optionMenu.Initialize();
        await gachaMenu.Initialize();
        await cardRelation.Initialize();

    }

    /// <summary>
    /// 使用前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();

        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;
        // 念のため以前登録されているイベントを解除する
        UnRegisterMenuEvent();
        // メニューイベントを登録
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

        // ガチャメニューが選択された時の処理を登録する
        gachaMenu.OnSelected += OnGachaSelected;

        // カード関連メニューが選択された時の処理を登録する
        cardRelation.OnSelected += OnCartListSelected;
    }

    /// <summary>
    /// メニューイベントを解除する
    /// </summary>
    private void UnRegisterMenuEvent() {
        // マッチングメニューの選択イベントを解除する
        matchmakingMenu.OnSelected -= OnMatchmakingSelected;
        // オプションメニューの選択イベントを解除する
        optionMenu.OnSelected -= OnOptionSelected;
        // ガチャメニューの選択イベントを解除する
        gachaMenu.OnSelected -= OnGachaSelected;
        // カード関連メニューの選択イベントを解除する
        cardRelation.OnSelected -= OnCartListSelected;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // BGM再生

        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;

        // マッチングメニューを表示する
        await matchmakingMenu.Open();

        // オプションメニューを表示する
        await optionMenu.Open();

        // ガチャメニューを表示する
        await gachaMenu.Open();

        // カード関連メニューを表示する
        await cardRelation.Open();

        // どれかのメニューが選択されるまで待機する
        while (selectMenu == MainMenuSelect.None) {

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
        selectMenu = MainMenuSelect.Matchmaking;
    }

    /// <summary>
    /// オプションメニューが選択された時の処理
    /// </summary>
    private void OnOptionSelected(MenuWindowBase menu) {

        // 遷移先をオプションに設定する
        selectMenu = MainMenuSelect.Option;
    }

    /// <summary>
    /// ガチャメニューが選択された時の処理
    /// </summary>
    /// <param name="menu"></param>
    private void OnGachaSelected(MenuWindowBase menu) {
        // 遷移先オプションに設定する
        selectMenu = MainMenuSelect.Gacha;
    }

    /// <summary>
    /// カード関連メニューが選択された時の処理
    /// </summary>
    /// <param name="menu"></param>
    private void OnCartListSelected(MenuWindowBase menu) {
        // 遷移先オプションに設定する
        selectMenu = MainMenuSelect.CardRelation;
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

        // ガチャメニューを閉じる
        await gachaMenu.Close();

        // カード関連メニューを閉じる
        await cardRelation.Close();
    }

    /// <summary>
    /// 選択されたメニューに対応したパートへ遷移する
    /// </summary>
    /// <returns></returns>
    private async UniTask TransitionSelectedPart() {

        // 選択されたメニューによって遷移先を変更する
        switch (selectMenu) {

            case MainMenuSelect.Matchmaking:

                // マッチングパートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Matchmaking);

                break;

            case MainMenuSelect.Option:

                // オプションパートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Option);

                break;

            case MainMenuSelect.CardRelation:

                // カード一覧パートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.CardRelation);

                break;

            case MainMenuSelect.Gacha:

                // ガチャパートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Gacha);

                break;
        }
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
        // 登録したイベントを解除
        UnRegisterMenuEvent();
        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;
    }
}
