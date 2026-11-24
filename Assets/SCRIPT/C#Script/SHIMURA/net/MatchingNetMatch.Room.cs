using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

/// <summary>MatchingNetMatchのROOM追加部分。同じコンポーネントのInspectorに表示される。</summary>
public partial class MatchingNetMatch {
    [Header("4桁ROOMマッチ（同じLAN内）")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField, Range(1, 65535)] private int roomDiscoveryPort = 55556;
    [SerializeField, Min(3f)] private float roomSearchSeconds = 5f;
    private RoomDiscovery roomDiscovery;
    private string activeRoomCode = "";
    private bool roomMatching;
    private bool roomHosting;
    private float roomJoinDeadline;

    /// <summary>部屋作成ボタンのOnClickに登録する。</summary>
    public void OnCreateRoomButton() => StartRoomMatch(true);
    /// <summary>部屋参加ボタンのOnClickに登録する。</summary>
    public void OnJoinRoomButton() => StartRoomMatch(false);

    private void StartRoomMatch(bool create) {
        // ランダムマッチ中・接続中に別の募集を並行開始しない。
        if (state != ScreenState.Menu || randomMatching || roomMatching ||
            NetworkClient.active || NetworkServer.active) return;
        string code = roomCodeInput != null ? roomCodeInput.text : "";
        if (!RoomDiscovery.IsValidCode(code)) {
            SetStatus("部屋番号を半角数字4桁で入力してください（例：0001）");
            return;
        }
        if (networkManager == null) { SetStatus("NetworkManagerが未設定です"); return; }
        StopSearch();
        ResetRole();
        hostsDisplayUI?.ResetPanel();
        if (roomDiscovery == null) roomDiscovery = gameObject.AddComponent<RoomDiscovery>();
        if (!roomDiscovery.BeginSearch(roomDiscoveryPort)) {
            SetStatus(roomDiscovery.LastError); return;
        }
        activeRoomCode = code;
        roomMatching = true;
        roomHosting = false;
        ShowScreen(ScreenState.Searching, create ? "同じ部屋番号がないか確認中..." : "部屋を検索中...");
        searchCoroutine = StartCoroutine(RoomSearchRoutine(create, code));
    }

    private IEnumerator RoomSearchRoutine(bool create, string code) {
        // 一定時間収集し、同番号の複数ホストも検出する。
        float deadline = Time.unscaledTime + Mathf.Max(3f, roomSearchSeconds);
        while (roomMatching && Time.unscaledTime < deadline) yield return null;
        searchCoroutine = null;
        if (!roomMatching) yield break;
        int count = roomDiscovery.FindRooms(code, "", out var room);
        if (create) {
            if (count > 0) { LeaveRoom("この番号は使用中です。別の4桁に変更してください"); yield break; }
            isHost = true;
            roomHosting = true;
            BeginConnecting("部屋を作成中...");
            if (!networkManager.BeginRoomHost(code)) {
                LeaveRoom("部屋を作成できませんでした"); yield break;
            }
            if (!roomDiscovery.Advertise(networkManager, code)) {
                LeaveRoom("部屋を公開できませんでした"); yield break;
            }
            SetStatus("参加者を待っています...");
        }
        else {
            if (count == 0) { LeaveRoom("部屋が見つかりません。番号とホストの待機状態を確認してください"); yield break; }
            if (count > 1) { LeaveRoom("同じ番号の部屋が複数あります。ホスト側で番号を変更してください"); yield break; }
            if (room.full) { LeaveRoom("この部屋は満員です"); yield break; }
            roomDiscovery.StopDiscovery();
            ipAddress = room.ip;
            isClient = true;
            roomJoinDeadline = Time.unscaledTime + Mathf.Max(1f, connectTimeoutSeconds);
            BeginConnecting("部屋へ接続中...");
            if (!networkManager.BeginRoomClient(room.ip, room.port, code, room.session))
                LeaveRoom("接続を開始できませんでした");
        }
    }

    private void UpdateRoomMatch() {
        if (!roomMatching) return;
        if (networkManager.IsMatchStarted || networkManager.ServerMatchStarted) {
            roomMatching = false;
            roomDiscovery?.StopDiscovery();
            return;
        }
        if (!roomHosting && isClient) {
            // TCP/KCP接続だけ完了して照合・対戦開始が進まないケースもタイムアウトする。
            if (Time.unscaledTime >= roomJoinDeadline)
                LeaveRoom("参加確認が完了しませんでした。部屋番号を確認して再試行してください");
            return;
        }
        if (roomDiscovery == null || !roomDiscovery.IsRunning) {
            LeaveRoom("ROOM検索または公開が停止しました。Consoleを確認してください");
            return;
        }
        if (roomHosting && roomDiscovery.FindRooms(activeRoomCode, networkManager.RoomSessionId, out _) > 0) {
            // 同時に同じ番号を作成した場合、誤接続を防ぐため募集を中止する。
            LeaveRoom("同じ番号の部屋が検出されました。別の番号で作成してください");
        }
    }

    private void StopRoomDiscovery() {
        roomMatching = false;
        roomHosting = false;
        activeRoomCode = "";
        roomDiscovery?.StopDiscovery();
    }
}
