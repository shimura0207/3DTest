using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 発見したホストごとにボタンを生成する。
///
/// ・IP＋ポートで部屋を識別
/// ・新規発見時はボタンを追加
/// ・既存ホストの情報は更新
/// ・受信が途絶えたホストは一覧から削除
/// ・参加可能な部屋を優先して並べ替え
/// </summary>
public class HostSelectUI : MonoBehaviour {
    [Header("ホスト検索")]
    public UDPListener listener;

    [Header("ボタンPrefab")]
    [SerializeField]
    private HostButton hostButtonPrefab;

    [Header("一覧の配置先")]
    [Tooltip("Scroll ViewのContentなどを設定する")]
    [SerializeField]
    private Transform UIRoot;

    /// <summary>
    /// 参加先が選択されたか。
    /// TitleManagerが確認する。
    /// </summary>
    public bool isSelected { get; private set; }

    /// <summary>
    /// 選択したホストの情報。
    /// </summary>
    public UDPListener.UdpMessage selectedHost { get; private set; }

    // キーはIP＋ポート。同じ部屋を重複生成しない。
    private readonly Dictionary<string, HostButton> buttons =
        new Dictionary<string, HostButton>();

    private readonly List<HostButton> sortedButtons =
        new List<HostButton>();

    private UDPListener subscribedListener;
    private bool showing;

    private void OnEnable() {
        Subscribe();

        // 非表示中に受信情報が変わっていた場合も反映
        if (showing && !isSelected && listener != null)
            ShowHostList(listener.discoveredHosts);
    }

    private void OnDisable() {
        Unsubscribe();
    }

    /// <summary>
    /// 受信イベントを登録する。
    /// 二重登録を防ぐため、一度解除してから登録する。
    /// </summary>
    private void Subscribe() {
        Unsubscribe();

        if (listener == null)
            return;

        subscribedListener = listener;
        subscribedListener.onHostUpdated += UpdateHost;
        subscribedListener.onHostRemoved += RemoveHost;
    }

    private void Unsubscribe() {
        if (subscribedListener != null) {
            subscribedListener.onHostUpdated -= UpdateHost;
            subscribedListener.onHostRemoved -= RemoveHost;
        }

        subscribedListener = null;
    }

    /// <summary>
    /// 現在までに見つかったホストを一覧表示する。
    /// この後に見つかったホストはイベントで追加される。
    /// </summary>
    public void ShowHostList(List<UDPListener.UdpMessage> hosts) {
        ResetPanel();

        if (listener == null ||
            hostButtonPrefab == null ||
            UIRoot == null) {
            Debug.LogError(
                "[HostSelectUI] Listener・Prefab・UIRootを設定してください。",
                this);
            return;
        }

        Subscribe();
        showing = true;

        if (hosts != null) {
            foreach (UDPListener.UdpMessage host in hosts)
                CreateOrUpdateButton(host);
        }

        SortButtons();
    }

    /// <summary>
    /// 新しいホスト、または状態が変わったホストを反映する。
    /// </summary>
    public void UpdateHost(UDPListener.UdpMessage host) {
        if (!showing || isSelected)
            return;

        CreateOrUpdateButton(host);
        SortButtons();
    }

    private void CreateOrUpdateButton(UDPListener.UdpMessage host) {
        string key = UDPListener.GetKey(host);

        if (!buttons.TryGetValue(key, out HostButton button) ||
            button == null) {
            // 新しいホストなら新しいボタンを生成
            button = Instantiate(hostButtonPrefab, UIRoot, false);
            button.name = $"Host_{host.ip}_{host.port}";

            buttons[key] = button;
        }

        // 既存ホストなら同じボタンのデータと表示を更新
        button.Setup(host, OnHostButtonClicked);
        button.gameObject.SetActive(true);
    }

    /// <summary>
    /// 参加可能 → 部屋名 → IP → ポートの順に並べる。
    /// 実際の縦配置はContentのVertical Layout Groupが担当する。
    /// </summary>
    private void SortButtons() {
        sortedButtons.Clear();

        foreach (HostButton button in buttons.Values) {
            if (button != null)
                sortedButtons.Add(button);
        }

        sortedButtons.Sort(CompareHosts);

        for (int i = 0; i < sortedButtons.Count; i++) {
            sortedButtons[i].transform.SetSiblingIndex(i);
        }
    }

    private static int CompareHosts(HostButton left, HostButton right) {
        UDPListener.UdpMessage a = left.hostData;
        UDPListener.UdpMessage b = right.hostData;

        // false（参加可能）をtrue（対戦中）より先にする
        int result = a.gamePlaying.CompareTo(b.gamePlaying);

        if (result != 0)
            return result;

        result = string.Compare(
            a.hostName,
            b.hostName,
            System.StringComparison.OrdinalIgnoreCase);

        if (result != 0)
            return result;

        // 同名の部屋でも順序を安定させる
        result = string.Compare(
            a.ip,
            b.ip,
            System.StringComparison.Ordinal);

        if (result != 0)
            return result;

        return a.port.CompareTo(b.port);
    }

    /// <summary>
    /// UDPListenerが期限切れと判定したホストを削除する。
    /// </summary>
    private void RemoveHost(UDPListener.UdpMessage host) {
        string key = UDPListener.GetKey(host);

        if (!buttons.TryGetValue(key, out HostButton button))
            return;

        buttons.Remove(key);

        if (button != null) {
            // Destroyはフレーム末なので、先に非表示にする
            button.gameObject.SetActive(false);
            Destroy(button.gameObject);
        }

        SortButtons();
    }

    /// <summary>
    /// 押したボタンに対応するホストを参加先にする。
    /// </summary>
    public void OnHostButtonClicked(UDPListener.UdpMessage host) {
        if (!showing || isSelected || listener == null)
            return;

        // 古いボタン情報ではなく、受信済みの最新情報で確認
        int index = listener.discoveredHosts.FindIndex(
            item => item.ip == host.ip &&
                    item.port == host.port);

        if (index < 0) {
            Debug.LogWarning(
                "[HostSelectUI] このホストは一覧から消えています。",
                this);
            return;
        }

        UDPListener.UdpMessage latest =
            listener.discoveredHosts[index];

        if (latest.gamePlaying)
            return;

        selectedHost = latest;
        isSelected = true;

        Debug.Log(
            $"[HostSelectUI] 選択: {latest.hostName} " +
            $"({latest.ip}:{latest.port})",
            this);
    }

    /// <summary>
    /// 再検索・画面初期化用。
    /// 自分が生成したボタンだけを削除する。
    /// </summary>
    public void ResetPanel() {
        showing = false;
        isSelected = false;
        selectedHost = default;

        foreach (HostButton button in buttons.Values) {
            if (button == null)
                continue;

            button.gameObject.SetActive(false);
            Destroy(button.gameObject);
        }

        buttons.Clear();
        sortedButtons.Clear();
    }

    private void OnDestroy() {
        Unsubscribe();
    }
}