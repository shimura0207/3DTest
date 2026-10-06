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


    // ========================================
    // Matching開始
    // ========================================

    public void StartMatching() {
        if (matching) {
            Debug.Log("すでにMatching中です。");
            return;
        }

        if (NetworkManager.Singleton == null) {
            Debug.LogError("NetworkManagerがありません。");
            return;
        }

        if (NetworkManager.Singleton.IsListening) {
            Debug.LogWarning(
                "すでにHostまたはClientとして接続しています。");

            return;
        }

        matching = true;
        matched = false;

        playerId =
            Guid.NewGuid().ToString("N");

        matchingStartTime =
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        try {
            // UDP 9001番を受信ポートとして開く
            udp = new UdpClient(matchingPort);

            udp.EnableBroadcast = true;

            udp.BeginReceive(
                Receive,
                null
            );

            Debug.Log(
                $"Matching開始 ID={playerId}");

            // 1秒ごとに待機通知
            InvokeRepeating(
                nameof(SendMatchingWait),
                0f,
                1f
            );
        }
        catch (Exception e) {
            Debug.LogError(
                $"Matching開始失敗: {e}");

            StopMatching();
        }
    }


    // ========================================
    // Matching待機通知
    // ========================================

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
            $"[Matching] UDP送信: {message}");

        SendBroadcast(data);

        IPAddress subnetBroadcast =
            GetSubnetBroadcastAddress();

        if (subnetBroadcast != null) {
            Debug.Log(
                $"[Matching] Subnet Broadcast送信: " +
                $"{subnetBroadcast}");

            SendTo(
                data,
                subnetBroadcast
            );
        }
        else {
            Debug.LogWarning(
                "[Matching] Subnet Broadcastを取得できませんでした。");
        }
    }


    // ========================================
    // UDP受信
    // ========================================

    private void Receive(
        IAsyncResult result) {
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
                $"[Matching] UDP受信: {message} " +
                $"from {sender.Address}");

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
            // 終了処理中
        }
        catch (Exception e) {
            Debug.LogError(
                $"Matching受信エラー: {e}");

            BeginReceiveAgain();
        }
    }


    // ========================================
    // WAITを受信
    // ========================================

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

        // 自分自身の通知なら無視
        if (otherPlayerId == playerId)
            return;


        // ========================================
        // どちらが先にMatchingしたか判定
        // ========================================

        bool otherIsEarlier =
            otherStartTime < matchingStartTime ||
            (
                otherStartTime == matchingStartTime &&
                string.CompareOrdinal(
                    otherPlayerId,
                    playerId
                ) < 0
            );


        if (otherIsEarlier) {
            // 相手が先
            // 自分はClientになる

            Debug.Log(
                "先にMatchingしていたプレイヤーを発見。Clientになります。");

            matched = true;

            CancelInvoke(
                nameof(SendMatchingWait)
            );

            string joinMessage =
                $"{MatchingJoin}|" +
                $"{otherPlayerId}|" +
                $"{playerId}";

            byte[] data =
                Encoding.UTF8.GetBytes(
                    joinMessage
                );

            SendTo(
                data,
                IPAddress.Parse(otherIP)
            );
        }
    }


    // ========================================
    // JOINを受信
    // ========================================

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


        // 自分宛てではない
        if (hostId != playerId)
            return;

        if (clientId == playerId)
            return;


        Debug.Log(
            "相手が見つかりました。Hostになります。");

        matched = true;

        CancelInvoke(
            nameof(SendMatchingWait)
        );


        // UnityのメインスレッドでHost開始
        MainThreadDispatcher.Instance.Enqueue(
            () => {
                StartHost(
                    clientId,
                    sender.Address
                );
            }
        );
    }


    // ========================================
    // Host開始
    // ========================================

    private void StartHost(
        string clientId,
        IPAddress clientIP) {
        if (NetworkManager.Singleton.IsListening)
            return;

        transport.SetConnectionData(
            "0.0.0.0",
            gamePort
        );

        bool success =
            NetworkManager.Singleton.StartHost();

        if (!success) {
            Debug.LogError(
                "Matching Host開始失敗");

            StopMatching();

            return;
        }

        Debug.Log(
            "Matching Host開始成功");

        // Host開始後、Clientに通知
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


    // ========================================
    // FOUNDを受信
    // ========================================

    private void ReceiveMatchingFound(
        string[] split) {
        if (!matching || matched)
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


        // 自分宛てでなければ無視
        if (targetClientId != playerId)
            return;


        Debug.Log(
            $"Hostを発見: {hostIP}:{hostPort}");

        matched = true;

        CancelInvoke(
            nameof(SendMatchingWait)
        );


        MainThreadDispatcher.Instance.Enqueue(
            () => {
                StartClient(
                    hostIP,
                    hostPort
                );
            }
        );
    }


    // ========================================
    // Client開始
    // ========================================

    private void StartClient(
        string hostIP,
        ushort hostPort) {
        if (NetworkManager.Singleton.IsListening)
            return;

        transport.SetConnectionData(
            hostIP,
            hostPort
        );

        bool success =
            NetworkManager.Singleton.StartClient();

        if (success) {
            Debug.Log(
                $"Matching Client開始: {hostIP}:{hostPort}");
        }
        else {
            Debug.LogError(
                "Matching Client開始失敗");

            StopMatching();
        }
    }


    // ========================================
    // UDP Broadcast
    // ========================================

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
                $"Broadcast送信エラー: {e}");
        }
    }


    // ========================================
    // 指定IPへ送信
    // ========================================

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
                $"UDP送信エラー: {e}");
        }
    }


    // ========================================
    // 次の受信
    // ========================================

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
    }


    // ========================================
    // 自分のIP取得
    // ========================================

    private string GetLocalIP() {
        foreach (
            NetworkInterface ni
            in NetworkInterface.GetAllNetworkInterfaces()) {
            if (
                ni.OperationalStatus !=
                OperationalStatus.Up) {
                continue;
            }

            if (
                ni.NetworkInterfaceType ==
                NetworkInterfaceType.Loopback) {
                continue;
            }

            foreach (
                UnicastIPAddressInformation ip
                in ni.GetIPProperties()
                   .UnicastAddresses) {
                if (
                    ip.Address.AddressFamily ==
                    AddressFamily.InterNetwork) {
                    return ip.Address.ToString();
                }
            }
        }

        return "127.0.0.1";
    }


    // ========================================
    // サブネットBroadcast取得
    // ========================================

    private IPAddress GetSubnetBroadcastAddress() {
        foreach (
            NetworkInterface ni
            in NetworkInterface.GetAllNetworkInterfaces()) {
            if (
                ni.OperationalStatus !=
                OperationalStatus.Up) {
                continue;
            }

            if (
                ni.NetworkInterfaceType ==
                NetworkInterfaceType.Loopback) {
                continue;
            }

            foreach (
                UnicastIPAddressInformation ip
                in ni.GetIPProperties()
                   .UnicastAddresses) {
                if (
                    ip.Address.AddressFamily !=
                    AddressFamily.InterNetwork) {
                    continue;
                }

                byte[] ipBytes =
                    ip.Address.GetAddressBytes();

                byte[] maskBytes =
                    ip.IPv4Mask.GetAddressBytes();

                byte[] broadcastBytes =
                    new byte[4];

                for (int i = 0; i < 4; i++) {
                    broadcastBytes[i] =
                        (byte)(
                            ipBytes[i] |
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


    // ========================================
    // Matching停止
    // ========================================

    public void StopMatching() {
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


    // ========================================
    // 終了時
    // ========================================

    private void OnDestroy() {
        StopMatching();
    }
}