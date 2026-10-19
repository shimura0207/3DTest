using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TitleManager : MonoBehaviour {
    public static TitleManager instance;

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
        if (state == ScreenState.Connecting &&
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
        if (started) {
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
    /// 3. 見つからなければ自分がホストになる
    /// </summary>
    public void OnRandomMatchButton() {
        // 連打や、通信中の再開始を防ぐ
        if (state != ScreenState.Menu)
            return;

        if (networkManager == null || receiver == null) {
            SetStatus("NetworkManagerまたはReceiverが未設定です");
            return;
        }

        StopSearch();
        ResetRole();

        if (hostsDisplayUI != null)
            hostsDisplayUI.ResetPanel();

        // 一覧選択画面は使わず、待機画面で検索する。
        // Connectingにはしないので、接続タイムアウト判定はまだ始まらない。
        ShowScreen(ScreenState.Waiting, "対戦相手を探しています...");

        if (!receiver.StartReceiveIP()) {
            // 受信処理の失敗を「ホストがいない」と扱わない
            ShowScreen(
                ScreenState.Menu,
                "検索を開始できません。Consoleを確認してください");
            return;
        }

        searchCoroutine = StartCoroutine(RandomMatchRoutine());
    }

    /// <summary>
    /// 自動検索と参加先の決定。
    /// キャンセル時は既存のStopSearchで停止できる。
    /// </summary>
    private IEnumerator RandomMatchRoutine() {
        // 同時に押したPCが、全く同じタイミングで
        // 検索を終了しないよう、待ち時間をランダムにずらす。
        float searchDuration =
            Mathf.Max(0.5f, searchSeconds) +
            UnityEngine.Random.Range(1f, 3f);

        yield return new WaitForSecondsRealtime(searchDuration);

        // 検索中に受信エラーなどで停止した場合
        if (!receiver.IsReceiving) {
            searchCoroutine = null;

            ShowScreen(
                ScreenState.Menu,
                "検索が停止しました。もう一度お試しください");
            yield break;
        }

        // 参加できるホストだけを候補にする
        var availableHosts =
            new System.Collections.Generic.List<UDPListener.UdpMessage>();

        foreach (UDPListener.UdpMessage host in receiver.discoveredHosts) {
            if (host.gamePlaying)
                continue;

            if (string.IsNullOrWhiteSpace(host.ip))
                continue;

            if (host.port < 1 || host.port > 65535)
                continue;

            availableHosts.Add(host);
        }

        // 検索結果を確定したので、受信を停止する
        receiver.StopReceiveIP();
        searchCoroutine = null;

        if (availableHosts.Count > 0) {
            // ----------------------------------------
            // 参加できる部屋あり → クライアントになる
            // ----------------------------------------

            int index = UnityEngine.Random.Range(0, availableHosts.Count);
            UDPListener.UdpMessage selected = availableHosts[index];

            ipAddress = selected.ip;
            isHost = false;
            isClient = true;

            Debug.Log(
                $"[RandomMatch] 部屋を選択: {selected.hostName} " +
                $"({selected.ip}:{selected.port})");

            // ここから接続タイムアウトの計測を開始する
            BeginConnecting("対戦相手に接続しています...");

            if (!networkManager.BeginClient(selected.ip, selected.port)) {
                LeaveRoom("接続を開始できませんでした");
            }

            yield break;
        }

        // ----------------------------------------
        // 参加できる部屋なし → 自分がホストになる
        // ----------------------------------------

        ipAddress = "";
        isHost = true;
        isClient = false;

        Debug.Log("[RandomMatch] 部屋がないためホストとして待機します。");

        BeginConnecting("部屋を作成しています...");

        // ホスト情報の配信はCustomNetworkManagerが開始する
        if (!networkManager.BeginHost()) {
            LeaveRoom("ホストを起動できませんでした");
        }
    }
}