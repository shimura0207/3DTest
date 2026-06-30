/// <summary>
/// RGカードの種族一覧。
/// 
/// ここを増やすと、カードデータのInspectorで選べる種族が増えます。
/// 例:
/// Food      → 料理系
/// Machine   → 機械系
/// Cyber     → サイバー系
/// Dragon    → ドラゴン系
/// </summary>
public enum RGCardRace
{
    None,

    Machine,
    Cyber,
    Dragon,
    Beast,
    Warrior,
    Demon,
    Angel,
    Aqua,
    Plant,
    Undead,

    Food,
    KungFu,
    Tool,
    Spell,
    Genesis
}