/*
 * @brief  NetworkUI
 * @author Kobayashi
 */

using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour {
    [Header("UI")]

    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private TMP_InputField roomCodeInput;


    [Header("Network")]

    [SerializeField]
    private ushort port = 7777;

    [SerializeField]
    private RoomBroadcaster broadcaster;

    [SerializeField]
    private RoomFinder finder;

    [SerializeField]
    private MatchingManager matchingManager;


    // =========================================================
    // 初期化
    // =========================================================

    private void Start() {
        if (NetworkManager.Singleton == null) {
            Debug.LogError(
                "NetworkUI : " +
                "NetworkManagerが存在しません。"
            );

            return;
        }

        NetworkManager.Singleton
            .OnClientConnectedCallback +=
            OnClientConnected;

        NetworkManager.Singleton
            .OnClientDisconnectCallback +=
            OnClientDisconnected;

        NetworkManager.Singleton
            .OnServerStarted +=
            OnServerStarted;

        Debug.Log(
            "[NetworkUI] NetworkManagerイベント登録完了"
        );
    }


    // =========================================================
    // Host開始
    // =========================================================

    public void StartHost() {
        if (NetworkManager.Singleton == null) {
            SetStatus("Network Manager Missing");

            return;
        }

        if (NetWorkSystemManager.Instance == null || NetWorkSystemManager.Instance.UnityTransport == null) {
            SetStatus("Network System Missing");
            return;
        }

        var transport = NetWorkSystemManager.Instance.UnityTransport;

        transport.SetConnectionData(
            "0.0.0.0",
            port
        );

        Debug.Log(
            $"[NetworkUI] StartHost開始: " +
            $"0.0.0.0:{port}"
        );

        bool success =
            NetworkManager.Singleton.StartHost();

        Debug.Log(
            $"[NetworkUI] StartHost結果: {success}"
        );

        if (success) {
            if (broadcaster != null) {
                broadcaster.StartBroadcast(
                    port
                );

                SetStatus(
                    $"{broadcaster.RoomCode}"
                );
            }
            else {
                SetStatus(
                    "Host Started"
                );
            }
        }
        else {
            SetStatus(
                "Host Failed"
            );
        }
    }


    // =========================================================
    // Client開始
    // =========================================================

    public void StartClient() {
        if (NetworkManager.Singleton == null) {
            SetStatus(
                "Network Manager Missing"
            );

            return;
        }

        string code =
            roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code)) {
            SetStatus(
                "Enter Code"
            );

            return;
        }

        SetStatus(
            "Searching..."
        );

        if (finder == null) {
            Debug.LogError(
                "[NetworkUI] RoomFinderが設定されていません。"
            );

            SetStatus(
                "Room Finder Missing"
            );

            return;
        }

        finder.SearchRoom(code);
    }


    // =========================================================
    // Matching開始
    // =========================================================

    public void StartMatching() {
        if (matchingManager == null) {
            Debug.LogError(
                "[NetworkUI] " +
                "MatchingManagerが設定されていません。"
            );

            SetStatus(
                "Matching Manager Missing"
            );

            return;
        }

        if (NetworkManager.Singleton == null) {
            Debug.LogError(
                "[NetworkUI] " +
                "NetworkManagerがありません。"
            );

            SetStatus(
                "Network Manager Missing"
            );

            return;
        }

        SetStatus(
            "Matching..."
        );

        Debug.Log(
            "[NetworkUI] Matching開始"
        );

        matchingManager.StartMatching();
    }


    // =========================================================
    // Server開始完了
    // =========================================================

    private void OnServerStarted() {
        Debug.Log(
            "[NetworkUI] OnServerStarted"
        );
    }


    // =========================================================
    // Client接続成功
    // =========================================================

    private void OnClientConnected(
        ulong clientId) {
        Debug.Log(
            $"[NetworkUI] " +
            $"Client Connected : {clientId}"
        );

        if (NetworkManager.Singleton == null)
            return;

        ulong localClientId =
            NetworkManager.Singleton.LocalClientId;

        Debug.Log(
            $"[NetworkUI] " +
            $"LocalClientId : {localClientId}"
        );

        // 自分自身の接続完了
        if (localClientId == clientId) {
            SetStatus(
                "Connection"
            );

            Debug.Log(
                "[NetworkUI] " +
                "★★★ Connection完了 ★★★"
            );
        }
    }


    // =========================================================
    // Client切断
    // =========================================================

    private void OnClientDisconnected(
        ulong clientId) {
        Debug.LogWarning(
            $"[NetworkUI] " +
            $"Client Disconnected : {clientId}"
        );

        if (NetworkManager.Singleton == null)
            return;

        ulong localClientId =
            NetworkManager.Singleton.LocalClientId;

        Debug.LogWarning(
            $"[NetworkUI] " +
            $"LocalClientId : {localClientId}"
        );

        // -----------------------------------------------------
        // Client側の場合
        // -----------------------------------------------------

        if (NetworkManager.Singleton.IsClient
            &&
            !NetworkManager.Singleton.IsServer) {
            string reason =
                NetworkManager.Singleton.DisconnectReason;

            if (string.IsNullOrEmpty(reason)) {
                reason =
                    "理由不明";
            }

            Debug.LogError(
                $"[NetworkUI] " +
                $"Client接続失敗 / 切断理由: {reason}"
            );

            SetStatus(
                $"Disconnected\n{reason}"
            );

            return;
        }

        // -----------------------------------------------------
        // Host側
        // -----------------------------------------------------

        if (NetworkManager.Singleton.IsServer) {
            Debug.LogWarning(
                "[NetworkUI] " +
                "Host側でClientが切断しました。"
            );
        }
    }


    // =========================================================
    // UI表示
    // =========================================================

    private void SetStatus(
        string message) {
        Debug.Log(
            $"[NetworkUI] Status: {message}"
        );

        if (statusText != null) {
            statusText.text =
                message;
        }
    }


    // =========================================================
    // Destroy
    // =========================================================

    private void OnDestroy() {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton
            .OnClientConnectedCallback -=
            OnClientConnected;

        NetworkManager.Singleton
            .OnClientDisconnectCallback -=
            OnClientDisconnected;

        NetworkManager.Singleton
            .OnServerStarted -=
            OnServerStarted;
    }
}