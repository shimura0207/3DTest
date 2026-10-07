/*
 * @brief  NetworkUI
 * @author Kobayashi
 */

using TMPro;
using Unity.Netcode;                    // Netcode for GameObjects を使用するためのもの
using Unity.Netcode.Transports.UTP;     // Unity Transport を使用するためのもの
using UnityEngine;

public class NetworkUI : MonoBehaviour {
    [Header("UI")]

    // 現在のネットワーク状態を表示するテキスト
    [SerializeField]
    private TMP_Text statusText;

    // Clientがルームコードを入力するInputField
    [SerializeField]
    private TMP_InputField roomCodeInput;


    [Header("Network")]

    // ネットワーク通信に使用するポート番号
    [SerializeField]
    private ushort port = 7777;

    // Hostが部屋を通知するためのクラス
    [SerializeField]
    private RoomBroadcaster broadcaster;

    // Clientが部屋を探すためのクラス
    [SerializeField]
    private RoomFinder finder;

    // マッチングを管理するクラス
    [SerializeField]
    private MatchingManager matchingManager;


    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start() {

        // NetworkManagerが存在するか確認
        if (NetworkManager.Singleton == null) {

            Debug.LogError(
                "NetworkUI : " +
                "NetworkManagerが存在しません。"
            );

            return;
        }

        // Client接続イベントを登録
        NetworkManager.Singleton
            .OnClientConnectedCallback +=
            OnClientConnected;

        // Client切断イベントを登録
        NetworkManager.Singleton
            .OnClientDisconnectCallback +=
            OnClientDisconnected;
    }


    /// <summary>
    /// Hostとしてゲームを開始する
    /// </summary>
    public void StartHost() {

        // NetworkManagerが存在するか確認
        if (NetworkManager.Singleton == null) {
            statusText.text = "Network Manager Missing";
            return;
        }

        // UnityTransportが取得できているか確認
        if (NetWorkSystemManager.Instance == null || NetWorkSystemManager.Instance.UnityTransport == null) {
            statusText.text = "Network System Missing";
            return;
        }

        // UnityTransportを取得
        var transport = NetWorkSystemManager.Instance.UnityTransport;

        // 使用するアドレスとポートを設定
        transport.SetConnectionData(
            "0.0.0.0",
            port
        );

        // Hostを開始
        bool success = NetworkManager.Singleton.StartHost();

        // Host開始に成功した場合
        if (success) {

            // 部屋情報の通知を開始
            broadcaster.StartBroadcast(
                port
            );

            // ルームコードを表示
            statusText.text = $"{broadcaster.RoomCode}";
        }
        else {

            // Host開始失敗
            statusText.text =
                "Host Failed";
        }
    }


    /// <summary>
    /// Clientとしてゲームに参加する
    /// </summary>
    public void StartClient() {

        // ルームコードを取得
        string code =
            roomCodeInput.text.Trim();

        // ルームコードが空の場合
        if (string.IsNullOrEmpty(code)) {

            statusText.text =
                "Enter Code";

            return;
        }

        // 部屋を検索中であることを表示
        statusText.text =
            "Searching...";

        // 部屋を検索
        finder.SearchRoom(code);
    }


    /// <summary>
    /// マッチングを開始する
    /// </summary>
    public void StartMatching() {

        // MatchingManagerが設定されているか確認
        if (matchingManager == null) {

            Debug.LogError(
                "NetworkUI : " +
                "MatchingManagerが設定されていません。"
            );

            statusText.text =
                "Matching Manager Missing";

            return;
        }

        // マッチング中であることを表示
        statusText.text =
            "Matching...";

        // マッチングを開始
        matchingManager.StartMatching();
    }


    /// <summary>
    /// Clientが接続した時に呼ばれる
    /// </summary>
    /// <param name="clientId">接続したClientのID</param>
    private void OnClientConnected(
        ulong clientId) {

        statusText.text =
            "Connection";

        Debug.Log(
            $"Client Connected : {clientId}"
        );
    }


    /// <summary>
    /// Clientが切断した時に呼ばれる
    /// </summary>
    /// <param name="clientId">切断したClientのID</param>
    private void OnClientDisconnected(
        ulong clientId) {

        statusText.text =
            "Disconnected";
    }


    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() {

        // NetworkManagerが存在しない場合は終了
        if (NetworkManager.Singleton == null)
            return;

        // 接続イベントを解除
        NetworkManager.Singleton
            .OnClientConnectedCallback -=
            OnClientConnected;

        // 切断イベントを解除
        NetworkManager.Singleton
            .OnClientDisconnectCallback -=
            OnClientDisconnected;
    }
}