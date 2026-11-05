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

    // デッキ編成メニューを開くボタン
    [SerializeField] private Button deckBuildButton;

    // デッキ一覧のContent
    [SerializeField] private Transform deckButtonContent;

    // デッキボタンのPrefab
    [SerializeField] private Button deckButtonPrefab;

    // 生成したデッキボタン一覧
    private List<Button> deckButtons = new List<Button>();

    // デッキ編成ボタンが押されたかどうか
    private bool isDeckBuild = false;

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
        deckButtonPrefab.gameObject.SetActive(false);
        // 選択状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;
        isDeckBuild = false;
        // ボタンのイベント登録、解除をまとめて行う
        AllListner();
        // 保存済みデッキを表示する
        RefreshDeckButtons();
        Debug.Log("デッキ構築画面表示中");
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    public override async UniTask Execute() {

        // 別のパートへ遷移するまで操作を受け付ける
        while (true) {

            // ボタンが押されるまで待機する
            await UniTask.WaitUntil(() =>
                selectMenu != DeckBuildMenuSelect.None || isDeckBuild);

            // デッキ編成メニューが選択された場合
            if (isDeckBuild) {

                // 次回の操作に備えてフラグを初期化する
                isDeckBuild = false;

                // デッキ編成メニューを開き、閉じるまで待機する
                await MenuWindowManager.Instance.Open<DeckBuildMenu>();
                // メニューを閉じた後、保存済みデッキを再表示する
                RefreshDeckButtons();
                // メニューを閉じた後も、このパートの入力待ちを続ける
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
            Destroy(button.gameObject);
        }

        // ボタン一覧を初期化する
        deckButtons.Clear();

        // セーブデータを読み込む
        PlayerSaveData saveData = SaveManager.Instance.Load();

        // デッキ一覧が存在しない場合は終了する
        if (saveData.deckList == null) {
            return;
        }

        // 保存されているデッキの数だけボタンを生成する
        foreach (DeckSaveData deckData in saveData.deckList) {

            deckButtonPrefab.gameObject.SetActive(true);

            // ボタン内のTextMeshProを取得する
            TMP_Text buttonText = deckButtonPrefab.GetComponentInChildren<TMP_Text>();

            // デッキ名を表示する
            if (buttonText != null) {
                buttonText.text = deckData.deckName;
            }

            // 生成したボタンを管理する
            deckButtons.Add(deckButtonPrefab);
        }
    }


    /// <summary>
    /// ボタンイベントの登録、解除をまとめて行う
    /// </summary>
    private void AllListner() {

        // ボタンイベントの二重登録を防ぐ
        // 戻るボタン
        returnButton.onClick.RemoveListener(OnReturnMenuSelected);
        returnButton.onClick.AddListener(OnReturnMenuSelected);
        // デッキ編成
        deckBuildButton.onClick.RemoveListener(OnDeckBuildSelected);
        deckBuildButton.onClick.AddListener(OnDeckBuildSelected);
    }

    /// <summary>
    /// メニューに戻るボタンが押されたときの処理
    /// </summary>
    private void OnReturnMenuSelected() {

        // メインメニューへの遷移を設定する
        selectMenu = DeckBuildMenuSelect.MainMenu;
    }

    /// <summary>
    /// デッキ編成ボタンが押されたときの処理
    /// </summary>
    private void OnDeckBuildSelected() {

        // デッキ編成メニューを開く
        isDeckBuild = true;
    }
}
