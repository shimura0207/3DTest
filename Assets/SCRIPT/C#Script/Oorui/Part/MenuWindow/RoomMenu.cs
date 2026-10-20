/*
 *  @file   HostMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using TMPro;
//using TMPro.EditorUtilities;
using UnityEngine;

/// <summary>
/// ホスト側で表示するメニューウィンドウ
/// </summary>
public class RoomMenu : MenuWindowBase {

    // 現在の接続役割を保持する
    private ConnectionState connectionState = ConnectionState.None;

    // 接続用クラス
    [SerializeField] private NetworkUI networkUI;

    // ホスト専用のUI
    [SerializeField] private GameObject hostUI;

    // クライアント専用のUI
    [SerializeField] private GameObject clientUI;

    [SerializeField] private TMP_Text hostNameText;
    [SerializeField] private TMP_Text clientNameText;

    /// <summary>
    /// ルーム画面を開く前に接続役割を設定する
    /// </summary>
    public void SetConnectionState(ConnectionState state) {
        connectionState = state;
    }

    /// <summary>
    /// プレイヤーの名前をセーブデータから取得する
    /// </summary>
    private void LoadPlayerName() {
        PlayerSaveData saveData = SaveManager.Instance.Load();

        switch (connectionState) {
            case ConnectionState.Host:
                hostNameText.text = saveData.playerName;
                break;
            case ConnectionState.Client:
                clientNameText.text = saveData.playerName;
                break;
        }
    }

    /// <summary>
    /// ルーム画面を表示する
    /// </summary>
    public override async UniTask Open() {
        await base.Open();
        // プレイヤーの名前を取得
        LoadPlayerName();
        // 接続状況によってメニュー表示を切り替える
        ConnectionSwitch();
    }

    /// <summary>
    /// 接続状況に応じて処理分けを行う
    /// </summary>
    private void ConnectionSwitch() {

        // ホスト専用UIはホストの場合だけ表示する
        hostUI.SetActive(connectionState == ConnectionState.Host);

        // クライアント専用UIはクライアントの場合だけ表示する
        clientUI.SetActive(connectionState == ConnectionState.Client);

        // ホストかクライアントかによって表示を切り替える
        switch (connectionState) {
            case ConnectionState.Host:
                networkUI.StartHost();
                break;

            case ConnectionState.Client:
                break;

            default:
                Debug.LogError("接続失敗");
                // 両方のUiを隠す
                hostUI.SetActive(false);
                clientUI.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// メニューウィンドウが閉じたときの処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
        await UniTask.CompletedTask;
    }
}
