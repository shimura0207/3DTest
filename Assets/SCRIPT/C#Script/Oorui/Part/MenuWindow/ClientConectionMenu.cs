/*
 *  @file   ClientConectionMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 入力画面
/// </summary>
public class ClientConectionMenu : MenuWindowBase {
    [SerializeField]
    private NetworkUI networkUI;

    // ボタン
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // 「はい」が選択されたかどうかを呼び出し元に公開する
    public bool IsAccepted { get; private set; } = false;

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
        // ボタンイベントの登録を行う
        ButtonReset();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // ボタンイベントのリセット、再登録を行う
        ButtonReset();
        IsAccepted = false;


        // ボタンの選択結果を受け取る
        completionSource = new UniTaskCompletionSource<bool>();
        await base.Open();

        // はい・いいえのどちらかが押されるまで待機
        bool result = await completionSource.Task;

        // 選択結果を保存
        IsAccepted = result;

        if (result) {
            // クライアント処理の実行
            networkUI.StartClient();
        }
        else {
            // ウィンドウを閉じる
            await Close();
        }
    }

    /// <summary>
    /// メニューウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
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

    /// <summary>
    /// ボタンイベントの再登録
    /// </summary>
    private void ButtonReset() {
        // ボタンイベントを登録し直して、再表示時にも反応させる
        yesButton.onClick.RemoveListener(OnClickYesButton);
        noButton.onClick.RemoveListener(OnClickNoButton);
        yesButton.onClick.AddListener(OnClickYesButton);
        noButton.onClick.AddListener(OnClickNoButton);
    }
}
