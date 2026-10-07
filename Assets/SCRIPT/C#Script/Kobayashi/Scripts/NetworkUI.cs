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
    }


    // =========================================================
    // Host開始
    // =========================================================

    public void StartHost() {
        if (NetworkManager.Singleton == null) {
            statusText.text =
                "Network Manager Missing";

            return;
        }

        if (
            NetWorkSystemManager.Instance == null
            ||
            NetWorkSystemManager.Instance.UnityTransport == null) {
            statusText.text =
                "Network System Missing";

            return;
        }

        var transport =
            NetWorkSystemManager.Instance.UnityTransport;

        transport.SetConnectionData(
            "0.0.0.0",
            port
        );

        bool success =
            NetworkManager.Singleton.StartHost();

        if (success) {
            if (broadcaster != null) {
                broadcaster.StartBroadcast(
                    port
                );

                statusText.text =
                    $"{broadcaster.RoomCode}";
            }
            else {
                statusText.text =
                    "Host";
            }
        }
        else {
            statusText.text =
                "Host Failed";
        }
    }


    // =========================================================
    // Client開始
    // =========================================================

    public void StartClient() {
        string code =
            roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code)) {
            statusText.text =
                "Enter Code";

            return;
        }

        statusText.text =
            "Searching...";

        if (finder == null) {
            Debug.LogError(
                "RoomFinderが設定されていません。"
            );

            statusText.text =
                "Room Finder Missing";

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
                "NetworkUI : " +
                "MatchingManagerが設定されていません。"
            );

            statusText.text =
                "Matching Manager Missing";

            return;
        }

        statusText.text =
            "Matching...";

        matchingManager.StartMatching();
    }


    // =========================================================
    // Netcode接続成功
    // =========================================================

    private void OnClientConnected(
        ulong clientId) {
        Debug.Log(
            $"Client Connected : {clientId}"
        );

        if (
            NetworkManager.Singleton.LocalClientId
            == clientId) {
            statusText.text =
                "Connection";

            Debug.Log(
                "自分のNetcode接続が完了しました。"
            );
        }
    }


    // =========================================================
    // Netcode接続切断
    // =========================================================

    private void OnClientDisconnected(
        ulong clientId) {
        Debug.Log(
            $"Client Disconnected : {clientId}"
        );

        if (
            NetworkManager.Singleton != null
            &&
            NetworkManager.Singleton.LocalClientId
            == clientId) {
            statusText.text =
                "Disconnected";
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
    }
}