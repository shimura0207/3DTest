using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReelStopAngles
{
    // 各コマの停止角度
    // 20コマなら20個分設定する
    public float[] stopAngles =
    {
        0f,
        18f,
        36f,
        54f,
        72f,
        90f,
        108f,
        126f,
        144f,
        162f,
        180f,
        198f,
        216f,
        234f,
        252f,
        270f,
        288f,
        306f,
        324f,
        342f
    };
}

public class ReelScript : MonoBehaviour
{


    Rolelottery roleLottery = new Rolelottery();

    [SerializeField] int currentTable = 1;

    Role currentRole;
    // 回転軸をInspectorから選べるようにする
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    // リールの回転速度
    [SerializeField] float reelSpeed = -320f;

    // リールの回転軸
    // 横に回るならXかZに変更して調整
    [SerializeField] RotationAxis rotationAxis = RotationAxis.Y;

    // MAXBETが押されているか
    bool maxBet = false;

    // リール本体
    // 0:左 1:中 2:右
    [SerializeField] GameObject[] reels = new GameObject[3];

    // リールごとの停止角度
    // Element 0:左リール
    // Element 1:中リール
    // Element 2:右リール

    [Header("Role Stop Angles")]
    [SerializeField]
    RoleStopAngleSetting[] roleStopAngleSettings =
{
    new RoleStopAngleSetting(),
    new RoleStopAngleSetting(),
    new RoleStopAngleSetting(),
    new RoleStopAngleSetting(),
    new RoleStopAngleSetting(),
    new RoleStopAngleSetting(),
    new RoleStopAngleSetting(),
    new RoleStopAngleSetting()
};


    [System.Serializable]
    public class RoleStopAngleSetting
    {
        // この設定を使う役
        public Role role;

        // 0:左リール 1:中リール 2:右リール
        public ReelStopAngles[] reelStopAngles =
        {
        new ReelStopAngles(),
        new ReelStopAngles(),
        new ReelStopAngles()
    };
    }
    // 停止キー
    // 0:左 1:中 2:右
    [SerializeField]
    KeyCode[] stopKeys =
    {
        KeyCode.LeftArrow,
        KeyCode.DownArrow,
        KeyCode.RightArrow
    };

    // 各リールが回転中か
    bool[] rotationNow;

    // 各リールが停止予約中か
    bool[] stoppingNow;

    // 各リールの停止予定角度
    float[] stopTargetAngles;

    // 各リールの現在角度
    // Transformから角度を読まず、この変数で管理する
    [Header("Debug")]
    [SerializeField] float[] reelAngles;

    // 各リールの初期回転
    Quaternion[] baseRotations;


    [Header("Current Reel Angles")]
    [SerializeField] float leftCurrentAngle;
    [SerializeField] float centerCurrentAngle;
    [SerializeField] float rightCurrentAngle;

    // Inspectorの値が変更されたか判定するための保存用
    float[] lastInspectorAngles = new float[3];

    void Start()
    {
        InitializeReels();
        InitializeDebugAngles();
    }

    void Update()
    {
        MAXBET_ON();
        LEVER_ON();
        REEL_STOP();
        REEL_START();

        SyncInspectorAngles();
    }

    // リール管理用の配列を初期化
    void InitializeReels()
    {
        int reelCount = reels.Length;

        rotationNow = new bool[reelCount];
        stoppingNow = new bool[reelCount];
        stopTargetAngles = new float[reelCount];
        reelAngles = new float[reelCount];
        baseRotations = new Quaternion[reelCount];

        for (int i = 0; i < reelCount; i++)
        {
            if (reels[i] == null)
            {
                continue;
            }

            // 最初のリール角度を基準として保存
            baseRotations[i] = reels[i].transform.localRotation;

            // 自前管理する角度は0度から開始
            reelAngles[i] = 0f;
        }
    }

    // MAXBETボタン処理
    void MAXBET_ON()
    {
        // RightControlが押され、かつリールが1つも回っていないならMAXBET可能
        if (Input.GetKeyDown(KeyCode.RightControl) && !IsAnyReelRotating())
        {
            maxBet = true;
        }
    }

    // レバーON処理
    void LEVER_ON()
    {
        // MAXBET後に上矢印キーでレバーON
        if (maxBet && Input.GetKeyDown(KeyCode.UpArrow))
        {
            maxBet = false;

            // 役抽選
            currentRole = roleLottery.DrawRole(currentTable);

            // 抽選結果をログに表示
            Debug.Log("抽選結果: " + roleLottery.GetRoleName(currentRole));

            // 全リール回転開始
            for (int i = 0; i < reels.Length; i++)
            {
                if (reels[i] == null)
                {
                    continue;
                }

                rotationNow[i] = true;
                stoppingNow[i] = false;
            }
        }
    }

    // リール回転処理
    void REEL_START()
    {
        ROTATION();
    }

    // リール停止入力処理
    void REEL_STOP()
    {
        for (int i = 0; i < reels.Length; i++)
        {
            if (i >= stopKeys.Length)
            {
                continue;
            }

            // 停止キーが押された
            if (Input.GetKeyDown(stopKeys[i]))
            {
                // 回転中、かつまだ停止予約していないなら停止予約
                if (rotationNow[i] && !stoppingNow[i])
                {
                    stoppingNow[i] = true;

                    // このリール用の停止角度リストから、次に止まる角度を決める
                    stopTargetAngles[i] = GetNextStopAngle(i, reelAngles[i], currentRole);
                }
            }
        }
    }

    // 実際にリールを回転させる
    void ROTATION()
    {
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] == null)
            {
                continue;
            }

            // 回転中でなければ何もしない
            if (!rotationNow[i])
            {
                continue;
            }

            // 停止予約中なら、停止予定角度まで滑らせる
            if (stoppingNow[i])
            {
                MoveToStopAngle(i);
            }
            else
            {
                // 通常回転
                reelAngles[i] += reelSpeed * Time.deltaTime;
                reelAngles[i] = NormalizeAngle(reelAngles[i]);

                ApplyReelRotation(i);
            }
        }
    }

    // 停止予定角度まで進める
    // ここでは絶対に逆方向へ戻さない
    void MoveToStopAngle(int reelIndex)
    {
        float currentAngle = NormalizeAngle(reelAngles[reelIndex]);
        float targetAngle = NormalizeAngle(stopTargetAngles[reelIndex]);

        // 1フレームで進む角度
        float moveAmount = Mathf.Abs(reelSpeed) * Time.deltaTime;

        // 現在角度から目標角度まで、回転方向に進んだ場合の距離
        float distanceToTarget;

        if (reelSpeed < 0)
        {
            // マイナス方向に回転している場合
            distanceToTarget = NormalizeAngle(currentAngle - targetAngle);
        }
        else
        {
            // プラス方向に回転している場合
            distanceToTarget = NormalizeAngle(targetAngle - currentAngle);
        }

        // 次の移動で目標角度を通り過ぎるなら、目標角度で止める
        if (distanceToTarget <= moveAmount)
        {
            reelAngles[reelIndex] = targetAngle;
            ApplyReelRotation(reelIndex);

            rotationNow[reelIndex] = false;
            stoppingNow[reelIndex] = false;
        }
        else
        {
            // 必ず回転方向にだけ進める
            if (reelSpeed < 0)
            {
                reelAngles[reelIndex] -= moveAmount;
            }
            else
            {
                reelAngles[reelIndex] += moveAmount;
            }

            reelAngles[reelIndex] = NormalizeAngle(reelAngles[reelIndex]);
            ApplyReelRotation(reelIndex);
        }
    }

    // 指定リールの停止角度リストから、進行方向の先にある一番近い角度を探す
    float GetNextStopAngle(int reelIndex, float currentAngle, Role role)
    {
        currentAngle = NormalizeAngle(currentAngle);

        float bestAngle = currentAngle;
        float bestDistance = 9999f;

        const float MIN_DISTANCE = 0.2f;

        float[] angles = GetStopAnglesByRole(reelIndex, role);

        if (angles == null || angles.Length == 0)
        {
            Debug.LogWarning(role + " の停止角度が設定されていません。現在角度で停止します。");
            return currentAngle;
        }

        for (int i = 0; i < angles.Length; i++)
        {
            float stopAngle = NormalizeAngle(angles[i]);

            float distance;

            if (reelSpeed < 0)
            {
                distance = NormalizeAngle(currentAngle - stopAngle);
            }
            else
            {
                distance = NormalizeAngle(stopAngle - currentAngle);
            }

            if (distance < MIN_DISTANCE)
            {
                distance = 360f;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestAngle = stopAngle;
            }
        }

        return bestAngle;
    }


    float[] GetStopAnglesByRole(int reelIndex, Role role)
    {
        if (roleStopAngleSettings == null)
        {
            return null;
        }

        for (int i = 0; i < roleStopAngleSettings.Length; i++)
        {
            RoleStopAngleSetting setting = roleStopAngleSettings[i];

            if (setting == null)
            {
                continue;
            }

            if (setting.role != role)
            {
                continue;
            }

            if (setting.reelStopAngles == null)
            {
                return null;
            }

            if (reelIndex < 0 || reelIndex >= setting.reelStopAngles.Length)
            {
                return null;
            }

            if (setting.reelStopAngles[reelIndex] == null)
            {
                return null;
            }

            return setting.reelStopAngles[reelIndex].stopAngles;
        }

        return null;
    }

    // 自前管理している角度をQuaternionでリールに反映
    void ApplyReelRotation(int reelIndex)
    {
        Vector3 axis = GetAxisVector();

        Quaternion addRotation = Quaternion.AngleAxis(reelAngles[reelIndex], axis);

        reels[reelIndex].transform.localRotation = baseRotations[reelIndex] * addRotation;
    }

    // Inspectorで選んだ回転軸をVector3に変換
    Vector3 GetAxisVector()
    {
        switch (rotationAxis)
        {
            case RotationAxis.X:
                return Vector3.right;

            case RotationAxis.Y:
                return Vector3.up;

            case RotationAxis.Z:
                return Vector3.forward;
        }

        return Vector3.up;
    }

    // 角度を0〜360の範囲に直す
    float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0)
        {
            angle += 360f;
        }

        return angle;
    }

    // どれか1つでもリールが回転中か調べる
    bool IsAnyReelRotating()
    {
        for (int i = 0; i < rotationNow.Length; i++)
        {
            if (rotationNow[i])
            {
                return true;
            }
        }

        return false;
    }

    void InitializeDebugAngles()
    {
        if (reelAngles == null || reelAngles.Length < 3)
        {
            return;
        }

        leftCurrentAngle = reelAngles[0];
        centerCurrentAngle = reelAngles[1];
        rightCurrentAngle = reelAngles[2];

        lastInspectorAngles[0] = leftCurrentAngle;
        lastInspectorAngles[1] = centerCurrentAngle;
        lastInspectorAngles[2] = rightCurrentAngle;
    }

    void SyncInspectorAngles()
    {
        if (reelAngles == null || reelAngles.Length < 3)
        {
            return;
        }

        float[] inspectorAngles =
        {
        leftCurrentAngle,
        centerCurrentAngle,
        rightCurrentAngle
    };

        for (int i = 0; i < 3; i++)
        {
            // Inspector側の値が変更された場合
            if (!Mathf.Approximately(inspectorAngles[i], lastInspectorAngles[i]))
            {
                // ここで必ず0〜360に戻す
                reelAngles[i] = NormalizeAngle(inspectorAngles[i]);

                ApplyReelRotation(i);

                // Inspector表示側にも正規化後の角度を戻す
                inspectorAngles[i] = reelAngles[i];

                lastInspectorAngles[i] = reelAngles[i];
            }
            else
            {
                // 通常時も必ず0〜360に戻す
                reelAngles[i] = NormalizeAngle(reelAngles[i]);

                inspectorAngles[i] = reelAngles[i];
                lastInspectorAngles[i] = reelAngles[i];
            }
        }

        leftCurrentAngle = inspectorAngles[0];
        centerCurrentAngle = inspectorAngles[1];
        rightCurrentAngle = inspectorAngles[2];
    }






}