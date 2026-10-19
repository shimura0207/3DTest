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

    // 種族
    private CardRaceID cardRaceID = CardRaceID.None;

    // カードタイプテキスト保存用
    private string type = null;

    // 種族テキスト保存用
    private string race = null;



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
        cardTypeText.text = TypeIDChangeText(data.CardType);
        //groupText.text =
    }

    /// <summary>
    /// タイプIDをテキストに変換
    /// </summary>
    private string TypeIDChangeText(int cardType) {
        if (cardType == 0) {
            type = "モンスター";
        }
        else if (cardType == 1) {
            type = "スペル";
        }
        return type;
    }

    /// <summary>
    /// 種族IDをテキストに変換
    /// </summary>
    /// <param name="cardRace"></param>
    /// <returns></returns>
    private string RaceIDChangeText(int cardRace) {
        cardRaceID = (CardRaceID)cardRace;
        switch (cardRaceID) {
            case CardRaceID.Robo:
                race = "ロボ";
                break;
            case CardRaceID.Warrior:
                race = "戦士";
                break;
            case CardRaceID.Human:
                race = "人";
                break;

        }
        return race;
    }
}
