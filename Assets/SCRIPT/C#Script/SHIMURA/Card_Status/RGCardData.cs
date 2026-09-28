using System.Collections.Generic;
using UnityEngine;

/*
 * @file   RGCardData.h
 * @author simura
 */
/// <summary>
/// カード1枚分の基本データを管理するScriptableObject。
/// 
/// ここには、カードID、画像、ATK、HP、SIZE、対応役、種族など、
/// 「カードそのものの固定ステータス」を入れます。
/// 
/// 例:
/// ・RG_REACHE
/// ・BELL
/// ・REI
/// ・Cucina Impasto
/// などを1枚ずつアセットとして作るイメージです。
/// </summary>
[CreateAssetMenu(
    fileName = "New_RGCardData",
    menuName = "RG/Card Data"
)]
public class RGCardData : ScriptableObject
{
    //============================================================
    // 基本情報
    //============================================================

    [Header("基本情報")]

    [Tooltip("カードを一意に識別するID。同じIDのカードは作らないようにする。例: RG_REACHE_001")]
    [SerializeField] private string cardId;

    [Tooltip("ゲーム中に表示するカード名。例: RG:REACHE")]
    [SerializeField] private string cardName;

    [TextArea(3, 8)]
    [Tooltip("カード効果やフレーバーテキストなど。")]
    [SerializeField] private string description;

    //============================================================
    // 画像情報
    //============================================================

    [Header("画像情報")]

    [Tooltip("カードに表示するイラスト画像。Spriteを入れる。")]
    [SerializeField] private Sprite cardImage;

    [Tooltip("画像の管理用メモ。ファイル名、生成プロンプト、差分名などを書いておく用。")]
    [SerializeField] private string imageMemo;

    //============================================================
    // ステータス
    //============================================================

    [Header("ステータス")]

    [Tooltip("攻撃力。攻撃時に与える基本ダメージ。")]
    [SerializeField] private int atk = 0;

    [Tooltip("体力。0以下になったら破壊される想定。")]
    [SerializeField] private int hp = 1;

    [Tooltip("サイズ。盤面コストや配置制限に使う想定。")]
    [SerializeField] private int size = 1;

    //============================================================
    // 対応役
    //============================================================

    [Header("対応役")]

    [Tooltip("このカードが反応するパチスロ役。既存の SlotSymbolRole を使う。")]
    [SerializeField] private PachisuroSymbolKoyakuEnum supportRole;
    [SerializeField] private PachisuroSymbolKoyakuEnum supportRole1;
    [SerializeField] private PachisuroSymbolKoyakuEnum supportRole2;

    //============================================================
    // 種族
    //============================================================

    [Header("種族 最大3種")]

    [Tooltip("種族1。必須にしたい場合はNone以外を選ぶ。")]
    [SerializeField] private RGCardRace race1 = RGCardRace.None;

    [Tooltip("種族2。不要ならNone。")]
    [SerializeField] private RGCardRace race2 = RGCardRace.None;

    [Tooltip("種族3。不要ならNone。")]
    [SerializeField] private RGCardRace race3 = RGCardRace.None;

    //============================================================
    // 外部から読み取るためのプロパティ
    //============================================================

    public string CardId => cardId;
    public string CardName => cardName;
    public string Description => description;

    public Sprite CardImage => cardImage;
    public string ImageMemo => imageMemo;

    public int Atk => atk;
    public int Hp => hp;
    public int Size => size;

    public PachisuroSymbolKoyakuEnum SupportRole => supportRole;
    public PachisuroSymbolKoyakuEnum SupportRole1 => supportRole1;
    public PachisuroSymbolKoyakuEnum SupportRole2 => supportRole2;

    public RGCardRace Race1 => race1;
    public RGCardRace Race2 => race2;
    public RGCardRace Race3 => race3;

    //============================================================
    // 種族チェック用関数
    //============================================================

    /// <summary>
    /// このカードが指定した種族を持っているか確認します。
    /// 
    /// 例:
    /// if (card.HasRace(RGCardRace.Food))
    /// {
    ///     // 料理カードだけ強化
    /// }
    /// </summary>
    public bool HasRace(RGCardRace race)
    {
        if (race == RGCardRace.None)
        {
            return false;
        }

        return race1 == race || race2 == race || race3 == race;
    }

    /// <summary>
    /// Noneを除いた種族だけをリストで返します。
    /// UI表示や検索に使えます。
    /// </summary>
    public List<RGCardRace> GetRaces()
    {
        List<RGCardRace> races = new List<RGCardRace>();

        AddRaceIfValid(races, race1);
        AddRaceIfValid(races, race2);
        AddRaceIfValid(races, race3);

        return races;
    }

    /// <summary>
    /// 種族リストに、None以外かつ重複していない種族だけ追加します。
    /// </summary>
    private void AddRaceIfValid(List<RGCardRace> races, RGCardRace race)
    {
        if (race == RGCardRace.None)
        {
            return;
        }

        if (races.Contains(race))
        {
            return;
        }

        races.Add(race);
    }

    //============================================================
    // Inspector入力ミス防止
    //============================================================

    /// <summary>
    /// Inspector上で値が変更されたときに呼ばれます。
    /// 
    /// ここで、
    /// ・IDが空ならアセット名を入れる
    /// ・ATK/HP/SIZEが変な値にならないようにする
    /// ・種族の重複を消す
    /// という補正をしています。
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(cardId))
        {
            cardId = name;
        }

        atk = Mathf.Max(0, atk);
        hp = Mathf.Max(1, hp);
        size = Mathf.Max(1, size);

        RemoveDuplicateRaces();
    }

    /// <summary>
    /// 同じ種族を複数枠に入れてしまった場合、後ろの枠をNoneに戻します。
    /// 
    /// 例:
    /// race1 = Food
    /// race2 = Food
    /// の場合、race2をNoneに戻す。
    /// </summary>
    private void RemoveDuplicateRaces()
    {
        if (race2 != RGCardRace.None && race2 == race1)
        {
            race2 = RGCardRace.None;
        }

        if (race3 != RGCardRace.None && (race3 == race1 || race3 == race2))
        {
            race3 = RGCardRace.None;
        }
    }
}