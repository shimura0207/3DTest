/*
 * @file    CardStatusUI.cs
 * @autor   Kuu
 */
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardStatusUI : MonoBehaviour
{
    // 親のCardObject
    [SerializeField] private CardObject cardObject = null;

    // ステータスCanvas
    [SerializeField] private Canvas statusCanvas = null;

    // サイズテキスト
    [SerializeField] private TMP_Text sizeText = null;

    // 攻撃力テキスト
    [SerializeField] private TMP_Text atkText = null;

    // HPテキスト
    [SerializeField] private TMP_Text hpText = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // データがなければ非表示
        if (cardObject == null) {
            statusCanvas.enabled = false;
            return;
        }

        // 自分のモンスターカードでなければ非表示
        if (!(cardObject.cardType == CardType.Monster) || 
            cardObject.SpriteRenderer.sprite == ScriptableObjectManager.instance.GetCardImage(0)) {
            statusCanvas.enabled = false;
            return;
        }

        // それぞれのステータスをテキストで表示する
        sizeText.text = "" + cardObject.currentStatus[MonsterStatus.SIZE];
        atkText.text = "" + cardObject.currentStatus[MonsterStatus.ATK];
        hpText.text = "" + cardObject.currentStatus[MonsterStatus.HP];
    }
}
