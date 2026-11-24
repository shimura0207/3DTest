using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using kcp2k;

// ランダム／ROOMの接続目的を、Readyとは別にサーバーへ伝える。
public struct RoomAdmissionRequest : NetworkMessage {
    public int version;
    public bool privateRoom;
    public string code;
    public string session;
}

public partial class CustomNetworkManager {
    private bool privateRoomMode;
    private string roomCode = "";
    private string roomSession = "";
    private readonly HashSet<int> admittedPlayers = new HashSet<int>();
    private readonly HashSet<int> pendingReady = new HashSet<int>();

    public string RoomSessionId => roomSession;
    public int RoomGamePort => transport is KcpTransport kcp ? kcp.Port : 0;

    // 番号だけでなく、その募集のIDも照合する。
    // 同じIP・番号で部屋を作り直しても、古い広告からは参加できない。
    private void OnRoomAdmission(NetworkConnectionToClient conn, RoomAdmissionRequest request) {
        // 受付済みの同一接続からの再送は無視する。
        if (admittedPlayers.Contains(conn.connectionId)) return;
        bool allowed = !stopping && !matchStarted && request.version == 1 &&
            request.privateRoom == privateRoomMode &&
            (!privateRoomMode || (request.code == roomCode && request.session == roomSession));
        if (!allowed) {
            Debug.LogWarning("[ROOM] 募集方式・番号・部屋IDの不一致、または受付終了です。");
            conn.Disconnect();
            return;
        }
        // 二重送信ではReadyを二重に数えない。
        if (!admittedPlayers.Add(conn.connectionId)) return;
        if (pendingReady.Remove(conn.connectionId)) OnServerReady(conn);
    }

    // 番号を送らない古いクライアントが接続枠を占有し続けるのを防ぐ。
    private IEnumerator AdmissionTimeout(NetworkConnectionToClient conn) {
        yield return new WaitForSecondsRealtime(10f);
        // 接続IDが再利用されても、別の接続を切断しないよう参照も確認する。
        if (NetworkServer.active &&
            NetworkServer.connections.TryGetValue(conn.connectionId, out var current) &&
            ReferenceEquals(current, conn) && !admittedPlayers.Contains(conn.connectionId))
            conn.Disconnect();
    }
}
