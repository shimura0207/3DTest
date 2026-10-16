using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPBroadcaster : MonoBehaviour {
    [Header("UDPListenerと合わせる")]
    [SerializeField, Range(1, 65535)]
    private int discoveryPort = 55555;

    [Header("同じゲームを識別する文字列")]
    [SerializeField] private string gameName = "RG_CardGame_v1";

    [Header("一覧に表示する部屋名")]
    [SerializeField] private string hostName = "カードゲームの部屋";

    [SerializeField, Min(0.1f)]
    private float interval = 0.5f;

    private UdpClient client;
    private IPEndPoint destination;
    private UDPListener.UdpMessage message;
    private float nextSendTime;

    public bool IsBroadcasting => client != null;

    public bool StartSendIP(int gamePort) {
        StopBroadcast();

        if (!isActiveAndEnabled ||
            discoveryPort < 1 || discoveryPort > 65535 ||
            gamePort < 1 || gamePort > 65535) {
            Debug.LogError("配信設定または有効状態を確認してください。", this);
            return false;
        }

        message = new UDPListener.UdpMessage {
            // 受信側が実際の送信元IPに置き換える
            ip = "",
            port = gamePort,
            gameName = gameName,
            hostName = string.IsNullOrWhiteSpace(hostName)
                ? "カードゲームの部屋"
                : hostName,
            gamePlaying = false
        };

        try {
            client = new UdpClient(AddressFamily.InterNetwork);
            client.EnableBroadcast = true;
            client.Client.Blocking = false;

            destination = new IPEndPoint(
                IPAddress.Broadcast, discoveryPort);

            nextSendTime = 0f;
            return true;
        }
        catch (Exception exception) {
            Debug.LogError($"配信開始失敗: {exception.Message}", this);
            StopBroadcast();
            return false;
        }
    }

    private void Update() {
        if (client == null || Time.unscaledTime < nextSendTime)
            return;

        nextSendTime = Time.unscaledTime + Mathf.Max(0.1f, interval);

        try {
            byte[] bytes = Encoding.UTF8.GetBytes(
                JsonUtility.ToJson(message));

            client.Send(bytes, bytes.Length, destination);
        }
        catch (SocketException exception) {
            if (exception.SocketErrorCode == SocketError.WouldBlock ||
                exception.SocketErrorCode == SocketError.NoBufferSpaceAvailable) {
                return;
            }

            Debug.LogError($"配信エラー: {exception.Message}", this);
            StopBroadcast();
        }
        catch (ObjectDisposedException) {
            StopBroadcast();
        }
    }

    public void SetGamePlaying(bool playing) {
        message.gamePlaying = playing;
        nextSendTime = 0f;
    }

    public void SetHostName(string value) {
        hostName = string.IsNullOrWhiteSpace(value)
            ? "カードゲームの部屋"
            : value.Trim();

        message.hostName = hostName;
        nextSendTime = 0f;
    }

    public void StopBroadcast() {
        UdpClient oldClient = client;
        client = null;
        destination = null;
        oldClient?.Close();
    }

    private void OnDisable() => StopBroadcast();
    private void OnDestroy() => StopBroadcast();
    private void OnApplicationQuit() => StopBroadcast();
}