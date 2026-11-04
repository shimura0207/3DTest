/*
 *  @file   MainMenuPart
 *  @author oorui
 */
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// メインメニューパート
/// </summary>
public class MainMenuPart : PartBase {

    /// <summary>
    /// 各種ボタン
    /// </summary>
    // マッチング画面遷移ボタン
    [SerializeField] private Button matching;
    // 設定画面遷移ボタン
    [SerializeField] private Button option;
    // ガチャ画面遷移ボタン
    [SerializeField] private Button gacha;
    // カード関連画面遷移ボタン
    [SerializeField] private Button card;
    // チュートリアル遷移ボタン
    [SerializeField] private Button tutorial;

    // プレイヤー名を表示するテキスト
    [SerializeField] private TMP_Text playerNameText;

    // 選択された遷移先
    private MainMenuSelect selectMenu = MainMenuSelect.None;
    // 各ボタンの多角形判定
    [SerializeField] private PolygonButtonHitArea matchingHitArea;
    [SerializeField] private PolygonButtonHitArea optionHitArea;
    [SerializeField] private PolygonButtonHitArea gachaHitArea;
    [SerializeField] private PolygonButtonHitArea cardHitArea;
    [SerializeField] private PolygonButtonHitArea tutorialArea;
    /// <summary>
    /// メニュー説明用テキスト
    /// </summary>
    [SerializeField] private TMP_Text menuDescriptionText;


    // 流すBGM
    private const string menuBGMName = "MainMenuBGM";

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
        // 保存済みのプレイヤー名を表示する
        LoadPlayerName();
        // フェードから開ける
        await FadeManager.Instance.FadeIn(FadeType.Black, 0.5f);
        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;
        // メニューイベントを登録
        RegisterMenuEvent();
        // BGM再生
        AudioManager.instance.PlayBGM(menuBGMName);
    }

    /// <summary>
    /// 保存済みのプレイヤー名を読み込んで表示する
    /// </summary>
    private void LoadPlayerName() {
        // セーブデータを読み込む
        PlayerSaveData saveData = SaveManager.Instance.Load();

        // 読み込んだ名前を画面に表示する
        playerNameText.text = saveData.playerName;
    }

    /// <summary>
    /// メニューイベントを登録する
    /// </summary>
    private void RegisterMenuEvent() {
        // クリックイベント
        matching.onClick.AddListener(OnMatchmakingSelected);
        option.onClick.AddListener(OnOptionSelected);
        gacha.onClick.AddListener(OnGachaSelected);
        card.onClick.AddListener(OnCartListSelected);
        tutorial.onClick.AddListener(OnTutorialSelected);

        // カーソルが重なったときのテキスト表示
        matchingHitArea.OnHover += () => {
            menuDescriptionText.text = "対戦相手を探し、対戦が可能です";
        };

        optionHitArea.OnHover += () => {
            menuDescriptionText.text = "ゲームの設定、名前を変更が可能です";
        };

        gachaHitArea.OnHover += () => {
            menuDescriptionText.text = "ガチャを引き、カードを取得することが可能です";
        };

        cardHitArea.OnHover += () => {
            menuDescriptionText.text = "所持カードやデッキを確認可能です";
        };

        tutorialArea.OnHover += () => {
            menuDescriptionText.text = "対戦の基本を学ぶことが可能です";
        };
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
                // 再生中のBGMを停止する
                AudioManager.instance.FadeOutBGM();
                // マッチングパートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Matchmaking);

                break;

            case MainMenuSelect.Option:
                // 再生中のBGMを停止する
                AudioManager.instance.FadeOutBGM();
                // オプションパートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Option);

                break;

            case MainMenuSelect.CardRelation:
                // 再生中のBGMを停止する
                AudioManager.instance.FadeOutBGM();
                // カード一覧パートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.CardRelation);

                break;

            case MainMenuSelect.Gacha:
                // 再生中のBGMを停止する
                AudioManager.instance.FadeOutBGM();
                // ガチャパートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Gacha);

                break;

            case MainMenuSelect.Tutorial:
                // 再生中のBGMを停止する
                AudioManager.instance.FadeOutBGM();
                // チュートリアルパートへ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Tutorial);
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
    /// チュートリアルメニューが選択された時の処理
    /// </summary>
    private void OnTutorialSelected() {
        selectMenu = MainMenuSelect.Tutorial;
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
