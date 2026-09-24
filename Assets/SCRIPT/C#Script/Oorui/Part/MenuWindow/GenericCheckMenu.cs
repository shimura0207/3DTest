/*
 * @file    GenericCheckMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
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

    // このウィンドウを閉じる
    public bool isCloseWindow = false;
    // 選択されたアクションを実行
    public bool isAction = false;

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
    }

    /// <summary>
    /// ウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // 基底側でメニューウィンドウを表示
        await base.Open();
        // ボタンが押されるまで待機
        await UniTask.WaitUntil(() => isCloseWindow || isAction);

        // ウィンドウを閉じる
        if (isCloseWindow) {
            await Close();
        }
        
        else if (isAction) {

        }

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
        // フラグ変更
        isCloseWindow = true;
    }

    /// <summary>
    /// はいボタンが押された時の処理
    /// </summary>
    private void OnClickYesButton() {
        isAction = true;
    }
}
