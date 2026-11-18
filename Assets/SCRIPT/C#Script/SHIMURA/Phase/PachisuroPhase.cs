using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 実行時生成するパチスロフェイズ。Inspector設定は不要。
/// シーンのPachisuroPhaseViewが背景・役UIの参照を管理する。
/// </summary>
public class PachisuroPhase : PhaseBase {
    public static PachisuroPhase Instance { get; private set; }

    private readonly Vector3 CAMERA_POSITION = new Vector3(0f, 1.16f, -1.27f);
    private readonly Vector3 CAMERA_EULER_ANGLES = new Vector3(3f, 0f, 0f);
    private const int FinishWaitMilliseconds = 3000;

    private SlotReelController slot;
    private PachisuroPhaseView phaseView;
    private Canvas handCanvas;
    private bool originalHandCanvasEnabled;
    private bool entered;
    private bool finished;
    private bool connectionWarningShown;
    private bool endingTaskStarted;
    private bool playbackEnded;
    private CancellationTokenSource phaseCancellation;

    public override void Setup() {
        CancelPendingFinish();
        base.Setup();
        phaseCancellation = new CancellationTokenSource();
        Instance = this;
        nextPhase = false;
        entered = false;
        finished = false;
        connectionWarningShown = false;
        endingTaskStarted = false;
        playbackEnded = false;

        // フェイズ自体にInspector参照を持たせず、シーン上の管理役を取得する。
        phaseView = PachisuroPhaseView.Instance;
        if (phaseView == null)
            phaseView = UnityEngine.Object.FindObjectOfType<PachisuroPhaseView>();
        if (phaseView != null) phaseView.Show();
        else Debug.LogWarning("[パチスロ] シーンにPachisuroPhaseViewを配置してください。背景・役UIを表示管理できません。");

        slot = SlotReelController.Instance;
        if (slot != null && slot.SOUND != null) slot.SOUND.Play();

        Camera camera = Camera.main;
        if (camera != null) {
            camera.transform.position = CAMERA_POSITION;
            camera.transform.eulerAngles = CAMERA_EULER_ANGLES;
        }

        handCanvas = null;
        GameObject hand = GameObject.Find("HANDCanvas");
        if (hand != null) {
            handCanvas = hand.GetComponent<Canvas>();
            if (handCanvas != null) {
                originalHandCanvasEnabled = handCanvas.enabled;
                handCanvas.enabled = false;
            }
        }

        // 手札を非表示に
        CardObjectManager.instance.HandHidden();
    }

    public override UniTask SelfExecute() {

        
        if (finished || !CanExecute()) return UniTask.CompletedTask;
        if (!entered) {
            entered = true;
            if (!slot.IsTutorialMode) PachisuroNetworkSync.Instance.BeginLocalPhase();
            slot.BeginLocalPlayback();
        }
        slot.SlotUpdate();

        if (slot.slotIndex >= slot.SLOT_TEARN_MAX_G &&
            !slot.IsAnyReelRotating() && !slot.NetworkReelLockRunning) {
            if (slot.IsTutorialMode || PachisuroNetworkSync.Instance.SendEnd(slot.slotIndex)) {
                finished = true;
                slot.DebugAAA();
                // 未使用のUniTask変数に入れず、終了処理を実行する。
                ExecuteAsync().Forget();
            }
        }
        return UniTask.CompletedTask;
    }

    public override UniTask OpponentExecute() {
        if (finished || !CanExecute()) return UniTask.CompletedTask;
        if (!entered) {
            entered = true;
            slot.BeginRemotePlayback();
            PachisuroNetworkSync.Instance.BeginRemotePhase();
        }
        slot.EnemySlotUpdate();

        if (PachisuroNetworkSync.Instance.RemoteFinished &&
            !slot.IsAnyReelRotating() && !slot.NetworkReelLockRunning) {
            finished = true;
            ExecuteAsync().Forget();
        }
        return UniTask.CompletedTask;
    }

    private bool CanExecute() {
        if (slot == null) slot = SlotReelController.Instance;
        if (slot == null || !slot.NetworkReady) return false;
        if (slot.IsTutorialMode) return true;
        PachisuroNetworkSync sync = PachisuroNetworkSync.Instance;
        if (sync == null || !sync.Connected || sync.HasError) {
            if (!connectionWarningShown) {
                connectionWarningShown = true;
                Debug.LogWarning("[パチスロ] 通信が利用できないため進行を停止しています。");
            }
            return false;
        }
        return true;
    }

    public async UniTask ExecuteAsync() {
        if (endingTaskStarted || phaseCancellation == null) return;
        endingTaskStarted = true;
        CancellationToken token = phaseCancellation.Token;
        try {
            // この3秒間は背景・カウント・オーブを表示したままにする。
            await UniTask.Delay(FinishWaitMilliseconds, cancellationToken: token);
            token.ThrowIfCancellationRequested();

            if (slot != null) {
                if (slot.efectManager != null) slot.efectManager.HideTrumps();
                EndPlaybackOnce();
                slot.HideStopEffectImage();
            }
            if (phaseView != null) phaseView.Hide();
            nextPhase = true;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) {
            // Teardown後に古い待機処理から新フェイズのUIを消さない。
        }
    }

    private void EndPlaybackOnce() {
        if (playbackEnded || !entered || slot == null) return;
        playbackEnded = true;
        slot.EndNetworkPlayback();
    }

    private void CancelPendingFinish() {
        if (phaseCancellation == null) return;
        phaseCancellation.Cancel();
        phaseCancellation.Dispose();
        phaseCancellation = null;
    }

    public override void Teardown() {
        CancelPendingFinish();
        EndPlaybackOnce();
        // 通常終了だけでなく、途中でフェイズを抜けた場合も必ず非表示。
        if (phaseView != null) phaseView.Hide();
        if (handCanvas != null) handCanvas.enabled = originalHandCanvasEnabled;
        entered = false;
        finished = false;
        if (Instance == this) Instance = null;
        base.Teardown();
    }
}

