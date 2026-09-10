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
    Matchmaking,    // マッチング画面
    Option,         // 設定画面
    CardList,       // カード画面
    Gacha,          // ガチャ画面
    MainGame,       // メインゲーム
    EndGame,        // エンディング
    Max,

}

/// <summary>
/// メインメニューから選択する遷移先
/// </summary>
public enum eMainMenuSelect {
    None,           // 未選択
    Matchmaking,    // マッチング
    Option,         // 設定
    CardList,       // カード一覧
    Gacha,          // ガチャ
    Title           // タイトル
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