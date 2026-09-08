using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * @file   RGHandCardSlot.h
 * @author simura
 */


/// <summary>
/// Scene上の手札カード1枠を管理するスクリプト。
/// HandCard1～5 に付ける。
/// </summary>
public class RGHandCardSlot : MonoBehaviour
{
    [Header("現在のカード")]
    [SerializeField] private RGCardData currentCard;

    [Header("表示用 Image")]
    [SerializeField] private Image cardImage;

    [Header("表示用 TextMeshPro")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text sizeText;
    [SerializeField] private TMP_Text supportRoleText;
    [SerializeField] private TMP_Text raceText;

    public RGCardData CurrentCard => currentCard;
    public bool HasCard => currentCard != null;

    private void Awake()
    {
        AutoBindViewParts();
        RefreshView();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoBindViewParts();
    }
#endif

    /// <summary>
    /// 子オブジェクト名から表示パーツを自動取得する。
    /// Inspectorに手動で入れなくても、名前が合っていれば自動で入る。
    /// </summary>
    private void AutoBindViewParts()
    {
        if (cardImage == null)
        {
            cardImage = FindChildComponent<Image>("CardImage");
        }

        if (cardNameText == null)
        {
            cardNameText = FindChildComponent<TMP_Text>("CardNameText");
        }

        if (atkText == null)
        {
            atkText = FindChildComponent<TMP_Text>("AtkText");
        }

        if (hpText == null)
        {
            hpText = FindChildComponent<TMP_Text>("HpText");
        }

        if (sizeText == null)
        {
            sizeText = FindChildComponent<TMP_Text>("SizeText");
        }

        if (supportRoleText == null)
        {
            supportRoleText = FindChildComponent<TMP_Text>("SupportRoleText");
        }

        if (raceText == null)
        {
            raceText = FindChildComponent<TMP_Text>("RaceText");
        }
    }

    /// <summary>
    /// 指定した名前の子オブジェクトからコンポーネントを探す。
    /// 孫オブジェクト以下も探せる。
    /// </summary>
    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform child = FindChildRecursive(transform, childName);

        if (child == null)
        {
            return null;
        }

        return child.GetComponent<T>();
    }

    /// <summary>
    /// 子階層を再帰的に探す。
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                return child;
            }

            Transform result = FindChildRecursive(child, targetName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// この手札枠にカードを入れる。
    /// </summary>
    public void SetCard(RGCardData card)
    {
        currentCard = card;

        if (currentCard != null)
        {
            Debug.Log(
                $"{gameObject.name} にカード追加: " +
                $"ID={currentCard.CardId}, " +
                $"Name={currentCard.CardName}, " +
                $"ATK={currentCard.Atk}, " +
                $"HP={currentCard.Hp}, " +
                $"SIZE={currentCard.Size}, " +
                $"Role={currentCard.SupportRole}"
            );
        }

        RefreshView();
    }

    /// <summary>
    /// この手札枠を空にする。
    /// </summary>
    public void ClearCard()
    {
        currentCard = null;
        RefreshView();
    }

    /// <summary>
    /// 現在のカード情報を見た目に反映する。
    /// </summary>
    public void RefreshView()
    {
        if (currentCard == null)
        {
            ShowEmpty();
            return;
        }

        ShowCard(currentCard);
    }

    private void ShowCard(RGCardData card)
    {
        if (cardImage != null)
        {
            cardImage.enabled = true;
            cardImage.sprite = card.CardImage;
        }

        SetText(cardNameText, card.CardName);
        SetText(atkText, "ATK " + card.Atk);
        SetText(hpText, "HP " + card.Hp);
        SetText(sizeText, "SIZE " + card.Size);
        SetText(supportRoleText, "役 " + card.SupportRole);
        SetText(raceText, GetRaceText(card));
    }

    private void ShowEmpty()
    {
        if (cardImage != null)
        {
            cardImage.sprite = null;
            cardImage.enabled = false;
        }

        SetText(cardNameText, "");
        SetText(atkText, "");
        SetText(hpText, "");
        SetText(sizeText, "");
        SetText(supportRoleText, "");
        SetText(raceText, "");
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.text = value;
    }

    private string GetRaceText(RGCardData card)
    {
        List<RGCardRace> races = card.GetRaces();

        if (races.Count == 0)
        {
            return "種族 なし";
        }

        return "種族 " + string.Join(" / ", races);
    }
}