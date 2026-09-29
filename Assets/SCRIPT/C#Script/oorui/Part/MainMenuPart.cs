/*
 *  @file   MainMenuPart
 *  @author oorui
 */
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// メインメニューパート
/// </summary>
public class MainMenuPart : PartBase {

    [SerializeField] private Button matching;   // マッチングボタン
    [SerializeField] private Button option;     // 設定ボタン
    [SerializeField] private Button gacha;      // ガチャボタン
    [SerializeField] private Button card;       // カード関連ボタン


    // 選択された遷移先
    private MainMenuSelect selectMenu = MainMenuSelect.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    /// <summary>
    /// 使用前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();

        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;
        // メニューイベントを登録
        RegisterMenuEvent();
    }

    /// <summary>
    /// メニューイベントを登録する
    /// </summary>
    private void RegisterMenuEvent() {
        matching.onClick.AddListener(OnMatchmakingSelected);
        option.onClick.AddListener(OnOptionSelected);
        gacha.onClick.AddListener(OnGachaSelected);
        card.onClick.AddListener(OnCartListSelected);
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが選択されるまで待機する
        await UniTask.WaitUntil(() => selectMenu != MainMenuSelect.None);

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
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnMatchmakingSelected() {

        // 遷移先をマッチングに設定する
        selectMenu = MainMenuSelect.Matchmaking;
    }

    /// <summary>
    /// オプションメニューが選択された時の処理
    /// </summary>
    private void OnOptionSelected() {

        // 遷移先をオプションに設定する
        selectMenu = MainMenuSelect.Option;
    }

    /// <summary>
    /// ガチャメニューが選択された時の処理
    /// </summary>
    /// <param name="menu"></param>
    private void OnGachaSelected() {
        // 遷移先オプションに設定する
        selectMenu = MainMenuSelect.Gacha;
    }

    /// <summary>
    /// カード関連メニューが選択された時の処理
    /// </summary>
    /// <param name="menu"></param>
    private void OnCartListSelected() {
        // 遷移先オプションに設定する
        selectMenu = MainMenuSelect.CardRelation;
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;
    }
}
