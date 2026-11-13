/*
 * @file    CardStatusUI.cs
 * @autor   Kuu
 */
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // サイズアイコン
    [SerializeField] private Image sizeImage = null;

    // 攻撃力アイコン
    [SerializeField] private Image atkImage = null;

    // HPアイコン
    [SerializeField] private Image hpImage = null;

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

        // スペルか裏向きカードなら非表示
        if (cardObject.cardType != CardType.Monster || 
            cardObject.SpriteRenderer.sprite == ScriptableObjectManager.instance.GetCardImage(0)) {
            statusCanvas.enabled = false;
            return;
        } else {
            statusCanvas.enabled = true;
        }

        // それぞれのステータスをテキストで表示する
        sizeText.text = "" + cardObject.currentStatus[MonsterStatus.SIZE];
        atkText.text = "" + cardObject.currentStatus[MonsterStatus.ATK];
        hpText.text = "" + cardObject.currentStatus[MonsterStatus.HP];

        // フィールドにいるときは大きくする
        if (cardObject.laneID != -1) {
            sizeImage.transform.localScale = new Vector3(3f, 3f, 3f);
            atkImage.transform.localScale = new Vector3(5f, 5f, 5f);
            hpImage.transform.localScale = new Vector3(4f, 4f, 4f);

            // 位置を調整する
            statusCanvas.transform.localPosition = new Vector3(-1f, 0.0f, 0.0f);
            //sizeImage.transform.position = new Vector3(3f, 3f, 3f);
            //atkImage.transform.localPosition = new Vector3(-6.25f, -2.79f, 1.0f);
            //hpImage.transform.localPosition = new Vector3(-5.94f, -4.77f, 1.0f);
        }
    }
}
