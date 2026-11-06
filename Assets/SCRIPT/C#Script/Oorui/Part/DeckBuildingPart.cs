/*
 *  @file   DeckBuildingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ構築パート
/// </summary>
public class DeckBuildingPart : PartBase {

    // メニューに戻るボタン
    [SerializeField] private Button returnButton;

    // デッキ一覧のContent
    [SerializeField] private Transform deckButtonContent;

    // デッキボタンのPrefab
    [SerializeField] private Button deckButtonPrefab;

    // 生成したデッキボタン一覧
    private List<Button> deckButtons = new List<Button>();

    // 新規作成ボタンが押されたかどうか
    private bool isDeckBuild = false;

    // 既存デッキが選択されたかどうか
    private bool isDeckSelect = false;

    // 選択されたデッキ情報
    private DeckSaveData selectedDeckData = null;

    // 選択された遷移先
    private DeckBuildMenuSelect selectMenu = DeckBuildMenuSelect.None;


    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();
    }


    /// <summary>
    /// 使用前準備
    /// </summary>
    public override async UniTask Setup() {

        await base.Setup();

        // 複製元のPrefabを非表示にする
        deckButtonPrefab.gameObject.SetActive(false);

        // 選択状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;
        isDeckBuild = false;
        isDeckSelect = false;
        selectedDeckData = null;

        // ボタンイベントの登録
        AllListner();

        // 保存済みデッキを表示する
        RefreshDeckButtons();
    }


    /// <summary>
    /// 実行処理
    /// </summary>
    public override async UniTask Execute() {

        // 別のパートへ遷移するまで操作を受け付ける
        while (true) {

            // ボタンが押されるまで待機する
            await UniTask.WaitUntil(() =>
                selectMenu != DeckBuildMenuSelect.None ||
                isDeckBuild ||
                isDeckSelect
            );


            // 新規作成ボタンが押された場合
            if (isDeckBuild) {

                // 次回の操作に備えてフラグを初期化する
                isDeckBuild = false;

                // 新規デッキ編成メニューを開く
                await MenuWindowManager.Instance.Open<DeckBuildMenu>();

                // メニューを閉じた後に一覧を更新する
                RefreshDeckButtons();

                continue;
            }


            // 既存デッキが選択された場合
            if (isDeckSelect) {

                // 次回の操作に備えてフラグを初期化する
                isDeckSelect = false;

                // 選択されたデッキ情報が存在するか確認する
                if (selectedDeckData == null) continue;

                // デッキ確認メニューを取得する
                DeckCreateMenu menu = MenuWindowManager.Instance.Get<DeckCreateMenu>();

                // メニューが取得できなければ終了する
                if (menu == null) continue;

                // 選択されたデッキ情報を設定する
                menu.SetDeckData(selectedDeckData);

                // デッキ確認メニューを開く
                await menu.Open();
                // メニューを閉じた後、デッキ一覧を更新する
                RefreshDeckButtons();
                // メニューを閉じた後も入力待ちを続ける
                continue;
            }


            // 選択された遷移先に応じて処理する
            switch (selectMenu) {

                case DeckBuildMenuSelect.MainMenu:

                    // メインメニューへ遷移する
                    await PartManager.Instance.TransitionPart(GamePart.MainMenu);
                    return;
            }
        }
    }


    /// <summary>
    /// 保存済みデッキのボタンを再生成する
    /// </summary>
    private void RefreshDeckButtons() {

        // 前回生成したボタンをすべて削除する
        foreach (Button button in deckButtons) {
            if (button != null) {
                Destroy(button.gameObject);
            }
        }

        // ボタン一覧を初期化する
        deckButtons.Clear();

        // 新規作成ボタンを生成する
        Button createButton = Instantiate(
            deckButtonPrefab,
            deckButtonContent
        );

        // 新規作成ボタンを表示する
        createButton.gameObject.SetActive(true);

        // ボタン内のテキストを取得する
        TMP_Text createText = createButton.GetComponentInChildren<TMP_Text>();

        // 新規作成の文字を設定する
        if (createText != null) {
            createText.text = "新規作成";
        }

        // 新規作成ボタンのイベントを登録する
        createButton.onClick.RemoveAllListeners();
        createButton.onClick.AddListener(OnDeckBuildSelected);

        // リストの0番目に登録する
        deckButtons.Add(createButton);

        // セーブデータを読み込む
        PlayerSaveData saveData = SaveManager.Instance.Load();

        // デッキ一覧が存在しない場合は終了する
        if (saveData.deckList == null) {
            return;
        }

        // 保存されているデッキの数だけボタンを生成する
        foreach (DeckSaveData deckData in saveData.deckList) {

            // デッキボタンを生成する
            Button newButton = Instantiate(
                deckButtonPrefab,
                deckButtonContent
            );

            // ボタンを表示する
            newButton.gameObject.SetActive(true);

            // ボタン内のテキストを取得する
            TMP_Text buttonText =
                newButton.GetComponentInChildren<TMP_Text>();

            // デッキ名を表示する
            if (buttonText != null) {
                buttonText.text = deckData.deckName;
            }

            // このボタンに対応するデッキ情報を保持する
            DeckSaveData targetDeck = deckData;

            // デッキ選択イベントを登録する
            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(
                () => OnDeckSelected(targetDeck)
            );

            // 生成したボタンをリストに追加する
            deckButtons.Add(newButton);
        }
    }


    /// <summary>
    /// ボタンイベントの登録をまとめて行う
    /// </summary>
    private void AllListner() {

        // 戻るボタンの二重登録を防ぐ
        returnButton.onClick.RemoveListener(OnReturnMenuSelected);
        returnButton.onClick.AddListener(OnReturnMenuSelected);
    }


    /// <summary>
    /// メニューに戻るボタンが押されたときの処理
    /// </summary>
    private void OnReturnMenuSelected() {

        // メインメニューへの遷移を設定する
        selectMenu = DeckBuildMenuSelect.MainMenu;
    }


    /// <summary>
    /// 新規作成ボタンが押されたときの処理
    /// </summary>
    private void OnDeckBuildSelected() {

        // 新規作成フラグを設定する
        isDeckBuild = true;
    }


    /// <summary>
    /// 既存デッキのボタンが押されたときの処理
    /// </summary>
    private void OnDeckSelected(DeckSaveData deckData) {

        // 選択されたデッキ情報を保持する
        selectedDeckData = deckData;

        // 既存デッキ選択フラグを設定する
        isDeckSelect = true;
    }
}