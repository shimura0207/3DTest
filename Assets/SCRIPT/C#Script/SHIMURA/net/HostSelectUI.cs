using System.Collections.Generic;
using UnityEngine;

public class HostSelectUI : MonoBehaviour {
    public UDPListener listener;

    [Header("HostButton付きのPrefab")]
    [SerializeField] private HostButton hostButtonPrefab;

    [Header("ボタンを配置する親")]
    [SerializeField] private Transform UIRoot;

    public bool isSelected { get; private set; }
    public UDPListener.UdpMessage selectedHost { get; private set; }

    private bool showing;
    private UDPListener subscribedListener;

    private readonly Dictionary<string, HostButton> buttons =
        new Dictionary<string, HostButton>();

    private void OnEnable() {
        Subscribe();
    }

    private void OnDisable() {
        Unsubscribe();
    }

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

    public void ShowHostList(List<UDPListener.UdpMessage> hosts) {
        ResetPanel();

        if (hostButtonPrefab == null || UIRoot == null || listener == null) {
            Debug.LogError("HostSelectUIの参照が未設定です。", this);
            return;
        }

        Subscribe();
        showing = true;

        if (hosts == null)
            return;

        foreach (var host in hosts)
            UpdateHost(host);
    }

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
    }

    public void UpdateHost(UDPListener.UdpMessage host) {
        if (!showing || isSelected)
            return;

        string key = UDPListener.GetKey(host);

        if (!buttons.TryGetValue(key, out HostButton button) ||
            button == null) {
            button = Instantiate(hostButtonPrefab, UIRoot);
            buttons[key] = button;
        }

        button.Setup(host, OnHostButtonClicked);
        button.gameObject.SetActive(true);
    }

    private void RemoveHost(UDPListener.UdpMessage host) {
        string key = UDPListener.GetKey(host);

        if (!buttons.TryGetValue(key, out HostButton button))
            return;

        buttons.Remove(key);

        if (button != null) {
            button.gameObject.SetActive(false);
            Destroy(button.gameObject);
        }
    }

    public void OnHostButtonClicked(UDPListener.UdpMessage host) {
        if (!showing || isSelected || listener == null)
            return;

        // 表示時ではなく、受信済みの最新情報を参照する
        int index = listener.discoveredHosts.FindIndex(
            item => item.ip == host.ip && item.port == host.port);

        if (index < 0)
            return;

        host = listener.discoveredHosts[index];

        if (host.gamePlaying)
            return;

        selectedHost = host;
        isSelected = true;
    }

    private void OnDestroy() {
        Unsubscribe();
    }
}