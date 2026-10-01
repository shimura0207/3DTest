/*
 * @brief  RoomBroadcaster
 * @author Kobayashi
 */

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class RoomBroadcaster : MonoBehaviour {
    // UDP通信を行うための変数
    private UdpClient udp;

    // ゲーム接続に使用するポート
    private ushort gamePort;

    // ルームコードを保存
    public string RoomCode {
        get;
        private set;
    }

    /// <summary>
    /// ルーム情報のブロードキャストを開始する
    /// </summary>
    public void StartBroadcast(ushort port) {
        // ゲーム接続用ポートを保存
        gamePort = port;

        // 4桁のコードを生成
        RoomCode =
            UnityEngine.Random.Range(
                1000,
                10000
            ).ToString();

        // 自分のLAN内IPを取得
        string localIP = GetLocalIP();

        // ログ
        Debug.Log(
            $"Host IP : {localIP}"
        );

        // UDPを作成
        udp = new UdpClient();

        // ブロードキャストを許可
        udp.EnableBroadcast = true;

        // 送信開始
        InvokeRepeating(
            nameof(SendBroadcast),
            0f,
            1f
        );

        // ログ
        Debug.Log(
            $"Room Code : {RoomCode}"
        );
    }

    /// <summary>
    /// ルーム情報をLANに送信する
    /// </summary>
    private void SendBroadcast() {
        if (udp == null)
            return;

        // 送信する情報
        string message =
            $"{RoomCode}|{GetLocalIP()}|{gamePort}";

        byte[] data =
            Encoding.UTF8.GetBytes(message);

        // 通常のブロードキャスト
        SendTo(
            data,
            IPAddress.Broadcast
        );

        // LAN固有のブロードキャスト
        IPAddress subnetBroadcast =
            GetSubnetBroadcastAddress();

        if (subnetBroadcast != null) {
            SendTo(
                data,
                subnetBroadcast
            );

            Debug.Log(
                $"Room Broadcast : " +
                $"{subnetBroadcast}:9000"
            );
        }
    }

    /// <summary>
    /// 指定したIPアドレスへUDPを送信する
    /// </summary>
    private void SendTo(
        byte[] data,
        IPAddress address) {
        try {
            IPEndPoint target =
                new IPEndPoint(
                    address,
                    9000
                );

            udp.Send(
                data,
                data.Length,
                target
            );
        }
        catch (Exception e) {
            Debug.LogError(
                $"UDP送信エラー : {e.Message}"
            );
        }
    }

    /// <summary>
    /// 自身のPCのLAN内IPv4アドレスを取得する
    /// </summary>
    private string GetLocalIP() {
        foreach (
            NetworkInterface ni
            in NetworkInterface.GetAllNetworkInterfaces()) {
            // 有効なネットワークだけを対象にする
            if (
                ni.OperationalStatus
                != OperationalStatus.Up) {
                continue;
            }

            // ループバックを除外
            if (
                ni.NetworkInterfaceType
                == NetworkInterfaceType.Loopback) {
                continue;
            }

            foreach (
                UnicastIPAddressInformation info
                in ni.GetIPProperties().UnicastAddresses) {
                // IPv4だけを対象にする
                if (
                    info.Address.AddressFamily
                    != AddressFamily.InterNetwork) {
                    continue;
                }

                // IPv4アドレスを返す
                return info.Address.ToString();
            }
        }

        return "127.0.0.1";
    }

    /// <summary>
    /// LANのブロードキャストアドレスを取得する
    /// </summary>
    private IPAddress GetSubnetBroadcastAddress() {
        foreach (
            NetworkInterface ni
            in NetworkInterface.GetAllNetworkInterfaces()) {
            if (
                ni.OperationalStatus
                != OperationalStatus.Up) {
                continue;
            }

            if (
                ni.NetworkInterfaceType
                == NetworkInterfaceType.Loopback) {
                continue;
            }

            foreach (
                UnicastIPAddressInformation info
                in ni.GetIPProperties().UnicastAddresses) {
                if (
                    info.Address.AddressFamily
                    != AddressFamily.InterNetwork) {
                    continue;
                }

                if (info.IPv4Mask == null)
                    continue;

                byte[] ip =
                    info.Address.GetAddressBytes();

                byte[] mask =
                    info.IPv4Mask.GetAddressBytes();

                byte[] broadcast =
                    new byte[4];

                for (int i = 0; i < 4; i++) {
                    broadcast[i] =
                        (byte)(
                            ip[i] | ~mask[i]
                        );
                }

                return new IPAddress(
                    broadcast
                );
            }
        }

        return null;
    }

    /// <summary>
    /// GameObjectが破棄される時に呼ばれる
    /// </summary>
    private void OnDestroy() {
        // InvokeRepeatingを停止
        CancelInvoke();

        // UDP通信を終了
        udp?.Close();

        udp = null;
    }
}