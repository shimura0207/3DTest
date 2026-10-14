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
    /// Random.Range(0, 99) なので、実際に出る値は 0〜99 です。
    /// </summary>
    private const int MaxRandom = 100;

    /// <summary>
    /// テーブル番号の最小値と最大値。
    /// Inspector で範囲外の値を入れても、この範囲に補正します。
    /// </summary>
    private const int MinTable = 1;
    private const int MaxTable = 5;

    int randomValue;
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
    /// 各行の合計は必ず 100 にしてください。
    /// 合計が足りない場合、最後にハズレへ逃げる可能性があります。
    /// 合計が多すぎる場合、後ろの役が抽選されにくくなります。
    /// </summary>
    private readonly int[,] roleTables =
    {
        { 45, 20, 0,  4,  6,  2, 2, 21 }, // T1
        { 40, 22, 2,  4, 6,  2, 2, 22 }, // T2
        { 35, 22, 2,  6,  8,  2,2, 22 }, // T3
        { 30, 24, 4,  6,  8,  3, 2, 23 }, // T4
        { 25, 25, 5,  6,  9,  4, 4, 23 }  // T5
    };

    /// <summary>
    /// 指定されたテーブルで役を抽選します。
    /// 
    /// table は 1〜5 を想定しています。
    /// 1未満なら1、5より大きければ5として扱います。
    /// </summary>
    public PachisuroSymbolKoyakuEnum DrawRole(int table)
    {
        // Inspector で範囲外の値が入っても壊れないように補正します。
        int tableIndex = Mathf.Clamp(table, MinTable, MaxTable) - 1;

        // 0〜65535 の乱数を取得します。
        randomValue = Random.Range(0, MaxRandom);

        // border は「ここまでの合計値」です。
        // 乱数が border 未満になった時点で、その役に当選します。
        int border = 0;

        for (int i = 0; i < roleTables.GetLength(1); i++)
        {
            border += roleTables[tableIndex, i];

            if (randomValue < border)
            {
                return (PachisuroSymbolKoyakuEnum)i;
            }
        }

        // 通常はここには来ません。
        // テーブル合計が 65536 未満だった場合などの保険です。
        return PachisuroSymbolKoyakuEnum.Miss;
    }

    //RUSH
    public PachisuroSymbolKoyakuEnum RUSHDrawRole(int table) {
        int sevenRandomValue = Random.Range(0, 2);

        if (sevenRandomValue == 0) {
            return PachisuroSymbolKoyakuEnum.Seven;
        }
        else {
            // Inspector で範囲外の値が入っても壊れないように補正します。
            int tableIndex = Mathf.Clamp(table, MinTable, MaxTable) - 1;

            // 0〜65535 の乱数を取得します。
            randomValue = Random.Range(0, MaxRandom);

            // border は「ここまでの合計値」です。
            // 乱数が border 未満になった時点で、その役に当選します。
            int border = 0;

            for (int i = 0; i < roleTables.GetLength(1); i++) {
                border += roleTables[tableIndex, i];

                if (randomValue < border) {
                    return (PachisuroSymbolKoyakuEnum)i;
                }
            }
        }
        // 通常はここには来ません。
        // テーブル合計が 65536 未満だった場合などの保険です。
        return PachisuroSymbolKoyakuEnum.Miss;
    }

    /// <summary>
    /// 役の日本語表示名を返します。
    /// Debug.Log や UI 表示に使います。
    /// </summary>
    public string GetRoleName(PachisuroSymbolKoyakuEnum role)
    {
        switch (role)
        {
            case PachisuroSymbolKoyakuEnum.Miss:
                return "ハズレ";

            case PachisuroSymbolKoyakuEnum.Bell:
                return "ベル";

            case PachisuroSymbolKoyakuEnum.Replay:
                return "リプレイ";

            case PachisuroSymbolKoyakuEnum.WeakCherry:
                return "弱チェリー";

            case PachisuroSymbolKoyakuEnum.Watermelon:
                return "スイカ";

            case PachisuroSymbolKoyakuEnum.Chance:
                return "チャンス目";

            case PachisuroSymbolKoyakuEnum.StrongCherry:
                return "強チェリー";

            case PachisuroSymbolKoyakuEnum.Seven:
                return "7揃い";

            default:
                return "不明";
        }
    }
}
