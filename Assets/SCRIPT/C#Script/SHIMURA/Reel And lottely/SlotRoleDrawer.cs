
using UnityEngine;

/// <summary>
/// パチスロの役抽選を管理するクラス。
///
/// MonoBehaviourではありません。
/// SlotReelController.csなどから new SlotRoleDrawer() して使用します。
///
/// 【機能】
/// ・T1～T50の役抽選
/// ・RUSH専用の役抽選
/// ・役の日本語表示名を取得
/// </summary>
public class SlotRoleDrawer {
    // 乱数範囲：0～99
    private const int MaxRandom = 100;

    // 使用できるテーブル範囲
    private const int MinTable = 1;
    private const int MaxTable = 50;

    /// <summary>
    /// 役抽選テーブル。
    ///
    /// 行：テーブル番号（T1～T50）
    ///
    /// 列：PachisuroSymbolKoyakuEnumの順番
    ///
    /// 0 Miss         はずれ
    /// 1 Bell         ベル
    /// 2 Replay       リプレイ
    /// 3 WeakCherry   弱チェリー
    /// 4 Suika        スイカ
    /// 5 Chance       チャンス目
    /// 6 StrongCherry 強チェリー
    /// 7 Seven        7揃い
    ///
    /// 各行の合計は必ず100％です。
    /// </summary>

    private readonly int[,] roleTables =
    {
        // はずれ, ベル, リプ, 弱チェ, スイカ, チャンス目, 強チェ, 7揃い

        { 45, 20, 20,  4,  6,  2,  2,  1 }, // T1
        { 40, 22, 22,  4,  6,  2,  2,  2 }, // T2
        { 35, 22, 23,  6,  8,  2,  2,  2 }, // T3
        { 30, 24, 24,  6,  8,  3,  2,  3 }, // T4
        { 25, 25, 25,  6,  8,  4,  4,  3 }, // T5

        { 23, 26, 25,  6,  8,  4,  4,  4 }, // T6
        { 21, 25, 25,  7,  9,  4,  4,  5 }, // T7
        { 19, 25, 25,  7,  9,  5,  5,  5 }, // T8
        { 17, 25, 25,  7, 10,  5,  5,  6 }, // T9
        { 15, 25, 25,  8, 10,  5,  6,  6 }, // T10

        { 15, 25, 25,  8, 10,  5,  6,  6 }, // T11
        { 14, 26, 25,  8, 10,  5,  6,  6 }, // T12
        { 14, 25, 25,  8, 11,  5,  6,  6 }, // T13
        { 14, 24, 24,  8, 11,  6,  6,  7 }, // T14
        { 13, 24, 23,  9, 11,  6,  7,  7 }, // T15
        { 13, 24, 23,  9, 11,  6,  7,  7 }, // T16
        { 12, 24, 24,  9, 11,  6,  7,  7 }, // T17
        { 12, 24, 24,  9, 11,  6,  7,  7 }, // T18
        { 12, 24, 23,  9, 12,  6,  7,  7 }, // T19
        { 11, 24, 24,  9, 12,  6,  7,  7 }, // T20

        { 11, 24, 24,  9, 12,  6,  7,  7 }, // T21
        { 11, 23, 23,  9, 12,  7,  7,  8 }, // T22
        { 10, 24, 23,  9, 12,  7,  7,  8 }, // T23
        { 10, 24, 23,  9, 12,  7,  7,  8 }, // T24
        {  9, 23, 22, 10, 13,  7,  8,  8 }, // T25
        {  9, 23, 22, 10, 13,  7,  8,  8 }, // T26
        {  9, 23, 22, 10, 13,  7,  8,  8 }, // T27
        {  8, 23, 23, 10, 13,  7,  8,  8 }, // T28
        {  8, 23, 23, 10, 13,  7,  8,  8 }, // T29
        {  8, 22, 21, 10, 14,  8,  8,  9 }, // T30

        {  7, 22, 22, 10, 14,  8,  8,  9 }, // T31
        {  7, 22, 22, 10, 14,  8,  8,  9 }, // T32
        {  6, 23, 22, 10, 14,  8,  8,  9 }, // T33
        {  6, 23, 22, 10, 14,  8,  8,  9 }, // T34
        {  6, 22, 21, 11, 14,  8,  9,  9 }, // T35
        {  5, 22, 21, 11, 15,  8,  9,  9 }, // T36
        {  5, 22, 21, 11, 15,  8,  9,  9 }, // T37
        {  5, 21, 20, 11, 15,  9,  9, 10 }, // T38
        {  4, 21, 21, 11, 15,  9,  9, 10 }, // T39
        {  4, 21, 21, 11, 15,  9,  9, 10 }, // T40

        {  3, 22, 21, 11, 15,  9,  9, 10 }, // T41
        {  3, 21, 21, 11, 16,  9,  9, 10 }, // T42
        {  3, 21, 21, 11, 16,  9,  9, 10 }, // T43
        {  2, 22, 21, 11, 16,  9,  9, 10 }, // T44
        {  2, 21, 20, 12, 16,  9, 10, 10 }, // T45
        {  1, 20, 20, 12, 16, 10, 10, 11 }, // T46
        {  1, 20, 20, 12, 16, 10, 10, 11 }, // T47
        {  1, 20, 19, 12, 17, 10, 10, 11 }, // T48
        {  0, 20, 20, 12, 17, 10, 10, 11 }, // T49
        {  0, 20, 20, 12, 17, 10, 10, 11 }, // T50
    };

    /// <summary>
    /// 通常時の役抽選。
    /// 指定テーブルに応じて役を抽選します。
    /// </summary>
    public PachisuroSymbolKoyakuEnum DrawRole(int table) {
        return DrawRoleFromTable(table);
    }

    /// <summary>
    /// RUSH時の役抽選。
    ///
    /// 現在の仕様：
    /// 50％で7揃い確定。
    /// 残り50％で指定テーブルから通常抽選。
    /// </summary>
    public PachisuroSymbolKoyakuEnum RUSHDrawRole(int table) {
        int sevenRandomValue = Random.Range(0, 2);

        if (sevenRandomValue == 0) {
            return PachisuroSymbolKoyakuEnum.Seven;
        }

        return DrawRoleFromTable(table);
    }

    /// <summary>
    /// テーブルに応じた共通役抽選処理。
    /// </summary>
    private PachisuroSymbolKoyakuEnum DrawRoleFromTable(int table) {
        // T1～T50の範囲に補正
        int tableIndex = Mathf.Clamp(table, MinTable, MaxTable) - 1;

        // 0～99の乱数
        int randomValue = Random.Range(0, MaxRandom);

        // 累積確率
        int border = 0;

        for (int i = 0; i < roleTables.GetLength(1); i++) {
            border += roleTables[tableIndex, i];

            if (randomValue < border) {
                return (PachisuroSymbolKoyakuEnum)i;
            }
        }

        // 保険
        return PachisuroSymbolKoyakuEnum.Miss;
    }

    /// <summary>
    /// 役の日本語表示名を返します。
    /// </summary>
    public string GetRoleName(PachisuroSymbolKoyakuEnum role) {
        switch (role) {
            case PachisuroSymbolKoyakuEnum.Miss:
                return "ハズレ";

            case PachisuroSymbolKoyakuEnum.Bell:
                return "ベル";

            case PachisuroSymbolKoyakuEnum.Replay:
                return "リプレイ";

            case PachisuroSymbolKoyakuEnum.WeakCherry:
                return "弱チェリー";

            case PachisuroSymbolKoyakuEnum.Suika:
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
