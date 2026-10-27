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
    [SerializeField] private Vector2 hostWaitSeconds = new Vector2(8f, 15f);
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
    /// 3. 見つからなければホストになり、無人なら停止して再検索する
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
    /// 検索→参加、または部屋作成→待機→停止→再検索。
    /// 通信の起動・停止を重ねず、必ず順番に処理する。
    /// </summary>
    private IEnumerator RandomMatchRoutine() {
        // StartCoroutineの代入が完了してから処理を始める。
        yield return null;
        int attempt = 0;

        while (randomMatching) {
            if (networkManager == null || receiver == null) {
                AbortRandomMatch("マッチング用コンポーネントが失われました");
                yield break;
            }

            // 期限切れの失敗候補を掃除する。
            var expired = new List<string>();
            foreach (var entry in failedRooms) {
                if (Time.unscaledTime >= entry.Value) expired.Add(entry.Key);
            }
            foreach (string key in expired) failedRooms.Remove(key);

            attempt++;
            attemptDisconnected = false;
            ResetRole();
            ShowScreen(ScreenState.Waiting, $"対戦相手を検索中... ({attempt}回目)");
            Debug.Log($"[RandomMatch] 検索開始 #{attempt}");

            if (!receiver.StartReceiveIP()) {
                AbortRandomMatch("検索を開始できません。Consoleを確認してください");
                yield break;
            }
            // 前の検索結果を再利用しない。
            receiver.discoveredHosts.Clear();

            float deadline = Time.unscaledTime + Mathf.Max(0.5f, searchSeconds)
                + RandomSeconds(new Vector2(0f, 2f));
            while (randomMatching && Time.unscaledTime < deadline) {
                if (!receiver.IsReceiving) {
                    AbortRandomMatch("ホスト検索が停止しました。Consoleを確認してください");
                    yield break;
                }
                yield return null;
            }
            if (!randomMatching) break;

            var candidates = new List<UDPListener.UdpMessage>();
            foreach (var host in receiver.discoveredHosts) {
                if (host.gamePlaying || string.IsNullOrWhiteSpace(host.ip) ||
                    host.port < 1 || host.port > 65535)
                    continue;

                if (networkManager.IsOwnEndpoint(host.ip, host.port)) continue;
                string key = host.ip + ":" + host.port;
                if (failedRooms.TryGetValue(key, out float until) &&
                    Time.unscaledTime < until) continue;
                candidates.Add(host);
            }
            receiver.StopReceiveIP();

            string attemptedRoom = null;
            bool began;
            if (candidates.Count > 0) {
                var host = candidates[matchRandom.Next(candidates.Count)];
                attemptedRoom = host.ip + ":" + host.port;
                ipAddress = host.ip;
                isClient = true;
                isHost = false;
                BeginConnecting("対戦相手に接続中...");
                Debug.Log($"[RandomMatch] 接続開始 {attemptedRoom}");
                began = networkManager.BeginClient(host.ip, host.port);
            }
            else {
                isHost = true;
                isClient = false;
                BeginConnecting("部屋を作成中...");
                Debug.Log("[RandomMatch] 候補なし。ホストを作成します");
                began = networkManager.BeginHost();
            }

            float connectDeadline = Time.unscaledTime + Mathf.Max(1f, connectTimeoutSeconds);
            float hostDeadline = Time.unscaledTime + RandomSeconds(hostWaitSeconds);
            float readyDeadline = -1f;
            bool hostWasReady = false;

            while (randomMatching && began && !attemptDisconnected) {
                if (networkManager.IsMatchStarted || networkManager.ServerMatchStarted) {
                    // 部屋状態メッセージによるNotifyRoomStateを待つ。
                    // サーバーが開始済みなら待機時間を理由に停止しない。
                    yield return null;
                    continue;
                }

                if (isHost) {
                    if (!NetworkServer.active || !NetworkClient.active) break;

                    bool localReady = NetworkClient.isConnected && NetworkClient.ready;
                    if (!localReady) {
                        if (Time.unscaledTime >= connectDeadline) break;
                    }
                    else {
                        if (!hostWasReady) {
                            hostWasReady = true;
                            hostDeadline = Time.unscaledTime + RandomSeconds(hostWaitSeconds);
                            ShowScreen(ScreenState.Waiting, "部屋を作成しました。対戦相手を待っています...");
                        }

                        if (networkManager.HasRemoteParticipant) {
                            if (readyDeadline < 0f) {
                                readyDeadline = Time.unscaledTime + Mathf.Max(1f, readyTimeoutSeconds);
                                SetStatus("相手が接続中です。準備完了を待っています...");
                            }
                            if (Time.unscaledTime >= readyDeadline) {
                                // 準備が終わらない相手だけ切断。部屋自体は維持する。
                                networkManager.DisconnectUnreadyRemoteParticipants();
                                readyDeadline = Time.unscaledTime + Mathf.Max(1f, readyTimeoutSeconds);
                            }
                        }
                        else {
                            if (readyDeadline >= 0f) {
                                readyDeadline = -1f;
                                hostDeadline = Time.unscaledTime + RandomSeconds(hostWaitSeconds);
                                SetStatus("対戦相手を待っています...");
                            }
                            if (Time.unscaledTime >= hostDeadline) {
                                // 停止直前にも相手の有無と対戦開始を再確認する。
                                internalStopping = true;
                                bool stopped = networkManager.TryStopEmptyHostForRematch();
                                internalStopping = false;
                                if (stopped) break;
                            }
                        }
                    }
                }
                else {
                    if (!NetworkClient.active) break;
                    if (NetworkClient.isConnected) {
                        if (readyDeadline < 0f) {
                            readyDeadline = Time.unscaledTime + Mathf.Max(1f, readyTimeoutSeconds);
                        }
                        if (Time.unscaledTime >= readyDeadline) break;
                    }
                    else if (Time.unscaledTime >= connectDeadline) break;
                }
                yield return null;
            }

            // 対戦開始または明示的キャンセル。再接続を行わない。
            if (!randomMatching) break;

            if (attemptedRoom != null) {
                failedRooms[attemptedRoom] = Time.unscaledTime +
                    Mathf.Max(1f, failedRoomCooldownSeconds);
            }

            Debug.Log("[RandomMatch] 試行終了。停止完了後に再検索します");
            yield return StopNetworkForRetry();
            if (!randomMatching) break;
            if (!stopSucceeded) {
                AbortRandomMatch("通信の停止が完了しませんでした。再生を停止して確認してください");
                yield break;
            }

            ResetRole();
            ShowScreen(ScreenState.Waiting, "対戦相手を探し直しています...");
            yield return new WaitForSecondsRealtime(RandomSeconds(retryDelaySeconds));
        }

        searchCoroutine = null;
    }

    /// <summary>
    /// 自動再試行用の終了処理。切断通知でメニューへ戻さない。
    /// </summary>
    private IEnumerator StopNetworkForRetry() {
        stopSucceeded = false;
        internalStopping = true;
        try {
            receiver.StopReceiveIP();
            networkManager.LeaveRoom();

            float deadline = Time.unscaledTime + Mathf.Max(1f, stopTimeoutSeconds);
            while (NetworkClient.active || NetworkServer.active) {
                if (Time.unscaledTime >= deadline) yield break;
                yield return null;
            }
            // 停止コールバックの処理を終えてから次の起動へ進む。
            yield return null;
            stopSucceeded = true;
        }
        finally {
            internalStopping = false;
        }
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
