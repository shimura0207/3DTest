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
    // デッキパートに戻るボタン
    [SerializeField] private Button returnDeckPartButton;

    // 選択された操作
    private DeckBuildMenuSelect selectMenu = DeckBuildMenuSelect.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();

        // ボタンイベントの二重登録を防止する
        returnDeckPartButton.onClick.RemoveListener(OnReturnButton);
        returnDeckPartButton.onClick.AddListener(OnReturnButton);
    }

    /// <summary>
    /// メニューを開いたときの処理
    /// </summary>
    public override async UniTask Open() {

        // メニューを表示する
        await base.Open();

        // 前回の選択状態を初期化する
        selectMenu = DeckBuildMenuSelect.None;

        // ボタンが選択されるまで待機する
        await UniTask.WaitUntil(() => selectMenu != DeckBuildMenuSelect.None);

        // 選択後にメニューを閉じる
        await Close();
    }

    /// <summary>
    /// メニューを閉じたときの処理
    /// </summary>
    public override async UniTask Close() {
        await base.Close();

        // 次回の入力待ちに備えて初期化する
        selectMenu = DeckBuildMenuSelect.None;
    }

    /// <summary>
    /// デッキパートに戻るボタンが押されたときの処理
    /// </summary>
    private void OnReturnButton() {
        // 戻る操作を設定する
        selectMenu = DeckBuildMenuSelect.DeckBuild;
    }
}
