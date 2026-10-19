using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using kcp2k;

// ============================================================
// ネットワークメッセージ
// 同じ名前の定義が別ファイルにある場合は、重複分を削除する。
// ============================================================

/// <summary>
/// サーバーから両プレイヤーへ通知する部屋の状態。
/// </summary>
public struct CardRoomStateMessage : NetworkMessage {
    public bool started;
    public string text;
}

/// <summary>
/// 送信したプレイヤーからサーバーへ送る整数。
/// </summary>
public struct SendValueRequest : NetworkMessage {
    public int value;
}

/// <summary>
/// サーバーから送信者の対戦相手へ転送する整数。
/// </summary>
public struct OpponentValueMessage : NetworkMessage {
    public int value;
}

/// <summary>
/// 1シーン・2人対戦用のMirrorネットワーク管理。
///
/// ・ホストを含む2人の準備完了で対戦開始
/// ・シーン移動なし
/// ・プレイヤーPrefabの自動生成なし
/// ・ホスト起動後にUDPで部屋情報を配信
/// ・双方から相手へ整数を送信可能
///
/// このコンポーネントは、画面切り替えで非表示になる
/// パネルの子には置かないこと。
/// </summary>
public class CustomNetworkManager : NetworkManager {
    // ============================================================
    // Inspector設定
    // ============================================================

    [Header("カードゲーム用の参照")]

    [Tooltip("画面切り替えを担当するTitleManager")]
    [SerializeField]
    private TitleManager title;

    [Tooltip("LAN内にホスト情報を配信するコンポーネント")]
    [SerializeField]
    private UDPBroadcaster broadcaster;

    [Header("整数送信テスト")]

    [Tooltip("SendTestValueを呼んだときに相手へ送信する値")]
    [SerializeField]
    private int testSendValue = 123;

    // ============================================================
    // 外部から確認できる状態
    // ============================================================

    /// <summary>
    /// このPCが対戦開始通知を受け取っているか。
    /// ホスト・クライアント双方で参照できる。
    /// </summary>
    public bool IsMatchStarted { get; private set; }

    /// <summary>
    /// この対戦で相手から整数を受信したことがあるか。
    /// 受信値が0の場合と、未受信の場合を区別できる。
    /// </summary>
    public bool HasReceivedValue { get; private set; }

    /// <summary>
    /// 最後に相手から受信した整数。
    /// メッセージを受信した際に、このPC上で更新される。
    /// </summary>
    public int receivedValue { get; private set; }

    /// <summary>
    /// 相手から整数を受信したときに、このPCで発生するイベント。
    /// </summary>
    public event Action<int> OnOpponentValueReceived;

    // ============================================================
    // サーバー側の管理状態
    // ============================================================

    /// <summary>
    /// 接続準備が完了したプレイヤーの接続ID。
    /// サーバー側だけで管理する。
    /// </summary>
    private readonly HashSet<int> readyPlayers =
        new HashSet<int>();

    /// <summary>
    /// サーバー側で管理する対戦開始状態。
    /// 通常のクライアントでは使用しない。
    /// </summary>
    private bool matchStarted;

    /// <summary>
    /// 自分から部屋を閉じている最中か。
    /// 終了中の退出通知から、待機状態に戻さないために使う。
    /// </summary>
    private bool stopping;

    // ============================================================
    // 通信開始
    // ============================================================

    /// <summary>
    /// ホストとして開始する。
    /// TitleManagerのホスト作成処理から呼ぶ。
    ///
    /// trueは起動処理が開始できたことを示す。
    /// 対戦開始は2人の準備完了後。
    /// </summary>
    public bool BeginHost() {
        if (!PrepareNetwork())
            return false;

        if (broadcaster == null) {
            Debug.LogError(
                "[Network] Broadcasterが未設定です。",
                this);
            return false;
        }

        if (!broadcaster.isActiveAndEnabled) {
            Debug.LogError(
                "[Network] UDPBroadcasterを有効にしてください。",
                this);
            return false;
        }

        stopping = false;

        try {
            Debug.Log("[Network] ホストを開始します。", this);

            StartHost();

            return NetworkServer.active && NetworkClient.active;
        }
        catch (Exception exception) {
            Debug.LogException(exception, this);
            LeaveRoom();
            return false;
        }
    }

    /// <summary>
    /// 選択したホストへ接続する。
    /// UDPで取得したIPとゲーム接続用ポートを指定する。
    ///
    /// trueは接続処理を開始できたことを示す。
    /// 接続成功はOnClientConnectで通知される。
    /// </summary>
    public bool BeginClient(string ip, int port) {
        if (string.IsNullOrWhiteSpace(ip)) {
            Debug.LogError("[Network] 接続先IPが空です。", this);
            return false;
        }

        if (port < 1 || port > 65535) {
            Debug.LogError(
                $"[Network] 接続先ポートが不正です: {port}",
                this);
            return false;
        }

        if (!PrepareNetwork())
            return false;

        stopping = false;

        // PrepareNetworkでKcpTransportであることを確認済み
        KcpTransport kcp = (KcpTransport)transport;

        // 選んだ部屋のゲーム接続用ポートを反映する
        kcp.Port = (ushort)port;
        networkAddress = ip.Trim();

        try {
            Debug.Log(
                $"[Network] 接続開始: {networkAddress}:{port}",
                this);

            StartClient();

            return NetworkClient.active;
        }
        catch (Exception exception) {
            Debug.LogException(exception, this);
            LeaveRoom();
            return false;
        }
    }

    /// <summary>
    /// 通信開始前の共通確認と状態初期化。
    /// </summary>
    private bool PrepareNetwork() {
        if (NetworkServer.active || NetworkClient.active) {
            Debug.LogWarning(
                "[Network] すでに通信中です。先に退出してください。",
                this);
            return false;
        }

        if (!(transport is KcpTransport)) {
            Debug.LogError(
                "[Network] TransportにKcpTransportを設定してください。",
                this);
            return false;
        }

        if (title == null) {
            Debug.LogError(
                "[Network] Titleの参照が未設定です。",
                this);
            return false;
        }

        // 今回はシーン移動を行わない
        onlineScene = "";
        offlineScene = "";

        // プレイヤーPrefabを使用せず、メッセージで通信する
        autoCreatePlayer = false;

        // ホスト自身を含めて2人
        maxConnections = 2;

        readyPlayers.Clear();
        matchStarted = false;

        ResetLocalMatchState();

        return true;
    }

    // ============================================================
    // サーバー・クライアント起動
    // ============================================================

    /// <summary>
    /// サーバー起動時。
    /// ホストでは、ローカルクライアントの接続前に呼ばれる。
    /// </summary>
    public override void OnStartServer() {
        base.OnStartServer();

        stopping = false;
        readyPlayers.Clear();
        matchStarted = false;

        // クライアントから送られてくる整数の受信処理を登録
        NetworkServer.RegisterHandler<SendValueRequest>(
            OnServerValueReceived);

        // ホスト起動後に部屋情報を配信する
        if (broadcaster != null && transport is KcpTransport kcp) {
            broadcaster.SetGamePlaying(false);

            bool started = broadcaster.StartSendIP(kcp.Port);

            if (!started) {
                Debug.LogError(
                    "[Network] サーバーは起動しましたが、" +
                    "ホスト情報の配信に失敗しました。",
                    this);
            }
        }

        Debug.Log("[Network] サーバー起動完了。", this);
    }

    /// <summary>
    /// クライアント開始時。
    /// ホスト自身のローカルクライアントでも呼ばれる。
    /// </summary>
    public override void OnStartClient() {
        base.OnStartClient();

        ResetLocalMatchState();

        // 対戦開始・待機状態の受信処理
        NetworkClient.RegisterHandler<CardRoomStateMessage>(
            OnRoomStateReceived);

        // 相手から転送された整数の受信処理
        NetworkClient.RegisterHandler<OpponentValueMessage>(
            OnOpponentValueMessage);
    }

    /// <summary>
    /// このPCのサーバー接続が完了したとき。
    /// </summary>
    public override void OnClientConnect() {
        base.OnClientConnect();

        // Mirrorの基本処理ですでにReadyになっていれば重複送信しない
        if (!NetworkClient.ready)
            NetworkClient.Ready();

        Debug.Log("[Network] サーバーへの接続完了。", this);

        if (title != null)
            title.NotifyConnected();
    }

    // ============================================================
    // 参加者管理・対戦開始
    // ============================================================

    /// <summary>
    /// 新しい接続がサーバーへ参加したとき。
    /// 定員2人の制限はNetworkManager側にも設定している。
    /// </summary>
    public override void OnServerConnect(
        NetworkConnectionToClient conn) {
        if (stopping || matchStarted) {
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);

        Debug.Log(
            $"[Network] 接続受付: ID={conn.connectionId}",
            this);
    }

    /// <summary>
    /// プレイヤーの接続準備が完了したとき。
    /// 手動の「準備完了」ボタンではなく、通信上のReadyを指す。
    /// </summary>
    public override void OnServerReady(
        NetworkConnectionToClient conn) {
        base.OnServerReady(conn);

        if (stopping)
            return;

        // 同じ接続を二重に数えない
        bool newlyReady = readyPlayers.Add(conn.connectionId);

        if (!newlyReady)
            return;

        Debug.Log(
            $"[Network] 準備完了: {readyPlayers.Count} / 2",
            this);

        if (!matchStarted && readyPlayers.Count == 2) {
            matchStarted = true;

            // 検索一覧に「対戦中」と通知する
            if (broadcaster != null)
                broadcaster.SetGamePlaying(true);

            // ホスト自身を含む両クライアントへ通知
            SendRoomState(true, "対戦開始");
        }
        else if (!matchStarted) {
            SendRoomState(
                false,
                $"対戦相手を待っています... {readyPlayers.Count} / 2");
        }
    }

    /// <summary>
    /// 部屋の状態を接続中のクライアントへ送る。
    /// </summary>
    private void SendRoomState(bool started, string message) {
        if (!NetworkServer.active)
            return;

        NetworkServer.SendToAll(new CardRoomStateMessage {
            started = started,
            text = message
        });
    }

    /// <summary>
    /// サーバーから届いた部屋の状態を、このPCへ反映する。
    /// </summary>
    private void OnRoomStateReceived(CardRoomStateMessage message) {
        bool wasStarted = IsMatchStarted;
        IsMatchStarted = message.started;

        // 待機に戻ったとき、または新しい対戦が始まったときに初期化
        if (!message.started || !wasStarted)
            ResetReceivedValue();

        if (title != null) {
            title.NotifyRoomState(
                message.started,
                message.text);
        }
    }

    // ============================================================
    // 相手への整数送信
    // ============================================================

    /// <summary>
    /// 対戦相手へ整数を送信する。
    /// ホスト・クライアントのどちらからでも使用できる。
    ///
    /// 例:
    /// networkManager.SendValueToOpponent(500);
    ///
    /// trueは送信要求を出したことを示す。
    /// 相手への到達確認を返すものではない。
    /// </summary>
    public bool SendValueToOpponent(int value) {
        if (!NetworkClient.isConnected || !NetworkClient.ready) {
            Debug.LogWarning(
                "[数値送信] まだ接続準備が完了していません。",
                this);
            return false;
        }

        if (!IsMatchStarted) {
            Debug.LogWarning(
                "[数値送信] 対戦開始後に送信してください。",
                this);
            return false;
        }

        // ホストもローカルクライアントから同じ経路で送信する
        NetworkClient.Send(new SendValueRequest {
            value = value
        });

        Debug.Log($"[数値送信] 送信要求: {value}", this);

        return true;
    }

    /// <summary>
    /// サーバー側で整数の送信要求を受け取る。
    /// 有効な参加者からの要求だけを相手へ転送する。
    /// </summary>
    private void OnServerValueReceived(
        NetworkConnectionToClient sender,
        SendValueRequest message) {
        if (stopping ||
            !matchStarted ||
            !sender.isAuthenticated ||
            !sender.isReady ||
            !readyPlayers.Contains(sender.connectionId)) {
            Debug.LogWarning(
                "[数値送信] 現在は送信できないため要求を無視しました。",
                this);
            return;
        }

        foreach (NetworkConnectionToClient target
                 in NetworkServer.connections.Values) {
            // 送信した本人へは返さない
            if (target.connectionId == sender.connectionId)
                continue;

            if (!target.isAuthenticated ||
                !target.isReady ||
                !readyPlayers.Contains(target.connectionId)) {
                continue;
            }

            target.Send(new OpponentValueMessage {
                value = message.value
            });
        }
    }

    /// <summary>
    /// 相手の整数を、このPCで受信する。
    /// </summary>
    private void OnOpponentValueMessage(OpponentValueMessage message) {
        // 対戦終了後に受け取ったデータは反映しない
        if (!IsMatchStarted)
            return;

        receivedValue = message.value;
        HasReceivedValue = true;

        Debug.Log(
            $"[数値受信] 相手から受信: {receivedValue}",
            this);

        // 表示用スクリプトなどへ通知
        OnOpponentValueReceived?.Invoke(receivedValue);
    }

    /// <summary>
    /// InspectorのButton.OnClickに登録するテスト関数。
    /// InspectorのTest Send Valueに設定した値を送る。
    /// </summary>
    public void SendTestValue() {
        SendValueToOpponent(testSendValue);
    }

    // ============================================================
    // 退出・切断
    // ============================================================

    /// <summary>
    /// 自分から部屋を退出する。
    ///
    /// ホスト: 部屋全体を終了
    /// クライアント: 自分だけ退出
    ///
    /// UIの退出ボタンからは、TitleManagerの
    /// OnLeaveRoomButtonを呼ぶこと。
    /// </summary>
    public void LeaveRoom() {
        stopping = true;
        ResetLocalMatchState();

        if (broadcaster != null)
            broadcaster.StopBroadcast();

        if (NetworkServer.active && NetworkClient.active) {
            StopHost();
        }
        else if (NetworkClient.active) {
            StopClient();
        }
        else if (NetworkServer.active) {
            StopServer();
        }
    }

    /// <summary>
    /// サーバー側で参加者の切断を処理する。
    /// クライアントが退出した場合、ホストは部屋に残る。
    /// </summary>
    public override void OnServerDisconnect(
        NetworkConnectionToClient conn) {
        bool wasReady = readyPlayers.Remove(conn.connectionId);

        base.OnServerDisconnect(conn);

        Debug.Log(
            $"[Network] 退出: ID={conn.connectionId}",
            this);

        // 部屋終了中、または準備前の接続が切れた場合は通知不要
        if (stopping || !wasReady)
            return;

        matchStarted = false;

        if (broadcaster != null)
            broadcaster.SetGamePlaying(false);

        SendRoomState(
            false,
            "相手が退出しました。対戦相手を待っています...");
    }

    /// <summary>
    /// このPCのクライアント停止時。
    /// 切断に伴うMirrorの基本処理は基底クラスへ任せる。
    /// </summary>
    public override void OnStopClient() {
        ResetLocalMatchState();

        // 独自メッセージの受信登録を解除
        NetworkClient.UnregisterHandler<CardRoomStateMessage>();
        NetworkClient.UnregisterHandler<OpponentValueMessage>();

        base.OnStopClient();

        if (title != null)
            title.NotifyDisconnected();
    }

    /// <summary>
    /// サーバー停止時。
    /// </summary>
    public override void OnStopServer() {
        stopping = true;
        readyPlayers.Clear();
        matchStarted = false;

        ResetLocalMatchState();

        if (broadcaster != null)
            broadcaster.StopBroadcast();

        NetworkServer.UnregisterHandler<SendValueRequest>();

        base.OnStopServer();
    }

    /// <summary>
    /// このPCの対戦・受信状態を初期化する。
    /// </summary>
    private void ResetLocalMatchState() {
        IsMatchStarted = false;
        ResetReceivedValue();
    }

    private void ResetReceivedValue() {
        receivedValue = 0;
        HasReceivedValue = false;
    }
}