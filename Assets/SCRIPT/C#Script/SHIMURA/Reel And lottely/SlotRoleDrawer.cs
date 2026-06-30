using UnityEngine;

/// <summary>
/// パチスロの役抽選を管理するクラス。
/// 
/// MonoBehaviour ではないので、GameObject にはアタッチしません。
/// SlotReelController.cs の中で new SlotRoleDrawer() して使います。
/// 
/// 【このクラスの役割】
/// ・現在のテーブル T1〜T5 に応じて役を抽選する
/// ・SlotSymbolRole を日本語表示名に変換する
/// 
/// 【ここをいじるとどうなる？】
/// roleTables の数字を変えると、役確率が変わります。
/// 例：Bell の数字を増やすとベルが当たりやすくなります。
/// ただし、各行の合計は必ず 65536 にしてください。
/// </summary>
public class SlotRoleDrawer
{
    /// <summary>
    /// 乱数の最大値。
    /// Random.Range(0, 65536) なので、実際に出る値は 0〜65535 です。
    /// </summary>
    private const int MaxRandom = 65536;

    /// <summary>
    /// テーブル番号の最小値と最大値。
    /// Inspector で範囲外の値を入れても、この範囲に補正します。
    /// </summary>
    private const int MinTable = 1;
    private const int MaxTable = 5;

    /// <summary>
    /// 役抽選テーブル。
    /// 
    /// 行：テーブル番号
    /// 0行目 = T1
    /// 1行目 = T2
    /// 2行目 = T3
    /// 3行目 = T4
    /// 4行目 = T5
    /// 
    /// 列：SlotSymbolRole enum の順番
    /// Miss, Bell, Replay, WeakCherry, Watermelon, Chance, StrongCherry, Seven
    /// 
    /// 【編集時の注意】
    /// 各行の合計は必ず 65536 にしてください。
    /// 合計が足りない場合、最後にハズレへ逃げる可能性があります。
    /// 合計が多すぎる場合、後ろの役が抽選されにくくなります。
    /// </summary>
    private readonly int[,] roleTables =
    {
        { 39196, 12288, 12288,  504,  504,  410, 218, 128 }, // T1
        { 35108, 13763, 13763,  983,  983,  590, 218, 128 }, // T2
        { 30720, 15073, 15073, 1769, 1769,  786, 218, 128 }, // T3
        { 24557, 17039, 17039, 2753, 2753, 1049, 218, 128 }, // T4
        { 19184, 18350, 18350, 3932, 3932, 1442, 218, 128 }  // T5
    };

    /// <summary>
    /// 指定されたテーブルで役を抽選します。
    /// 
    /// table は 1〜5 を想定しています。
    /// 1未満なら1、5より大きければ5として扱います。
    /// </summary>
    public SlotSymbolRole DrawRole(int table)
    {
        // Inspector で範囲外の値が入っても壊れないように補正します。
        int tableIndex = Mathf.Clamp(table, MinTable, MaxTable) - 1;

        // 0〜65535 の乱数を取得します。
        int randomValue = Random.Range(0, MaxRandom);

        // border は「ここまでの合計値」です。
        // 乱数が border 未満になった時点で、その役に当選します。
        int border = 0;

        for (int i = 0; i < roleTables.GetLength(1); i++)
        {
            border += roleTables[tableIndex, i];

            if (randomValue < border)
            {
                return (SlotSymbolRole)i;
            }
        }

        // 通常はここには来ません。
        // テーブル合計が 65536 未満だった場合などの保険です。
        return SlotSymbolRole.Miss;
    }

    /// <summary>
    /// 役の日本語表示名を返します。
    /// Debug.Log や UI 表示に使います。
    /// </summary>
    public string GetRoleName(SlotSymbolRole role)
    {
        switch (role)
        {
            case SlotSymbolRole.Miss:
                return "ハズレ";

            case SlotSymbolRole.Bell:
                return "ベル";

            case SlotSymbolRole.Replay:
                return "リプレイ";

            case SlotSymbolRole.WeakCherry:
                return "弱チェリー";

            case SlotSymbolRole.Watermelon:
                return "スイカ";

            case SlotSymbolRole.Chance:
                return "チャンス目";

            case SlotSymbolRole.StrongCherry:
                return "強チェリー";

            case SlotSymbolRole.Seven:
                return "7揃い";

            default:
                return "不明";
        }
    }
}
