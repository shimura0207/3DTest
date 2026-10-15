/*
 *  @file   ClientConectionMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クライアント接続の確認画面
/// </summary>
public class ClientConectionMenu : MenuWindowBase {

    // クライアント接続処理を行うクラス
    [SerializeField]
    private NetworkUI networkUI;

    // はい・いいえボタン
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // ボタン選択結果の完了通知
    private UniTaskCompletionSource<bool> completionSource;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {

        // 基底クラスの初期化処理
        await base.Initialize();

        // ボタンイベントを一度だけ登録
        yesButton.onClick.AddListener(OnClickYesButton);
        noButton.onClick.AddListener(OnClickNoButton);
    }

    /// <summary>
    /// 確認画面を開き、選択結果を返す
    /// </summary>
    public async UniTask<bool> OpenAndWaitForSelection() {

        // 選択結果を初期化
        completionSource = new UniTaskCompletionSource<bool>();

        // 確認画面を表示
        await base.Open();

        // はい・いいえが押されるまで待機
        bool result = await completionSource.Task;

        // 選択後に確認画面を閉じる
        await base.Close();

        // はいが選択された場合はクライアント接続を開始
        if (result) {
            networkUI.StartClient();
        }

        // 選択結果を呼び出し元へ返す
        return result;
    }

    /// <summary>
    /// いいえボタンが押された時の処理
    /// </summary>
    private void OnClickNoButton() {

        // いいえを選択したことを通知
        completionSource?.TrySetResult(false);
    }

    /// <summary>
    /// はいボタンが押された時の処理
    /// </summary>
    private void OnClickYesButton() {

        // はいを選択したことを通知
        completionSource?.TrySetResult(true);
    }
}