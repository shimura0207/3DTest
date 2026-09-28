/*
 * @brief  NetworkUI
 * @author Kobayashi
 */

using TMPro;
using Unity.Netcode;                    // Netcode for GameObjects を使用するためのもの
using Unity.Netcode.Transports.UTP;     // Unity Transport を使用するためのもの
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [Header("UI")]
    // 現在のネットワーク状態を表示するテキスト
    [SerializeField]
    private TMP_Text statusText;

    // Clientがコードを入力するためのInputField
    [SerializeField]
    private TMP_InputField roomCodeInput;

    [Header("Network")]
    // ネットワーク通信西陽するポート番号
    [SerializeField]
    private ushort port = 7777;

    // Hostが部屋を作ったことを通知するためのクラス
    [SerializeField]
    private RoomBroadcaster broadcaster;

    // Clientが部屋を探すためのクラス
    [SerializeField]
    private RoomFinder finder;

    // Unity Transportそのものを格納するための変数
    private UnityTransport transport;

    private void Start()
    {
        // ゲームで使用しているNetworkManagerを取得する
        transport =
            NetworkManager.Singleton
            .GetComponent<UnityTransport>();

        // 画面に「Waiting...」と表示する
        statusText.text =
            "Waiting...";

        // Clientが接続した時に OnClientConnected を呼ぶ
        NetworkManager.Singleton
            .OnClientConnectedCallback +=
            OnClientConnected;

        // Clientが接続した時に OnClientDisconnected を呼ぶ
        NetworkManager.Singleton
            .OnClientDisconnectCallback +=
            OnClientDisconnected;
    }

    /// <summary>
    /// Hostとしてゲームを開始するための関数
    /// </summary>
    public void StartHost()
    {
        // どのアドレス・ポートで通信するか
        transport.SetConnectionData(
            "0.0.0.0",
            port
        );

        // 実際にHostを開始する
        bool success =
            NetworkManager.Singleton
            .StartHost();
        
        // Host開始に成功したら
        if (success)
        {
            broadcaster.StartBroadcast(
                port
            );

            // ルームコードを表示
            statusText.text = $"Room Code : {broadcaster.RoomCode}";
        }
        // Host開始に失敗したら
        else
        {
            // 失敗したテキストを表示
            statusText.text = "Host Failed";
        }
    }

    /// <summary>
    /// Clientとしてゲームに参加するための関数
    /// </summary>
    public void StartClient()
    {
        // InputFieldから文字を取得
        string code = roomCodeInput.text.Trim();

        // ルームコードが空かどうかチェック
        if (string.IsNullOrEmpty(code))
        {
            // 空という事をテキストで表示
            statusText.text = "Enter Code";

            // 処理を終える
            return;
        }

        // 部屋を探しているテキストを表示
        statusText.text = "Searching...";

        // ルームコードを探す
        finder.SearchRoom(code);
    }

    /// <summary>
    /// 誰かがネットワークに接続した時に呼ばれる関数
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientConnected(ulong clientId)
    {
        // このPCがHostだった場合
        if (NetworkManager.Singleton.IsHost)
        {
            // 接続してきたClientIDと、自分自身のClientIDが違うか
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                // 誰かが参加してきた事をテキストで表示
                statusText.text ="Player Joined";
            }
        }

        // Clientではある状態　かつ　Hostではない状態の時
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            // 接続が完了した事をテキストで表示
            statusText.text = "Connected!";
        }
    }

    /// <summary>
    /// Clientが切断された時に呼ばれる関数
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientDisconnected(ulong clientId)
    {
        // 切断された事をテキストで表示
        statusText.text = "Disconnected";
    }

    /// <summary>
    /// Unityのオブジェクトが破棄された時に呼ばれる関数
    /// </summary>
    private void OnDestroy()
    {
        // NetworkManagerが存在しないなら
        if (NetworkManager.Singleton == null)
            // 何もしない
            return;

        // 接続イベントを解除
        NetworkManager.Singleton
            .OnClientConnectedCallback -=
            OnClientConnected;

        // 切断イベントを解除
        NetworkManager.Singleton
            .OnClientDisconnectCallback -=
            OnClientDisconnected;
    }
}