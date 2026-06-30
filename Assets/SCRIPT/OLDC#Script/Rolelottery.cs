using UnityEngine;

/// <summary>
/// パチスロの役抽選を管理するクラス。
/// </summary>
public class Rolelottery
{
    // 乱数の最大値
    private const int MaxRandom = 65536;

    // テーブル範囲
    private const int MinTable = 1;
    private const int MaxTable = 5;

    // 役抽選テーブル
    // Role enum の順番と一致させること
    // Miss, Bell, Replay, WeakCherry, Watermelon, Chance, StrongCherry, Seven
    private readonly int[,] roleTables =
    {
        { 39196, 12288, 12288,  504,  504,  410, 218, 128 }, // T1
        { 35108, 13763, 13763,  983,  983,  590, 218, 128 }, // T2
        { 30720, 15073, 15073, 1769, 1769,  786, 218, 128 }, // T3
        { 24557, 17039, 17039, 2753, 2753, 1049, 218, 128 }, // T4
        { 19184, 18350, 18350, 3932, 3932, 1442, 218, 128 }, // T5
    };

    /// <summary>
    /// 指定されたテーブルで役を抽選する。
    /// table は 1〜5 を指定する。
    /// </summary>
    public Role DrawRole(int table)
    {
        // table が範囲外でも 1〜5 に補正する
        int tableIndex = Mathf.Clamp(table, MinTable, MaxTable) - 1;

        // 0〜65535 の乱数を取得
        int randomValue = Random.Range(0, MaxRandom);

        int border = 0;

        for (int i = 0; i < roleTables.GetLength(1); i++)
        {
            border += roleTables[tableIndex, i];

            if (randomValue < border)
            {
                return (Role)i;
            }
        }

        // 念のため、どの範囲にも入らなかった場合はハズレ
        return Role.Miss;
    }

    /// <summary>
    /// 役の表示名を返す。
    /// </summary>
    public string GetRoleName(Role role)
    {
        switch (role)
        {
            case Role.Miss:
                return "ハズレ";

            case Role.Bell:
                return "ベル";

            case Role.Replay:
                return "リプレイ";

            case Role.WeakCherry:
                return "弱チェリー";

            case Role.Watermelon:
                return "スイカ";

            case Role.Chance:
                return "チャンス目";

            case Role.StrongCherry:
                return "強チェリー";

            case Role.Seven:
                return "7揃い";

            default:
                return "不明";
        }
    }
}

/// <summary>
/// パチスロの役。
/// roleTables の列順と必ず一致させる。
/// </summary>
public enum Role
{
    Miss,          // ハズレ
    Bell,          // ベル
    Replay,        // リプレイ
    WeakCherry,    // 弱チェリー
    Watermelon,    // スイカ
    Chance,        // チャンス目
    StrongCherry,  // 強チェリー
    Seven          // 7揃い
}