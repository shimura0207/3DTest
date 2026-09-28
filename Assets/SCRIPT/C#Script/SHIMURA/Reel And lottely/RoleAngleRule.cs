/// <summary>
/// 役ごとの停止角度設定。
/// 
/// 例：
/// role = Bell の設定なら、ベル成立時に使う停止角度をここに入れます。
/// reelStopAngles[0] = 左リール
/// reelStopAngles[1] = 中リール
/// reelStopAngles[2] = 右リール
/// 
/// 【ここをいじるとどうなる？】
/// ・role を変える → この設定がどの役のときに使われるかが変わる
/// ・reelStopAngles[0] を変える → 左リールの停止可能角度が変わる
/// ・reelStopAngles[1] を変える → 中リールの停止可能角度が変わる
/// ・reelStopAngles[2] を変える → 右リールの停止可能角度が変わる
/// </summary>
[System.Serializable]
public class RoleAngleRule
{
    /// <summary>
    /// この設定を使う役。
    /// </summary>
    public PachisuroSymbolKoyakuEnum role;

    /// <summary>
    /// 0:左リール / 1:中リール / 2:右リール の停止角度。
    /// </summary>
    public StopAngleList[] reelStopAngles =
    {
        new StopAngleList(),
        new StopAngleList(),
        new StopAngleList()
    };

    /// <summary>
    /// Unity の Inspector 表示用の空コンストラクタ。
    /// </summary>
    public RoleAngleRule()
    {
    }

    /// <summary>
    /// 初期値として役を指定したいとき用。
    /// SlotReelController.cs の初期配列で使っています。
    /// </summary>
    public RoleAngleRule(PachisuroSymbolKoyakuEnum role)
    {
        this.role = role;
    }
}
