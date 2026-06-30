using UnityEngine;

/// <summary>
/// リール実行中の状態をまとめて管理するクラス。
/// 
/// MonoBehaviour ではありません。
/// そのため GameObject に直接アタッチするものではなく、SlotReelController.cs から内部的に使います。
/// 
/// 【このクラスの役割】
/// ・各リールが回転中かどうか
/// ・各リールが停止予約中かどうか
/// ・停止予定角度
/// ・現在角度
/// ・リールの初期回転姿勢
/// をまとめて持ちます。
/// 
/// 【ここをいじるとどうなる？】
/// 基本的にゲームの仕様変更ではあまり触りません。
/// 「リールを3本から増やす」「内部状態の持ち方を変える」ような大きい変更のときに触ります。
/// </summary>
public class SlotReelState
{
    /// <summary>
    /// 各リールが回転中か。
    /// 0:左 / 1:中 / 2:右
    /// </summary>
    public bool[] RotationNow { get; private set; }

    /// <summary>
    /// 各リールが停止予約中か。
    /// 停止ボタンを押したあと、目標角度まで滑っている間 true になります。
    /// </summary>
    public bool[] StoppingNow { get; private set; }

    /// <summary>
    /// 各リールの停止予定角度。
    /// 停止ボタンを押した瞬間に決定されます。
    /// </summary>
    public float[] StopTargetAngles { get; private set; }

    /// <summary>
    /// 各リールの現在角度。
    /// Transform.eulerAngles から毎回読むのではなく、この値を正として管理します。
    /// </summary>
    public float[] ReelAngles { get; private set; }

    /// <summary>
    /// 各リールの開始時点の localRotation。
    /// モデル自体の初期傾きを壊さないために保存します。
    /// </summary>
    public Quaternion[] BaseRotations { get; private set; }

    /// <summary>
    /// リール本数に合わせて配列を作ります。
    /// </summary>
    public SlotReelState(int reelCount)
    {
        RotationNow = new bool[reelCount];
        StoppingNow = new bool[reelCount];
        StopTargetAngles = new float[reelCount];
        ReelAngles = new float[reelCount];
        BaseRotations = new Quaternion[reelCount];
    }

    /// <summary>
    /// 指定したリールを回転開始状態にします。
    /// </summary>
    public void StartReel(int reelIndex)
    {
        RotationNow[reelIndex] = true;
        StoppingNow[reelIndex] = false;
    }

    /// <summary>
    /// 指定したリールに停止予約を入れます。
    /// すぐに止めるのではなく、StopTargetAngles まで滑ってから止まります。
    /// </summary>
    public void ReserveStop(int reelIndex, float targetAngle)
    {
        StoppingNow[reelIndex] = true;
        StopTargetAngles[reelIndex] = targetAngle;
    }

    /// <summary>
    /// 指定したリールを完全停止状態にします。
    /// </summary>
    public void StopReel(int reelIndex)
    {
        RotationNow[reelIndex] = false;
        StoppingNow[reelIndex] = false;
    }

    /// <summary>
    /// どれか1つでもリールが回転中なら true を返します。
    /// MAXBET の受付可否判定などで使います。
    /// </summary>
    public bool IsAnyReelRotating()
    {
        for (int i = 0; i < RotationNow.Length; i++)
        {
            if (RotationNow[i])
            {
                return true;
            }
        }

        return false;
    }
}
