using Mirror;
using UnityEngine;

/// <summary>
/// Mirror で各プレイヤーの現在のテーブルを同期する。
/// 【アタッチ先】NetworkManager の Player Prefab（NetworkIdentity と同じオブジェクト）
/// ※ 1人につき1つ生成される、所有権のある Player オブジェクト専用。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkIdentity))]
public class TableNetworkSync : NetworkBehaviour
{
    // SyncVar はサーバーから全クライアントへ自動で同期される。
    // -1 はまだ値を受信していない状態。
    [SyncVar]
    private int syncedTable = -1;

    public int SyncedTable => syncedTable;
    public bool HasSyncedTable => syncedTable >= 0;

    // 同じ値を毎フレーム送らないための記録。
    private int lastSubmittedTable = int.MinValue;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // 参加直後に初期値を必ず送る。
        lastSubmittedTable = int.MinValue;
    }

    private void Update()
    {
        // 自分が所有している Player だけが自分のテーブルを送信する。
        // 相手側の Player から、自分の SlotReelController を送ってはいけない。
        if (!isLocalPlayer || !NetworkClient.isConnected)
            return;

        SlotReelController slot = SlotReelController.Instance;
        if (slot == null)
            return;

        int currentTable = slot.currentTable;

        // -1 などの未設定値は送らない。0 は有効値として扱う。
        if (currentTable < 0 || currentTable == lastSubmittedTable)
            return;

        if (isServer)
        {
            // Host は同一プロセス内のサーバー値を直接変更できる。
            SetTableOnServer(currentTable);
        }
        else
        {
            // Client は Command でサーバーに送信する。
            CmdSetTable(currentTable);
        }

        lastSubmittedTable = currentTable;
    }

    // Command はこの Player を所有するクライアントからのみ実行可能。
    [Command]
    private void CmdSetTable(int newTable)
    {
        SetTableOnServer(newTable);
    }

    [Server]
    private void SetTableOnServer(int newTable)
    {
        if (newTable < 0)
            return;

        if (syncedTable == newTable)
            return;

        // 値が変化すると Mirror が相手PCに同期する。
        syncedTable = newTable;
    }
}
