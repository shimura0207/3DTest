using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>ホスト公開と並行して利用できる、部屋広告の受信処理。</summary>
public class UDPListener : MonoBehaviour {
    // roomIdを持たない旧版と、自動マッチングを混在させないための番号。
    public const int ProtocolVersion = 2;
    [Serializable]
    public struct UdpMessage {
        public int protocolVersion;
        public string roomId;
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
        public bool gamePlaying;
    }

    [Header("送信側と合わせる")]
    [SerializeField, Range(1, 65535)] private int discoveryPort = 55555;
    [SerializeField] private string gameName = "RG_CardGame_v1";
    [Header("ホスト情報の有効期限")]
    [SerializeField, Min(2f)] private float hostTimeoutSeconds = 5f;
    [Tooltip("同じPCの複数アプリがブロードキャスト受信ポートを共有する設定")]
    [SerializeField] private bool shareDiscoveryPort = true;
    [SerializeField] private bool verboseLogging;

    public List<UdpMessage> discoveredHosts = new List<UdpMessage>();
    public event Action<UdpMessage> onHostUpdated;
    public event Action<UdpMessage> onHostRemoved;
    public bool isGetIP => discoveredHosts.Count > 0;
    public bool IsReceiving => client != null;
    private UdpClient client;
    private readonly Dictionary<string, float> lastSeen = new Dictionary<string, float>();
    private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

    // 既存の一覧UIとの互換性のため、キー形式はIP:ポートを維持。
    public static string GetKey(UdpMessage host) => host.ip + ":" + host.port;

    /// <summary>公開停止後の古い広告を接続先として採用しないための確認。</summary>
    public bool IsFresh(UdpMessage host) {
        return IsReceiving && lastSeen.TryGetValue(GetKey(host), out float time) &&
            Time.unscaledTime - time <= Mathf.Max(2f, hostTimeoutSeconds);
    }

    public bool StartReceiveIP() {
        StopReceiveIP();
        discoveredHosts.Clear();
        lastSeen.Clear();
        if (!isActiveAndEnabled || discoveryPort < 1 || discoveryPort > 65535) {
            Debug.LogError("受信設定または有効状態を確認してください。", this);
            return false;
        }
        try {
            client = new UdpClient(AddressFamily.InterNetwork);
            // ゲーム用KCPポートの共有ではありません。検索用だけの設定です。
            if (shareDiscoveryPort) {
                client.Client.ExclusiveAddressUse = false;
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            client.Client.Blocking = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Any, discoveryPort));
            Debug.Log($"[UDP検索] 開始 port={discoveryPort} 自分={UDPBroadcaster.LocalRoomId}", this);
            return true;
        }
        catch (Exception e) {
            Debug.LogError($"受信開始失敗: {e.Message}", this);
            StopReceiveIP();
            return false;
        }
    }

    private void Update() {
        if (client == null) return;
        // 大量受信がゲームの1フレームを占有することを防ぎます。
        for (int i = 0; i < 32; i++) {
            if (!TryReceive(out byte[] bytes, out IPEndPoint sender)) break;
            if (!TryParse(bytes, sender, out UdpMessage host)) continue;
            RegisterHost(host);
            if (client == null) return;
        }
        RemoveExpiredHosts();
    }
    private bool TryReceive(out byte[] bytes, out IPEndPoint sender) {
        bytes = null;
        sender = new IPEndPoint(IPAddress.Any, 0);
        try {
            if (client == null || client.Available == 0) return false;
            bytes = client.Receive(ref sender);
            return true;
        }
        catch (SocketException e) {
            if (e.SocketErrorCode == SocketError.WouldBlock) return false;
            Debug.LogError($"受信エラー: {e.Message}", this);
            StopReceiveIP();
            return false;
        }
        catch (ObjectDisposedException) { StopReceiveIP(); return false; }
    }
    private bool TryParse(byte[] bytes, IPEndPoint sender, out UdpMessage host) {
        host = default;
        if (bytes == null || bytes.Length == 0 || bytes.Length > 4096) return false;
        try {
            string json = StrictUtf8.GetString(bytes).Trim();
            if (verboseLogging) Debug.Log($"[UDP受信] 送信元={sender} JSON={json}", this);
            if (!json.StartsWith("{") || !json.EndsWith("}")) return false;
            host = JsonUtility.FromJson<UdpMessage>(json);
            if (host.protocolVersion != ProtocolVersion || host.gameName != gameName ||
                host.port < 1 || host.port > 65535 || !Guid.TryParseExact(host.roomId, "N", out Guid id))
                return false;
            host.roomId = id.ToString("N");
            if (host.roomId == UDPBroadcaster.LocalRoomId) return false;
            host.ip = sender.Address.ToString();
            if (string.IsNullOrWhiteSpace(host.hostName)) host.hostName = host.ip;
            return true;
        }
        catch (ArgumentException) { return false; }
    }
    private void RegisterHost(UdpMessage host) {
        lastSeen[GetKey(host)] = Time.unscaledTime;
        int index = discoveredHosts.FindIndex(item => GetKey(item) == GetKey(host));
        if (index < 0) {
            discoveredHosts.Add(host);
            Debug.Log($"[UDP発見] {host.hostName} {host.ip}:{host.port} roomId={host.roomId}", this);
            onHostUpdated?.Invoke(host);
            return;
        }
        var previous = discoveredHosts[index];
        discoveredHosts[index] = host;
        if (previous.roomId != host.roomId || previous.hostName != host.hostName ||
            previous.gamePlaying != host.gamePlaying) onHostUpdated?.Invoke(host);
    }
    private void RemoveExpiredHosts() {
        for (int i = discoveredHosts.Count - 1; i >= 0; i--) {
            var host = discoveredHosts[i];
            if (IsFresh(host)) continue;
            discoveredHosts.RemoveAt(i);
            lastSeen.Remove(GetKey(host));
            onHostRemoved?.Invoke(host);
            // UIイベントから検索が停止・再開された場合、次フレームへ。
            if (client == null || i > discoveredHosts.Count) return;
        }
    }
    public void StopReceiveIP() {
        var oldClient = client;
        client = null;
        oldClient?.Close();
    }
    private void OnDisable() => StopReceiveIP();
    private void OnDestroy() => StopReceiveIP();
    private void OnApplicationQuit() => StopReceiveIP();
}
