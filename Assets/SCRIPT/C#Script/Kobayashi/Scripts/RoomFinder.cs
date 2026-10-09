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

public class RoomFinder : MonoBehaviour {
    [Header("Network")]

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
    public void SearchRoom(string code) {
        // 既に検索済みかどうか
        if (foundRoom) {
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
        try {
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
        catch (Exception e) {
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
                $"受信データ : {message}"
            );

            string[] split =
                message.Split('|');

            if (split.Length < 3) {
                Debug.LogWarning(
                    "データ形式不正"
                );

                return;
            }

            string hostCode = split[0];
            string hostIP = split[1];

            if (!ushort.TryParse(
                split[2],
                out ushort hostPort)) {
                Debug.LogWarning(
                    "ポート番号が不正です"
                );

                return;
            }

            string inputCode =
                (string)result.AsyncState;

            Debug.Log(
                $"Hostコード:[{hostCode}] " +
                $"入力:[{inputCode}]"
            );

            if (hostCode == inputCode) {
                foundRoom = true;

                CancelInvoke(
                    nameof(SearchTimeout)
                );

                Debug.Log(
                    $"接続先発見 : " +
                    $"{hostIP}:{hostPort}"
                );

                MainThreadDispatcher
                    .Instance
                    .Enqueue(
                    () => {
                        Connect(
                            hostIP,
                            hostPort
                        );
                    });

                return;
            }

            udp.BeginReceive(
                Receive,
                inputCode
            );
        }
        catch (Exception e) {
            Debug.LogError(
                $"UDP受信エラー : {e}"
            );
        }
    }

    /// <summary>
    /// 見つけたHostへNetcodeで接続するための関数
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    private void Connect(string ip, ushort port) {
        // NetworkManagerのシングルトンがあるか
        if (NetworkManager.Singleton == null) return;
        // NetworkManagerがUnityTransporterを所持しているか
        if (NetWorkSystemManager.Instance == null || NetWorkSystemManager.Instance.UnityTransport) return;
        // UnityTransportの取得
        var transport = NetWorkSystemManager.Instance.UnityTransport;

        // Unity Transportがあるかどうか確認
        if (transport == null) {
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
        if (result) {
            // ログを表示
            Debug.Log(
                "Client開始"
            );
        }
        // 接続が失敗していれば
        else {
            // ログを表示
            Debug.LogError(
                "Client開始失敗"
            );
        }
    }

    /// <summary>
    /// 5秒探しても目的の部屋が見つからなかった場合
    /// </summary>
    private void SearchTimeout() {
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
    private void OnDestroy() {
        // SearchTimeout() などのInvokeを停止
        CancelInvoke();

        // UDPを閉じる
        udp?.Close();

        // UDPを使用していない状態にする
        udp = null;
    }
}