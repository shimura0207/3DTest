/*
 * @file   GameEnum
 * @author oorui
 */
public enum PachisuroSymbolKoyakuEnum {
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

/// <summary>
/// ゲームのパート
/// </summary>
public enum GamePart {
    Invalid = -1,
    Standby,        // 待機
    Title,          // タイトル
    MainMenu,       // メニュー
    Matchmaking,    // マッチング画面
    RandomMatch,    // ランダム対戦
    PrivateMatch,   // プライベート対戦
    Option,         // 設定画面
    CardRelation,   // カード関連画面
    CardList,       // カード一覧画面
    DeckBuild,      // デッキ構築画面
    Gacha,          // ガチャ画面
    MainGame,       // メインゲーム
    EndGame,        // エンディング
    Tutorial,       // チュートリアル
    Max,

}

/// <summary>
/// メインメニューから選択する遷移先
/// </summary>
public enum MainMenuSelect {
    None,           // 未選択
    Matchmaking,    // マッチング
    Option,         // 設定
    CardRelation,   // カード関連
    Gacha,          // ガチャ
    Title,          // タイトル
    EndGame         // リザルト
}

/// <summary>
/// カード関連パートから選択する遷移先
/// </summary>
public enum CardRelationMenuSelect {
    None,           // 未選択
    CardList,       // カード一覧
    DeckBuilding,   // デッキ構築
    MainMenu        // メインメニュー
}
/// <summary>
/// 設定パートで選択する遷移先
/// </summary>
public enum OptionMenuSelect {
    None,
    MainMenu,   // メインメニュー
    GameEnd,    // ゲーム終了
    Max
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

/// <summary>
/// 自分か対戦相手か
/// @author Riku
/// </summary>
public enum PlayerType {
    None = -1,

    // 自分
    Self,
    // 相手
    Opponent,

    Max
}

/// <summary>
/// バトルの各フェイズ
/// </summary>
public enum BattlePhase {
    None = -1,

    // ターン開始フェイズ
    StartPhase,
    // メインフェイズ
    MainPhase,
    // パチスロフェイズ
    PachisuroPhase,
    // アタックフェイズ
    AttackPhase,
    // ターン終了フェイズ
    EndPhase,

    Max
}

/// <summary>
/// 対戦の状態
/// </summary>
public enum BattleState {
    None = -1,

    // 対戦進行中
    InProgress,
    // 勝利
    Win,
    // 敗北
    Lose,

    Max
}

/// <summary>
/// 接続状況
/// </summary>
public enum ConnectionState {
    None = -1,
    Host,   // ホスト
    Client, // クライアント
    Max
}