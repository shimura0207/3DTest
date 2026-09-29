using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TextLinkTest : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text effectText;

    [SerializeField] private Camera uiCamera;

    [SerializeField] private TextMeshProUGUI text;

    void Start() {
        effectText.text = "この効果は<link=\"LINK\">仮</link>である。".ToString();
    }

    void Update() {
        
    }

    public void OnPointerClick(PointerEventData eventData) {
        // その場所にリンクがあるか判定
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(effectText, eventData.position,null);

        if (linkIndex == -1)
            return;

        // リンクの情報
        TMP_LinkInfo linkInfo = effectText.textInfo.linkInfo[linkIndex];

        string linkId = linkInfo.GetLinkID();

        if (linkId == "LINK") {
            effectText.text = "これはリンクである。".ToString();
        }
        //Debug.Log("クリックされたリンクID: " + linkId);
        //Debug.Log("クリックされた文字: " + linkInfo.GetLinkText());
    }
}
