/*
 *  @file   OptionMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 設定画面で表示するメニュー
/// </summary>
public class OptionMenu : MenuWindowBase {

    // プレイヤー名の入力欄
    [SerializeField]
    private TMP_InputField playerNameInputField;

    // 名前を確定するボタン
    [SerializeField]
    private Button savePlayerNameButton;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();

        // 現在保存されているプレイヤー名を入力欄に表示する
        SaveData saveData = SaveManager.Instance.Load();
        playerNameInputField.text = saveData.playerName;

        // ボタンのイベントを登録する
        savePlayerNameButton.onClick.RemoveListener(OnSavePlayerName);
        savePlayerNameButton.onClick.AddListener(OnSavePlayerName);
    }

    /// <summary>
    /// メニューウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        await base.Open();
    }

    /// <summary>
    /// メニューウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 登録したイベントを解除する
        savePlayerNameButton.onClick.RemoveListener(OnSavePlayerName);

        await base.Close();
    }

    /// <summary>
    /// 名前の確定ボタンが押されたときの処理
    /// </summary>
    private void OnSavePlayerName() {
        // 入力された名前の前後にある空白を取り除く
        string playerName = playerNameInputField.text.Trim();

        // 名前が空の場合は保存しない
        if (string.IsNullOrEmpty(playerName)) {
            Debug.LogWarning("プレイヤー名を入力してください。");
            return;
        }

        // 現在のセーブデータを取得する
        SaveData saveData = SaveManager.Instance.Load();

        // プレイヤー名だけを更新する
        saveData.playerName = playerName;

        // 更新したセーブデータを保存する
        SaveManager.Instance.Save(saveData);

        Debug.Log($"プレイヤー名を保存しました: {playerName}");
    }
}
