using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using kcp2k;

/// <summary>
/// サーバーから両クライアントへ送る部屋状態。
/// </summary>
public struct CardRoomStateMessage : NetworkMessage {
    public bool started;
    public string text;
}

/// <summary>
/// 1シーン・2人対戦用の通信管理。
/// </summary>
public class CustomNetworkManager : NetworkManager {
    [Header("カードゲーム用の参照")]
    [SerializeField] private TitleManager title;
    [SerializeField] private UDPBroadcaster broadcaster;

    private readonly HashSet<int> readyPlayers =
        new HashSet<int>();

    private bool matchStarted;
    private bool stopping;

    public bool BeginHost() {
        if (!PrepareNetwork())
            return false;

        stopping = false;

        try {
            StartHost();
            return NetworkServer.active && NetworkClient.active;
        }
        catch (Exception exception) {
            Debug.LogException(exception, this);
            LeaveRoom();
            return false;
        }
    }

    public bool BeginClient(string ip, int port) {
        if (string.IsNullOrWhiteSpace(ip) ||
            port < 1 || port > 65535 ||
            !PrepareNetwork()) {
            return false;
        }

        stopping = false;

        // 選択した部屋が通知するゲーム接続ポートを使用
        KcpTransport kcp = (KcpTransport)transport;
        kcp.Port = (ushort)port;
        networkAddress = ip;

        try {
            StartClient();
            return NetworkClient.active;
        }
        catch (Exception exception) {
            Debug.LogException(exception, this);
            LeaveRoom();
            return false;
        }
    }

    private bool PrepareNetwork() {
        if (NetworkServer.active || NetworkClient.active) {
            Debug.LogWarning("すでに通信中です。", this);
            return false;
        }

        if (!(transport is KcpTransport)) {
            Debug.LogError(
                "TransportにKcpTransportを設定してください。",
                this);
            return false;
        }

        if (title == null) {
            Debug.LogError("Titleの参照が未設定です。", this);
            return false;
        }

        // この一式ではシーン移動・自動プレイヤー生成を行わない
        onlineScene = "";
        offlineScene = "";
        autoCreatePlayer = false;
        maxConnections = 2;

        return true;
    }

    public override void OnStartServer() {
        base.OnStartServer();

        readyPlayers.Clear();
        matchStarted = false;

        Debug.Log("[HOST] サーバー起動完了");

        if (broadcaster == null) {
            Debug.LogError(
                "[HOST] Broadcasterが未設定です。" +
                "CustomNetworkManagerのBroadcaster欄に設定してください。");
            return;
        }

        Debug.Log(
            $"[HOST] 配信オブジェクト={broadcaster.name} " +
            $"activeInHierarchy={broadcaster.gameObject.activeInHierarchy} " +
            $"enabled={broadcaster.enabled}");

        if (!(transport is KcpTransport kcp)) {
            Debug.LogError("[HOST] TransportがKcpTransportではありません。");
            return;
        }

        broadcaster.SetGamePlaying(false);

        bool started = broadcaster.StartSendIP(kcp.Port);

        Debug.Log($"[HOST] 配信開始結果={started}");
    }

    public override void OnStartClient() {
        base.OnStartClient();

        NetworkClient.RegisterHandler<CardRoomStateMessage>(
            OnRoomStateReceived);
    }

    public override void OnClientConnect() {
        base.OnClientConnect();

        // Auto Create PlayerはOFFだが、接続準備完了は通知する
        if (!NetworkClient.ready)
            NetworkClient.Ready();

        title.NotifyConnected();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn) {
        if (stopping || matchStarted) {
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);
    }

    public override void OnServerReady(NetworkConnectionToClient conn) {
        base.OnServerReady(conn);

        if (stopping)
            return;

        readyPlayers.Add(conn.connectionId);

        if (!matchStarted && readyPlayers.Count == 2) {
            matchStarted = true;

            if (broadcaster != null)
                broadcaster.SetGamePlaying(true);

            SendRoomState(true, "対戦開始");
        }
        else if (!matchStarted) {
            SendRoomState(false, "対戦相手を待っています... 1 / 2");
        }
    }

    public override void OnServerDisconnect(
        NetworkConnectionToClient conn) {
        bool wasReady = readyPlayers.Remove(conn.connectionId);

        base.OnServerDisconnect(conn);

        if (stopping || !wasReady)
            return;

        matchStarted = false;

        if (broadcaster != null)
            broadcaster.SetGamePlaying(false);

        // ホストは部屋に残り、新しい相手を待つ
        SendRoomState(
            false,
            "相手が退出しました。対戦相手を待っています...");
    }

    private void SendRoomState(bool started, string message) {
        if (!NetworkServer.active)
            return;

        NetworkServer.SendToAll(new CardRoomStateMessage {
            started = started,
            text = message
        });
    }

    private void OnRoomStateReceived(CardRoomStateMessage message) {
        if (title != null)
            title.NotifyRoomState(message.started, message.text);
    }

    public void LeaveRoom() {
        stopping = true;

        if (broadcaster != null)
            broadcaster.StopBroadcast();

        if (NetworkServer.active && NetworkClient.active)
            StopHost();
        else if (NetworkClient.active)
            StopClient();
        else if (NetworkServer.active)
            StopServer();
    }

    public override void OnStopClient() {
        base.OnStopClient();

        if (title != null)
            title.NotifyDisconnected();
    }

    public override void OnStopServer() {
        stopping = true;
        readyPlayers.Clear();
        matchStarted = false;

        if (broadcaster != null)
            broadcaster.StopBroadcast();

        base.OnStopServer();
    }
}