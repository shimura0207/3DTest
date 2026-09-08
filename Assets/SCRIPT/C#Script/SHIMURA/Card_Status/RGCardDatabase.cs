using System.Collections.Generic;
using UnityEngine;
/*
 * @file   RGCardDatabase.h
 * @author simura
 */
/// <summary>
/// RGカードをまとめて管理するデータベース。
/// 
/// 役割:
/// ・カード一覧をInspectorで管理する
/// ・カードIDからカードを探す
/// ・対応役からカードを探す
/// ・種族からカードを探す
/// 
/// これはゲーム中の検索用です。
/// </summary>
[CreateAssetMenu(
    fileName = "New_RGCardDatabase",
    menuName = "RG/Card Database"
)]
public class RGCardDatabase : ScriptableObject
{
    [Header("カード一覧")]

    [Tooltip("ゲームに登場するカードデータを全部ここに入れる。")]
    [SerializeField] private List<RGCardData> cards = new List<RGCardData>();

    /// <summary>
    /// 登録されているカード一覧を読み取り専用で返します。
    /// </summary>
    public IReadOnlyList<RGCardData> Cards => cards;

    /// <summary>
    /// カードIDからカードを探します。
    /// 
    /// 例:
    /// RGCardData card = database.GetCardById("RG_REACHE_001");
    /// </summary>
    public RGCardData GetCardById(string cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId))
        {
            return null;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            RGCardData card = cards[i];

            if (card == null)
            {
                continue;
            }

            if (card.CardId == cardId)
            {
                return card;
            }
        }

        Debug.LogWarning($"カードIDが見つかりません: {cardId}");
        return null;
    }

    /// <summary>
    /// 指定した対応役を持つカードを全部取得します。
    /// 
    /// 例:
    /// ベル対応カードだけ取得したい場合に使う。
    /// </summary>
    public List<RGCardData> GetCardsBySupportRole(SlotSymbolRole role)
    {
        List<RGCardData> result = new List<RGCardData>();

        for (int i = 0; i < cards.Count; i++)
        {
            RGCardData card = cards[i];

            if (card == null)
            {
                continue;
            }

            if (card.SupportRole == role)
            {
                result.Add(card);
            }
        }

        return result;
    }

    /// <summary>
    /// 指定した種族を持つカードを全部取得します。
    /// 
    /// 例:
    /// Food種族のカードだけサーチしたい場合に使う。
    /// </summary>
    public List<RGCardData> GetCardsByRace(RGCardRace race)
    {
        List<RGCardData> result = new List<RGCardData>();

        if (race == RGCardRace.None)
        {
            return result;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            RGCardData card = cards[i];

            if (card == null)
            {
                continue;
            }

            if (card.HasRace(race))
            {
                result.Add(card);
            }
        }

        return result;
    }

    /// <summary>
    /// Inspector上でカードIDの重複をチェックします。
    /// 
    /// 同じIDのカードがあると、ID検索時にどちらを使うべきか分からなくなるので、
    /// Consoleに警告を出します。
    /// </summary>
    private void OnValidate()
    {
        CheckDuplicateCardIds();
    }

    private void CheckDuplicateCardIds()
    {
        HashSet<string> usedIds = new HashSet<string>();

        for (int i = 0; i < cards.Count; i++)
        {
            RGCardData card = cards[i];

            if (card == null)
            {
                continue;
            }

            string id = card.CardId;

            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            if (usedIds.Contains(id))
            {
                Debug.LogWarning($"カードIDが重複しています: {id}", this);
            }
            else
            {
                usedIds.Add(id);
            }
        }
    }
}