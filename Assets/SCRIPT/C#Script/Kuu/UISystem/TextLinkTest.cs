using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TextLinkTest : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text effectText;

    void Start() {
        effectText.text = "この効果は<link=\"LINK\"><color=#0089FF>仮</color></link>である。";
    }

    void Update() {
        
    }

    public void OnPointerClick(PointerEventData eventData) {
        // クリックした場所にリンクがあるか調べる
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            effectText,
            eventData.position,
            eventData.pressEventCamera
        );

        // リンクがなければ終了
        if (linkIndex == -1)
            return;

        // クリックされたリンクの情報を取得
        TMP_LinkInfo linkInfo = effectText.textInfo.linkInfo[linkIndex];

        // リンクIDとリンク文字を取得
        string linkId = linkInfo.GetLinkID();
        string linkText = linkInfo.GetLinkText();

        Debug.Log("クリックされたリンクID: " + linkId);
        Debug.Log("クリックされた文字: " + linkText);

        // リンクIDによって処理を分ける
        switch (linkId) {
            case "LINK":
                effectText.text = "リンク成功";
                break;
        }
    }
}
