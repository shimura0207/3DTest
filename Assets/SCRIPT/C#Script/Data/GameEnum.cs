/*
 * @file   GameEnum
 * @author oorui
 */

/// <summary>
/// ゲームのパート
/// </summary>
public enum eGamePart {
    Invalid = -1,
    Standby,        // 待機
    Title,          // タイトル
    MainMenu,       // メニュー
    MainGame,       // メインゲーム
    EndGame,        // エンディング
    Max,

}

/// <summary>
/// RGカードの種族一覧
/// </summary>
public enum RGCardRace {
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