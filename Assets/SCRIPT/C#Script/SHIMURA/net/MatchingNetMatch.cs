using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MatchingNetMatch : MonoBehaviour {
    public static MatchingNetMatch instance;

    [Header("管理コンポーネント")]
    [SerializeField] private CustomNetworkManager networkManager;
    [SerializeField] private UDPListener receiver;
    [SerializeField] private HostSelectUI hostsDisplayUI;

    [Header("画面")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject searchPanel;
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private GameObject battlePanel;

    [Header("表示")]
    [SerializeField] private TMP_Text statusText;

    [Header("時間設定")]
    [SerializeField] private float searchSeconds = 3f;
    [SerializeField] private float connectTimeoutSeconds = 15f;

    [Header("ゲーム側へ接続するイベント")]
    [Tooltip("両端末で呼ばれる。カード処理の通信同期は別途必要")]
    public UnityEvent onBattleStarted = new UnityEvent();

    [Tooltip("対戦中に相手が退出、または自分が退出した際に呼ばれる")]
    public UnityEvent onBattleEnded = new UnityEvent();

    public string ipAddress { get; private set; } = "";
    public bool isHost { get; private set; }
    public bool isClient { get; private set; }
    public bool isTitle { get; private set; } = true;

    private enum ScreenState {
        Menu,
        Searching,
        Selecting,
        Connecting,
        Waiting,
        Battle
    }

    private ScreenState state;
    private Coroutine searchCoroutine;
    private float connectionDeadline;
    private bool battleRunning;

    [Header("自動マッチング再試行")]
    [SerializeField] private Vector2 retryDelaySeconds = new Vector2(0.5f, 2f);
    [SerializeField] private float readyTimeoutSeconds = 15f;
    [SerializeField] private float failedRoomCooldownSeconds = 20f;
    [SerializeField] private float stopTimeoutSeconds = 5f;

    public bool IsRandomMatching => randomMatching;
    private bool randomMatching;
    private bool internalStopping;
    private bool attemptDisconnected;
    private bool stopSucceeded;
    private readonly Dictionary<string, float> failedRooms = new Dictionary<string, float>();

    // Unityの抽選乱数とは分離し、PCごとに異なる待機時間にする。
    private readonly System.Random matchRandom =
        new System.Random(System.Guid.NewGuid().GetHashCode());

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // 1シーン構成なのでDontDestroyOnLoadは不要
    }

    private void Start() {
        ShowScreen(ScreenState.Menu, "");
    }

    private void Update() {
        if (!randomMatching && state == ScreenState.Connecting &&
            Time.unscaledTime >= connectionDeadline) {
            LeaveRoom("接続がタイムアウトしました");
        }
    }

    /// <summary>
    /// ホスト作成ボタン。
    /// </summary>
    public void OnStartHostButton() {
        if (state != ScreenState.Menu)
            return;

        if (networkManager == null) {
            SetStatus("NetworkManagerが未設定です");
            return;
        }

        isHost = true;
        isClient = false;

        BeginConnecting("ホストを起動中...");

        if (!networkManager.BeginHost()) {
            LeaveRoom("ホストを起動できませんでした");
        }
    }

    /// <summary>
    /// 参加ボタン・再検索ボタン。
    /// </summary>
    public void OnStartClientButton() {
        if (randomMatching) return;
        if (state != ScreenState.Menu &&
            state != ScreenState.Searching &&
            state != ScreenState.Selecting) {
            return;
        }

        if (receiver == null || hostsDisplayUI == null ||
            networkManager == null) {
            SetStatus("検索に必要な参照が未設定です");
            return;
        }

        StopSearch();

        isHost = false;
        isClient = false;
        ipAddress = "";

        hostsDisplayUI.ResetPanel();
        ShowScreen(ScreenState.Searching, "ホストを検索中...");

        if (!receiver.StartReceiveIP()) {
            ShowScreen(
                ScreenState.Selecting,
                "検索を開始できません。Consoleを確認してください");
            return;
        }

        searchCoroutine = StartCoroutine(SearchHosts());
    }

    private IEnumerator SearchHosts() {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.1f, searchSeconds));

        hostsDisplayUI.ShowHostList(receiver.discoveredHosts);

        ShowScreen(
            ScreenState.Selecting,
            receiver.discoveredHosts.Count > 0
                ? "参加するホストを選んでください"
                : "ホストが見つかりません。検索を継続しています");

        // 一覧表示後も受信を続け、新しく見つかった部屋を追加する
        while (!hostsDisplayUI.isSelected) {
            if (!receiver.IsReceiving) {
                SetStatus("検索が停止しました。再検索してください");
                searchCoroutine = null;
                yield break;
            }

            yield return null;
        }

        UDPListener.UdpMessage selected =
            hostsDisplayUI.selectedHost;

        receiver.StopReceiveIP();
        searchCoroutine = null;

        ipAddress = selected.ip;
        isHost = false;
        isClient = true;

        BeginConnecting($"{selected.hostName} に接続中...");

        if (!networkManager.BeginClient(selected.ip, selected.port)) {
            LeaveRoom("接続を開始できませんでした");
        }
    }

    private void BeginConnecting(string message) {
        connectionDeadline = Time.unscaledTime +
            Mathf.Max(1f, connectTimeoutSeconds);

        ShowScreen(ScreenState.Connecting, message);
    }

    /// <summary>
    /// NetworkManagerから接続完了時に呼ばれる。
    /// </summary>
    public void NotifyConnected() {
        if (internalStopping) return;
        if (state == ScreenState.Connecting) {
            ShowScreen(
                ScreenState.Waiting,
                "接続完了。対戦相手を待っています...");
        }
    }

    /// <summary>
    /// サーバーから届いた部屋状態を反映する。
    /// </summary>
    public void NotifyRoomState(bool started, string message) {
        if (internalStopping) return;
        if (started) {
            // イベントが画面を切り替える前に自動再試行を終了する。
            randomMatching = false;
            failedRooms.Clear();
            receiver?.StopReceiveIP();
            ShowScreen(ScreenState.Battle, message);

            if (!battleRunning) {
                battleRunning = true;
                onBattleStarted.Invoke();
            }
        }
        else {
            EndBattleIfNeeded();
            ShowScreen(ScreenState.Waiting, message);
        }
    }

    public void NotifyDisconnected() {
        if (internalStopping) return;

        if (randomMatching) {
            // コルーチンは止めず、現在の試行だけ失敗にする。
            attemptDisconnected = true;
            SetStatus("接続できませんでした。再検索します...");
            return;
        }

        StopSearch();
        EndBattleIfNeeded();
        ResetRole();
        ShowScreen(ScreenState.Menu, "接続を終了しました");
    }

    /// <summary>
    /// 検索の戻る・接続キャンセル・退室ボタンに登録する。
    /// </summary>
    public void OnReturnButtonClicked() {
        LeaveRoom("");
    }

    public void OnLeaveRoomButton() {
        LeaveRoom("");
    }

    private void LeaveRoom(string message) {
        StopSearch();

        if (networkManager != null)
            networkManager.LeaveRoom();

        EndBattleIfNeeded();
        ResetRole();
        ShowScreen(ScreenState.Menu, message);
    }

    private void StopSearch() {
        randomMatching = false;
        failedRooms.Clear();
        if (searchCoroutine != null) {
            StopCoroutine(searchCoroutine);
            searchCoroutine = null;
        }

        if (receiver != null)
            receiver.StopReceiveIP();
    }

    private void EndBattleIfNeeded() {
        if (!battleRunning)
            return;

        battleRunning = false;
        onBattleEnded.Invoke();
    }

    private void ResetRole() {
        isHost = false;
        isClient = false;
        ipAddress = "";
    }

    private void ShowScreen(ScreenState next, string message) {
        state = next;
        isTitle = next != ScreenState.Battle;

        SetPanel(menuPanel, next == ScreenState.Menu);
        SetPanel(searchPanel,
            next == ScreenState.Searching ||
            next == ScreenState.Selecting);
        SetPanel(waitingPanel,
            next == ScreenState.Connecting ||
            next == ScreenState.Waiting);
        SetPanel(battlePanel, next == ScreenState.Battle);

        SetStatus(message);
    }

    private static void SetPanel(GameObject panel, bool visible) {
        if (panel != null)
            panel.SetActive(visible);
    }

    private void SetStatus(string message) {
        if (statusText != null)
            statusText.text = message;
    }

    private void OnDestroy() {
        StopSearch();

        if (instance == this)
            instance = null;
    }


    /// <summary>
    /// ランダムマッチボタンから呼ぶ。
    ///
    /// 1. ホストを検索する
    /// 2. 参加可能なホストがいればランダムに参加する
    /// 3. 見つからなければホストを維持したまま検索する
    /// </summary>
    public void OnRandomMatchButton() {
        if (state != ScreenState.Menu || randomMatching ||
            NetworkClient.active || NetworkServer.active)
            return;

        if (networkManager == null || receiver == null) {
            SetStatus("NetworkManagerまたはReceiverが未設定です");
            return;
        }

        StopSearch();
        ResetRole();
        if (hostsDisplayUI != null) hostsDisplayUI.ResetPanel();

        randomMatching = true;
        attemptDisconnected = false;
        searchCoroutine = StartCoroutine(RandomMatchRoutine());
    }

    /// <summary>
    /// 最初は検索のみ。候補がなければ部屋を公開しながら検索します。
    /// ホスト同士ではroomIdの小さい方を残し、大きい方だけが参加します。
    /// IDは部屋を作り直しても変えないため、相互に役割を交換し続けません。
    /// </summary>
    private IEnumerator RandomMatchRoutine() {
        yield return null;
        while (randomMatching) {
            attemptDisconnected = false;
            ResetRole();
            ShowScreen(ScreenState.Waiting, "対戦相手を検索中...");
            if (!receiver.StartReceiveIP()) {
                AbortRandomMatch("検索を開始できません。Consoleを確認してください");
                yield break;
            }

            float searchEnd = Time.unscaledTime + Mathf.Max(0.5f, searchSeconds)
                + RandomSeconds(new Vector2(0f, 1f));
            while (randomMatching && Time.unscaledTime < searchEnd) yield return null;
            if (!randomMatching) break;
            if (!receiver.IsReceiving) {
                AbortRandomMatch("UDP検索が停止しました。Consoleを確認してください");
                yield break;
            }

            UDPListener.UdpMessage target;
            bool hasTarget = TryGetCandidate(false, out target);
            if (!hasTarget) {
                isHost = true;
                BeginConnecting("部屋を作成中...");
                Debug.Log("[RandomMatch] 部屋を公開しながら検索を継続します");
                // UDPListenerは停止しません。ゲーム接続とは別のソケットです。
                if (!networkManager.BeginHost()) {
                    AbortRandomMatch("ホストを開始できません。ゲームポートの使用状況を確認してください");
                    yield break;
                }

                float localDeadline = Time.unscaledTime + Mathf.Max(1f, connectTimeoutSeconds);
                float remoteReadyDeadline = -1f;
                while (randomMatching && !attemptDisconnected) {
                    // Ready通知が処理されるまで部屋を維持します。
                    if (networkManager.ServerMatchStarted || networkManager.IsMatchStarted) {
                        yield return null;
                        continue;
                    }
                    if (!NetworkServer.active || !NetworkClient.active) break;
                    if (!receiver.IsReceiving || !networkManager.IsRoomBroadcasting) {
                        AbortRandomMatch("公開中のUDP検索または配信が停止しました");
                        yield break;
                    }
                    if (!NetworkClient.isConnected || !NetworkClient.ready) {
                        if (Time.unscaledTime >= localDeadline) break;
                        yield return null;
                        continue;
                    }

                    // 参加処理中の相手を置き去りにして別の部屋へ移動しません。
                    if (networkManager.HasRemoteParticipant) {
                        if (remoteReadyDeadline < 0f)
                            remoteReadyDeadline = Time.unscaledTime + Mathf.Max(1f, readyTimeoutSeconds);
                        if (Time.unscaledTime >= remoteReadyDeadline) {
                            networkManager.DisconnectUnreadyRemoteParticipants();
                            remoteReadyDeadline = Time.unscaledTime + Mathf.Max(1f, readyTimeoutSeconds);
                        }
                    }
                    else {
                        remoteReadyDeadline = -1f;
                        // 自分より優先される部屋だけを選びます。
                        if (TryGetCandidate(true, out target)) {
                            bool stopped;
                            internalStopping = true;
                            try {
                                // 停止する直前にも参加者・対戦開始状態を確認します。
                                stopped = networkManager.TryStopEmptyHostForRematch();
                            }
                            finally { internalStopping = false; }
                            if (stopped) {
                                Debug.Log($"[RandomMatch] 優先部屋へ移動: {target.roomId} {target.ip}:{target.port}");
                                hasTarget = true;
                                yield return WaitForNetworkStop();
                                if (!randomMatching) break;
                                if (!stopSucceeded) {
                                    AbortRandomMatch("ホスト停止が完了しませんでした");
                                    yield break;
                                }
                                break;
                            }
                        }
                    }
                    yield return null;
                }
            }
            if (!randomMatching) break;

            if (hasTarget) {
                // 接続先を選んだら検索を一旦止めます。接続失敗なら再開します。
                receiver.StopReceiveIP();
                ResetRole();
                isClient = true;
                ipAddress = target.ip;
                attemptDisconnected = false;
                BeginConnecting($"{target.hostName} に接続中...");
                bool began = networkManager.BeginClient(target.ip, target.port);
                float deadline = Time.unscaledTime + Mathf.Max(1f, connectTimeoutSeconds);
                float readyDeadline = -1f;
                while (randomMatching && began && !attemptDisconnected) {
                    if (networkManager.IsMatchStarted) {
                        yield return null;
                        continue;
                    }
                    if (!NetworkClient.active) break;
                    if (NetworkClient.isConnected) {
                        if (readyDeadline < 0f)
                            readyDeadline = Time.unscaledTime + Mathf.Max(1f, readyTimeoutSeconds);
                        if (Time.unscaledTime >= readyDeadline) break;
                    }
                    else if (Time.unscaledTime >= deadline) break;
                    yield return null;
                }
                if (!randomMatching) break;
                // 広告受信直後に満員・退出となる競合は再試行で処理します。
                failedRooms[UDPListener.GetKey(target)] = Time.unscaledTime
                    + Mathf.Max(1f, failedRoomCooldownSeconds);
            }

            yield return StopNetworkForRetry();
            if (!randomMatching) break;
            if (!stopSucceeded) {
                AbortRandomMatch("通信の停止が完了しませんでした");
                yield break;
            }
            ShowScreen(ScreenState.Waiting, "接続できなかったため再検索します...");
            yield return new WaitForSecondsRealtime(RandomSeconds(retryDelaySeconds));
        }
        searchCoroutine = null;
    }

    /// <summary>
    /// 自分・満員・古い広告・最近失敗した部屋を除外します。
    /// ホスト中は自分より小さいIDのみ。候補内でも最小IDを優先します。
    /// </summary>
    private bool TryGetCandidate(bool hosting, out UDPListener.UdpMessage selected) {
        selected = default;
        bool found = false;
        var expired = new List<string>();
        foreach (var entry in failedRooms)
            if (Time.unscaledTime >= entry.Value) expired.Add(entry.Key);
        foreach (var key in expired) failedRooms.Remove(key);

        foreach (var host in receiver.discoveredHosts) {
            if (host.gamePlaying || !receiver.IsFresh(host) ||
                string.IsNullOrWhiteSpace(host.ip) || string.IsNullOrEmpty(host.roomId) ||
                host.roomId == UDPBroadcaster.LocalRoomId || host.port < 1 || host.port > 65535)
                continue;
            if (hosting && System.String.CompareOrdinal(host.roomId, UDPBroadcaster.LocalRoomId) >= 0)
                continue;
            if (failedRooms.ContainsKey(UDPListener.GetKey(host))) continue;
            if (!found || System.String.CompareOrdinal(host.roomId, selected.roomId) < 0) {
                selected = host;
                found = true;
            }
        }
        return found;
    }

    /// <summary>停止コールバック完了後に次の接続を開始します。</summary>
    private IEnumerator WaitForNetworkStop() {
        stopSucceeded = false;
        internalStopping = true;
        try {
            float deadline = Time.unscaledTime + Mathf.Max(1f, stopTimeoutSeconds);
            while (NetworkClient.active || NetworkServer.active) {
                if (Time.unscaledTime >= deadline) yield break;
                yield return null;
            }
            yield return null;
            stopSucceeded = true;
        }
        finally { internalStopping = false; }
    }

    private IEnumerator StopNetworkForRetry() {
        internalStopping = true;
        try {
            receiver.StopReceiveIP();
            networkManager.LeaveRoom();
            yield return WaitForNetworkStop();
        }
        finally { internalStopping = false; }
    }

    private float RandomSeconds(Vector2 range) {
        float min = Mathf.Max(0f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
        return min + (float)matchRandom.NextDouble() * (max - min);
    }

    private void AbortRandomMatch(string message) {
        randomMatching = false;
        receiver?.StopReceiveIP();
        internalStopping = true;
        try { networkManager?.LeaveRoom(); }
        finally { internalStopping = false; }
        ResetRole();
        searchCoroutine = null;
        ShowScreen(ScreenState.Menu, message);
        Debug.LogError("[RandomMatch] " + message);
    }
}
