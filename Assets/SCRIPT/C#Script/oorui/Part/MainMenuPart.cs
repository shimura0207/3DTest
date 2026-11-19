/*
 *  @file   MainMenuPart
 *  @author oorui
 */
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private PolygonButtonHitArea cardHitArea;
    [SerializeField] private PolygonButtonHitArea tutorialArea;
    /// <summary>
    /// メニュー説明用テキスト
    /// </summary>
    [SerializeField] private TMP_Text menuDescriptionText;

    // テーマ画像の表示設定
    [SerializeField]
    private List<ThemeObjectData> themeObjectList = new List<ThemeObjectData>();

    // 汎用テーマのID
    [SerializeField]
    private int normalThemeID;

    // テーマ画像の浮遊移動量
    [SerializeField]
    private float floatHeight = 15.0f;

    // テーマ画像が上下する速さ
    [SerializeField]
    private float floatSpeed = 1.5f;

    // テーマ画像の移動処理を管理する
    private CancellationTokenSource floatTokenSource;

    // テーマ画像の初期位置
    private Vector2 themeInitialPosition;


    // 流すBGM
    private const string menuBGMName = "MainMenuBGM";
    // SEの名前
    private const string buttonSEName = "ButtonClick";

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

        // 選択中のデッキに対応するテーマ画像を表示する
        RefreshThemeObject();

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
        card.onClick.AddListener(OnCartListSelected);
        tutorial.onClick.AddListener(OnTutorialSelected);

        // カーソルが重なったときのテキスト表示
        matchingHitArea.OnHover += () => {
            menuDescriptionText.text = "対戦相手を探し、対戦が可能です";
        };

        optionHitArea.OnHover += () => {
            menuDescriptionText.text = "ゲームの設定、名前を変更が可能です";
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

        // メニューが選択されるまで繰り返し待機する
        while (true) {

            // Escapeキーが押されたか確認する
            bool isEscapePressed = false;

            // ボタンが選択されるか、Escapeキーが押されるまで待機する
            await UniTask.WaitUntil(() => {

                // Escapeキーの入力状態を取得する
                isEscapePressed = Input.GetKeyDown(KeyCode.Escape);

                return selectMenu != MainMenuSelect.None || isEscapePressed;
            });

            // Escapeキーが押された場合
            if (isEscapePressed) {

                // タイトルに戻るか確認する
                bool isGameEnd = await MenuWindowManager.Instance.Confirm("タイトルに戻りますか？");

                // 「いいえ」が選択された場合
                if (!isGameEnd) {
                    // 入力待ちに戻る
                    continue;
                }

                // 再生中のBGMを停止する
                AudioManager.instance.FadeOutBGM();

                // タイトル画面へ遷移する
                await PartManager.Instance.TransitionPart(GamePart.Title);

                return;
            }

            // 選択されたメニューによって遷移先を変更する
            switch (selectMenu) {

                case MainMenuSelect.Matchmaking:
                    //AudioManager.instance.FadeOutBGM();
                    await PartManager.Instance.TransitionPart(GamePart.Matchmaking);
                    return;

                case MainMenuSelect.Option:
                    await PartManager.Instance.TransitionPart(GamePart.Option);
                    return;

                case MainMenuSelect.CardRelation:
                    //AudioManager.instance.FadeOutBGM();
                    await PartManager.Instance.TransitionPart(GamePart.CardRelation);
                    return;

                case MainMenuSelect.Tutorial:
                    //AudioManager.instance.FadeOutBGM();
                    await PartManager.Instance.TransitionPart(
                        GamePart.MainGame, 1, MainGamePartType.Tutotrial
                    );
                    return;
            }
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
    /// デッキのKEYカードに対応するテーマ画像を表示する
    /// </summary>
    private void RefreshThemeObject() {

        // 表示するテーマIDを汎用テーマに設定する
        int targetThemeID = normalThemeID;

        // 選択中のデッキIDを取得する
        int deckID = DeckSelectionManager.SelectedDeckID;

        // デッキが選択されている場合
        if (deckID >= 0) {

            // デッキ情報を取得する
            DeckSaveData deckData = SaveManager.Instance.GetDeckData(deckID);

            // デッキ情報が存在する場合
            if (deckData != null) {

                // KEYカードIDを取得する
                int keyCardID = deckData.keyCardID;

                // KEYカードのマスターデータを取得する
                var cardData = CardDataMasterUtility.GetCardMaster(keyCardID);

                // カード情報が存在する場合
                if (cardData != null) {

                    // KEYカードのテーマIDを取得する
                    targetThemeID = (int)cardData.themeID;
                }
            }
        }

        // 登録されているテーマ画像をすべて非表示にする
        foreach (ThemeObjectData themeData in themeObjectList) {

            if (themeData.targetObject != null) {
                themeData.targetObject.SetActive(false);
            }
        }

        // 対応するテーマIDのオブジェクトを表示する
        foreach (ThemeObjectData themeData in themeObjectList) {

            // テーマIDが一致した場合
            if (themeData.themeID == targetThemeID) {

                // 対象のオブジェクトを表示する
                if (themeData.targetObject != null) {

                    themeData.targetObject.SetActive(true);

                    // RectTransformを取得する
                    RectTransform rect =
                        themeData.targetObject.GetComponent<RectTransform>();

                    // 浮遊処理を開始する
                    if (rect != null) {

                        // 前回の浮遊処理を停止する
                        floatTokenSource?.Cancel();
                        floatTokenSource?.Dispose();

                        // 新しいキャンセルトークンを作成する
                        floatTokenSource = new CancellationTokenSource();

                        // 浮遊処理を開始する
                        FloatThemeObject(
                            rect,
                            floatTokenSource.Token
                        ).Forget();
                    }
                }

                return;
            }
        }
    }

    /// <summary>
    /// テーマ画像を上下に浮遊させる
    /// </summary>
    private async UniTask FloatThemeObject(
        RectTransform target,
        CancellationToken token) {

        // 初期位置を取得する
        Vector2 initialPosition = target.anchoredPosition;

        // 経過時間
        float time = 0.0f;

        while (!token.IsCancellationRequested && target != null) {

            // 経過時間を加算する
            time += Time.deltaTime * floatSpeed;

            // Sin波を使って上下に移動させる
            float offsetY = Mathf.Sin(time) * floatHeight;

            // 初期位置から上下に移動させる
            target.anchoredPosition =
                initialPosition + new Vector2(0.0f, offsetY);

            // 次のフレームまで待機する
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        // 浮遊処理を停止する
        floatTokenSource?.Cancel();
        floatTokenSource?.Dispose();
        floatTokenSource = null;

        await base.Teardown();
        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;
    }
}
