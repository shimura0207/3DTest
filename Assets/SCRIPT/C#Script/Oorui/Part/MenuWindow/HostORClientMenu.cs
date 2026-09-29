/*
 * @file    HostORClientMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class HostORClientMenu : MenuWindowBase {

    // Inspectorからホストボタンを設定する
    [SerializeField] private Button hostButton;

    // Inspectorからクライアントボタンを設定する
    [SerializeField] private Button clientButton;

    // キャンセルボタン
    [SerializeField] private Button canccelButton;

    // ボタンが選択されるまで待機するための完了通知
    private UniTaskCompletionSource<ConnectionState> completionSource;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    public async UniTask Setup() {
        // ボタンの処理登録
        hostButton.onClick.AddListener(OnClickHostButton);
        clientButton.onClick.AddListener(OnClickClientButton);
        canccelButton.onClick.AddListener(OnCanccelButton);
    }

    /// <summary>
    /// ウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public async UniTask<ConnectionState> Open(ConnectionState connect) {
        // ボタン選択結果を受け取るための待機処理を作成
        completionSource = new UniTaskCompletionSource<ConnectionState>();

        // 基底側でメニューウィンドウを表示
        await base.Open();

        // どれかボタンが押されるまで待機
        connect = await completionSource.Task;

        // ウィンドウを閉じる
        await Close();

        return connect;
    }

    /// <summary>
    /// ウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
        await UniTask.CompletedTask;
    }


    /// <summary>
    /// ホストボタンが押された時の処理
    /// </summary>
    private void OnClickHostButton() {
        // ホストを選択したことを呼び出し元へ通知
        completionSource.TrySetResult(ConnectionState.Host);
    }


    /// <summary>
    /// クライアントボタンが押された時の処理
    /// </summary>
    private void OnClickClientButton() {
        // クライアントを選択したことを呼び出し元へ通知
        completionSource.TrySetResult(ConnectionState.Client);
    }

    /// <summary>
    /// キャンセルボタンが押された時の処理
    /// </summary>
    private void OnCanccelButton() {
        // キャンセルが選択されたことを呼び出し元へ通知
        completionSource.TrySetResult(ConnectionState.None);
    }
}
