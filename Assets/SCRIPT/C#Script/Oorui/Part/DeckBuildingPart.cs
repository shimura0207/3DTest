/*
 *  @file   DeckBuildingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
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

    // 選択された遷移先
    private GamePart selectMenu = GamePart.Invalid;
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
        selectMenu = GamePart.Invalid;
        returnButton.onClick.AddListener(OnReturnMenuSelected);
        Debug.Log("デッキ構築画面表示中");

    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが選択されるまで待機する
        await UniTask.WaitUntil(() => selectMenu != GamePart.Invalid);
        switch (selectMenu) {
            case GamePart.MainMenu:
                await PartManager.Instance.TransitionPart(selectMenu);
                break;

        }
        
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューに戻るボタンが押されたら
    /// </summary>
    private void OnReturnMenuSelected() {
        // 遷移先を設定
        selectMenu = GamePart.MainMenu;
    }
}
