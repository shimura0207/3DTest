using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パチスロ専用の通信窓口。
///
/// ・既存の文字列送受信を使用します。
/// ・受信内容はキューに保存します。
/// ・相手フェイズに入る前に届いた通知も保持します。
///
/// CustomNetworkManagerと同じ、常時有効なGameObjectへ
/// アタッチしてください。
/// </summary>
public class PachisuroNetworkSync : MonoBehaviour {
    public static PachisuroNetworkSync Instance { get; private set; }

    // 他のゲーム用メッセージと区別する識別文字列。
    private const string Prefix = "RG_SLOT_V1";

    private const int MaxQueuedPackets = 256;

    private CustomNetworkManager network;

    private readonly Queue<Packet> packets = new Queue<Packet>();

    // 再生済みフェイズの遅延・重複通知を無視するために保持。
    private readonly HashSet<string> completedPhases =
        new HashSet<string>();

    // 自分が送信しているフェイズの識別子。
    private string sendingPhase;

    // 相手側で現在再生しているフェイズの識別子。
    private string receivingPhase;

    private int receivingSpin;

    public bool RemoteFinished { get; private set; }
    public bool HasError { get; private set; }

    public bool Connected =>
        network != null && network.IsMatchStarted;

    private enum PacketKind {
        Start,
        Stop,
        End
    }

    /// <summary>
    /// 分解・検証済みの受信内容。
    /// </summary>
    private class Packet {
        public PacketKind kind;
        public string phase;
        public int spin;

        public PachisuroSymbolKoyakuEnum role;
        public bool rush;
        public bool nextRush;
        public int table;

        public int reel;
        public float angle;
        public float speed;
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnEnable() {
        BindNetwork();
    }

    private void Update() {
        BindNetwork();

        // 切断・マッチ終了後は前回の通知を残さない。
        if (!Connected) {
            ResetSession();
        }
    }

    private void OnDisable() {
        if (network != null) {
            network.OnOpponentStringReceived -= OnReceived;
        }

        network = null;
        ResetSession();
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    /// <summary>
    /// NetworkManagerの生成順が前後しても購読できるようにします。
    /// </summary>
    private void BindNetwork() {
        CustomNetworkManager current = CustomNetworkManager.instance;

        if (network == current)
            return;

        if (network != null) {
            network.OnOpponentStringReceived -= OnReceived;
        }

        network = current;
        ResetSession();

        if (network != null) {
            network.OnOpponentStringReceived += OnReceived;
        }
    }

    private void ResetSession() {
        packets.Clear();
        completedPhases.Clear();

        sendingPhase = null;
        receivingPhase = null;
        receivingSpin = 0;

        RemoteFinished = false;
        HasError = false;
    }

    // ========================================================
    // 自分の操作を送信
    // ========================================================

    /// <summary>
    /// 自分のパチスロフェイズ開始時に1回呼びます。
    /// 前のフェイズと区別するため、新しいIDを作ります。
    /// </summary>
    public void BeginLocalPhase() {
        sendingPhase = Guid.NewGuid().ToString("N");
    }

    public bool SendStart(
        int spin,
        PachisuroSymbolKoyakuEnum role,
        bool rush,
        bool nextRush,
        int table) {
        return Send(NetworkValueConverter.Pack(
            Prefix,
            "START",
            sendingPhase,
            spin,
            (int)role,
            rush,
            nextRush,
            table
        ));
    }

    public bool SendStop(
        int spin,
        int reel,
        float angle,
        float speed) {
        return Send(NetworkValueConverter.Pack(
            Prefix,
            "STOP",
            sendingPhase,
            spin,
            reel,
            angle,
            speed
        ));
    }

    public bool SendEnd(int spin) {
        return Send(NetworkValueConverter.Pack(
            Prefix,
            "END",
            sendingPhase,
            spin
        ));
    }

    private bool Send(string message) {
        if (!Connected || HasError || string.IsNullOrEmpty(sendingPhase))
            return false;

        if (!network.SendStringToOpponent(message)) {
            Fail("パチスロ情報を送信できませんでした。");
            return false;
        }

        return true;
    }

    // ========================================================
    // 受信・保存
    // ========================================================

    private void OnReceived(string message) {
        if (!Connected || HasError)
            return;

        if (!NetworkValueConverter.TryUnpack(message, out string[] v))
            return;

        if (v.Length == 0 || v[0] != Prefix)
            return;

        if (v.Length < 4 ||
            string.IsNullOrEmpty(v[2]) ||
            !NetworkValueConverter.TryGetInt(v[3], out int spin) ||
            spin < 1) {
            Fail("パチスロ情報の形式が不正です。");
            return;
        }

        if (completedPhases.Contains(v[2]))
            return;

        Packet packet = new Packet {
            phase = v[2],
            spin = spin
        };

        switch (v[1]) {
            case "START":
                if (v.Length != 8 ||
                    !NetworkValueConverter.TryGetInt(v[4], out int role) ||
                    !Enum.IsDefined(typeof(PachisuroSymbolKoyakuEnum), role) ||
                    role == (int)PachisuroSymbolKoyakuEnum.NONE ||
                    !NetworkValueConverter.TryGetBool(v[5], out packet.rush) ||
                    !NetworkValueConverter.TryGetBool(v[6], out packet.nextRush) ||
                    !NetworkValueConverter.TryGetInt(v[7], out packet.table)) {
                    Fail("回転開始情報が不正です。");
                    return;
                }

                packet.kind = PacketKind.Start;
                packet.role = (PachisuroSymbolKoyakuEnum)role;
                break;

            case "STOP":
                if (v.Length != 7 ||
                    !NetworkValueConverter.TryGetInt(v[4], out packet.reel) ||
                    packet.reel < 0 || packet.reel > 2 ||
                    !NetworkValueConverter.TryGetFloat(v[5], out packet.angle) ||
                    !NetworkValueConverter.TryGetFloat(v[6], out packet.speed)) {
                    Fail("リール停止情報が不正です。");
                    return;
                }

                packet.kind = PacketKind.Stop;
                packet.angle = AngleNormalizer.NormalizeAngle(packet.angle);
                break;

            case "END":
                if (v.Length != 4) {
                    Fail("フェイズ終了情報が不正です。");
                    return;
                }

                packet.kind = PacketKind.End;
                break;

            default:
                Fail("不明なパチスロ処理IDです。");
                return;
        }

        if (packets.Count >= MaxQueuedPackets) {
            Fail("パチスロ受信情報が溜まりすぎています。");
            return;
        }

        packets.Enqueue(packet);
    }

    // ========================================================
    // 相手フェイズ中の再生
    // ========================================================

    /// <summary>
    /// 相手フェイズ開始時に呼びます。
    /// 先に届いた通知を残すため、キューは消しません。
    /// </summary>
    public void BeginRemotePhase() {
        receivingPhase = null;
        receivingSpin = 0;
        RemoteFinished = false;
    }

    /// <summary>
    /// 相手フェイズ中に毎フレーム呼びます。
    /// 1回につき最大1通知を処理します。
    /// </summary>
    public void PumpRemote(SlotReelController slot) {
        if (!Connected || HasError || RemoteFinished ||
            slot == null || !slot.NetworkReady || packets.Count == 0) {
            return;
        }

        Packet packet = packets.Peek();

        if (completedPhases.Contains(packet.phase)) {
            packets.Dequeue();
            return;
        }

        if (receivingPhase == null) {
            // 相手フェイズの最初は必ず1G目の開始通知。
            if (packet.kind != PacketKind.Start || packet.spin != 1) {
                Fail("相手フェイズの開始通知が不足しています。");
                return;
            }

            receivingPhase = packet.phase;
        }

        if (packet.phase != receivingPhase) {
            Fail("前のパチスロフェイズが終了していません。");
            return;
        }

        switch (packet.kind) {
            case PacketKind.Start:
                // 同じ開始通知の二重適用を防止。
                if (packet.spin <= receivingSpin) {
                    packets.Dequeue();
                    return;
                }

                if (packet.spin != receivingSpin + 1) {
                    Fail("回転番号が連続していません。");
                    return;
                }

                // 前の回転が停止するまで次の回転を開始しない。
                if (slot.IsAnyReelRotating())
                    return;

                if (!slot.StartRemoteSpin(
                    packet.spin,
                    packet.role,
                    packet.rush,
                    packet.nextRush,
                    packet.table)) {
                    Fail("相手リールを回転開始できませんでした。");
                    return;
                }

                receivingSpin = packet.spin;
                packets.Dequeue();
                break;

            case PacketKind.Stop:
                if (packet.spin < receivingSpin) {
                    packets.Dequeue();
                    return;
                }

                if (packet.spin != receivingSpin) {
                    Fail("回転開始前の停止通知を受信しました。");
                    return;
                }

                // ロック演出中は、演出終了まで停止通知を保持。
                if (slot.NetworkReelLockRunning)
                    return;

                if (!slot.ReserveRemoteStop(
                    packet.reel,
                    packet.angle,
                    packet.speed)) {
                    Fail("相手リールの停止予約に失敗しました。");
                    return;
                }

                packets.Dequeue();
                break;

            case PacketKind.End:
                if (packet.spin != receivingSpin) {
                    Fail("終了通知の回転番号が一致しません。");
                    return;
                }

                // 通知が届いても、見た目の停止完了を待つ。
                if (slot.IsAnyReelRotating() ||
                    slot.NetworkReelLockRunning) {
                    return;
                }

                packets.Dequeue();
                completedPhases.Add(receivingPhase);
                RemoteFinished = true;
                break;
        }
    }

    private void Fail(string reason) {
        HasError = true;
        Debug.LogError("[パチスロ通信] " + reason);
    }
}