/*
 *  @file   CardRelationPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カード関連の処理をするパート
/// </summary>
public class CardRelationPart : PartBase {

    // カード一覧画面の階層パス
    private const string _MENUWINDOW_CARDHAVEMENU = "Prefab/Part/MenuWindow/CardHaveListMenu";
    // デッキ編成画面の階層パス
    private const string _MENUWINDOW_DECKBUILDINGMENU = "Prefab/Part/MenuWindow/DeckBuildingMenu";
    // メインメニューに戻るメニューの階層パス
    private const string _MENUWINDOW_RETURNTOMAINMENU = "Prefab/Part/MenuWindow/ReturnToMainMenu";


    // カード一覧メニュー
    private CardHaveListMenu cardList;

    // デッキ構築メニュー
    private DeckBuildingMenu deckBuild;

    // 戻るメニュー
    private ReturnToMainMenu returnMenu;

    // 選択された遷移先
    private CardRelationMenuSelect selectMenu = CardRelationMenuSelect.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 基底側の処理を行う
        await base.Initialize();

        // 各メニューを取得
        cardList = MenuWindowManager.instance.Get<CardHaveListMenu>(_MENUWINDOW_CARDHAVEMENU);
        deckBuild = MenuWindowManager.instance.Get<DeckBuildingMenu>(_MENUWINDOW_DECKBUILDINGMENU);
        returnMenu = MenuWindowManager.instance.Get<ReturnToMainMenu>(_MENUWINDOW_RETURNTOMAINMENU);
        // 各メニューを初期化
        await cardList.Initialize();
        await deckBuild.Initialize();
        await returnMenu.Initialize();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 使用前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();
        Debug.Log("カード関連画面を表示中");

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

        // カード一覧メニューが選択された時の処理を登録する
        cardList.OnSelected += OnCardListSelected;

        // デッキ構築メニューが選択された時の処理を登録する
        deckBuild.OnSelected += OnDeckBuildSelected;

        // メインメニューに戻るが選択された時の処理を登録
        returnMenu.OnSelected += OnReturnMenuSelected;
    }

    /// <summary>
    /// メニューイベントを解除する
    /// </summary>
    private void UnRegisterMenuEvent() {
        // カード一覧メニューの選択イベントを解除する
        cardList.OnSelected -= OnCardListSelected;

        // デッキ構築メニューの選択イベントを解除する
        deckBuild.OnSelected -= OnDeckBuildSelected;

        // メインメニューの選択イベントを解除する
        returnMenu.OnSelected -= OnReturnMenuSelected;
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnCardListSelected(MenuWindowBase menu) {

        // 遷移先をマッチングに設定する
        selectMenu = CardRelationMenuSelect.CardList;
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnDeckBuildSelected(MenuWindowBase menu) {

        // 遷移先をマッチングに設定する
        selectMenu = CardRelationMenuSelect.DeckBuilding;
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
        await cardList.Open();
        await deckBuild.Open();
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
        await cardList.Close();
        await deckBuild.Close();
        await returnMenu.Close();
    }

    private async UniTask TransitionSelectedPart() {
        // 選択されたメニューによって遷移先を変更する
        switch (selectMenu) {
            case CardRelationMenuSelect.CardList:
                // カード一覧パートに遷移
                await PartManager.Instance.TransitionPart(GamePart.CardList);
                break;
            case CardRelationMenuSelect.DeckBuilding:
                // デッキ構築パートに遷移
                await PartManager.Instance.TransitionPart(GamePart.DeckBuild);
                break;
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
