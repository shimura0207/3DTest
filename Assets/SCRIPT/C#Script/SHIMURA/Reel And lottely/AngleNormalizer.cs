/// <summary>
/// リール角度に関する便利処理をまとめた static クラス。
/// 
/// 【このクラスの役割】
/// 角度を 0〜360 に戻す処理など、どこからでも使える処理を置きます。
/// </summary>
public static class AngleNormalizer
{
    /// <summary>
    /// 角度を 0以上360未満 の範囲に補正します。
    /// 
    /// 例：
    /// 370  → 10
    /// -18  → 342
    /// 720  → 0
    /// 
    /// 【ここをいじるとどうなる？】
    /// currentAngle が 360 を超えたり、マイナスになったりする挙動に関係します。
    /// 基本的には変更しないのがおすすめです。
    /// </summary>
    public static float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0f)
        {
            angle += 360f;
        }

        return angle;
    }
}
