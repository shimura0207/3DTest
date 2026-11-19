
/*
 *  @file   MattchingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;

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
    // デッキのKEYカードを表示する
    [SerializeField] private Image deckImage;

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
        isDeckSelect = false;

        // 最初はキーカード画像を非表示にする
        if (deckImage != null) {
            deckImage.sprite = null;
            deckImage.enabled = false;
            deckImage.preserveAspect = true;
        }


        // 前回選択したデッキが存在するか確認する
        DeckSaveData deckData = null;

        if (DeckSelectionManager.SelectedDeckID >= 0) {
            deckData = SaveManager.Instance.GetDeckData(
                DeckSelectionManager.SelectedDeckID
            );
        }

        // デッキが存在しない場合
        if (deckData == null) {

            // デッキIDを未選択状態にする
            DeckSelectionManager.SetSelectedDeckID(-1);

            // 未選択状態の表示
            if (selectedDeckText != null) {
                selectedDeckText.text = "未選択";
            }
        }
        else {
            // 前回選択したデッキ名を表示する
            selectedDeckText.text = deckData.deckName;

            // キーカードIDを取得する
            int keyCardID = deckData.keyCardID;

            // カード画像を取得する
            Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(keyCardID);

            // カード画像を設定する
            if (deckImage != null) {
                deckImage.sprite = cardSprite;

                // 画像が取得できた場合のみ表示する
                deckImage.enabled = cardSprite != null;
            }
        }
        // ボタンイベントを登録する
        randomButton.onClick.RemoveListener(OnClickRandomButton);
        randomButton.onClick.AddListener(OnClickRandomButton);

        privateButton.onClick.RemoveListener(OnClickMatchingButton);
        privateButton.onClick.AddListener(OnClickMatchingButton);

        returnButton.onClick.RemoveListener(OnClickReturnButton);
        returnButton.onClick.AddListener(OnClickReturnButton);

        deckSelectButton.onClick.RemoveListener(OnClickDeckSelectButton);
        deckSelectButton.onClick.AddListener(OnClickDeckSelectButton);
        // フェードから開ける
        await FadeManager.Instance.FadeIn(FadeType.Black, 0.5f);
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
                int? result = await MenuWindowManager.Instance.DeckSelectMenuOpen(
                    DeckSelectionManager.SelectedDeckID
                );

                // キャンセルされた場合
                if (result == null) continue;

                // 決定されたデッキIDを保持する
                DeckSelectionManager.SetSelectedDeckID(result.Value);

                // 選択されたデッキ情報を取得する
                DeckSaveData deckData = SaveManager.Instance.GetDeckData(
                    DeckSelectionManager.SelectedDeckID
                );

                // 選択されたデッキ情報を表示する
                if (deckData != null) {

                    // デッキ名を表示する
                    if (selectedDeckText != null) {
                        selectedDeckText.text = deckData.deckName;
                    }

                    // キーカードIDを取得する
                    int keyCardID = deckData.keyCardID;

                    // カード画像を取得する
                    Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(keyCardID);

                    // カード画像を設定する
                    if (deckImage != null) {
                        deckImage.sprite = cardSprite;

                        // 画像が取得できた場合のみ表示する
                        deckImage.enabled = cardSprite != null;
                        deckImage.preserveAspect = true;
                    }
                }
            }
            // ランダムマッチが選択された場合
            if (isRandomMatch) {
                // デッキが選択されていない場合
                if (DeckSelectionManager.SelectedDeckID < 0) {
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

                // 自分のプレイヤー情報をセーブデータから取得する
                PlayerSaveData saveData = SaveManager.Instance.Load();

                // 自分のプレイヤー名を取得する
                string playerName = saveData.playerName;

                // 自分の名前を対戦相手へ送信する
                PlayerNameManager.SendPlayerName(playerName);

                // 変更後
                int keyCardID = SaveManager.Instance.GetDeckKeyCardID(
                    DeckSelectionManager.SelectedDeckID
                );

                // 自分のKEYカードIDを対戦相手へ送信する
                PlayerDeckInfoManager.SendOwnKeyCardID(keyCardID);

                // 対戦相手の名前とKEYカードIDを受信するまで待機する
                await UniTask.WaitUntil(() =>
                    PlayerNameManager.isOpponentNameReceived &&
                    PlayerDeckInfoManager.isOpponentKeyCardReceived
                );
                // 対戦相手確認メニューを開く
                await MenuWindowManager.Instance.Open<OpponentConfirmMenu>();
                // フェードを挟む
                await FadeManager.Instance.FadeOut(FadeType.White, 0.5f);
                AudioManager.instance.FadeOutBGM();
                // 選択されたデッキIDを渡して対戦画面へ遷移する
                await PartManager.Instance.TransitionPart(GamePart.MainGame, DeckSelectionManager.SelectedDeckID, MainGamePartType.Matching);

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
