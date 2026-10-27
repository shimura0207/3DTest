using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// パチスロフェイズ。
///
/// 自分：入力・抽選・送信。
/// 相手：受信した役と停止角度で再生。
///
/// 先攻やゲーム全体のフェイズ同期は、このクラスでは行いません。
/// </summary>
public class PachisuroPhase : PhaseBase {
    public static PachisuroPhase Instance { get; private set; }

    private readonly Vector3 CAMERA_POSITION =
        new Vector3(0f, 1.16f, -1.27f);

    private readonly Vector3 CAMERA_EULER_ANGLES =
        new Vector3(3f, 0f, 0f);

    private SlotReelController slot;

    private Canvas handCanvas;
    private bool originalHandCanvasEnabled;

    private bool entered;
    private bool finished;
    private bool localTurn;
    private bool connectionWarningShown;

    public override void Setup() {
        base.Setup();

        Instance = this;

        nextPhase = false;
        entered = false;
        finished = false;
        connectionWarningShown = false;

        slot = SlotReelController.Instance;

        if (slot != null && slot.SOUND != null) {
            slot.SOUND.Play();
        }

        Camera camera = Camera.main;

        if (camera != null) {
            camera.transform.position = CAMERA_POSITION;
            camera.transform.eulerAngles = CAMERA_EULER_ANGLES;
        }

        // 既存の手札表示処理を引き継ぐ。
        // 毎フレームFindせず、Setup時に1回だけ取得する。
        handCanvas = null;

        GameObject hand = GameObject.Find("HANDCanvas");

        if (hand != null) {
            handCanvas = hand.GetComponent<Canvas>();

            if (handCanvas != null) {
                originalHandCanvasEnabled = handCanvas.enabled;
                handCanvas.enabled = false;
            }
        }
    }

    public override UniTask SelfExecute() {
        if (finished || !CanExecute())
            return UniTask.CompletedTask;

        if (!entered) {
            entered = true;
            localTurn = true;

            PachisuroNetworkSync.Instance.BeginLocalPhase();
            slot.BeginLocalPlayback();
        }

        slot.SlotUpdate();

        // 5Gを開始済み、かつ全リールの停止が完了した。
        if (slot.slotIndex >= slot.SLOT_TEARN_MAX_G &&
            !slot.IsAnyReelRotating() &&
            !slot.NetworkReelLockRunning) {
            // 相手へ終了を知らせてから自分の状態を更新する。
            if (PachisuroNetworkSync.Instance.SendEnd(slot.slotIndex)) {
                finished = true;

                // 自分のRUSH・テーブル更新は既存処理を使用する。
                slot.DebugAAA();
                slot.efectManager.HideTrumps();
                slot.EndNetworkPlayback();
                nextPhase = true;
            }
        }

        return UniTask.CompletedTask;
    }

    public override UniTask OpponentExecute() {
        if (finished || !CanExecute())
            return UniTask.CompletedTask;

        if (!entered) {
            entered = true;
            localTurn = false;

            slot.BeginRemotePlayback();
            PachisuroNetworkSync.Instance.BeginRemotePhase();
        }

        // 受信処理とリール更新を毎フレーム行う。
        slot.EnemySlotUpdate();

        // END受信＋リール停止完了で次へ進む。
        if (PachisuroNetworkSync.Instance.RemoteFinished) {
            finished = true;

            // 相手を表示するため一時的に上書きした状態を戻す。
            // 自分のテーブルを進めないためDebugAAAは呼ばない。
            slot.EndNetworkPlayback();
            
            nextPhase = true;
        }

        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 通信とリールの初期化状態を確認します。
    /// 切断・通信エラー時はフェイズを勝手に進めません。
    /// </summary>
    private bool CanExecute() {
        if (slot == null) {
            slot = SlotReelController.Instance;
        }

        PachisuroNetworkSync sync = PachisuroNetworkSync.Instance;

        if (sync == null || !sync.Connected || sync.HasError) {
            if (!connectionWarningShown) {
                connectionWarningShown = true;

                Debug.LogWarning(
                    "[パチスロ] 通信が利用できないため進行を停止しています。"
                );
            }

            return false;
        }

        return slot != null && slot.NetworkReady;
    }

    public override void Teardown() {
        if (entered && slot != null) {
            slot.EndNetworkPlayback();
        }

        if (handCanvas != null) {
            handCanvas.enabled = originalHandCanvasEnabled;
        }

        entered = false;
        finished = false;
        localTurn = false;

        if (Instance == this) {
            Instance = null;
        }

        base.Teardown();
    }
}