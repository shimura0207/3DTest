using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 手札カードを選択し、PlayerPanelに配置する流れを管理するスクリプト。
/// 
/// 修正版:
/// AwakeではなくStartでButton登録する。
/// 理由:
/// 他のスクリプトのAwakeがまだ終わっていない状態でButtonを探すと、
/// PanelButtonがnullになってクリック処理が登録されないことがあるため。
/// </summary>
public class RGCardPlayController : MonoBehaviour
{
    [Header("デッキ/手札管理")]
    [SerializeField] private RGDeckController deckController;

    [Header("手札スロット")]
    [Tooltip("HandCard1～5を左から順番に入れる。Element0がHandCard1。")]
    [SerializeField] private RGHandCardSlot[] handSlots = new RGHandCardSlot[5];

    [Header("PlayerPanel")]
    [Tooltip("PlayerPanel1～5を左から順番に入れる。")]
    [SerializeField] private RGFieldPanelSlot[] playerPanels = new RGFieldPanelSlot[5];

    /// <summary>
    /// 現在選択中の手札番号。
    /// -1なら何も選択していない。
    /// </summary>
    private int selectedHandIndex = -1;

    /// <summary>
    /// Button登録済みかどうか。
    /// 二重登録を防ぐためのフラグ。
    /// </summary>
    private bool isBound = false;

    private void Start()
    {
        BindAllButtons();
        SetAllPlayerPanelButtons(false);
    }

    /// <summary>
    /// 手札ButtonとPlayerPanelButtonのクリック処理を登録する。
    /// </summary>
    private void BindAllButtons()
    {
        if (isBound)
        {
            return;
        }

        BindHandButtons();
        BindPlayerPanelButtons();

        isBound = true;

        Debug.Log("RGCardPlayController: Button登録完了");
    }

    /// <summary>
    /// HandCard1～5のButtonにクリック処理を登録します。
    /// </summary>
    private void BindHandButtons()
    {
        for (int i = 0; i < handSlots.Length; i++)
        {
            int index = i;

            if (handSlots[i] == null)
            {
                Debug.LogWarning("HandSlots Element " + i + " が空です。");
                continue;
            }

            Button handButton = handSlots[i].GetComponent<Button>();

            if (handButton == null)
            {
                Debug.LogWarning(handSlots[i].gameObject.name + " にButtonがありません。");
                continue;
            }

            handButton.onClick.AddListener(() => SelectHandCard(index));

            Debug.Log("手札Button登録: HandCard" + (index + 1));
        }
    }

    /// <summary>
    /// PlayerPanel1～5のButtonにクリック処理を登録します。
    /// </summary>
    private void BindPlayerPanelButtons()
    {
        for (int i = 0; i < playerPanels.Length; i++)
        {
            int index = i;

            if (playerPanels[i] == null)
            {
                Debug.LogWarning("PlayerPanels Element " + i + " が空です。");
                continue;
            }

            Button panelButton = playerPanels[i].GetPanelButton();

            if (panelButton == null)
            {
                Debug.LogWarning(playerPanels[i].gameObject.name + " にButtonがありません。");
                continue;
            }

            panelButton.onClick.AddListener(() => PlaceSelectedCardToPanel(index));

            Debug.Log("PlayerPanel Button登録: PlayerPanel" + (index + 1));
        }
    }

    /// <summary>
    /// 手札カードを選択します。
    /// </summary>
    private void SelectHandCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= handSlots.Length)
        {
            return;
        }

        if (handSlots[handIndex] == null)
        {
            return;
        }

        RGCardData card = handSlots[handIndex].CurrentCard;

        if (card == null)
        {
            Debug.Log("空の手札は選択できません。HandCard" + (handIndex + 1));
            ClearSelection();
            return;
        }

        selectedHandIndex = handIndex;

        SetAllPlayerPanelButtons(true);

        Debug.Log("手札を選択しました: HandCard" + (handIndex + 1) + " / " + card.CardName);
    }

    /// <summary>
    /// 選択中の手札カードを、指定されたPlayerPanelに配置します。
    /// </summary>
    private void PlaceSelectedCardToPanel(int panelIndex)
    {
        Debug.Log("PlayerPanelが押されました: PlayerPanel" + (panelIndex + 1));

        if (selectedHandIndex < 0)
        {
            Debug.LogWarning("手札が選択されていません。");
            return;
        }

        if (panelIndex < 0 || panelIndex >= playerPanels.Length)
        {
            Debug.LogWarning("Panel番号が範囲外です: " + panelIndex);
            return;
        }

        if (deckController == null)
        {
            Debug.LogWarning("RGDeckControllerが設定されていません。");
            return;
        }

        RGHandCardSlot selectedHandSlot = handSlots[selectedHandIndex];

        if (selectedHandSlot == null)
        {
            Debug.LogWarning("選択中のHandSlotがnullです。");
            ClearSelection();
            return;
        }

        RGCardData selectedCard = selectedHandSlot.CurrentCard;

        if (selectedCard == null)
        {
            Debug.LogWarning("選択中の手札カードDataがnullです。");
            ClearSelection();
            return;
        }

        RGFieldPanelSlot targetPanel = playerPanels[panelIndex];

        if (targetPanel == null)
        {
            Debug.LogWarning("配置先Panelがnullです。");
            return;
        }

        bool placed = targetPanel.PlaceCardFromHandSlot(selectedHandSlot, selectedCard);

        if (!placed)
        {
            Debug.LogWarning("カード配置に失敗しました。手札は消費しません。");
            return;
        }

        Debug.Log("カード配置成功: " + selectedCard.CardName);

        // 手札を消費して左詰め
        deckController.RemoveHandCardAtAndCompact(selectedHandIndex);

        // 選択解除
        ClearSelection();
    }

    /// <summary>
    /// 選択状態を解除します。
    /// </summary>
    private void ClearSelection()
    {
        selectedHandIndex = -1;
        SetAllPlayerPanelButtons(false);
    }

    /// <summary>
    /// PlayerPanel1～5のButtonをまとめて押せる/押せない状態にします。
    /// </summary>
    private void SetAllPlayerPanelButtons(bool enabled)
    {
        for (int i = 0; i < playerPanels.Length; i++)
        {
            if (playerPanels[i] == null)
            {
                continue;
            }

            playerPanels[i].SetPanelButtonEnabled(enabled);
        }
    }
}