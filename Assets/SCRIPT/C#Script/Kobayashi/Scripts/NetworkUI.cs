/*
 * @brief NetworkUI：ルーム作成・コード参加・自動マッチングのUI
 * 元コード作者：Kobayashi
 *
 * 【前提】
 * ・先ほどの完全版MatchingManager（State / IsMatching / IsMatched / LastErrorあり）を使用。
 * ・RoomBroadcaster.StartBroadcast(port)、RoomCode、RoomFinder.SearchRoom(code)は既存を使用。
 * ・RoomBroadcaster / RoomFinderの内部実装は未確認。そこでの通信処理は変更しない。
 * ・NetworkManagerの実際のNetworkTransportを使うためNetWorkSystemManagerへの依存を除去。
 *
 * 【設置】
 * 1. このファイルで既存のNetworkUI.csを置き換える。
 * 2. Inspectorの各参照を設定。使わないルーム機能の参照は空欄でもよい。
 * 3. ボタンのOnClickはStartHost / StartClient / StartMatchingを引き続き使用できる。
 * 4. NetworkUIの表示を隠す場合、MatchingManagerやNetworkManagerまで無効化しない。
 * 5. UIを非表示にしても、このコードは通信をShutdownしない。
 *
 * 【役割】
 * このファイルは操作入口と状態表示。ゲーム用通信のタイムアウト原因そのものを
 * 必ず解消するものではない。MatchingManagerの接続診断は引き続き必要。
 */

using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[DisallowMultipleComponent]
public class NetworkUI : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_InputField roomCodeInput;

    [Header("Network")]
    [Tooltip("ルーム作成時のゲーム用ポート。自動マッチング時はMatchingManager側の設定を使用。")]
    [SerializeField] private ushort port = 7777;
    [SerializeField] private RoomBroadcaster broadcaster;
    [SerializeField] private RoomFinder finder;
    [SerializeField] private MatchingManager matchingManager;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = true;

    // 登録先を保持し、Singletonが変更・破棄されても同じ相手からイベントを解除する。
    private NetworkManager registeredManager;

    // 自動マッチング時はMatchingManager.Stateを表示の正とする。
    // Netcodeのイベント通知順序に左右されず、失敗理由もUIに反映できる。
    private bool showMatchingState;
    private bool hasShownMatchingState;
    private MatchingManager.MatchState lastMatchingState;
    private string lastMatchingError;

    // RoomFinderのAPIに検索完了/キャンセルがないため、連打を短時間だけ抑止する。
    // これは検索タイムアウトではない。検索の重複防止・停止はRoomFinder側にも必要。
    private float nextRoomSearchAllowedAt;
    private const float RoomSearchClickInterval = 1f;
    private string lastStatus;

    // OnEnableはStartより早い。Singletonが未生成ならStart/Updateで登録を再試行する。
    private void OnEnable() { RefreshEventRegistration(); }
    private void Start() { RefreshEventRegistration(); }

    private void Update() {
        RefreshEventRegistration();
        if (showMatchingState) RefreshMatchingStatus();
    }

    private void RefreshEventRegistration() {
        NetworkManager current = NetworkManager.Singleton;
        if (registeredManager == current) return;

        UnregisterEvents();
        if (current == null) return;
        registeredManager = current;
        current.OnClientConnectedCallback += OnClientConnected;
        current.OnClientDisconnectCallback += OnClientDisconnected;
        current.OnServerStarted += OnServerStarted;
        Log("NetworkManagerイベント登録完了");
    }

    private void UnregisterEvents() {
        if (registeredManager != null) {
            registeredManager.OnClientConnectedCallback -= OnClientConnected;
            registeredManager.OnClientDisconnectCallback -= OnClientDisconnected;
            registeredManager.OnServerStarted -= OnServerStarted;
        }
        registeredManager = null;
    }

    /// <summary>3種類の開始ボタンに共通するチェック。二重起動を防ぐ。</summary>
    private bool CanStartNetwork() {
        if (!isActiveAndEnabled) return false;
        RefreshEventRegistration();
        if (registeredManager == null) {
            SetStatus("NetworkManagerがありません");
            return false;
        }
        if (matchingManager != null && (matchingManager.IsMatching || matchingManager.IsMatched)) {
            Log("自動マッチング中または接続済みのため、新しい開始操作を無視しました。");
            return false;
        }
        if (registeredManager.IsListening || registeredManager.ShutdownInProgress) {
            Log("通信中または終了処理中のため、新しい開始操作を無視しました。");
            return false;
        }
        return true;
    }

    // =========================================================
    // 手動ルーム作成：ホスト起動後、既存のRoomBroadcasterで部屋を公開
    // =========================================================
    public void StartHost() {
        if (!CanStartNetwork()) return;
        showMatchingState = false;

        // NetWorkSystemManagerの別参照ではなく、Netcodeが実際に使うTransportを取得。
        UnityTransport transport = registeredManager.NetworkConfig.NetworkTransport as UnityTransport;
        if (transport == null || port == 0) {
            SetStatus("UnityTransportまたはポート設定を確認してください");
            return;
        }

        try {
            // 第1引数はホスト自身用。第3引数でLANを含む全IPv4 NICから待ち受ける。
            transport.SetConnectionData("127.0.0.1", port, "0.0.0.0");
            SetStatus("ホスト起動中...");
            bool success = registeredManager.StartHost();
            Log("StartHost結果=" + success + " / 待受=0.0.0.0:" + port);
            if (!success) {
                SetStatus("ホスト起動失敗。Consoleを確認してください");
                return;
            }
        }
        catch (Exception ex) {
            SetStatus("ホスト起動エラー。Consoleを確認してください");
            Debug.LogException(ex, this);
            return;
        }

        // ホストの起動成功は対戦相手の接続成功ではない。
        if (broadcaster == null) {
            SetStatus("ホスト起動済み・相手の接続待ち（ルーム公開なし）");
            return;
        }
        try {
            broadcaster.StartBroadcast(port);
            SetStatus("ルームコード：" + broadcaster.RoomCode + "\n相手の接続待ち...");
        }
        catch (Exception ex) {
            // ホストはすでに起動しているため、起動失敗とは表示しない。
            SetStatus("ホスト起動済み・ルーム公開に失敗。Consoleを確認してください");
            Debug.LogException(ex, this);
        }
    }

    // =========================================================
    // コード参加：部屋の探索と接続は既存のRoomFinderへ委譲
    // =========================================================
    public void StartClient() {
        if (!CanStartNetwork()) return;
        if (Time.realtimeSinceStartup < nextRoomSearchAllowedAt) return;
        showMatchingState = false;
        if (roomCodeInput == null) {
            SetStatus("ルームコード入力欄が未設定です");
            return;
        }
        string code = roomCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code)) {
            SetStatus("ルームコードを入力してください");
            return;
        }
        if (finder == null) {
            SetStatus("RoomFinderが未設定です");
            return;
        }

        nextRoomSearchAllowedAt = Time.realtimeSinceStartup + RoomSearchClickInterval;
        SetStatus("ルーム検索中...");
        try { finder.SearchRoom(code); }
        catch (Exception ex) {
            SetStatus("ルーム検索エラー。Consoleを確認してください");
            Debug.LogException(ex, this);
        }
    }

    // =========================================================
    // 自動マッチング：開始・停止・接続管理はMatchingManagerに任せる
    // =========================================================
    public void StartMatching() {
        if (!CanStartNetwork()) return;
        if (matchingManager == null || !matchingManager.isActiveAndEnabled) {
            SetStatus("有効なMatchingManagerを設定してください");
            return;
        }
        showMatchingState = true;
        hasShownMatchingState = false;
        Log("自動マッチング開始");
        matchingManager.StartMatching();
        RefreshMatchingStatus(); // 同期的な設定エラーもすぐに画面へ表示する。
    }

    /// <summary>自動マッチングのキャンセルボタン用。接続済みの対戦は切断しない。</summary>
    public void StopMatching() {
        if (matchingManager == null) return;
        matchingManager.StopMatching();
        showMatchingState = true;
        hasShownMatchingState = false;
        RefreshMatchingStatus();
    }

    private void RefreshMatchingStatus() {
        if (matchingManager == null) return;
        MatchingManager.MatchState current = matchingManager.State;
        string error = matchingManager.LastError;
        // 毎フレーム同じ文字列を代入・ログ出力しない。
        if (hasShownMatchingState && current == lastMatchingState && error == lastMatchingError) return;
        hasShownMatchingState = true;
        lastMatchingState = current;
        lastMatchingError = error;

        switch (current) {
            case MatchingManager.MatchState.Idle: SetStatus("待機中"); break;
            case MatchingManager.MatchState.Searching: SetStatus("対戦相手を探しています..."); break;
            case MatchingManager.MatchState.WaitingForHost: SetStatus("相手発見・ホスト起動待ち..."); break;
            case MatchingManager.MatchState.WaitingForClient: SetStatus("ホスト起動中／相手の接続待ち..."); break;
            case MatchingManager.MatchState.Connecting: SetStatus("ホストに接続中..."); break;
            case MatchingManager.MatchState.Connected: SetStatus("対戦相手との接続完了！"); break;
            case MatchingManager.MatchState.Failed: SetStatus("接続失敗／切断\n" + error); break;
        }
    }

    private void OnServerStarted() {
        // ここはサーバーの起動通知。画面遷移・対戦開始は行わない。
        Log("OnServerStarted：サーバー起動。対戦相手の接続完了とは別です。");
    }

    private void OnClientConnected(ulong clientId) {
        if (registeredManager == null) return;
        Log("Client Connected=" + clientId + " / LocalClientId="
            + registeredManager.LocalClientId + " / IsHost=" + registeredManager.IsHost);

        // 自動マッチングの表示はStateに一本化する。
        // この通知時点でMatchingManagerのイベントがまだ実行されていなくても問題ない。
        if (showMatchingState) return;

        if (registeredManager.IsServer) {
            // ホスト自身が接続しただけなら、相手はまだいない。
            if (clientId == NetworkManager.ServerClientId) {
                Log("ホスト自身の接続通知。対戦成立とは判定しません。");
                return;
            }
            SetStatus("対戦相手が接続しました！");
        }
        else if (clientId == registeredManager.LocalClientId) {
            SetStatus("ホストへの接続完了！");
        }
    }

    private void OnClientDisconnected(ulong clientId) {
        if (registeredManager == null) return;
        string reason = registeredManager.DisconnectReason;
        if (string.IsNullOrEmpty(reason)) reason = "詳細通知なし。相手側Consoleも確認してください。";
        Debug.LogWarning(Prefix + "Client Disconnected=" + clientId + " / 理由=" + reason, this);
        if (showMatchingState) return; // MatchingManagerが失敗理由を確定し、Updateで表示。

        if (registeredManager.IsServer && clientId != NetworkManager.ServerClientId)
            SetStatus("クライアントが切断されました");
        else
            SetStatus("接続終了／接続失敗\n" + reason);
    }

    private void SetStatus(string message) {
        if (lastStatus != message) Log("Status: " + message);
        lastStatus = message;
        if (statusText != null && statusText.text != message) statusText.text = message;
    }

    // 異なるUIが2つ存在する場合、名前とInstanceIDで見分けられる。
    // DisallowMultipleComponentは同じGameObject内のみ。別GameObjectの重複は別途確認する。
    private string Prefix => "[NetworkUI " + gameObject.name + " #" + GetInstanceID() + "] ";
    private void Log(string message) {
        if (verboseLog) Debug.Log(Prefix + message, this);
    }

    private void OnDisable() {
        UnregisterEvents();
        hasShownMatchingState = false;
        // UI非表示は通信終了ではない。ここではShutdownしない。
    }

    private void OnDestroy() { UnregisterEvents(); }
}
