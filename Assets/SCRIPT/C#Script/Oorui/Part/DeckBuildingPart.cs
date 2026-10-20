/*
 *  @file   DeckBuildingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

        // 選択状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;
        isDeckBuild = false;

        // ボタンイベントの二重登録を防ぐ
        returnButton.onClick.RemoveListener(OnReturnMenuSelected);
        returnButton.onClick.AddListener(OnReturnMenuSelected);

        deckBuildButton.onClick.RemoveListener(OnDeckBuildSelected);
        deckBuildButton.onClick.AddListener(OnDeckBuildSelected);

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
