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


    // 選択された遷移先
    private CardRelationMenuSelect selectMenu = CardRelationMenuSelect.None;
    // メインメニュー復帰ボタン
    [SerializeField] private Button returnMenu;
    // デッキ編成ボタン
    [SerializeField] private Button deckBuild;
    // カード一覧ボタン
    [SerializeField] private Button cardList;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 基底側の処理を行う
        await base.Initialize();
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
        // イベント登録
        RegisterMenuEvent();
    }

    /// <summary>
    /// メニューイベントを登録する
    /// </summary>
    private void RegisterMenuEvent() {

        // カード一覧メニューが選択された時の処理を登録する
        cardList.onClick.AddListener(OnCardListSelected);

        // デッキ構築メニューが選択された時の処理を登録する
        deckBuild.onClick.AddListener(OnDeckBuildSelected);

        // メインメニューに戻るが選択された時の処理を登録
        returnMenu.onClick.AddListener(OnReturnMenuSelected);
    }
    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnCardListSelected() {

        // 遷移先をマッチングに設定する
        selectMenu = CardRelationMenuSelect.CardList;
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnDeckBuildSelected() {

        // 遷移先をマッチングに設定する
        selectMenu = CardRelationMenuSelect.DeckBuilding;
    }

    /// <summary>
    /// メインメニューが選択された時の処理
    /// </summary>
    /// <param name="menu"></param>
    private void OnReturnMenuSelected() {
        // 遷移先をメインメニューに設定する
        selectMenu = CardRelationMenuSelect.MainMenu;
    }


    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        while (true) {
            // ボタンが押されるまで待機
            await UniTask.WaitUntil(() => selectMenu != CardRelationMenuSelect.None);

            // 選択されたボタンによって処理を分岐
            switch (selectMenu) {
                case CardRelationMenuSelect.CardList:
                    await PartManager.Instance.TransitionPart(GamePart.CardList);
                    break;
                case CardRelationMenuSelect.DeckBuilding:
                    await PartManager.Instance.TransitionPart(GamePart.DeckBuild);
                    break;
                case CardRelationMenuSelect.MainMenu:
                    await PartManager.Instance.TransitionPart(GamePart.MainMenu);
                    return;
            }
        }
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
        // 選択状態を初期化する
        selectMenu = CardRelationMenuSelect.None;
    }

}
