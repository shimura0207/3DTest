using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPListener : MonoBehaviour {
    [Serializable]
    public struct UdpMessage {
        public string ip;
        public int port;
        public string gameName;
        public string hostName;
        public bool gamePlaying;
    }

    [Header("送信側と合わせる")]
    [SerializeField, Range(1, 65535)]
    private int discoveryPort = 55555;

    [SerializeField]
    private string gameName = "RG_CardGame_v1";

    [Header("ホスト情報の有効期限")]
    [SerializeField, Min(2f)]
    private float hostTimeoutSeconds = 5f;

    public List<UdpMessage> discoveredHosts = new List<UdpMessage>();

    public event Action<UdpMessage> onHostUpdated;
    public event Action<UdpMessage> onHostRemoved;

    public bool isGetIP => discoveredHosts.Count > 0;
    public bool IsReceiving => client != null;

    private UdpClient client;

    private readonly Dictionary<string, float> lastSeen =
        new Dictionary<string, float>();

    private static readonly UTF8Encoding StrictUtf8 =
        new UTF8Encoding(false, true);

    public static string GetKey(UdpMessage host) {
        return host.ip + ":" + host.port;
    }

    public bool StartReceiveIP() {
        StopReceiveIP();
        discoveredHosts.Clear();
        lastSeen.Clear();

        if (!isActiveAndEnabled ||
            discoveryPort < 1 || discoveryPort > 65535) {
            Debug.LogError("受信設定または有効状態を確認してください。", this);
            return false;
        }

        try {
            client = new UdpClient(AddressFamily.InterNetwork);
            client.Client.Blocking = false;
            client.Client.Bind(
                new IPEndPoint(IPAddress.Any, discoveryPort));

            return true;
        }
        catch (Exception exception) {
            Debug.LogError($"受信開始失敗: {exception.Message}", this);
            StopReceiveIP();
            return false;
        }
    }

    private void Update() {
        if (client == null)
            return;

        // 大量受信でフレームを占有しない
        for (int i = 0; i < 32; i++) {
            if (!TryReceive(out byte[] bytes, out IPEndPoint sender))
                break;

            if (!TryParse(bytes, sender, out UdpMessage host))
                continue;

            RegisterHost(host);

            if (client == null)
                return;
        }

        RemoveExpiredHosts();
    }

    private bool TryReceive(out byte[] bytes, out IPEndPoint sender) {
        bytes = null;
        sender = new IPEndPoint(IPAddress.Any, 0);

        try {
            if (client == null || client.Available == 0)
                return false;

            bytes = client.Receive(ref sender);
            return true;
        }
        catch (SocketException exception) {
            if (exception.SocketErrorCode == SocketError.WouldBlock)
                return false;

            Debug.LogError($"受信エラー: {exception.Message}", this);
            StopReceiveIP();
            return false;
        }
        catch (ObjectDisposedException) {
            StopReceiveIP();
            return false;
        }
    }

    private bool TryParse(
        byte[] bytes,
        IPEndPoint sender,
        out UdpMessage host) {
        host = default;

        if (bytes == null || bytes.Length == 0 || bytes.Length > 4096)
            return false;

        try {
            string json = StrictUtf8.GetString(bytes).Trim();
            // 追加：フィルターで除外する前の受信内容を確認
            Debug.Log($"[UDP受信] 送信元={sender} JSON={json}");
            if (!json.StartsWith("{") || !json.EndsWith("}"))
                return false;

            host = JsonUtility.FromJson<UdpMessage>(json);

            if (host.gameName != gameName ||
                host.port < 1 || host.port > 65535) {
                return false;
            }

            host.ip = sender.Address.ToString();

            if (string.IsNullOrWhiteSpace(host.hostName))
                host.hostName = host.ip;

            return true;
        }
        catch (ArgumentException) {
            return false;
        }
    }

    private void RegisterHost(UdpMessage host) {
        lastSeen[GetKey(host)] = Time.unscaledTime;

        int index = discoveredHosts.FindIndex(
            item => item.ip == host.ip && item.port == host.port);

        if (index < 0) {
            discoveredHosts.Add(host);
            onHostUpdated?.Invoke(host);
            return;
        }

        UdpMessage previous = discoveredHosts[index];
        discoveredHosts[index] = host;

        if (previous.hostName != host.hostName ||
            previous.gamePlaying != host.gamePlaying) {
            onHostUpdated?.Invoke(host);
        }
    }

    private void RemoveExpiredHosts() {
        float now = Time.unscaledTime;

        for (int i = discoveredHosts.Count - 1; i >= 0; i--) {
            UdpMessage host = discoveredHosts[i];
            string key = GetKey(host);

            if (lastSeen.TryGetValue(key, out float time) &&
                now - time <= Mathf.Max(2f, hostTimeoutSeconds)) {
                continue;
            }

            discoveredHosts.RemoveAt(i);
            lastSeen.Remove(key);
            onHostRemoved?.Invoke(host);
        }
    }

    public void StopReceiveIP() {
        UdpClient oldClient = client;
        client = null;
        oldClient?.Close();
    }

    private void OnDisable() => StopReceiveIP();
    private void OnDestroy() => StopReceiveIP();
    private void OnApplicationQuit() => StopReceiveIP();
}