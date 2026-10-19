/*
 *  @file   DeckBuildMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 新規デッキ編成を行う画面
/// </summary>
public class DeckBuildMenu : MenuWindowBase {

    // デッキパートに戻る
    [SerializeField] private Button returnDeckPartButton;
    // 選択された遷移先
    private DeckBuildMenuSelect selectMenu = DeckBuildMenuSelect.None;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        selectMenu = DeckBuildMenuSelect.None;
        // ボタンイベント登録
        returnDeckPartButton.onClick.AddListener(OnReturnButton);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューが開かれた時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        await base.Open();
        // ボタンが選択されるまで待機する
        await UniTask.WaitUntil(() => selectMenu != DeckBuildMenuSelect.None);

        await Close();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        await base.Close();
        selectMenu = DeckBuildMenuSelect.None;
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// デッキパートに戻るボタン
    /// </summary>
    private void OnReturnButton() {
        // 遷移先を設定
        selectMenu = DeckBuildMenuSelect.DeckBuild;
    }
}
