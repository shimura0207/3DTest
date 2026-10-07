using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MatchingManager : MonoBehaviour {
    [Header("Network")]
    [SerializeField]
    private UnityTransport transport;

    [Header("Matching")]
    [SerializeField]
    private ushort matchingPort = 9001;

    [SerializeField]
    private ushort gamePort = 7777;

    private UdpClient udp;

    private bool matching;
    private bool matched;

    private string playerId;
    private long matchingStartTime;

    private const string MatchingWait = "MATCHING_WAIT";
    private const string MatchingJoin = "MATCHING_JOIN";
    private const string MatchingFound = "MATCHING_FOUND";

    // =========================================================
    // Matching開始
    // =========================================================
    public void StartMatching() {
        if (matching) {
            Debug.Log("すでにMatching中です。");
            return;
        }

        if (NetworkManager.Singleton == null) {
            Debug.LogError("NetworkManagerが見つかりません。");
            return;
        }

        if (NetworkManager.Singleton.IsListening) {
            Debug.LogWarning("すでにNetwork接続中です。");
            return;
        }

        matching = true;
        matched = false;

        playerId =
            Guid.NewGuid().ToString("N");

        matchingStartTime =
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        try {
            udp =
                new UdpClient(matchingPort);

            udp.EnableBroadcast = true;

            udp.BeginReceive(
                Receive,
                null
            );

            Debug.Log(
                $"Matching開始 ID={playerId}"
            );

            InvokeRepeating(
                nameof(SendMatchingWait),
                0f,
                1f
            );
        }
        catch (Exception e) {
            Debug.LogError(
                $"Matching開始エラー: {e}"
            );

            StopMatching();
        }
    }

    // =========================================================
    // MATCHING_WAIT送信
    // =========================================================
    private void SendMatchingWait() {
        if (!matching || matched)
            return;

        string localIP =
            GetLocalIP();

        string message =
            $"{MatchingWait}|" +
            $"{playerId}|" +
            $"{localIP}|" +
            $"{gamePort}|" +
            $"{matchingStartTime}";

        byte[] data =
            Encoding.UTF8.GetBytes(message);

        Debug.Log(
            $"[Matching] UDP送信: {message}"
        );

        SendBroadcast(data);

        IPAddress subnetBroadcast =
            GetSubnetBroadcastAddress();

        if (subnetBroadcast != null) {
            Debug.Log(
                $"[Matching] Subnet Broadcast送信: " +
                $"{subnetBroadcast}"
            );

            SendTo(
                data,
                subnetBroadcast
            );
        }
        else {
            Debug.LogWarning(
                "[Matching] Subnet Broadcastアドレスを取得できませんでした。"
            );
        }
    }

    // =========================================================
    // UDP受信
    // =========================================================
    private void Receive(IAsyncResult result) {
        if (udp == null)
            return;

        try {
            IPEndPoint sender = null;

            byte[] data =
                udp.EndReceive(
                    result,
                    ref sender
                );

            string message =
                Encoding.UTF8.GetString(data);

            Debug.Log(
                $"[Matching] UDP受信: " +
                $"{message} from {sender.Address}"
            );

            string[] split =
                message.Split('|');

            if (split.Length == 0) {
                BeginReceiveAgain();
                return;
            }

            string type =
                split[0];

            if (type == MatchingWait) {
                ReceiveMatchingWait(
                    split,
                    sender
                );
            }
            else if (type == MatchingJoin) {
                ReceiveMatchingJoin(
                    split,
                    sender
                );
            }
            else if (type == MatchingFound) {
                ReceiveMatchingFound(
                    split
                );
            }

            BeginReceiveAgain();
        }
        catch (ObjectDisposedException) {
            // UDPが閉じられた場合は何もしない
        }
        catch (Exception e) {
            Debug.LogError(
                $"Matching受信エラー: {e}"
            );

            BeginReceiveAgain();
        }
    }

    // =========================================================
    // MATCHING_WAIT受信
    // =========================================================
    private void ReceiveMatchingWait(
        string[] split,
        IPEndPoint sender) {
        if (!matching || matched)
            return;

        if (split.Length < 5)
            return;

        string otherPlayerId =
            split[1];

        string otherIP =
            split[2];

        if (!ushort.TryParse(
                split[3],
                out ushort otherPort)) {
            return;
        }

        if (!long.TryParse(
                split[4],
                out long otherStartTime)) {
            return;
        }

        // 自分自身のMATCHING_WAITは無視
        if (otherPlayerId == playerId)
            return;

        bool otherIsEarlier =
            otherStartTime < matchingStartTime
            ||
            (
                otherStartTime == matchingStartTime
                &&
                string.CompareOrdinal(
                    otherPlayerId,
                    playerId
                ) < 0
            );

        if (otherIsEarlier) {
            Debug.Log(
                "先にMatchingしていたプレイヤーを発見。Clientになります。"
            );

            matched = true;

            string hostId =
                otherPlayerId;

            string clientId =
                playerId;

            string joinMessage =
                $"{MatchingJoin}|" +
                $"{hostId}|" +
                $"{clientId}";

            byte[] data =
                Encoding.UTF8.GetBytes(
                    joinMessage
                );

            // Host候補へJOINを送信
            SendTo(
                data,
                IPAddress.Parse(otherIP)
            );

            // Unityの処理はメインスレッドで行う
            MainThreadDispatcher.Instance.Enqueue(
                () => {
                    CancelInvoke(
                        nameof(SendMatchingWait)
                    );

                    Debug.Log(
                        "Matching Client処理を開始します。"
                    );
                }
            );
        }
    }

    // =========================================================
    // MATCHING_JOIN受信
    // =========================================================
    private void ReceiveMatchingJoin(
        string[] split,
        IPEndPoint sender) {
        if (!matching || matched)
            return;

        if (split.Length < 3)
            return;

        string hostId =
            split[1];

        string clientId =
            split[2];

        if (hostId != playerId)
            return;

        if (clientId == playerId)
            return;

        Debug.Log(
            "相手が見つかりました。Hostになります。"
        );

        matched = true;

        IPAddress clientIP =
            sender.Address;

        MainThreadDispatcher.Instance.Enqueue(
            () => {
                CancelInvoke(
                    nameof(SendMatchingWait)
                );

                StartHost(
                    clientId,
                    clientIP
                );
            }
        );
    }

    // =========================================================
    // Host開始
    // =========================================================
    private void StartHost(
        string clientId,
        IPAddress clientIP) {
        if (NetworkManager.Singleton.IsListening)
            return;

        if (transport == null) {
            Debug.LogError(
                "UnityTransportが設定されていません。"
            );

            StopMatching();
            return;
        }

        transport.SetConnectionData(
            "0.0.0.0",
            gamePort
        );

        bool success =
            NetworkManager.Singleton.StartHost();

        if (!success) {
            Debug.LogError(
                "Matching Host開始失敗"
            );

            StopMatching();
            return;
        }

        Debug.Log(
            "Matching Host開始成功"
        );

        string hostIP =
            GetLocalIP();

        string message =
            $"{MatchingFound}|" +
            $"{clientId}|" +
            $"{hostIP}|" +
            $"{gamePort}";

        byte[] data =
            Encoding.UTF8.GetBytes(message);

        SendTo(
            data,
            clientIP
        );
    }

    // =========================================================
    // MATCHING_FOUND受信
    // =========================================================
    private void ReceiveMatchingFound(
        string[] split) {
        if (!matching || !matched)
            return;

        if (split.Length < 4)
            return;

        string targetClientId =
            split[1];

        string hostIP =
            split[2];

        if (!ushort.TryParse(
                split[3],
                out ushort hostPort)) {
            return;
        }

        if (targetClientId != playerId)
            return;

        Debug.Log(
            $"Hostを発見: " +
            $"{hostIP}:{hostPort}"
        );

        MainThreadDispatcher.Instance.Enqueue(
            () => {
                CancelInvoke(
                    nameof(SendMatchingWait)
                );

                StartClient(
                    hostIP,
                    hostPort
                );
            }
        );
    }

    // =========================================================
    // Client開始
    // =========================================================
    private void StartClient(string hostIP, ushort hostPort) {
        Debug.Log("StartClient() に入りました。");

        if (NetworkManager.Singleton == null) {
            Debug.LogError("NetworkManager.Singleton が null です。");
            return;
        }

        if (NetworkManager.Singleton.IsListening) {
            Debug.LogWarning("すでにNetworkManagerはListening状態です。");
            return;
        }

        Debug.Log($"接続先: {hostIP}:{hostPort}");

        if (transport == null) {
            Debug.LogError("UnityTransportが設定されていません。");
            return;
        }

        Debug.Log("UnityTransportがあります。");

        transport.SetConnectionData(hostIP, hostPort);

        Debug.Log($"UnityTransport設定完了: {hostIP}:{hostPort}");

        bool success = NetworkManager.Singleton.StartClient();

        Debug.Log($"StartClient() 結果: {success}");

        if (success) {
            Debug.Log("Matching Client開始成功");
        }
        else {
            Debug.LogError("Matching Client開始失敗");
        }
    }

    // =========================================================
    // Broadcast送信
    // =========================================================

    private void SendBroadcast(
        byte[] data) {
        if (udp == null)
            return;

        try {
            udp.Send(
                data,
                data.Length,
                new IPEndPoint(
                    IPAddress.Broadcast,
                    matchingPort
                )
            );
        }
        catch (Exception e) {
            Debug.LogError(
                $"Matching Broadcast送信エラー: {e.Message}"
            );
        }
    }


    // =========================================================
    // 指定IPへUDP送信
    // =========================================================

    private void SendTo(
        byte[] data,
        IPAddress ip) {
        if (udp == null)
            return;

        try {
            udp.Send(
                data,
                data.Length,
                new IPEndPoint(
                    ip,
                    matchingPort
                )
            );
        }
        catch (Exception e) {
            Debug.LogError(
                $"Matching UDP送信エラー: {e.Message}"
            );
        }
    }


    // =========================================================
    // UDP受信再開
    // =========================================================

    private void BeginReceiveAgain() {
        if (udp == null)
            return;

        if (!matching)
            return;

        if (matched)
            return;

        try {
            udp.BeginReceive(
                Receive,
                null
            );
        }
        catch (ObjectDisposedException) {
        }
        catch (Exception e) {
            Debug.LogError(
                $"UDP受信再開エラー: {e}"
            );
        }
    }


    // =========================================================
    // ローカルIP取得
    // =========================================================

    private string GetLocalIP() {
        foreach (
            NetworkInterface ni
            in NetworkInterface.GetAllNetworkInterfaces()) {
            if (
                ni.OperationalStatus
                != OperationalStatus.Up)
                continue;

            if (
                ni.NetworkInterfaceType
                == NetworkInterfaceType.Loopback)
                continue;

            foreach (
                UnicastIPAddressInformation ip
                in ni.GetIPProperties().UnicastAddresses) {
                if (
                    ip.Address.AddressFamily
                    == AddressFamily.InterNetwork) {
                    return ip.Address.ToString();
                }
            }
        }

        return "127.0.0.1";
    }


    // =========================================================
    // Subnet Broadcast取得
    // =========================================================

    private IPAddress GetSubnetBroadcastAddress() {
        foreach (
            NetworkInterface ni
            in NetworkInterface.GetAllNetworkInterfaces()) {
            if (
                ni.OperationalStatus
                != OperationalStatus.Up)
                continue;

            if (
                ni.NetworkInterfaceType
                == NetworkInterfaceType.Loopback)
                continue;

            foreach (
                UnicastIPAddressInformation ip
                in ni.GetIPProperties().UnicastAddresses) {
                if (
                    ip.Address.AddressFamily
                    != AddressFamily.InterNetwork)
                    continue;

                byte[] ipBytes =
                    ip.Address.GetAddressBytes();

                byte[] maskBytes =
                    ip.IPv4Mask.GetAddressBytes();

                byte[] broadcastBytes =
                    new byte[4];

                for (int i = 0; i < 4; i++) {
                    broadcastBytes[i] =
                        (byte)(
                            ipBytes[i]
                            |
                            (byte)~maskBytes[i]
                        );
                }

                return new IPAddress(
                    broadcastBytes
                );
            }
        }

        return null;
    }


    // =========================================================
    // Matching停止
    // =========================================================

    public void StopMatching() {
        // StopMatchingは外部から呼ばれる可能性があるため、
        // Unity APIを使用する部分はメインスレッドで処理する。

        MainThreadDispatcher.Instance.Enqueue(
            () => {
                matching = false;
                matched = false;

                CancelInvoke(
                    nameof(SendMatchingWait)
                );

                if (udp != null) {
                    try {
                        udp.Close();
                    }
                    catch {
                    }

                    udp = null;
                }

                Debug.Log(
                    "Matching停止"
                );
            }
        );
    }


    // =========================================================
    // Destroy
    // =========================================================

    private void OnDestroy() {
        matching = false;
        matched = false;

        CancelInvoke(
            nameof(SendMatchingWait)
        );

        if (udp != null) {
            try {
                udp.Close();
            }
            catch {
            }

            udp = null;
        }
    }
}