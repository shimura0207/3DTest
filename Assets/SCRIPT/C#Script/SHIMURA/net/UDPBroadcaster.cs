using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>ゲーム接続とは別のUDPソケットで部屋を定期公開します。</summary>
public class UDPBroadcaster : MonoBehaviour {
    // Inspectorや保存データには入れません。同じPCの別プロセスも別IDになります。
    // 同一実行中のホスト再作成では維持し、優先順位の逆転を防ぎます。
    private static string localRoomId;
    public static string LocalRoomId {
        get {
            if (string.IsNullOrEmpty(localRoomId)) localRoomId = Guid.NewGuid().ToString("N");
            return localRoomId;
        }
    }

    // Domain Reload無効でもPlay開始時にIDを更新します。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRoomId() { localRoomId = null; }

    [Header("UDPListenerと合わせる")]
    [SerializeField, Range(1, 65535)] private int discoveryPort = 55555;
    [Header("同じゲームを識別する文字列")]
    [SerializeField] private string gameName = "RG_CardGame_v1";
    [Header("一覧に表示する部屋名")]
    [SerializeField] private string hostName = "カードゲームの部屋";
    [SerializeField, Min(0.1f)] private float interval = 0.5f;
    [SerializeField] private bool verboseLogging;

    private UdpClient client;
    private IPEndPoint destination;
    private UDPListener.UdpMessage message;
    private float nextSendTime;
    public bool IsBroadcasting => client != null;

    /// <summary>gamePortはKCP接続用。discoveryPortは検索用です。</summary>
    public bool StartSendIP(int gamePort) {
        StopBroadcast();
        if (!isActiveAndEnabled || discoveryPort < 1 || discoveryPort > 65535 ||
            gamePort < 1 || gamePort > 65535) {
            Debug.LogError("配信設定または有効状態を確認してください。", this);
            return false;
        }
        message = new UDPListener.UdpMessage {
            protocolVersion = UDPListener.ProtocolVersion,
            roomId = LocalRoomId,
            ip = "", // 受信側が実際の送信元IPを設定します。
            port = gamePort,
            gameName = gameName,
            hostName = string.IsNullOrWhiteSpace(hostName) ? "カードゲームの部屋" : hostName,
            gamePlaying = false
        };
        try {
            client = new UdpClient(AddressFamily.InterNetwork);
            client.EnableBroadcast = true;
            client.Client.Blocking = false;
            destination = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
            nextSendTime = 0f;
            Debug.Log($"[UDP公開] roomId={LocalRoomId} gamePort={gamePort}", this);
            return true;
        }
        catch (Exception e) {
            Debug.LogError($"配信開始失敗: {e.Message}", this);
            StopBroadcast();
            return false;
        }
    }

    private void Update() {
        if (client == null || Time.unscaledTime < nextSendTime) return;
        nextSendTime = Time.unscaledTime + Mathf.Max(0.1f, interval);
        try {
            // Ready前の参加者がいる間も、新規参加先の候補から外します。
            var advertisement = message;
            var manager = CustomNetworkManager.instance;
            if (manager != null && manager.HasRemoteParticipant) advertisement.gamePlaying = true;
            string json = JsonUtility.ToJson(advertisement);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            client.Send(bytes, bytes.Length, destination);
            if (verboseLogging) Debug.Log($"[UDP送信] 宛先={destination} JSON={json}", this);
        }
        catch (SocketException e) {
            if (e.SocketErrorCode == SocketError.WouldBlock ||
                e.SocketErrorCode == SocketError.NoBufferSpaceAvailable) return;
            Debug.LogError($"配信エラー: {e.Message}", this);
            StopBroadcast();
        }
        catch (ObjectDisposedException) { StopBroadcast(); }
    }

    public void SetGamePlaying(bool playing) {
        message.gamePlaying = playing;
        nextSendTime = 0f;
    }
    public void SetHostName(string value) {
        hostName = string.IsNullOrWhiteSpace(value) ? "カードゲームの部屋" : value.Trim();
        message.hostName = hostName;
        nextSendTime = 0f;
    }
    public void StopBroadcast() {
        var oldClient = client;
        client = null;
        destination = null;
        oldClient?.Close();
    }
    private void OnDisable() => StopBroadcast();
    private void OnDestroy() => StopBroadcast();
    private void OnApplicationQuit() => StopBroadcast();
}
