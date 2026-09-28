/*
 * @brief  GameNetworkConnector
 * @author Kobayashi
 */

using UnityEngine;

public class GameNetworkConnector : MonoBehaviour
{
    /// <summary>
    /// サーバーへ接続するための関数
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="port"></param>
    /// <param name="serverName"></param>
    public void ConnectToServer(
        string ipAddress,
        int port,
        string serverName)
    {
        // コンソールにメッセージを表示する
        Debug.Log(
            $"{serverName}へ接続します。" +
            $" 接続先：{ipAddress}:{port}");
    }
}