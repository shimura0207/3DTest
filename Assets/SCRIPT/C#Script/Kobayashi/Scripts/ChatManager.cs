/*
 * @brief  ChatManager
 * @author Kobayashi
 */

using TMPro;
using Unity.Netcode;        // Netcode for GameObjects を使用するためのもの
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    // チャットのテキスト
    [SerializeField]
    private TMP_Text chatText;

    // テキストを入力するための場所を格納する変数
    [SerializeField]
    private TMP_InputField inputField;

    /// <summary>
    /// チャットを送信するための関数
    /// </summary>
    public void SendChat()
    {
        // テキストが入力されていなければ
        if (string.IsNullOrWhiteSpace(inputField.text))
            // この関数の処理を終了する
            return;

        // 入力した内容を渡す
        SendChatServerRpc(inputField.text);

        // 入力欄を空にする
        inputField.text = "";
    }

    // ServerRpcの宣言
    [ServerRpc(
        RequireOwnership = false
    )]

    private void SendChatServerRpc(
        // 入力したテキストの内容
        string message,
        // 送信したClientのID
        ServerRpcParams rpcParams = default
    )
    {
        // ServerRpcを呼び出したClientのIDを取得
        ulong id =
            rpcParams.Receive
            .SenderClientId;

        // IDが0ならHost、それ以外ならClient
        string name =
            id == 0
            ? "Host"
            : $"Client {id}";

        // ClientRpcを呼び出す
        ReceiveChatClientRpc(
            name,
            message
        );
    }

    [ClientRpc]
    // Serverから接続しているClientへ処理を送る
    private void ReceiveChatClientRpc(
        // 誰が送ったか
        string sender,
        // 送られたチャットの内容
        string message
    )
    {
        // チャットの内容を表示
        chatText.text += $"{sender}:{message}\n";
    }
}