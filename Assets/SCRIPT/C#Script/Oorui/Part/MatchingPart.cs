
/*
 *  @file   MattchingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// マッチングパート
/// </summary>
public class MatchingPart : PartBase {
    private bool isRandomMatch = false;     // ランダムマッチが選択されたか
    private bool isPrivateMatch = false;    // プライベートマッチが選択されたか
    private bool isReturn = false;          // メニューに戻るが選択されたか

    // Inspectorからランダムマッチボタンを設定する
    [SerializeField] private Button randomButton;

    // Inspectorからプライベートマッチボタンを設定する
    [SerializeField] private Button privateButton;

    // Inspectorからメニューに戻るボタンを設定する
    [SerializeField] private Button returnButton;

    // デッキ選択ボタン
    [SerializeField] private Button deckSelectButton;

    // 選択中のデッキ名を表示するテキスト
    [SerializeField] private TMP_Text selectedDeckText;

    // 選択されたデッキID（未選択は-1）
    private int selectedDeckID = -1;

    // デッキ選択ボタンが押されたか
    private bool isDeckSelect = false;


    // 接続状況
    // private ConnectionState conection = ConnectionState.None;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    public override async UniTask Setup() {
        await base.Setup();
        Debug.Log("マッチング画面表示中");
        // 接続状況の初期化
        //conection = ConnectionState.None;

        // 選択状態を初期化する
        isRandomMatch = false;
        isPrivateMatch = false;
        isReturn = false;
        selectedDeckID = -1;

        // ボタンイベントを登録する
        randomButton.onClick.RemoveListener(OnClickRandomButton);
        randomButton.onClick.AddListener(OnClickRandomButton);

        privateButton.onClick.RemoveListener(OnClickMatchingButton);
        privateButton.onClick.AddListener(OnClickMatchingButton);

        returnButton.onClick.RemoveListener(OnClickReturnButton);
        returnButton.onClick.AddListener(OnClickReturnButton);

        deckSelectButton.onClick.RemoveListener(OnClickDeckSelectButton);
        deckSelectButton.onClick.AddListener(OnClickDeckSelectButton);
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが選択されるまで繰り返し待機する
        while (true) {
            // ランダムマッチまたはプライベートマッチが選択されるまで待機
            await UniTask.WaitUntil(() => isRandomMatch || isPrivateMatch || isReturn || isDeckSelect);
            // デッキ選択ボタンが押された場合
            if (isDeckSelect) {

                // デッキ選択フラグを初期化する
                isDeckSelect = false;

                // 現在のデッキIDを渡してデッキ選択メニューを開く
                int? result = await MenuWindowManager.Instance.DeckSelectMenuOpen(selectedDeckID);

                // キャンセルされた場合は現在の選択状態を維持する
                if (result == null) continue;

                // 決定されたデッキIDを保持する
                selectedDeckID = result.Value;

                // 選択されたデッキ情報を取得する
                DeckSaveData deckData = SaveManager.Instance.GetDeckData(selectedDeckID);

                // 選択されたデッキ名を表示する
                if (deckData != null && selectedDeckText != null) {
                    selectedDeckText.text = deckData.deckName;
                }

                // 選択されたデッキIDをログに表示する
                Debug.Log($"使用デッキID：{selectedDeckID}");
            }
            // ランダムマッチが選択された場合
            if (isRandomMatch) {
                // デッキが選択されていない場合
                if (selectedDeckID < 0) {
                    // デッキを選択していないことを伝えるメニュー表示をする

                    // 選択状態を解除する
                    isRandomMatch = false;
                    continue;
                }
                // マッチング画面を開く
                await MenuWindowManager.Instance.Open<RandomMatchingMenu>();

                // キャンセルされた場合
                if (RandomMatchingMenu.IsMatchingCancelled) {
                    isRandomMatch = false;
                    continue;
                }

                // 選択されたデッキIDを渡して対戦画面へ遷移する
                await PartManager.Instance.TransitionPart(GamePart.MainGame, selectedDeckID,MainGamePartType.Matching);

                return;
            }

            // プライベートマッチが選択された場合
            if (isPrivateMatch) {
                // // ホストまたはクライアントを選択するメニューを開く
                // Debug.Log("プライベートマッチを選択");
                // conection = await MenuWindowManager.Instance.ConnectMenuOpen(conection);
                // 
                // // キャンセルされた場合
                // if (conection == ConnectionState.None) {
                //     // プライベートマッチの選択状態を解除
                //     isPrivateMatch = false;
                // 
                //     // ランダムマッチの選択状態も初期化
                //     isRandomMatch = false;
                // 
                //     // 再度ボタン選択を待つ
                //     continue;
                // }
                // // クライアントで接続された場合
                // else if (conection == ConnectionState.Client) {
                //     // クライアント接続の確認画面を表示し、選択結果を取得
                //     bool isAccepted = await MenuWindowManager.Instance.ClientConnectionConfirm();
                // 
                //     // 「いいえ」が選択された場合
                //     if (!isAccepted) {
                // 
                //         // 接続状態を初期化
                //         conection = ConnectionState.None;
                // 
                //         // プライベートマッチの選択状態を解除
                //         isPrivateMatch = false;
                // 
                //         // ランダムマッチの選択状態も初期化
                //         isRandomMatch = false;
                // 
                //         // 最初の選択画面に戻り、再度ボタン入力を待つ
                //         continue;
                //     }
                // }
                // 
                // // ホストまたはクライアントが選択された場合
                // // プライベートマッチパートに遷移
                // await PartManager.Instance.TransitionPart(GamePart.PrivateMatch, conection);
                await PartManager.Instance.TransitionPart(GamePart.MainGame, 1, MainGamePartType.Matching);
                return;
            }

            // メインメニューに戻るが選択された時
            if (isReturn) {
                // メインメニューに遷移する
                await PartManager.Instance.TransitionPart(GamePart.MainMenu);
            }
        }
    }


    /// <summary>
    /// ランダムマッチボタンが押された時の処理
    /// </summary>
    private void OnClickRandomButton() {
        // ランダムマッチフラグをONにする
        isRandomMatch = true;
    }


    /// <summary>
    /// プライベートマッチボタンが押された時の処理
    /// </summary>
    private void OnClickMatchingButton() {
        // プライベートマッチフラグをONにする
        isPrivateMatch = true;
    }

    /// <summary>
    /// メニューに戻るボタンが押された時の処理
    /// </summary>
    private void OnClickReturnButton() {
        // メニューに戻るフラグをONにする
        isReturn = true;
    }

    /// <summary>
    /// デッキ選択ボタンが押されたときの処理
    /// </summary>
    private void OnClickDeckSelectButton() {

        // デッキ選択フラグを有効にする
        isDeckSelect = true;
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        // フラグを初期化
        // ボタンイベントを解除して重複登録を防ぐ
        randomButton.onClick.RemoveListener(OnClickRandomButton);
        privateButton.onClick.RemoveListener(OnClickMatchingButton);
        returnButton.onClick.RemoveListener(OnClickReturnButton);
        deckSelectButton.onClick.RemoveListener(OnClickDeckSelectButton);
        // フラグを初期化する
        isPrivateMatch = false;
        isRandomMatch = false;
        isReturn = false;
        await base.Teardown();
    }
}
