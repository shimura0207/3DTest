/*
 * @file    GenericCheckMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 汎用的な確認画面のメニューウィンドウ
/// </summary>
public class GenericCheckMenu : MenuWindowBase {

    // いいえボタン
    [SerializeField] private Button noButton;
    // はいボタン
    [SerializeField] private Button yesButton;

    // 確認メッセージを表示するテキスト
    [SerializeField] private TMP_Text messageText;

    // ボタンが選択されるまで待機するための完了通知
    private UniTaskCompletionSource<bool> completionSource;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    /// <summary>
    /// 準備処理
    /// </summary>
    /// <returns></returns>
    public async UniTask Setup() {
        // いいえボタンが押された時の処理を登録
        noButton.onClick.AddListener(OnClickNoButton);
        // はいボタンが押された時
        yesButton.onClick.AddListener(OnClickYesButton);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public async UniTask<bool> Open(string message) {
        // 表示するメッセージを設定
        messageText.text = message;

        // ボタン選択結果を受け取るための待機処理を作成
        completionSource = new UniTaskCompletionSource<bool>();

        // ウィンドウを表示
        await base.Open();

        // はい・いいえのどちらかが押されるまで待機
        bool result = await completionSource.Task;

        // ウィンドウを閉じる
        await Close();

        // 選択結果を呼び出し元へ返す
        return result;

    }

    /// <summary>
    /// ウィドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
    }

    /// <summary>
    /// いいえボタンが押された時の処理
    /// </summary>
    private void OnClickNoButton() {
        // 「いいえ」を選択したことを呼び出し元へ通知
        completionSource.TrySetResult(false);
    }

    /// <summary>
    /// はいボタンが押された時の処理
    /// </summary>
    private void OnClickYesButton() {
        // 「はい」を選択したことを呼び出し元へ通知
        completionSource.TrySetResult(true);
    }
}
