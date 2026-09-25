using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 左Ctrlを押したら15G分だけ自動で回すスクリプト。
/// 15G終了後、Consoleに引いた役のリザルトを表示します。
/// 
/// 使い方:
/// SlotReelController と同じ GameObject に追加してください。
/// </summary>
public class AutoSpin15GamesController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SlotReelController slotReelController;

    [Header("Auto Spin Settings")]
    [SerializeField] private int autoGameCount = 15;

    [SerializeField] private float waitBeforeFirstStop = 0.7f;
    [SerializeField] private float waitBetweenStops = 0.3f;
    [SerializeField] private float waitBetweenGames = 0.5f;

    [SerializeField] private KeyCode startAutoSpinKey = KeyCode.LeftControl;

    /// <summary>
    /// 現在、自動回転中かどうか。
    /// 連打で二重起動しないようにするためのフラグ。
    /// </summary>
    private bool isAutoSpinning = false;

    /// <summary>
    /// 15G中に引いた役の回数を保存する辞書。
    /// 例:
    /// Bell → 5回
    /// Replay → 3回
    /// Miss → 7回
    /// </summary>
    private Dictionary<PachisuroSymbolKoyakuEnum, int> roleResultCounts =
        new Dictionary<PachisuroSymbolKoyakuEnum, int>();

    private void Awake()
    {
        if (slotReelController == null)
        {
            slotReelController = GetComponent<SlotReelController>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(startAutoSpinKey))
        {
            StartAutoSpin();
        }
    }

    /// <summary>
    /// 自動回転を開始します。
    /// </summary>
    public void StartAutoSpin()
    {
        if (isAutoSpinning)
        {
            Debug.Log("すでに自動回転中です。");
            return;
        }

        if (slotReelController == null)
        {
            Debug.LogWarning("SlotReelController が設定されていません。");
            return;
        }

        StartCoroutine(AutoSpinRoutine());
    }

    /// <summary>
    /// 15G自動で回す本体処理。
    /// 
    /// 流れ:
    /// 1. リザルト初期化
    /// 2. 1G開始
    /// 3. 引いた役を記録
    /// 4. 左・中・右リールを順番に停止
    /// 5. 15G終わったらConsoleに結果表示
    /// </summary>
    private IEnumerator AutoSpinRoutine()
    {
        isAutoSpinning = true;

        // 前回のリザルトが残らないように初期化
        roleResultCounts.Clear();

        Debug.Log($"自動回転開始: {autoGameCount}G");

        for (int game = 1; game <= autoGameCount; game++)
        {
            // 全リールが止まるまで待つ
            yield return new WaitUntil(() => slotReelController.CanStartSpin());

            Debug.Log($"自動回転 {game}G目 開始");

            // 1G開始
            bool started = slotReelController.TryStartSpin();

            if (!started)
            {
                Debug.LogWarning($"{game}G目を開始できませんでした。");
                continue;
            }

            // 今ゲームで引いた役を取得
            PachisuroSymbolKoyakuEnum drawnRole = slotReelController.GetCurrentSlotSymbolRole();

            // 引いた役を集計
            AddRoleResult(drawnRole);

            Debug.Log($"{game}G目の抽選結果: {slotReelController.GetSlotSymbolRoleName(drawnRole)}");

            // 少し待ってから停止開始
            yield return new WaitForSeconds(waitBeforeFirstStop);

            int reelCount = slotReelController.GetReelCount();

            // 左→中→右の順番で停止予約
            for (int reelIndex = 0; reelIndex < reelCount; reelIndex++)
            {
                slotReelController.TryReserveStop(reelIndex);
                yield return new WaitForSeconds(waitBetweenStops);
            }

            // 全リールが止まるまで待つ
            yield return new WaitUntil(() => !slotReelController.IsAnyReelRotating());

            Debug.Log($"自動回転 {game}G目 終了");

            yield return new WaitForSeconds(waitBetweenGames);
        }

        // 15G分の結果をConsoleに表示
        ShowAutoSpinResult();

        Debug.Log("自動回転終了");

        isAutoSpinning = false;
    }

    /// <summary>
    /// 引いた役を1回分カウントします。
    /// </summary>
    private void AddRoleResult(PachisuroSymbolKoyakuEnum role)
    {
        if (!roleResultCounts.ContainsKey(role))
        {
            roleResultCounts.Add(role, 0);
        }

        roleResultCounts[role]++;
    }

    /// <summary>
    /// 15G終了後にConsoleへリザルトを表示します。
    /// </summary>
    private void ShowAutoSpinResult()
    {
        StringBuilder resultText = new StringBuilder();

        resultText.AppendLine("========== 15G自動回転リザルト ==========");

        int totalGame = 0;

        foreach (KeyValuePair<PachisuroSymbolKoyakuEnum, int> result in roleResultCounts)
        {
            totalGame += result.Value;
        }

        resultText.AppendLine($"総ゲーム数: {totalGame}G");
        resultText.AppendLine("");

        foreach (KeyValuePair<PachisuroSymbolKoyakuEnum, int> result in roleResultCounts)
        {
            string roleName = slotReelController.GetSlotSymbolRoleName(result.Key);
            int count = result.Value;

            resultText.AppendLine($"{roleName}: {count}回");
        }

        resultText.AppendLine("========================================");

        Debug.Log(resultText.ToString());
    }
}