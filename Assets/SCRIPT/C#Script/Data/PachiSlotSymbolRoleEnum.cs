/// <summary>
/// パチスロの役一覧。
/// 
/// 【重要】
/// SlotRoleDrawer.cs の抽選テーブルは、この enum の並び順と一致させています。
/// つまり、ここで順番を変える場合は SlotRoleDrawer.cs の roleTables の列順も必ず変えてください。
/// 
/// 例：
/// Miss が 0 番目、Bell が 1 番目、Replay が 2 番目……という扱いになります。
/// </summary>
public enum PachiSlotSymbolRoleEnum
{
    Miss,          // ハズレ
    Bell,          // ベル
    Replay,        // リプレイ
    WeakCherry,    // 弱チェリー
    Watermelon,    // スイカ
    Chance,        // チャンス目
    StrongCherry,  // 強チェリー
    Seven,          // 7揃い

    NONE,
}
