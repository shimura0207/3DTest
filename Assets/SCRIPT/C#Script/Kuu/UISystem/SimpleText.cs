/*
 * @file    SimpleText.cs
 * @autor   Kuu
 */
using Mirror.Examples.CCU;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleText : MonoBehaviour {
    [SerializeField] private Canvas cardInfo = null;

    // 名前テキスト
    [SerializeField] private TMP_Text nameText = null;

    // カードタイプテキスト
    [SerializeField] private TMP_Text cardTypeText = null;

    // 種族テキスト
    [SerializeField] private TMP_Text groupText = null;

    // ステータステキスト
    [SerializeField] private TMP_Text statusText = null;

    // 効果テキスト
    [SerializeField] private TMP_Text abilityText = null;

    // カードタイプテキスト保存用
    private string type = null;

    // Start is called before the first frame update
    void Start() {
        cardInfo.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        var data = UIManager.instance.cardData;
        // データがなければ非表示
        if (data == null) {
            cardInfo.enabled = false;
            return;
        }

        // 各データをテキストにする
        cardInfo.enabled = true;
        nameText.text = data.name;
        //cardTypeText.text = TypeIDChangeText(data.GetType());
    }

    /// <summary>
    /// タイプIDをテキストに変換
    /// </summary>
    //private string TypeIDChangeText(System.Type cardType) {
    //    int getType = cardType;
    //    if (cardType == ) {
    //        type = "モンスター";
    //    }
    //    else if (cardType == 1) {
    //        type = "スペル";
    //    }
    //    return type;
    //}
}
