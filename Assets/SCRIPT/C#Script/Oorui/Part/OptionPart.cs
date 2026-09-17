/*
 *  @file   OptionPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 設定パート
/// </summary>
public class OptionPart : PartBase {

    // メインメニューに戻るメニューの階層パス
    private const string _MENUWINDOW_RETURNTOMAINMENU = "Prefab/Part/MenuWindow/ReturnToMainMenu";

    // 戻るメニュー
    private ReturnToMainMenu returnMenu;

    // 選択された遷移先
    private CardRelationMenuSelect selectMenu = CardRelationMenuSelect.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // 各メニューを取得
        returnMenu = MenuWindowManager.instance.Get<ReturnToMainMenu>(_MENUWINDOW_RETURNTOMAINMENU);
        // 各メニューを初期化
        await returnMenu.Initialize();
        await UniTask.CompletedTask;
    }

    public override async UniTask Setup() {
        await base.Setup();
        Debug.Log("設定画面表示中");
        // 選択状態を初期化する
        selectMenu = CardRelationMenuSelect.None;

        // 念のため以前登録されているイベントを解除する
        UnRegisterMenuEvent();
        // メニューイベントを登録
        RegisterMenuEvent();
    }

    /// <summary>
    /// メニューイベントを登録する
    /// </summary>
    private void RegisterMenuEvent() {
        // メインメニューに戻るが選択された時の処理を登録
        returnMenu.OnSelected += OnReturnMenuSelected;
    }
    /// <summary>
    /// メニューイベントを解除する
    /// </summary>
    private void UnRegisterMenuEvent() {
        // メインメニューの選択イベントを解除する
        returnMenu.OnSelected -= OnReturnMenuSelected;
    }
    /// <summary>
    /// メインメニューが選択された時の処理
    /// </summary>
    /// <param name="menu"></param>
    private void OnReturnMenuSelected(MenuWindowBase menu) {
        // 遷移先をメインメニューに設定する
        selectMenu = CardRelationMenuSelect.MainMenu;
    }
    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // BGM再生

        // 選択状態を初期化する
        selectMenu = CardRelationMenuSelect.None;

        // 各メニュー表示
        await returnMenu.Open();

        while (selectMenu == CardRelationMenuSelect.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }

        // すべてのメニューを閉じる
        await CloseAllMenu();

        // 選択されたメニューに応じて遷移する
        await TransitionSelectedPart();
    }

    /// <summary>
    /// すべてのメニューを閉じる
    /// </summary>
    /// <returns></returns>
    private async UniTask CloseAllMenu() {

        // 各メニューを閉じる
        await returnMenu.Close();
    }

    private async UniTask TransitionSelectedPart() {
        // 選択されたメニューによって遷移先を変更する
        switch (selectMenu) {
            case CardRelationMenuSelect.MainMenu:
                // メインメニューパートに遷移
                await PartManager.Instance.TransitionPart(GamePart.MainMenu);
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
        selectMenu = CardRelationMenuSelect.None;
    }
}
