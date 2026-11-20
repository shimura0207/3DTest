using Mirror;
using TMPro;
using UnityEngine;

/// <summary>
/// 自分・相手のテーブルを別々の TMP_Text に表示する。
/// 自分 = SlotReelController.Instance.currentTable
/// 相手 = 相手の Mirror Player に付いた TableNetworkSync.SyncedTable
/// 【アタッチ先】Canvas 内の UI 管理用 GameObject
/// </summary>
public class TableUI : MonoBehaviour {
    [Header("表示する TextMeshPro オブジェクト")]
    [SerializeField] private TMP_Text selfTable;
    [SerializeField] private TMP_Text opponentTable;

    [Header("表示設定")]
    [Min(1)]
    [SerializeField] private int maxTable = 15;

    [SerializeField] private string waitingValue = "--";

    private void Start() {
        UpdateText(selfTable, FormatTable(-1));
        UpdateText(opponentTable, FormatTable(-1));
    }

    private void Update() {
        // 自分の値はゲーム内の管理元から直接読む。
        SlotReelController slot = SlotReelController.Instance;
        int myTable = slot != null ? slot.currentTable : -1;
        UpdateText(selfTable, FormatTable(myTable));

        // 相手の値はネットワークで同期された相手 Player から読む。
        int remoteTable = FindOpponentTable();
        UpdateText(opponentTable, FormatTable(remoteTable));
    }

    private int FindOpponentTable() {
        // 未接続・切断時は古い値を保持せず未受信表示に戻す。
        if (!NetworkClient.isConnected)
            return -1;

        foreach (NetworkIdentity identity in NetworkClient.spawned.Values) {
            if (identity == null || identity.isLocalPlayer)
                continue;

            // Player Prefab にのみ TableNetworkSync を付けること。
            if (!identity.TryGetComponent(out TableNetworkSync tableSync))
                continue;

            if (tableSync.HasSyncedTable)
                return tableSync.SyncedTable;
        }

        return -1;
    }

    private string FormatTable(int value) {
        string valueText = value >= 0 ? value.ToString() : waitingValue;
        return $"Table:{valueText}/{maxTable}";
    }

    private static void UpdateText(TMP_Text target, string text) {
        if (target != null && target.text != text)
            target.text = text;
    }
}