using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HostButton : MonoBehaviour {
    [Header("ボタン内のテキスト")]
    [SerializeField] private TMP_Text hostNameText;

    [Tooltip("未設定でもOK")]
    [SerializeField] private TMP_Text detailText;

    public UDPListener.UdpMessage hostData;

    private Button button;
    private Action<UDPListener.UdpMessage> onSelected;

    private void Awake() {
        EnsureButton();
    }

    private void EnsureButton() {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveListener(HandleClick);
        button.onClick.AddListener(HandleClick);
    }

    public void Setup(
        UDPListener.UdpMessage data,
        Action<UDPListener.UdpMessage> callback) {
        EnsureButton();

        hostData = data;
        onSelected = callback;

        RefreshUI();
    }

    public void RefreshUI() {
        if (button == null)
            EnsureButton();

        string status = hostData.gamePlaying ? "対戦中" : "参加可能";

        if (hostNameText != null) {
            hostNameText.text = detailText != null
                ? hostData.hostName
                : $"{hostData.hostName}  [{status}]";
        }

        if (detailText != null) {
            detailText.text =
                $"{hostData.ip}:{hostData.port}  /  {status}";
        }

        button.interactable = !hostData.gamePlaying;
    }

    private void HandleClick() {
        if (!hostData.gamePlaying)
            onSelected?.Invoke(hostData);
    }

    private void OnDestroy() {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        onSelected = null;
    }
}