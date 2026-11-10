/*
 *  @file   DeckSelectMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 使用するデッキを選択するメニュー
/// </summary>
public class DeckSelectMenu : MenuWindowBase {

    // デッキ一覧を表示するContent
    [SerializeField] private Transform deckButtonContent;

    // デッキボタンのPrefab
    [SerializeField] private Button deckButtonPrefab;

    // デッキ決定ボタン
    [SerializeField] private Button decideButton;

    // キャンセルボタン
    [SerializeField] private Button cancelButton;

    // 生成したデッキボタン一覧
    private List<Button> deckButtons = new List<Button>();

    // 選択中のデッキID
    private int selectedDeckID = -1;

    // 決定フラグ
    private bool isDecided = false;

    // キャンセルフラグ
    private bool isCancelled = false;

    /// <summary>
    /// デッキ選択メニューを開き、選択結果を返す
    /// </summary>
    /// <param name="currentDeckID">現在選択されているデッキID</param>
    /// <returns>決定されたデッキID。キャンセル時はnull</returns>
    public async UniTask<int?> OpenAndWaitForSelection(int currentDeckID) {

        // 選択状態を初期化する
        selectedDeckID = currentDeckID;
        isDecided = false;
        isCancelled = false;

        // デッキ一覧を生成する
        RefreshDeckButtons();

        // 決定ボタンのイベントを登録する
        decideButton.onClick.RemoveListener(OnClickDecideButton);
        decideButton.onClick.AddListener(OnClickDecideButton);

        // キャンセルボタンのイベントを登録する
        cancelButton.onClick.RemoveListener(OnClickCancelButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);

        // メニューを表示する
        await base.Open();

        // 決定またはキャンセルされるまで待機する
        await UniTask.WaitUntil(() => isDecided || isCancelled);

        // ボタンイベントを解除する
        decideButton.onClick.RemoveListener(OnClickDecideButton);
        cancelButton.onClick.RemoveListener(OnClickCancelButton);


        // メニューを閉じる
        await base.Close();

        // 決定された場合はデッキIDを返す
        if (isDecided) {
            return selectedDeckID;
        }

        // キャンセルされた場合はnullを返す
        return null;
    }

    /// <summary>
    /// 保存済みデッキのボタンを生成する
    /// </summary>
    private void RefreshDeckButtons() {

        // 前回生成したボタンを削除する
        foreach (Button button in deckButtons) {
            if (button != null) {
                Destroy(button.gameObject);
            }
        }

        // ボタン一覧を初期化する
        deckButtons.Clear();

        // セーブデータを読み込む
        PlayerSaveData saveData = SaveManager.Instance.Load();

        // デッキ一覧が存在しない場合は終了する
        if (saveData == null || saveData.deckList == null) {
            return;
        }

        // デッキIDの昇順で並び替える
        saveData.deckList.Sort((a, b) => {
            // 両方nullなら同じ位置として扱う
            if (a == null && b == null) return 0;

            // nullのデッキ情報は最後に配置する
            if (a == null) return 1;
            if (b == null) return -1;

            // デッキIDを比較する
            return a.deckID.CompareTo(b.deckID);
        });

        // 保存されているデッキの数だけボタンを生成する
        foreach (DeckSaveData deckData in saveData.deckList) {

            // nullのデッキ情報はスキップする
            if (deckData == null) {
                continue;
            }

            // デッキボタンを生成する
            Button newButton = Instantiate(deckButtonPrefab, deckButtonContent);

            // ボタンを表示する
            newButton.gameObject.SetActive(true);

            // ボタン内のテキストを取得する
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();

            // デッキ名を表示する
            if (buttonText != null) {
                buttonText.text = deckData.deckName;
            }

            // ボタンに対応するデッキIDを保持する
            int targetDeckID = deckData.deckID;

            // デッキ選択イベントを登録する
            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() => {

                // 選択されたデッキIDを保持する
                selectedDeckID = targetDeckID;

                // 選択されたデッキIDをログに表示する
                Debug.Log($"選択されたデッキID：{selectedDeckID}");
            });

            // 生成したボタンを一覧に追加する
            deckButtons.Add(newButton);
        }
    }

    /// <summary>
    /// デッキ決定ボタンが押されたときの処理
    /// </summary>
    private void OnClickDecideButton() {

        // デッキが選択されていない場合は終了する
        if (selectedDeckID < 0) {
            Debug.LogWarning("デッキを選択してください");
            return;
        }

        // 決定フラグを有効にする
        isDecided = true;
    }

    /// <summary>
    /// キャンセルボタンが押されたときの処理
    /// </summary>
    private void OnClickCancelButton() {

        // キャンセルフラグを有効にする
        isCancelled = true;
    }
}