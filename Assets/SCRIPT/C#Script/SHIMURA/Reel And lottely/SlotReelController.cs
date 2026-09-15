using UnityEngine;

/// <summary>
/// リール全体を管理するメインクラス。
/// 
/// このスクリプトだけを GameObject にアタッチします。
/// 他のクラスは、この SlotReelController を分かりやすくするための補助クラスです。
/// 
/// 【このクラスの役割】
/// ・MAXBET入力
/// ・レバー入力
/// ・役抽選の呼び出し
/// ・リール回転開始
/// ・停止ボタン入力
/// ・停止可能角度まで滑らせて停止
/// ・Inspector用の現在角度表示
/// 
/// 【基本的によくいじる場所】
/// ・currentTable      → 現在の抽選テーブル
/// ・reelSpeed         → リールの回転速度
/// ・rotationAxis      → リールの回転軸
/// ・reels             → 左/中/右リールの GameObject
/// ・roleStopAngleSettings → 役ごとの停止可能角度
/// ・stopKeys          → 停止ボタン
/// </summary>
public class SlotReelController : MonoBehaviour
{
    //============================================================
    // 抽選関連
    //============================================================

    /// <summary>
    /// 役抽選クラス。
    /// 役確率を変更したい場合は SlotRoleDrawer.cs を編集します。
    /// </summary>
    private SlotRoleDrawer roleLottery = new SlotRoleDrawer();

    /// <summary>
    /// 現在の抽選テーブル。
    /// 1〜5 の範囲で使用します。
    /// 
    /// 【ここをいじるとどうなる？】
    /// 1ならT1、5ならT5で抽選します。
    /// 数字が大きいほど、現在のテーブル設定ではレア役などが引きやすくなっています。
    /// </summary>
    [Header("Lottery")]
    [SerializeField] private int currentTable = 1;

    /// <summary>
    /// レバーON時に抽選された現在の役。
    /// 停止角度を決めるときに使います。
    /// </summary>
    private SlotSymbolRole currentSlotSymbolRole = SlotSymbolRole.Miss;

    //============================================================
    // リール基本設定
    //============================================================

    /// <summary>
    /// リールの回転速度。
    /// 
    /// 【ここをいじるとどうなる？】
    /// 数値の絶対値を大きくすると速く回ります。
    /// マイナスなら逆方向、プラスなら正方向に回ります。
    /// 例：-320 → 今の速度
    /// 例：-500 → もっと速い
    /// 例： 320 → 逆向きに回る
    /// </summary>
    [Header("Reel Settings")]
    [SerializeField] private float reelSpeed = -320f;

    /// <summary>
    /// リールの回転軸。
    /// 
    /// 【ここをいじるとどうなる？】
    /// リールが横向きに倒れて回る、変な方向に回る、という場合に X/Y/Z を変更して調整します。
    /// </summary>
    [SerializeField] private SpinAxis rotationAxis = SpinAxis.Y;

    /// <summary>
    /// MAXBET 済みかどうか。
    /// true のときだけレバーONを受け付けます。
    /// </summary>
    private bool maxBet = false;

    /// <summary>
    /// リール本体の GameObject。
    /// 
    /// 0:左リール
    /// 1:中リール
    /// 2:右リール
    /// 
    /// 【ここをいじるとどうなる？】
    /// Inspector で各リールの GameObject を入れます。
    /// ここが未設定だと、そのリールは動きません。
    /// </summary>
    [SerializeField] private GameObject[] reels = new GameObject[3];

    //============================================================
    // 役ごとの停止角度設定
    //============================================================

    /// <summary>
    /// 役ごとの停止角度設定。
    /// 
    /// 【ここをいじるとどうなる？】
    /// 成立役ごとに、左/中/右リールが止まってよい角度を指定できます。
    /// 例えば Bell の左リールに 0, 72, 144 だけ入れれば、ベル成立時の左リールはその角度にしか止まりません。
    /// 
    /// 【注意】
    /// 同じ role の設定を複数作ると、上にある設定が優先されます。
    /// </summary>
    [Header("Role Stop Angles")]
    [SerializeField]
    private RoleAngleRule[] roleStopAngleSettings =
    {
        new RoleAngleRule(SlotSymbolRole.Miss),
        new RoleAngleRule(SlotSymbolRole.Bell),
        new RoleAngleRule(SlotSymbolRole.Replay),
        new RoleAngleRule(SlotSymbolRole.WeakCherry),
        new RoleAngleRule(SlotSymbolRole.Watermelon),
        new RoleAngleRule(SlotSymbolRole.Chance),
        new RoleAngleRule(SlotSymbolRole.StrongCherry),
        new RoleAngleRule(SlotSymbolRole.Seven)
    };

    //============================================================
    // 入力設定
    //============================================================

    /// <summary>
    /// 停止キー。
    /// 
    /// 0:左リール停止
    /// 1:中リール停止
    /// 2:右リール停止
    /// 
    /// 【ここをいじるとどうなる？】
    /// Inspector で変更すると、停止ボタンを好きなキーにできます。
    /// </summary>
    [Header("Input")]
    [SerializeField]
    private KeyCode[] stopKeys =
    {
        KeyCode.LeftArrow,
        KeyCode.DownArrow,
        KeyCode.RightArrow
    };

    //============================================================
    // 実行中状態
    //============================================================

    /// <summary>
    /// リールの実行中状態。
    /// 回転中フラグ、停止予約フラグ、現在角度などをまとめて持ちます。
    /// </summary>
    private SlotReelState state;

    //============================================================
    // Debug / Inspector表示用
    //============================================================

    /// <summary>
    /// 左リールの現在角度。
    /// Inspector から値を直接変更すると、その角度にリールを動かせます。
    /// </summary>
    [Header("Debug - Current Reel Angles")]
    [SerializeField] private float leftCurrentAngle;

    /// <summary>
    /// 中リールの現在角度。
    /// </summary>
    [SerializeField] private float centerCurrentAngle;

    /// <summary>
    /// 右リールの現在角度。
    /// </summary>
    [SerializeField] private float rightCurrentAngle;

    /// <summary>
    /// Inspector の角度が手動変更されたか判定するための前回値。
    /// </summary>
    private float[] lastInspectorAngles = new float[3];

    //============================================================
    // Unityイベント
    //============================================================

    private void Start()
    {
        InitializeReels();
        InitializeDebugAngles();
    }

    private void Update()
    {
        
    }

    //============================================================
    // 初期化
    //============================================================

    /// <summary>
    /// リール管理用の状態を初期化します。
    /// 
    /// 各リールの最初の localRotation を保存しておくことで、
    /// モデルの初期角度を保ったまま追加回転だけをかけられます。
    /// </summary>
    /// 

    public void SlotUpdate() {
        HandleMaxBetInput();
        HandleLeverInput();
        HandleStopInput();
        UpdateReelRotation();
        SyncInspectorAngles();
        TableUp();
    }
    private void InitializeReels()
    {
        int reelCount = reels.Length;
        state = new SlotReelState(reelCount);

        for (int i = 0; i < reelCount; i++)
        {
            if (reels[i] == null)
            {
                Debug.LogWarning($"reels[{i}] が設定されていません。このリールは動きません。");
                continue;
            }

            // 最初のリール角度を基準として保存します。
            state.BaseRotations[i] = reels[i].transform.localRotation;

            // 自前管理する角度は0度から開始します。
            state.ReelAngles[i] = 0f;
        }
    }

    /// <summary>
    /// Inspector に表示する現在角度を初期化します。
    /// </summary>
    private void InitializeDebugAngles()
    {
        if (state == null || state.ReelAngles == null || state.ReelAngles.Length < 3)
        {
            return;
        }

        leftCurrentAngle = state.ReelAngles[0];
        centerCurrentAngle = state.ReelAngles[1];
        rightCurrentAngle = state.ReelAngles[2];

        lastInspectorAngles[0] = leftCurrentAngle;
        lastInspectorAngles[1] = centerCurrentAngle;
        lastInspectorAngles[2] = rightCurrentAngle;
    }

    //============================================================
    // 入力処理
    //============================================================

    /// <summary>
    /// MAXBET入力。
    /// 
    /// RightControl が押され、かつリールが1つも回っていないなら MAXBET 成立です。
    /// </summary>
    private void HandleMaxBetInput()
    {
        if (Input.GetKeyDown(KeyCode.RightControl) && !state.IsAnyReelRotating())
        {
            maxBet = true;
        }
    }

    /// <summary>
    /// レバーON入力。
    /// 
    /// MAXBET 後に UpArrow を押すと役抽選を行い、全リールを回転開始します。
    /// </summary>
    private void HandleLeverInput()
    {
        if (!maxBet)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.UpArrow))
        {
            return;
        }

        TryStartSpin();
    }

    /// <summary>
    /// 停止ボタン入力。
    /// 
    /// stopKeys に設定されたキーが押されたら、そのリールの停止予約を入れます。
    /// 停止予約時に「どの角度まで滑るか」を決定します。
    /// </summary>
    private void HandleStopInput()
    {
        for (int i = 0; i < reels.Length; i++)
        {
            if (i >= stopKeys.Length)
            {
                continue;
            }

            if (!Input.GetKeyDown(stopKeys[i]))
            {
                continue;
            }

            TryReserveStop(i);
        }
    }


    //============================================================
    // 外部スクリプト用 公開メソッド
    //============================================================

    /// <summary>
    /// どれか1つでもリールが回っているかを返します。
    /// 自動回転側が「次Gに進んでいいか」を確認するために使います。
    /// </summary>
    public bool IsAnyReelRotating()
    {
        return state != null && state.IsAnyReelRotating();
    }

    /// <summary>
    /// 新しく1G開始できる状態かを返します。
    /// 全リール停止中なら true。
    /// </summary>
    public bool CanStartSpin()
    {
        return state != null && !state.IsAnyReelRotating();
    }

    /// <summary>
    /// リール本数を返します。
    /// 基本は3本です。
    /// </summary>
    public int GetReelCount()
    {
        if (reels == null)
        {
            return 0;
        }

        return reels.Length;
    }

    /// <summary>
    /// 1G分の回転を開始します。
    /// 手動のレバーONと同じ処理を外部から呼べるようにしたものです。
    /// </summary>
    public bool TryStartSpin()
    {
        if (state == null)
        {
            Debug.LogWarning("SlotReelController がまだ初期化されていません。");
            return false;
        }

        if (state.IsAnyReelRotating())
        {
            return false;
        }

        maxBet = false;

        currentSlotSymbolRole = roleLottery.DrawRole(currentTable);

        Debug.Log("抽選結果: " + roleLottery.GetRoleName(currentSlotSymbolRole));

        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] == null)
            {
                continue;
            }

            state.StartReel(i);
        }

        return true;
    }

    /// <summary>
    /// 指定したリールに停止予約を入れます。
    /// reelIndex は 0=左, 1=中, 2=右。
    /// </summary>
    public bool TryReserveStop(int reelIndex)
    {
        if (state == null)
        {
            return false;
        }

        if (reels == null || reelIndex < 0 || reelIndex >= reels.Length)
        {
            return false;
        }

        if (reels[reelIndex] == null)
        {
            return false;
        }

        if (!state.RotationNow[reelIndex] || state.StoppingNow[reelIndex])
        {
            return false;
        }

        float nextStopAngle = GetNextStopAngle(
            reelIndex,
            state.ReelAngles[reelIndex],
            currentSlotSymbolRole
        );

        state.ReserveStop(reelIndex, nextStopAngle);

        return true;
    }
    //============================================================
    // リール回転処理
    //============================================================

    /// <summary>
    /// 毎フレーム、各リールの回転を更新します。
    /// 
    /// 停止予約中なら停止予定角度まで滑らせます。
    /// 停止予約中でなければ通常回転します。
    /// </summary>
    private void UpdateReelRotation()
    {
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] == null)
            {
                continue;
            }

            if (!state.RotationNow[i])
            {
                continue;
            }

            if (state.StoppingNow[i])
            {
                MoveToStopAngle(i);
            }
            else
            {
                RotateNormally(i);
            }
        }
    }

    /// <summary>
    /// 通常回転。
    /// 停止ボタンがまだ押されていない間はこの処理で回ります。
    /// </summary>
    private void RotateNormally(int reelIndex)
    {
        state.ReelAngles[reelIndex] += reelSpeed * Time.deltaTime;
        state.ReelAngles[reelIndex] = AngleNormalizer.NormalizeAngle(state.ReelAngles[reelIndex]);

        ApplyReelRotation(reelIndex);
    }

    /// <summary>
    /// 停止予定角度まで滑らせます。
    /// 
    /// 【重要】
    /// ここでは絶対に逆方向へ戻しません。
    /// reelSpeed がマイナスならマイナス方向、プラスならプラス方向にだけ進みます。
    /// </summary>
    private void MoveToStopAngle(int reelIndex)
    {
        float currentAngle = AngleNormalizer.NormalizeAngle(state.ReelAngles[reelIndex]);
        float targetAngle = AngleNormalizer.NormalizeAngle(state.StopTargetAngles[reelIndex]);

        // 1フレームで進む角度。
        float moveAmount = Mathf.Abs(reelSpeed) * Time.deltaTime;

        // 現在角度から目標角度まで、回転方向に進んだ場合の距離。
        float distanceToTarget;

        if (reelSpeed < 0f)
        {
            // マイナス方向に回転している場合。
            distanceToTarget = AngleNormalizer.NormalizeAngle(currentAngle - targetAngle);
        }
        else
        {
            // プラス方向に回転している場合。
            distanceToTarget = AngleNormalizer.NormalizeAngle(targetAngle - currentAngle);
        }

        // 次の移動で目標角度を通り過ぎるなら、目標角度で止めます。
        if (distanceToTarget <= moveAmount)
        {
            state.ReelAngles[reelIndex] = targetAngle;
            ApplyReelRotation(reelIndex);
            state.StopReel(reelIndex);
            return;
        }

        // まだ目標角度まで届かない場合は、回転方向にだけ進めます。
        if (reelSpeed < 0f)
        {
            state.ReelAngles[reelIndex] -= moveAmount;
        }
        else
        {
            state.ReelAngles[reelIndex] += moveAmount;
        }

        state.ReelAngles[reelIndex] = AngleNormalizer.NormalizeAngle(state.ReelAngles[reelIndex]);
        ApplyReelRotation(reelIndex);
    }

    //============================================================
    // 停止角度計算
    //============================================================

    /// <summary>
    /// 指定リールの停止角度リストから、回転方向の先にある一番近い角度を探します。
    /// 
    /// reelIndex: 0=左、1=中、2=右
    /// currentAngle: 今のリール角度
    /// role: 今回成立した役
    /// </summary>
    private float GetNextStopAngle(int reelIndex, float currentAngle, SlotSymbolRole role)
    {
        currentAngle = AngleNormalizer.NormalizeAngle(currentAngle);

        float bestAngle = currentAngle;
        float bestDistance = 9999f;

        // 今の角度とほぼ同じ角度に即停止してしまうのを避けるための最小距離。
        // 0.2度以内なら「次周の同じ角度」として扱います。
        const float MinDistance = 0.2f;

        float[] angles = GetStopAnglesByRole(reelIndex, role);

        if (angles == null || angles.Length == 0)
        {
            Debug.LogWarning(role + " の停止角度が設定されていません。現在角度で停止します。");
            return currentAngle;
        }

        for (int i = 0; i < angles.Length; i++)
        {
            float stopAngle = AngleNormalizer.NormalizeAngle(angles[i]);
            float distance;

            if (reelSpeed < 0f)
            {
                distance = AngleNormalizer.NormalizeAngle(currentAngle - stopAngle);
            }
            else
            {
                distance = AngleNormalizer.NormalizeAngle(stopAngle - currentAngle);
            }

            if (distance < MinDistance)
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

    /// <summary>
    /// 成立役とリール番号から、使用する停止角度一覧を取得します。
    /// 
    /// 例：
    /// role = Bell, reelIndex = 0 なら、ベル成立時の左リール停止角度を返します。
    /// </summary>
    private float[] GetStopAnglesByRole(int reelIndex, SlotSymbolRole role)
    {
        if (roleStopAngleSettings == null)
        {
            return null;
        }

        for (int i = 0; i < roleStopAngleSettings.Length; i++)
        {
            RoleAngleRule setting = roleStopAngleSettings[i];

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

    //============================================================
    // Transform反映
    //============================================================

    /// <summary>
    /// 自前管理している角度を Quaternion に変換して、実際のリールに反映します。
    /// 
    /// baseRotation * addRotation にすることで、
    /// 最初からモデルについていた角度を壊さずに回転だけ追加できます。
    /// </summary>
    private void ApplyReelRotation(int reelIndex)
    {
        if (reels == null || reelIndex < 0 || reelIndex >= reels.Length)
        {
            return;
        }

        if (reels[reelIndex] == null)
        {
            return;
        }

        Vector3 axis = GetAxisVector();
        Quaternion addRotation = Quaternion.AngleAxis(state.ReelAngles[reelIndex], axis);

        reels[reelIndex].transform.localRotation = state.BaseRotations[reelIndex] * addRotation;
    }

    /// <summary>
    /// Inspector で選んだ回転軸を Vector3 に変換します。
    /// </summary>
    private Vector3 GetAxisVector()
    {
        switch (rotationAxis)
        {
            case SpinAxis.X:
                return Vector3.right;

            case SpinAxis.Y:
                return Vector3.up;

            case SpinAxis.Z:
                return Vector3.forward;

            default:
                return Vector3.up;
        }
    }

    //============================================================
    // Inspector角度同期
    //============================================================

    /// <summary>
    /// Inspector に表示している現在角度と、内部管理している角度を同期します。
    /// 
    /// 【ここをいじるとどうなる？】
    /// Debug 用の leftCurrentAngle / centerCurrentAngle / rightCurrentAngle の挙動に関係します。
    /// Inspector から角度を直接入力してリール位置を確認したい場合に使います。
    /// </summary>
    private void SyncInspectorAngles()
    {
        if (state == null || state.ReelAngles == null || state.ReelAngles.Length < 3)
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
            // Inspector 側の値が変更された場合。
            if (!Mathf.Approximately(inspectorAngles[i], lastInspectorAngles[i]))
            {
                // Inspector から入力された角度も必ず 0〜360 に戻します。
                state.ReelAngles[i] = AngleNormalizer.NormalizeAngle(inspectorAngles[i]);

                ApplyReelRotation(i);

                // Inspector 表示側にも正規化後の角度を戻します。
                inspectorAngles[i] = state.ReelAngles[i];
                lastInspectorAngles[i] = state.ReelAngles[i];
            }
            else
            {
                // 通常時も必ず 0〜360 に戻します。
                state.ReelAngles[i] = AngleNormalizer.NormalizeAngle(state.ReelAngles[i]);

                inspectorAngles[i] = state.ReelAngles[i];
                lastInspectorAngles[i] = state.ReelAngles[i];
            }
        }

        leftCurrentAngle = inspectorAngles[0];
        centerCurrentAngle = inspectorAngles[1];
        rightCurrentAngle = inspectorAngles[2];
    }
    /// <summary>
    /// 今回のゲームで抽選された役を返します。
    /// 自動回転リザルト表示で「何の役を引いたか」を集計するために使います。
    /// </summary>
    public SlotSymbolRole GetCurrentSlotSymbolRole()
    {
        return currentSlotSymbolRole;
    }

    /// <summary>
    /// 役を日本語名で返します。
    /// Console表示用です。
    /// </summary>
    public string GetSlotSymbolRoleName(SlotSymbolRole role)
    {
        return roleLottery.GetRoleName(role);
    }


    public void TableUp() {
        if (Input.GetKeyDown(KeyCode.U)) {
            currentTable += 1;
            Debug.Log(currentTable);
        }   
    }
}
