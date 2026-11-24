using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

/// <summary>MatchingNetMatchのROOM追加部分。同じコンポーネントのInspectorに表示される。</summary>
public partial class MatchingNetMatch {
    [Header("4桁ROOMマッチ（同じLAN内）")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [Tooltip("未登録時に自動取得するTMP_InputFieldのGameObject名。完全一致で検索します。")]
    [SerializeField] private string roomCodeInputObjectName = "RoomCodeInput";
    [SerializeField, Range(1, 65535)] private int roomDiscoveryPort = 55556;
    [SerializeField, Min(3f)] private float roomSearchSeconds = 5f;
    private RoomDiscovery roomDiscovery;
    private string activeRoomCode = "";
    private bool roomMatching;
    private bool roomHosting;
    private float roomJoinDeadline;

    // 自動生成UIは既存statusTextの登録有無に依存せず、この表示文を読む。
    public string RoomMatchStatus { get; private set; } = "";
    public bool CanStartRoomMatch => state == ScreenState.Menu && !randomMatching &&
        !roomMatching && !NetworkClient.active && !NetworkServer.active;

    // 動的UIを直接結び付ける。戻り値を使って画面終了時に元の参照へ戻す。
    public TMP_InputField BindRoomInput(TMP_InputField input) {
        TMP_InputField previous = roomCodeInput;
        roomCodeInput = input;
        return previous;
    }

    /// <summary>部屋作成ボタンのOnClickに登録する。</summary>
    public void OnCreateRoomButton() => StartRoomMatch(true);
    /// <summary>部屋参加ボタンのOnClickに登録する。</summary>
    public void OnJoinRoomButton() => StartRoomMatch(false);

    /// <summary>
    /// ボタンを押した時点で入力欄を取得する。
    /// 起動後に生成されたUIも対象にし、毎フレームの検索は行わない。
    /// Inspector登録済み・取得済みなら、その参照を優先する。
    /// </summary>
    private bool ResolveRoomCodeInput() {
        if (roomCodeInput != null) return true;
        TMP_InputField found = null;
        int count = 0;

        // まず、この画面に割り当てられたメニュー内を検索する。
        // true指定で非表示の子オブジェクトも取得する。
        if (menuPanel != null) {
            foreach (var input in menuPanel.GetComponentsInChildren<TMP_InputField>(true)) {
                if (input.gameObject.name != roomCodeInputObjectName) continue;
                found = input;
                count++;
            }
        }

        // メニュー外に配置されている場合は、ロード済みシーンを検索する。
        // Resourcesで取得したPrefab等のアセットはscene判定で除外する。
        if (count == 0) {
            foreach (var input in Resources.FindObjectsOfTypeAll<TMP_InputField>()) {
                if (!input.gameObject.scene.IsValid() || !input.gameObject.scene.isLoaded ||
                    input.gameObject.name != roomCodeInputObjectName) continue;
                found = input;
                count++;
            }
        }

        // 同名が複数あるときは、別画面の番号を誤って使わないよう中止する。
        if (count != 1) {
            string message = count == 0
                ? $"入力欄「{roomCodeInputObjectName}」が見つかりません"
                : $"入力欄「{roomCodeInputObjectName}」が複数あります。名前を一意にしてください";
            SetStatus(message);
            Debug.LogError("[ROOM] " + message, this);
            return false;
        }
        roomCodeInput = found;
        return true;
    }

    private void StartRoomMatch(bool create) {
        // ランダムマッチ中・接続中に別の募集を並行開始しない。
        if (state != ScreenState.Menu || randomMatching || roomMatching ||
            NetworkClient.active || NetworkServer.active) return;
        if (!ResolveRoomCodeInput()) return;
        string code = roomCodeInput.text;
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
