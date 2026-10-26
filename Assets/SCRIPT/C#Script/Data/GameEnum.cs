/*
 * @file   GameEnum
 * @author oorui
 */
public enum PachisuroSymbolKoyakuEnum {
    Miss,          // ハズレ
    Bell,          // ベル
    Replay,        // リプレイ
    WeakCherry,    // 弱チェリー
    Suika,         // スイカ
    Chance,        // チャンス目
    StrongCherry,  // 強チェリー
    Seven,         // 7揃い

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
/// デッキ作成から選択される遷移先
/// </summary>
public enum DeckBuildMenuSelect {
    None,
    DeckBuild,
    MainMenu
}

/// <summary>
/// カード関連パートから選択する遷移先
/// </summary>
public enum CardRelationMenuSelect {
    None,           // 未選択
    CardList,       // カード一覧
    DeckBuilding,   // デッキ構築
    MainMenu,       // メインメニュー
    CardRelation    // カード一覧画面
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
/// リザルト画面から遷移するパート
/// </summary>
public enum ResultSelectMenu {
    None,
    MainMenu,   //メインメニュー画面
    Matching    // マッチング画面
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

/// <summary>
/// カードのレアリティ
/// </summary>
public enum CardRearity {
    None = -1,
    N = 1,
    R = 2,
    SR = 3,
    LR = 4
}

/// <summary>
/// カードのテーマ 
/// </summary>
public enum CardTheme {

}
/// <summary>
/// カードのタイプ
/// モンスター
/// スペル
/// </summary>
public enum CardType {
    None = -1,
    Monster = 0,
    Spell = 1
}
/// <summary>
/// カードの対応役
/// </summary>
public enum CardSuportRole {
    None = -1,

    NULL = 0,
    Miss = 1,
    Replay = 2,
    Cherry = 3,
    Bell = 4,
    Suika = 5,
    ALL = 6,
    Cherry_Suika = 7,

}

/// <summary>
/// カードの種族
/// @author oorui
/// </summary>
public enum CardRaceID {
    None = -1,
    Robo = 1,
    Warrior = 2,
    Human = 3,

    Max
}

/// <summary>
/// フェードの画像種類
/// </summary>
public enum FadeType {
    White,  // 白
    Black,  // 黒
    Max,
}

public enum VFXType {
    Invalid = -1,
    ClickEffect,

    max,
}

/// <summary>
/// フィールドのレーン番号
/// @author Riku
/// </summary>
public enum FieldLane {
    None = -1,

    Lane1,
    Lane2,
    Lane3,
    Lane4,
    Lane5,

    Max,
}

/// <summary>
/// 効果の種類
/// @author Riku
/// </summary>
public enum AbilityType {
    None = -1,

    // 組み合わせで作れる通常効果
    Normal,
    // 組み合わせでは作れない複雑な特殊効果
    Special,
    // キーワード効果
    Keyword,

    Max,
}

/// <summary>
/// 特殊効果
/// @author Riku
/// </summary>
public enum SpecialAbility {
    None = -1,

    // イスカの1番目の効果(仮)
    Isuka1,
    // イスカの2番目の効果(仮)
    Isuka2,

    Max,
}

/// <summary>
/// キーワード効果
/// @author Riku
/// </summary>
public enum KeywordAbility {
    None = -1,
    
    // アスリート(疲労しない)
    Athlete,
    // ターゲット集中(相手の攻撃を全て受ける)
    TargetFocus,

    Max,
}

/// <summary>
/// 効果の発動タイミング
/// @author Riku
/// </summary>
public enum AbilityTiming {
    None = -1,

    // ターン開始時
    TurnStart,
    // カード使用時
    Play,
    // 攻撃時
    Attack,
    // ターン終了時
    TurnEnd,

    Max,
}

/// <summary>
/// 効果の発動対象
/// @author Riku
/// </summary>
public enum AbilityTarget {
    None = -1,

    // 自身
    SelfPlayer,
    // 相手
    OpponentPlayer,
    // ランダム
    Random,
    // 選択
    Select,

    Max,
}

/// <summary>
/// 効果の発動条件
/// @author Riku
/// </summary>
public enum AbilityCondition {
    None = -1,

    // 無条件
    Free,
    // 現在のサイズによって
    CurrentSize,

    Max,
}

/// <summary>
/// 効果の発動内容
/// @author Riku
/// </summary>
public enum AbilityProcess {
    None = -1,

    // カードのドロー
    Draw,
    // カードのサーチ
    Search,
    // ダメージ付与
    GiveDamage,
    // HP回復
    HeelHP,
    // 手札を捨てる
    DiscardHand,
    // ATK上昇
    UpATK,
    // ATK減少
    DownATK,
    // モンスターを出す
    SummonMonster,
    // モンスター変化
    TransformationMonster,
    // 状態付与
    GiveState,
    // モンスター破壊
    DeleteMonster,

    Max,
}


/// <summary>
/// 通信メッセージの先頭に付ける処理ID。
/// </summary>
public enum NetworkUseID {
    // 攻撃力
    atk, 
    // HP
    hp, 
    // ドロー
    draw, 
    // 召喚
    samon, 
    // デッキ
    deck,
}