/*
 * @brief  RoomFinder
 * @author Kobayashi
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class RoomFinder : MonoBehaviour
{
    [Header("Network")]
    // ゲーム接続に使用するUnity Transport
    [SerializeField]
    private UnityTransport transport;

    [Header("Search")]
    // ルーム検索用のポート番号
    [SerializeField]
    private int broadcastPort = 9000;

    // UDP通信を行うための変数
    private UdpClient udp;

    // 目的の部屋を見つけたかどうか
    private bool foundRoom;

    /// <summary>
    /// ルーム検索を開始するメソッド
    /// </summary>
    /// <param name="code"></param>
    public void SearchRoom(string code)
    {
        // 既に検索済みかどうか
        if (foundRoom)
        {
            // ログを表示
            Debug.Log(
                "すでに検索済みです"
            );

            // 処理を終える
            return;
        }

        // ルームを見つけていない状態にする
        foundRoom = false;

        // ログを表示
        Debug.Log(
            $"Room検索開始 : [{code}]"
        );

        // 通信処理中にエラーが起きても、ゲーム全体が止まらないようにする
        try
        {
            // UDP通信の準備
            udp =
                new UdpClient(
                    broadcastPort
                );

            // UDPデータが届いたら Receive() を呼ぶ
            udp.BeginReceive(
                Receive,
                code
            );

            // 5秒後に SearchTimeout() を実行
            Invoke(
                nameof(SearchTimeout),
                5f
            );
        }

        // もしエラーが発生したら
        catch (Exception e)
        {
            // エラーログを表示
            Debug.LogError(
                e.Message
            );
        }
    }

    /// <summary>
    /// Hostから届いた部屋情報を受け取るための関数
    /// </summary>
    /// <param name="result"></param>
    private void Receive(IAsyncResult result)
    {
        // UDP通信が既に終了している場合
        if (udp == null)
            // 処理を終える
            return;

        // データを送ってきた相手のIPアドレス・ポート番号を保存する変数
        IPEndPoint sender = null;

        // 届いたUDPデータを取得
        byte[] data =
            udp.EndReceive(
                result,
                ref sender
            );

        // UTF-8から文字列に戻す
        string message = Encoding.UTF8.GetString(data);

        // ログを表示
        Debug.Log(
            $"受信データ : {message}"
        );

        // 受信したポート番号を | で分割する
        string[] split = message.Split('|');

        // 必要なデータが足りていなければ
        if (split.Length < 3)
        {
            // ログを表示
            Debug.LogWarning(
                "データ形式不正"
            );

            // 処理を終える
            return;
        }

        // ルームコード
        string hostCode = split[0];

        // HostのIP
        string hostIP = split[1];

        // Hostのポート
        ushort hostPort =
            ushort.Parse(
                split[2]
            );

        // 入力したコードを取り出す
        string inputCode = (string)result.AsyncState;

        // ログを表示
        Debug.Log(
            $"Hostコード:[{hostCode}] 入力:[{inputCode}]"
        );

        // コードが一致した場合
        if (hostCode == inputCode)
        {
            // ルームを発見した状態にする
            foundRoom = true;

            // タイムアウトをキャンセル
            CancelInvoke(
                nameof(SearchTimeout)
            );

            // ログを表示
            Debug.Log(
                $"接続先発見 : {hostIP}:{hostPort}"
            );

            // Connect() をUnityのメインスレッドで実行する
            MainThreadDispatcher
                .Instance
                .Enqueue(
                () =>
                {
                    Connect(
                        hostIP,
                        hostPort
                    );
                });
        }
        // コードが一致しなかった場合
        else
        {
            // まだ目的のHostが見つかっていない
            udp.BeginReceive(
                Receive,
                inputCode
            );
        }
    }

    /// <summary>
    /// 見つけたHostへNetcodeで接続するための関数
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    private void Connect(string ip,ushort port)
    {
        // Unity Transportがあるかどうか確認
        if (transport == null)
        {
            // エラーログを表示
            Debug.LogError(
                "UnityTransportが設定されていません"
            );

            // 処理を終える
            return;
        }

        // Unity Transportに接続先を設定
        transport.SetConnectionData(
            ip,
            port
        );

        // NetcodeのClientを開始
        bool result =
            NetworkManager
            .Singleton
            .StartClient();

        // 接続が成功していれば
        if (result)
        {
            // ログを表示
            Debug.Log(
                "Client開始"
            );
        }
        // 接続が失敗していれば
        else
        {
            // ログを表示
            Debug.LogError(
                "Client開始失敗"
            );
        }
    }

    /// <summary>
    /// 5秒探しても目的の部屋が見つからなかった場合
    /// </summary>
    private void SearchTimeout()
    {
        // 既に目的の部屋を見つけていれば
        if (foundRoom)
            // 処理を終える
            return;

        // エラーログを表示
        Debug.LogWarning(
            "Roomが見つかりませんでした"
        );

        // UDPを閉じる
        udp?.Close();

        // UDPを使用していない状態にする
        udp = null;
    }

    /// <summary>
    /// GameObjectが破棄されるときに呼ばれる関数
    /// </summary>
    private void OnDestroy()
    {
        // SearchTimeout() などのInvokeを停止
        CancelInvoke();

        // UDPを閉じる
        udp?.Close();

        // UDPを使用していない状態にする
        udp = null;
    }
}