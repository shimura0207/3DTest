/*
 *  @file   ClientConectionMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クライアント接続の確認画面
/// </summary>
public class ClientConectionMenu : MenuWindowBase {


    // 接続先のルーム番号を入力する欄
    [SerializeField] private TMP_InputField roomCodeInput;

    // はい・いいえボタン
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // ボタン選択結果を通知する
    private UniTaskCompletionSource<bool> completionSource;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();

        // ボタンイベントの重複登録を防ぐ
        yesButton.onClick.RemoveListener(OnClickYesButton);
        noButton.onClick.RemoveListener(OnClickNoButton);

        // ボタンイベントを登録する
        yesButton.onClick.AddListener(OnClickYesButton);
        noButton.onClick.AddListener(OnClickNoButton);

        // 入力内容が変更されたらボタンの状態を更新する
        roomCodeInput.onValueChanged.AddListener(OnRoomCodeChanged);

        // 初期状態の入力内容を確認する
        UpdateYesButton();
    }

    /// <summary>
    /// 入力内容に応じて入室ボタンを有効・無効にする
    /// </summary>
    private void OnRoomCodeChanged(string value) {
        UpdateYesButton();
    }

    /// <summary>
    /// ルーム番号が入力されているか確認する
    /// </summary>
    private void UpdateYesButton() {
        // 空文字や空白だけの入力は無効とする
        yesButton.interactable =
            !string.IsNullOrWhiteSpace(roomCodeInput.text);
    }

    /// <summary>
    /// 確認画面を開いて選択結果を待つ
    /// </summary>
    public async UniTask<bool> OpenAndWaitForSelection() {
        // 選択結果を初期化する
        completionSource = new UniTaskCompletionSource<bool>();

        // 画面を開く前にも入力状態を確認する
        UpdateYesButton();

        // 確認画面を表示する
        await base.Open();

        // ボタンが押されるまで待機する
        bool result = await completionSource.Task;

        // 選択後に画面を閉じる
        await base.Close();

        // クライアント側の接続処理を開始
        if (result) {
            // networkUI.StartClient();
        }

        // 接続の仕方を返す
        return result;
    }

    /// <summary>
    /// はいボタンが押されたときの処理
    /// </summary>
    private void OnClickYesButton() {
        // 空の状態で処理はしない
        if (string.IsNullOrWhiteSpace(roomCodeInput.text)) {
            UpdateYesButton();
            return;
        }

        // 入室を選択したことを通知する
        completionSource?.TrySetResult(true);
    }

    /// <summary>
    /// いいえボタンが押されたときの処理
    /// </summary>
    private void OnClickNoButton() {
        // キャンセルを通知する
        completionSource?.TrySetResult(false);
    }
}