using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class LanMatchmaker : MonoBehaviour {
    [Header("UI")]
    // マッチングボタン
    [SerializeField] private Button matchingButton;
    // 接続中などに表示するテキスト
    [SerializeField] private TMP_Text statusText;

    [Header("Network")]
    [SerializeField] private ushort gamePort = 7777;
    [SerializeField] private ushort discoveryPort = 47777;

    private NetworkManager networkManager;
    private UnityTransport transport;
    private UdpClient hostDiscoverySocket;

    // 検索中かどうか
    private bool isSearching;

    private void Start() {
        networkManager = NetworkManager.Singleton;

        // NetworkManagerがなければ
        if (networkManager == null) {
            SetStatus("NetworkManager not found.");
            enabled = false;

            // 処理を終える
            return;
        }

        transport = networkManager.GetComponent<UnityTransport>();

        if (transport == null) {
            SetStatus("UnityTransport not found.");
            enabled = false;

            // 処理を終える
            return;
        }

        networkManager.NetworkConfig.ConnectionApproval = true;
        networkManager.ConnectionApprovalCallback += ApprovalCheck;

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        matchingButton.onClick.AddListener(OnMatchingClicked);

        // テキストの変更
        SetStatus("Ready");
    }

    private void OnMatchingClicked() {
        if (isSearching || networkManager.IsListening)
            return;

        StartCoroutine(MatchingRoutine());
    }

    private IEnumerator MatchingRoutine() {
        isSearching = true;
        matchingButton.interactable = false;
        SetStatus("Searching...");

        UdpClient discoveryClient = null;
        string foundHostIp = null;
        ushort foundHostPort = gamePort;
        bool sawFullRoom = false;

        try {
            yield return new WaitForSeconds(
                UnityEngine.Random.Range(0.2f, 0.8f)
            );

            discoveryClient = new UdpClient(0);
            discoveryClient.EnableBroadcast = true;

            IPEndPoint broadcastEndpoint =
                new IPEndPoint(IPAddress.Broadcast, discoveryPort);

            float startTime = Time.realtimeSinceStartup;
            float nextSendTime = 0f;

            while (Time.realtimeSinceStartup - startTime < 3f) {
                if (Time.realtimeSinceStartup >= nextSendTime) {
                    byte[] request = Encoding.UTF8.GetBytes("DISCOVER");

                    discoveryClient.Send(
                        request, request.Length, broadcastEndpoint
                    );

                    nextSendTime = Time.realtimeSinceStartup + 0.25f;
                }

                while (discoveryClient.Available > 0) {
                    IPEndPoint remoteEndpoint =
                        new IPEndPoint(IPAddress.Any, 0);

                    byte[] data = discoveryClient.Receive(
                        ref remoteEndpoint
                    );

                    string message = Encoding.UTF8.GetString(data);

                    if (message == "FULL") {
                        sawFullRoom = true;
                        continue;
                    }

                    string[] parts = message.Split('|');

                    if (parts.Length == 2 &&
                        parts[0] == "HOST" &&
                        ushort.TryParse(parts[1], out ushort port)) {
                        foundHostIp = remoteEndpoint.Address.ToString();
                        foundHostPort = port;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(foundHostIp))
                    break;

                yield return null;
            }
        }
        finally {
            if (discoveryClient != null)
                discoveryClient.Close();
        }

        if (!string.IsNullOrEmpty(foundHostIp)) {
            SetStatus("Connecting...");

            transport.SetConnectionData(foundHostIp, foundHostPort);

            if (!networkManager.StartClient()) {
                SetStatus("Connection failed. Try again.");
                FinishSearch();
            }
        }
        else if (sawFullRoom) {
            SetStatus("Room is full.");
            FinishSearch();
        }
        else {
            SetStatus("Creating room...");

            transport.SetConnectionData(
                "127.0.0.1", gamePort, "0.0.0.0"
            );

            if (networkManager.StartHost()) {
                SetStatus("Waiting for another player...");
                StartCoroutine(HostDiscoveryRoutine());
            }
            else {
                SetStatus("Host start failed.");
                FinishSearch();
            }
        }
    }

    private IEnumerator HostDiscoveryRoutine() {
        // UDPソケットの作成時だけ例外を処理する。
        try {
            hostDiscoverySocket = new UdpClient(discoveryPort);
        }
        catch (Exception e) {
            Debug.LogError("Host discovery start error: " + e.Message);
            yield break;
        }

        while (networkManager != null && networkManager.IsHost) {
            try {
                while (hostDiscoverySocket != null &&
                       hostDiscoverySocket.Available > 0) {
                    IPEndPoint remoteEndpoint =
                        new IPEndPoint(IPAddress.Any, 0);

                    byte[] data = hostDiscoverySocket.Receive(
                        ref remoteEndpoint
                    );

                    string message = Encoding.UTF8.GetString(data);

                    if (message != "DISCOVER")
                        continue;

                    int playerCount =
                        networkManager.ConnectedClientsIds.Count;

                    string reply = playerCount >= 2
                        ? "FULL"
                        : "HOST|" + gamePort;

                    byte[] response =
                        Encoding.UTF8.GetBytes(reply);

                    hostDiscoverySocket.Send(
                        response,
                        response.Length,
                        remoteEndpoint
                    );
                }
            }
            catch (Exception e) {
                Debug.LogError("Host discovery error: " + e.Message);
                break;
            }

            // yield return は try-catch の外に置く。
            yield return new WaitForSeconds(0.05f);
        }

        if (hostDiscoverySocket != null) {
            hostDiscoverySocket.Close();
            hostDiscoverySocket = null;
        }
    }

    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response) {
        // Host自身が1人いる場合、あと1人だけ許可する。
        bool roomAvailable =
            networkManager.ConnectedClientsIds.Count < 2;

        response.Approved = roomAvailable;
        response.CreatePlayerObject = false;
        response.Pending = false;

        if (!roomAvailable)
            response.Reason = "Room is full.";
    }

    private void OnClientConnected(ulong clientId) {
        if (clientId != networkManager.LocalClientId)
            return;

        isSearching = false;
        matchingButton.interactable = false;

        if (networkManager.IsHost) {
            SetStatus("Connected! You are Host.");
        }
        else {
            SetStatus("Connected! You are Client.");
        }
    }

    private void OnClientDisconnected(ulong clientId) {
        if (clientId == networkManager.LocalClientId) {
            SetStatus("Disconnected. Press Matching to retry.");
            FinishSearch();
        }
        else if (networkManager.IsHost) {
            SetStatus("Player left. Waiting for another player...");
        }
    }

    private void FinishSearch() {
        isSearching = false;

        if (matchingButton != null &&
            networkManager != null &&
            !networkManager.IsListening) {
            matchingButton.interactable = true;
        }
    }

    private void SetStatus(string message) {
        if (statusText != null)
            statusText.text = message;

        Debug.Log("[LAN Matchmaking] " + message);
    }

    private void OnDestroy() {
        if (matchingButton != null)
            matchingButton.onClick.RemoveListener(OnMatchingClicked);

        if (networkManager != null) {
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        if (hostDiscoverySocket != null) {
            hostDiscoverySocket.Close();
            hostDiscoverySocket = null;
        }
    }
}