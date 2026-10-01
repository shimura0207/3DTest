/*
 * @brief  RoomBroadcaster
 * @author Kobayashi
 */

using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class RoomBroadcaster : MonoBehaviour
{
    // UDP通信を行うための変数
    private UdpClient udp;

    private ushort gamePort;

    // ルームコードを保存
    public string RoomCode
    {
        get;
        private set;
    }

    /// <summary>
    /// ルーム情報のブロードキャストを開始するための関数
    /// </summary>
    /// <param name="port"></param>
    public void StartBroadcast(ushort port) {
        gamePort = port;

        RoomCode =
            Random.Range(
                1000,
                10000
            ).ToString();

        Debug.Log($"Host IP : {GetLocalIP()}");

        udp = new UdpClient();
        udp.EnableBroadcast = true;

        InvokeRepeating(
            nameof(SendBroadcast),
            0f,
            1f
        );

        Debug.Log(
            "Room Code : "
            + RoomCode
        );
    }

    private void SendBroadcast() {
        string message =
            $"{RoomCode}|{GetLocalIP()}|{gamePort}";

        byte[] data =
            Encoding.UTF8.GetBytes(message);

        IPEndPoint target =
            new IPEndPoint(
                IPAddress.Broadcast,
                9000
            );

        udp.Send(
            data,
            data.Length,
            target
        );
    }

    /// <summary>
    /// 自身のPCのLAN内IPアドレスを取得する
    /// </summary>
    /// <returns></returns>
    private string GetLocalIP()
    {
        // 取得したIPアドレスを1つずつ調べる
        foreach (IPAddress ip in
            Dns.GetHostEntry(
                Dns.GetHostName()
            ).AddressList)
        {
            // IPv4のIPアドレスかどうか
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                // IPアドレスを返す
                return ip.ToString();
            }
        }

        // 見つからなかったら "127.0.0.1" を返す
        return "127.0.0.1";
    }

    /// <summary>
    /// UnityでGameObjectが破棄される時に呼ばれる関数
    /// </summary>
    private void OnDestroy()
    {
        // InvokeRepeating() で設定した繰り返し処理を停止する
        CancelInvoke();

        // UDP通信を終了する
        udp?.Close();
    }
}