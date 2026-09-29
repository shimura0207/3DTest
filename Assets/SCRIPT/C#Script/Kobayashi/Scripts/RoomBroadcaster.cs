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
    public void StartBroadcast(ushort port)
    {
        // 4桁のコードを生成する
        RoomCode =
            Random.Range(
                1000,
                10000
            ).ToString();

        // UDPを作成
        udp = new UdpClient();
        // ブロードキャスト送信を許可する
        udp.EnableBroadcast = true;

        // SendBroadcastを繰り返し実行する
        InvokeRepeating(
            nameof(SendBroadcast),
            0f,
            1f
        );

        // ログを表示
        Debug.Log(
            "Room Code : "
            + RoomCode
        );
    }

    /// <summary>
    /// ルーム情報をLANに送信するための関数
    /// </summary>
    private void SendBroadcast()
    {
        // 送信する文字列を作成
        string message = $"{RoomCode}|{GetLocalIP()}|7777";
        // 文字列をバイト配列に変換
        byte[] data = Encoding.UTF8.GetBytes(message);

        // 送信する先を指定
        IPEndPoint target =
            new IPEndPoint(
                IPAddress.Broadcast,
                9000
            );

        // 実際に送信を行う
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