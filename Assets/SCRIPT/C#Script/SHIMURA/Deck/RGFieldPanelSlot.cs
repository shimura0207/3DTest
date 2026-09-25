using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

/*
 * @file   RGFieldPanelSlot.h
 * @author simura
 */


/// <summary>
/// フィールド上のPlayerPanel1～5を管理するスクリプト。
/// 
/// 役割:
/// ・PanelのButtonを普段は押せない状態にする
/// ・手札が選択されたときだけ押せるようにする
/// ・クリックされたら、手札カードの見た目をPanel上にコピー表示する
/// </summary>
[RequireComponent(typeof(Button))]
public class RGFieldPanelSlot : MonoBehaviour
{

    /// <summary>
    /// このフィールドに置かれているカードのScriptableObject。
    /// </summary>
    private RGCardData placedCardData;

    /// <summary>
    /// 現在フィールドに置かれているカードのScriptableObjectを取得。
    /// 他のスクリプトからカードステータスを読むために使用します。
    /// </summary>
    public RGCardData PlacedCardData => placedCardData;


    [Header("Panel Button")]

    [Tooltip("PlayerPanelのButton。空なら自動取得します。")]
    [SerializeField] private Button panelButton;

    [Header("カード配置先")]

    [Tooltip("カードを重ねて表示する場所。空ならこのPanel自身の上に配置します。")]
    [SerializeField] private RectTransform cardParent;

    [Header("配置ルール")]

    [Tooltip("すでにカードが置かれているPanelに上書き配置できるか。")]
    [SerializeField] private bool allowReplace = false;

    /// <summary>
    /// このPanel上に置かれているカード表示オブジェクト。
    /// 手札カードの見た目をコピーしたものです。
    /// </summary>
    private GameObject placedCardObject;

    public Button PanelButton => panelButton;

    public bool HasCard => placedCardObject != null;

    private void Awake()
    {
        if (panelButton == null)
        {
            panelButton = GetComponent<Button>();
        }

        if (cardParent == null)
        {
            cardParent = GetComponent<RectTransform>();
        }

        SetPanelButtonEnabled(false);
    }

    /// <summary>
    /// このPanelのButtonを押せる/押せない状態にします。
    /// 
    /// 基本は押せない。
    /// 手札選択中だけ RGCardPlayController から true にされます。
    /// </summary>
    public void SetPanelButtonEnabled(bool enabled)
    {
        if (panelButton == null)
        {
            return;
        }

        if (HasCard && !allowReplace)
        {
            panelButton.interactable = false;
            return;
        }

        panelButton.interactable = enabled;
    }
    public void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            ATTACK();
        }
    }
    /// <summary>
    /// 手札カードをこのPanel上に配置します。
    /// 
    /// 実際には手札GameObjectを直接移動するのではなく、
    /// 手札カードの見た目をコピーしてPanel上に置きます。
    /// 
    /// 理由:
    /// 手札のGameObject自体を移動すると、HandCard1～5の枠が壊れるため。
    /// </summary>
    public bool PlaceCardFromHandSlot(RGHandCardSlot sourceHandSlot, RGCardData cardData)
    {
        if (sourceHandSlot == null)
        {
            Debug.LogWarning("配置元の手札Slotがnullです。");
            return false;
        }

        if (cardData == null)
        {
            Debug.LogWarning("配置しようとしたカードDataがnullです。");
            return false;
        }

        if (HasCard && !allowReplace)
        {
            Debug.LogWarning($"{gameObject.name} にはすでにカードが置かれています。");
            return false;
        }

        if (HasCard && allowReplace)
        {
            Destroy(placedCardObject);
        }

        placedCardObject = Instantiate(sourceHandSlot.gameObject, cardParent);
        placedCardObject.name = "FieldCard_"/* + cardData.CardName*/;

        placedCardData = cardData;
        // コピーしたカードのButtonを無効化。
        // フィールド上のカードを手札ボタンとして押せないようにします。
        DisableButtonsInChildren(placedCardObject);

        // コピーしたカード側にもDataを明示的に入れ直します。
        RGHandCardSlot copiedSlot = placedCardObject.GetComponent<RGHandCardSlot>();

        if (copiedSlot != null)
        {
            //copiedSlot.SetCard(cardData);
        }

        // フィールドカード用コンポーネントを取得
        RGFieldCard fieldCard =
            placedCardObject.GetComponent<RGFieldCard>();

        // なければ追加
        if (fieldCard == null) {
            fieldCard =
                placedCardObject.AddComponent<RGFieldCard>();
        }

        // ScriptableObjectを設定
        fieldCard.SetCardData(cardData);

        fieldCard.SetCardData(cardData);

        FitCardToPanel(placedCardObject);

        //Debug.Log($"{gameObject.name} にカードを配置しました: {cardData.CardName}");
        // ScriptableObjectを設定
        fieldCard.SetCardData(cardData);

       /* // 配置したカードのステータスをDebug.Log
        Debug.Log(
            $"===== フィールドカード配置 =====\n" +
            $"カード名 : {cardData.CardName}\n" +
            $"CardID   : {cardData.CardId}\n" +
            $"ATK      : {cardData.Atk}\n" +
            $"HP       : {cardData.Hp}\n" +
            $"SIZE     : {cardData.Size}\n" +
            $"対応役   : {cardData.SupportRole}\n" +
            $"種族     : {cardData.Race1}, {cardData.Race2}, {cardData.Race3}\n" +
            $"=============================="
        );
       */
        return true;
    }

    /// <summary>
    /// Panel上に置いたカードを削除します。
    /// 今後、カード破壊や墓地送りを作るときに使えます。
    /// </summary>
    public void ClearPlacedCard()
    {
        if (placedCardObject != null)
        {
            Destroy(placedCardObject);
            placedCardObject = null;
        }
    }

    /// <summary>
    /// コピーしたカードのButtonをすべて押せない状態にします。
    /// </summary>
    private void DisableButtonsInChildren(GameObject target)
    {
        Button[] buttons = target.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
    }

    /// <summary>
    /// コピーしたカードをPanelに重なるように広げます。
    /// 
    /// Panelと同じサイズにしたくない場合は、
    /// ここのanchorやoffsetを変更します。
    /// </summary>
    private void FitCardToPanel(GameObject cardObject)
    {
        RectTransform rectTransform = cardObject.GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            return;
        }

        rectTransform.SetParent(cardParent, false);

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// PlayerPanelについているButtonを返します。
    /// まだ取得できていなければ、この場でGetComponentします。
    /// </summary>
    public Button GetPanelButton()
    {
        if (panelButton == null)
        {
            panelButton = GetComponent<Button>();
        }

        return panelButton;
    }


    public void ATTACK() {
        
    }
}