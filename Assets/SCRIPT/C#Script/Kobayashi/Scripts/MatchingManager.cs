using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 同じLANにある「別々のPC」2台用の自動マッチング。
/// 相手探しはUDP、ゲーム接続はNetcode for GameObjects + UnityTransport。
/// 両方のPCでこの完全版を使用してください。旧版とは通信形式が異なります。
///
/// 導入:
/// 1. 既存MatchingManager.csをこのファイルで置き換える。
/// 2. シーンにNetworkManagerとUnityTransportを用意する。
/// 3. InspectorのNetwork Manager / Transportにそれぞれ設定する。
/// 4. NetworkManagerのConnection ApprovalはOFFにしておく。
///    このスクリプトが2人用の接続承認を一時的に設定する。
/// 5. ボタンのOnClickにStartMatching()を登録する。
/// 6. OnMatchConnectedに「接続完了後の処理」を登録する。
///
/// 注意:
/// ・WebGL、インターネット越し、同一PCの複数起動はこの自動探索の対象外。
/// ・学校Wi-Fiの端末隔離、OSファイアウォールはコードでは解除できない。
/// ・このコンポーネントは接続中も有効なまま維持すること。
/// ・同じNetworkManagerを別の接続管理スクリプトと同時操作しないこと。
/// ・既存の接続承認処理を持つプロジェクトでは、その処理との統合が必要。
/// </summary>
[DisallowMultipleComponent]
public class MatchingManager : MonoBehaviour {
    public enum MatchState {
        Idle,                 // まだ開始していない、または停止済み
        Searching,            // LANへ待機通知を出し、相手を探している
        WaitingForHost,       // ホスト候補へ参加要求を送り、起動完了を待つ
        WaitingForClient,     // ホストとして起動済み。相手の接続を待つ
        Connecting,           // クライアントを起動済み。まだ接続完了ではない
        Connected,            // 接続完了コールバックを受け取った
        Failed                // エラー、タイムアウト、切断
    }

    [Serializable]
    public class MessageEvent : UnityEvent<string> { }

    [Header("Network：実際に使用するNetworkManagerとTransport")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private UnityTransport transport;

    [Header("Matching：両方のPCで同じ設定にする")]
    [Tooltip("他のゲームや旧版の通知を無視するための識別文字列。| は使わない。")]
    [SerializeField] private string applicationKey = "RG_LAN_V2";
    [SerializeField] private ushort matchingPort = 9001;
    [SerializeField] private ushort gamePort = 7777;
    [Min(0.2f)][SerializeField] private float retryInterval = 1f;
    [Tooltip("相手が見つからない場合の制限秒数。0なら無制限。")]
    [Min(0f)][SerializeField] private float searchTimeout = 60f;
    [Tooltip("相手決定後、実際の接続完了まで待つ最大秒数。")]
    [Min(5f)][SerializeField] private float connectionTimeout = 20f;

    [Header("Events：Inspectorで必要な処理を登録")]
    [SerializeField] private UnityEvent onMatchConnected = new UnityEvent();
    [SerializeField] private MessageEvent onMatchFailed = new MessageEvent();

    [Header("Debug")]
    [SerializeField] private bool verboseLog = true;
    [SerializeField] private MatchState state = MatchState.Idle;

    public MatchState State => state;
    public bool IsMatched => state == MatchState.Connected;
    public bool IsMatching => state == MatchState.Searching
        || state == MatchState.WaitingForHost
        || state == MatchState.WaitingForClient
        || state == MatchState.Connecting;
    public string LastError { get; private set; } = "";

    // UDP受信はUpdateで少しずつ処理する。
    // 非同期コールバックからUnity APIを呼ばないのでMainThreadDispatcherは不要。
    private UdpClient udp;
    private readonly List<IPAddress> broadcastAddresses = new List<IPAddress>();
    private string playerId;
    private long startedAtUtc;
    private float phaseStartedAt;
    private float nextSendAt;

    // 相手を決めたら固定し、別の相手からの通知で上書きしない。
    private string peerId;
    private IPEndPoint peerEndpoint;
    private ushort peerGamePort;

    // このスクリプトが起動したネットワークだけをShutdownする。
    private bool ownsNetwork;
    private bool eventsRegistered;
    private bool approvalInstalled;
    private bool previousApproval;
    private byte[] previousConnectionData;
    private Action<NetworkManager.ConnectionApprovalRequest,
        NetworkManager.ConnectionApprovalResponse> previousApprovalCallback;
    private ulong? reservedClientId;

    // 連続受信で1フレームが占有されないよう上限を設ける。
    private const int MaxPacketsPerFrame = 32;
    private const int MaxPacketBytes = 1024;

    /// <summary>ボタンなどから呼び出すマッチング開始処理。</summary>
    public void StartMatching() {
        if (!isActiveAndEnabled) {
            Debug.LogWarning("[Matching] 有効なGameObject上で開始してください。", this);
            return;
        }
        if (IsMatching || IsMatched) {
            Debug.LogWarning("[Matching] マッチング中または接続済みです。", this);
            return;
        }

        // Inspectorの参照を優先し、未設定ならSingletonから取得する。
        if (networkManager == null) networkManager = NetworkManager.Singleton;
        if (networkManager == null) {
            Fail("NetworkManagerが見つかりません。");
            return;
        }
        if (networkManager.IsListening || networkManager.ShutdownInProgress) {
            Debug.LogWarning("[Matching] NetworkManagerが動作中/終了処理中です。終了後に再試行してください。", this);
            return;
        }

        // 設定を書き換えるTransportと、Netcodeが使用するTransportが同じか検証。
        // 別コンポーネントを設定しても、実際の接続先には反映されないため。
        if (transport == null)
            transport = networkManager.NetworkConfig.NetworkTransport as UnityTransport;
        if (transport == null || networkManager.NetworkConfig.NetworkTransport != transport) {
            Fail("NetworkManagerが使用するUnityTransportをTransport欄に設定してください。");
            return;
        }
        if (matchingPort == 0 || gamePort == 0 || matchingPort == gamePort
            || string.IsNullOrWhiteSpace(applicationKey) || applicationKey.Contains("|")
            || applicationKey.Length > 64) {
            Fail("ポートは異なる1～65535の値、Application Keyは64文字以内・空欄と | 不可です。");
            return;
        }

        // 既存の接続承認を無断で置き換えない。
        if (networkManager.NetworkConfig.ConnectionApproval
            || networkManager.ConnectionApprovalCallback != null) {
            Fail("既存のConnection Approval設定があります。この版の承認処理と統合するか、単独テストでは既存承認を解除してください。");
            return;
        }

        LastError = "";
        playerId = Guid.NewGuid().ToString("N");
        startedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        peerId = null;
        peerEndpoint = null;
        reservedClientId = null;

        try {
            // 接続イベントをStartHost/StartClientより前に登録する。
            RegisterNetworkEvents();
            InstallApproval();
            BuildBroadcastAddresses();

            // このポートは1プロセスだけが使用する。
            // 同じPCでEditorとビルドを同時に使うテストには向かない。
            udp = new UdpClient(AddressFamily.InterNetwork);
            udp.Client.ExclusiveAddressUse = true;
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, matchingPort));
            udp.EnableBroadcast = true;
            udp.Client.Blocking = false;

            ChangeState(MatchState.Searching);
            nextSendAt = 0f; // 次のUpdateですぐに最初の通知を送る。
            Log("探索開始 ID=" + playerId + " / UDP=" + matchingPort);
        }
        catch (Exception ex) {
            Fail("開始できませんでした。ポートの使用状況も確認してください。\n" + ex.Message);
        }
    }

    private void Update() {
        // 受信・状態変更・Netcodeの起動をすべてメインスレッドで行う。
        // Socketを非ブロッキングにしているので、通信を待って画面を止めない。
        ReceivePackets();
        if (!IsMatching) return;

        float now = Time.realtimeSinceStartup;
        float limit = state == MatchState.Searching ? searchTimeout : connectionTimeout;
        if (limit > 0f && now - phaseStartedAt >= limit) {
            Fail("タイムアウト。状態=" + state
                + " / 相手=" + (peerEndpoint == null ? "未発見" : peerEndpoint.ToString())
                + " / 接続先ゲームポート=" + peerGamePort
                + "。両PCのログ、LAN設定、UDPポートの許可を確認してください。");
            return;
        }

        // WAITだけでなくJOIN/FOUNDも再送し、UDPの一時的な取りこぼしに対応。
        if (now < nextSendAt) return;
        nextSendAt = now + Mathf.Max(0.2f, retryInterval);
        if (state == MatchState.Searching) {
            string message = applicationKey + "|WAIT|" + playerId + "|"
                + startedAtUtc + "|" + gamePort;
            foreach (IPAddress address in broadcastAddresses)
                Send(message, new IPEndPoint(address, matchingPort));
        }
        else if (state == MatchState.WaitingForHost) {
            SendJoin();
        }
        else if (state == MatchState.WaitingForClient) {
            SendFound();
        }
    }

    /// <summary>受信済みデータだけを処理。過剰なパケットや別ゲームの通知は捨てる。</summary>
    private void ReceivePackets() {
        for (int i = 0; i < MaxPacketsPerFrame && udp != null; i++) {
            try {
                if (!udp.Client.Poll(0, SelectMode.SelectRead)) break;
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udp.Receive(ref sender);
                if (data.Length == 0 || data.Length > MaxPacketBytes) continue;
                if (sender.Port != matchingPort) continue;

                string[] parts = Encoding.UTF8.GetString(data).Split('|');
                if (parts.Length < 2 || parts[0] != applicationKey) continue;
                switch (parts[1]) {
                    case "WAIT": HandleWait(parts, sender); break;
                    case "JOIN": HandleJoin(parts, sender); break;
                    case "FOUND": HandleFound(parts, sender); break;
                }
            }
            catch (SocketException ex) {
                // 受信する瞬間にデータがなくなった場合は次フレームへ。
                if (ex.SocketErrorCode == SocketError.WouldBlock) break;
                Fail("UDP受信エラー: " + ex.SocketErrorCode + " / " + ex.Message);
                break;
            }
            catch (Exception ex) {
                Fail("受信処理エラー: " + ex.Message);
                break;
            }
        }
    }

    private void HandleWait(string[] parts, IPEndPoint sender) {
        if (state != MatchState.Searching || parts.Length != 5) return;
        string otherId = parts[2];
        if (!Guid.TryParseExact(otherId, "N", out _) || otherId == playerId) return;
        if (!long.TryParse(parts[3], out long otherStart)) return;
        if (!ushort.TryParse(parts[4], out ushort otherPort) || otherPort == 0) return;

        // 早い開始時刻の側をホストにする。同じ時刻ならIDの辞書順で決定。
        // PCの時計がずれていても比較結果は一致するが、現実の開始順とは限らない。
        bool otherIsHost = otherStart < startedAtUtc
            || (otherStart == startedAtUtc && string.CompareOrdinal(otherId, playerId) < 0);
        if (!otherIsHost) return;

        peerId = otherId;
        peerEndpoint = new IPEndPoint(sender.Address, sender.Port);
        peerGamePort = otherPort;
        ChangeState(MatchState.WaitingForHost);
        Log("ホスト候補=" + peerEndpoint.Address + ":" + peerGamePort);
        SendJoin();
    }

    private void SendJoin() {
        Send(applicationKey + "|JOIN|" + peerId + "|" + playerId, peerEndpoint);
    }

    private void HandleJoin(string[] parts, IPEndPoint sender) {
        if (parts.Length != 4 || parts[2] != playerId || parts[3] == playerId) return;
        if (!Guid.TryParseExact(parts[3], "N", out _)) return;

        // 同じ相手のJOIN再送にはFOUNDを再送。別の相手には乗り換えない。
        if (state == MatchState.WaitingForClient) {
            if (parts[3] == peerId && IsPeer(sender)) SendFound();
            return;
        }
        if (state != MatchState.Searching) return;

        peerId = parts[3];
        peerEndpoint = new IPEndPoint(sender.Address, sender.Port);
        peerGamePort = gamePort;
        ChangeState(MatchState.WaitingForClient);

        // 第1引数はホスト自身のローカルクライアント用アドレス。
        // 第3引数の0.0.0.0で、全IPv4ネットワークインターフェースから待ち受ける。
        transport.SetConnectionData("127.0.0.1", gamePort, "0.0.0.0");
        ownsNetwork = true;
        bool started = networkManager.StartHost();
        // 起動中のTransportエラーでFailが先に呼ばれた場合は二重処理しない。
        if (state != MatchState.WaitingForClient) return;
        if (!started) {
            Fail("StartHostに失敗しました。ゲーム用ポートの競合を確認してください。");
            return;
        }
        Log("ホスト起動完了。相手の接続待ち / 0.0.0.0:" + gamePort);
        SendFound();
    }

    private void SendFound() {
        Send(applicationKey + "|FOUND|" + playerId + "|" + peerId + "|" + gamePort,
            peerEndpoint);
    }

    private void HandleFound(string[] parts, IPEndPoint sender) {
        if (state != MatchState.WaitingForHost || parts.Length != 5) return;
        if (parts[2] != peerId || parts[3] != playerId || !IsPeer(sender)) return;
        if (!ushort.TryParse(parts[4], out ushort port) || port != peerGamePort) return;

        // 本文に書かれた自己申告IPではなく、実際に受信したパケットの送信元を使う。
        // 「最初に見つかったNICのIP」を返す旧GetLocalIPは不要になる。
        transport.SetConnectionData(sender.Address.ToString(), port);
        networkManager.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(playerId);
        ChangeState(MatchState.Connecting);
        ownsNetwork = true;
        bool started = networkManager.StartClient();
        if (state != MatchState.Connecting) return;
        if (!started) {
            Fail("StartClientに失敗しました。");
            return;
        }
        Log("クライアント起動成功。接続完了待ち / " + sender.Address + ":" + port);
    }

    private bool IsPeer(IPEndPoint sender) {
        return peerEndpoint != null && sender.Address.Equals(peerEndpoint.Address)
            && sender.Port == peerEndpoint.Port;
    }

    private void Send(string message, IPEndPoint target) {
        if (udp == null || target == null) return;
        try {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udp.Send(data, data.Length, target);
        }
        catch (SocketException ex) {
            // 1つのNICの送信失敗で全体を中断しない。次回再送とタイムアウトで判断。
            if (verboseLog) Debug.LogWarning("[Matching] UDP送信失敗 " + target
                + " / " + ex.SocketErrorCode, this);
        }
    }

    private void BuildBroadcastAddresses() {
        broadcastAddresses.Clear();
        broadcastAddresses.Add(IPAddress.Broadcast);
        // 使用可能な全NICのサブネット宛ても追加し、有線/Wi-Fi複数構成に対応。
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()) {
            if (nic.OperationalStatus != OperationalStatus.Up
                || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            foreach (UnicastIPAddressInformation info in nic.GetIPProperties().UnicastAddresses) {
                if (info.Address.AddressFamily != AddressFamily.InterNetwork
                    || info.IPv4Mask == null) continue;
                byte[] address = info.Address.GetAddressBytes();
                byte[] mask = info.IPv4Mask.GetAddressBytes();
                byte[] result = new byte[4];
                for (int i = 0; i < 4; i++) result[i] = (byte)(address[i] | ~mask[i]);
                IPAddress broadcast = new IPAddress(result);
                if (!broadcastAddresses.Contains(broadcast)) broadcastAddresses.Add(broadcast);
                Log("NIC=" + nic.Name + " / IPv4=" + info.Address + " / Broadcast=" + broadcast);
            }
        }
    }

    private void InstallApproval() {
        // 終了時に戻せるよう、変更前の設定を保存する。
        previousApproval = networkManager.NetworkConfig.ConnectionApproval;
        previousConnectionData = networkManager.NetworkConfig.ConnectionData;
        previousApprovalCallback = networkManager.ConnectionApprovalCallback;
        networkManager.NetworkConfig.ConnectionApproval = true;
        networkManager.ConnectionApprovalCallback = ApproveConnection;
        approvalInstalled = true;
    }

    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response) {
        // ホスト自身は許可。外部クライアントは選んだ相手のIDと一致する1人だけ許可。
        // このIDはLANに公開される識別子であり、暗号認証・不正対策用の秘密ではない。
        bool localHost = request.ClientNetworkId == NetworkManager.ServerClientId;
        string claimedId = request.Payload == null ? "" : Encoding.UTF8.GetString(request.Payload);
        bool allowed = localHost || (state == MatchState.WaitingForClient
            && claimedId == peerId && (!reservedClientId.HasValue
                || reservedClientId.Value == request.ClientNetworkId));
        if (allowed && !localHost) reservedClientId = request.ClientNetworkId;
        response.Approved = allowed;
        response.CreatePlayerObject = allowed && networkManager.NetworkConfig.PlayerPrefab != null;
        response.Pending = false;
        response.Reason = allowed ? "" : "This match is reserved for another client.";
    }

    private void RegisterNetworkEvents() {
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        networkManager.OnTransportFailure += OnTransportFailure;
        eventsRegistered = true;
    }

    private void OnClientConnected(ulong clientId) {
        if (!ownsNetwork) return;
        if (networkManager.IsHost) {
            // ホスト起動時はホスト自身の接続通知も来る。これを対戦成立と誤認しない。
            if (clientId == NetworkManager.ServerClientId) return;
            if (state != MatchState.WaitingForClient || reservedClientId != clientId) return;
        }
        else {
            if (state != MatchState.Connecting || clientId != networkManager.LocalClientId) return;
        }

        ChangeState(MatchState.Connected);
        CloseDiscovery(); // ゲーム通信は維持し、相手探し用UDPだけ閉じる。
        Debug.Log("[Matching] ★接続完了！ role="
            + (networkManager.IsHost ? "Host" : "Client") + " / ClientId=" + clientId, this);
        onMatchConnected.Invoke();
    }

    private void OnClientDisconnected(ulong clientId) {
        if (!ownsNetwork) return;
        // ホスト側では、拒否した第三者の切断通知で対戦を終了させない。
        if (networkManager.IsHost && clientId != NetworkManager.ServerClientId
            && (!reservedClientId.HasValue || clientId != reservedClientId.Value)) return;
        string reason = networkManager.DisconnectReason;
        Fail("接続失敗または切断。ClientId=" + clientId + " / 理由="
            + (string.IsNullOrEmpty(reason) ? "詳細通知なし。相手側ログも確認してください。" : reason));
    }

    private void OnTransportFailure() {
        if (ownsNetwork) Fail("Transportエラー。ポート競合やネットワーク状態を確認してください。");
    }

    private void ChangeState(MatchState next) {
        Log(state + " → " + next);
        state = next;
        phaseStartedAt = Time.realtimeSinceStartup;
    }

    private void Fail(string message) {
        LastError = message;
        Cleanup(true);
        state = MatchState.Failed;
        Debug.LogError("[Matching] " + message, this);
        onMatchFailed.Invoke(message);
    }

    /// <summary>
    /// マッチングをキャンセルする。接続済みならゲーム通信を維持する。
    /// 接続待ちでこのスクリプトが起動したHost/Clientは停止する。
    /// </summary>
    public void StopMatching() {
        if (IsMatched) {
            CloseDiscovery();
            return;
        }
        Cleanup(true);
        state = MatchState.Idle;
        Log("マッチング停止");
    }

    /// <summary>退出ボタン用。接続済みゲーム通信も含めて終了する。</summary>
    public void Disconnect() {
        Cleanup(true);
        state = MatchState.Idle;
        Log("通信終了。再開はNetworkManagerの終了処理完了後に行ってください。");
    }

    private void CloseDiscovery() {
        if (udp == null) return;
        udp.Close();
        udp = null;
    }

    private void Cleanup(bool shutdown) {
        CloseDiscovery();
        if (networkManager != null) {
            // 自分が起こすShutdownで失敗イベントを再発火させないよう、先に解除。
            if (eventsRegistered) {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
                networkManager.OnTransportFailure -= OnTransportFailure;
            }
            if (shutdown && ownsNetwork) networkManager.Shutdown();
            if (approvalInstalled) {
                networkManager.ConnectionApprovalCallback = previousApprovalCallback;
                networkManager.NetworkConfig.ConnectionApproval = previousApproval;
                networkManager.NetworkConfig.ConnectionData = previousConnectionData;
            }
        }
        eventsRegistered = false;
        approvalInstalled = false;
        ownsNetwork = false;
        reservedClientId = null;
    }

    private void Log(string message) {
        if (verboseLog) Debug.Log("[Matching] " + message, this);
    }

    private void OnDisable() {
        // この管理オブジェクトを無効化/破棄すると、自分が開始した通信も終了する。
        // 接続後にUIだけ隠す場合は、このスクリプトのGameObjectを無効化しないこと。
        Cleanup(true);
        state = MatchState.Idle;
    }
}
