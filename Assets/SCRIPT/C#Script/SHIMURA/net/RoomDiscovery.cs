using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// ROOM専用のLAN広告。既存のUDPListener / UDPBroadcasterは変更しない。
/// ランダム検索とはポートとメッセージ形式を分離する。
/// MatchingNetMatchが必要なときに自動追加するため、手動アタッチ不要。
/// </summary>
public sealed class RoomDiscovery : MonoBehaviour {
    [Serializable]
    public struct Advertisement {
        public string game;
        public int version;
        public string session;
        public string code;
        public int port;
        public bool full;
        [NonSerialized] public string ip;
    }
    private sealed class Entry {
        public Advertisement room;
        public float seen;
    }
    private readonly Dictionary<string, Entry> rooms = new Dictionary<string, Entry>();
    private readonly List<string> expired = new List<string>();
    private UdpClient receiver;
    private UdpClient sender;
    private IPEndPoint destination;
    private CustomNetworkManager manager;
    private string advertisedCode;
    private float nextSend;
    private const string Game = "RG_ROOM_LAN_v1";
    private const float Lifetime = 3f;
    public bool IsRunning => receiver != null;
    public string LastError { get; private set; } = "";

    // int変換しないので0000～9999の先頭ゼロも保持する。
    public static bool IsValidCode(string code) {
        if (code == null || code.Length != 4) return false;
        foreach (char c in code) if (c < '0' || c > '9') return false;
        return true;
    }

    public bool BeginSearch(int port) {
        StopDiscovery();
        LastError = "";
        if (port < 1 || port > 65535) { LastError = "ROOM検索ポートが不正です"; return false; }
        try {
            receiver = new UdpClient(AddressFamily.InterNetwork);
            receiver.Client.ExclusiveAddressUse = false;
            receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiver.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            receiver.Client.Blocking = false;
            destination = new IPEndPoint(IPAddress.Broadcast, port);
            return true;
        }
        catch (Exception e) { Fail("ROOM検索を開始できません: " + e.Message); return false; }
    }

    public bool Advertise(CustomNetworkManager owner, string code) {
        if (!IsRunning || owner == null || !IsValidCode(code)) return false;
        try {
            sender = new UdpClient(AddressFamily.InterNetwork);
            sender.EnableBroadcast = true;
            sender.Client.Blocking = false;
            manager = owner;
            advertisedCode = code;
            nextSend = 0f;
            return true;
        }
        catch (Exception e) { Fail("ROOM公開を開始できません: " + e.Message); return false; }
    }

    // 同番号が複数あれば、どちらかへ勝手に接続せず呼び出し側で中止する。
    public int FindRooms(string code, string excludedSession, out Advertisement selected) {
        selected = default;
        int count = 0;
        foreach (Entry entry in rooms.Values) {
            if (Time.unscaledTime - entry.seen > Lifetime || entry.room.code != code ||
                entry.room.session == excludedSession) continue;
            selected = entry.room;
            count++;
        }
        return count;
    }

    private void Update() {
        if (receiver == null) return;
        try {
            // 一度の受信量を制限し、フレームを占有させない。
            for (int i = 0; i < 32 && receiver.Available > 0; i++) {
                var endpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = receiver.Receive(ref endpoint);
                if (bytes.Length == 0 || bytes.Length > 2048) continue;
                Advertisement room;
                try { room = JsonUtility.FromJson<Advertisement>(Encoding.UTF8.GetString(bytes)); }
                catch (ArgumentException) { continue; }
                if (room.game != Game || room.version != 1 || !IsValidCode(room.code) ||
                    !Guid.TryParseExact(room.session, "N", out _) || room.port < 1 || room.port > 65535) continue;
                room.ip = endpoint.Address.ToString();
                // IPは広告の自己申告ではなく実際の送信元を使用する。
                rooms[room.session] = new Entry { room = room, seen = Time.unscaledTime };
            }
            expired.Clear();
            foreach (var pair in rooms)
                if (Time.unscaledTime - pair.Value.seen > Lifetime) expired.Add(pair.Key);
            foreach (string key in expired) rooms.Remove(key);

            if (sender != null && manager != null && Time.unscaledTime >= nextSend) {
                nextSend = Time.unscaledTime + 0.5f;
                var room = new Advertisement {
                    game = Game, version = 1, code = advertisedCode,
                    session = manager.RoomSessionId, port = manager.RoomGamePort,
                    full = manager.HasRemoteParticipant || manager.ServerMatchStarted
                };
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(room));
                sender.Send(bytes, bytes.Length, destination);
            }
        }
        catch (SocketException e) {
            if (e.SocketErrorCode != SocketError.WouldBlock &&
                e.SocketErrorCode != SocketError.NoBufferSpaceAvailable) Fail("ROOM通信エラー: " + e.Message);
        }
        catch (ObjectDisposedException) { StopDiscovery(); }
    }

    private void Fail(string message) {
        LastError = message;
        Debug.LogError(message, this);
        StopDiscovery();
    }
    public void StopDiscovery() {
        receiver?.Close(); receiver = null;
        sender?.Close(); sender = null;
        manager = null;
        rooms.Clear();
    }
    private void OnDisable() => StopDiscovery();
    private void OnDestroy() => StopDiscovery();
    private void OnApplicationQuit() => StopDiscovery();
}
