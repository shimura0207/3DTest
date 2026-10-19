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
    [SerializeField] private Button deckBuildButton;
    // デッキビルドボタンが押されたかどうか
    private bool isDeckBuild = false;

    // 選択された遷移先
    private DeckBuildMenuSelect selectMenu = DeckBuildMenuSelect.None;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();
        // 選択状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;
        // ボタンイベントを登録する
        // Setupが複数回実行されても二重登録されないようにする
        returnButton.onClick.RemoveListener(OnReturnMenuSelected);
        returnButton.onClick.AddListener(OnReturnMenuSelected);

        deckBuildButton.onClick.RemoveListener(OnDeckBuildSelected);
        deckBuildButton.onClick.AddListener(OnDeckBuildSelected);

        Debug.Log("デッキ構築画面表示中");

        // フラグを初期化
        isDeckBuild = false;

    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが選択されるまで待機する
        await UniTask.WaitUntil(() => selectMenu != DeckBuildMenuSelect.None || isDeckBuild);

        if (isDeckBuild) {
            await MenuWindowManager.Instance.Open<DeckBuildMenu>();
            isDeckBuild = false;
        }

        switch (selectMenu) {
            case DeckBuildMenuSelect.MainMenu:
                await PartManager.Instance.TransitionPart(GamePart.MainMenu);
                break;

        }

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューに戻るボタンが押されたら
    /// </summary>
    private void OnReturnMenuSelected() {
        // 遷移先を設定
        selectMenu = DeckBuildMenuSelect.MainMenu;
    }

    /// <summary>
    /// デッキ作成ボタンが押されたら
    /// </summary>
    private void OnDeckBuildSelected() {
        // メニューを開く
        isDeckBuild = true;
    }
}
