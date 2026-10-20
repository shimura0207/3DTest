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

    [SerializeField] private Canvas status = null;

    // 名前テキスト
    [SerializeField] private TMP_Text nameText = null;

    // カードタイプテキスト
    [SerializeField] private TMP_Text cardTypeText = null;

    // 種族テキスト
    [SerializeField] private TMP_Text raceText = null;

    // HPテキスト
    [SerializeField] private TMP_Text HPText = null;

    // ATKテキスト
    [SerializeField] private TMP_Text ATKText = null;

    // SIZEテキスト
    [SerializeField] private TMP_Text SIZEText = null;

    // 効果テキスト
    [SerializeField] private TMP_Text abilityText = null;

    // 種族enum
    private CardRaceID cardRaceID = CardRaceID.None;

    // カードタイプテキスト保存用
    private string type = null;

    // 種族テキスト保存用
    private string races = null;

    private DisplayKoyaku instance = null;

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
        raceText.text = RaceIDChangeText(data.FirstRaceID, data.SecondRaceID, data.ThildRaceID);
        // モンスター以外ステータスは表示しない
        if (data.CardType == 0) {
            status.enabled = true;
            HPText.text = "HP:" + data.HP;
            ATKText.text = "ATK:" + data.ATK;
            SIZEText.text = "SIZE:" + data.SIZE;
        }
        else {
            status.enabled = false;
        }
        abilityText.text = data.ability;
        DisplayKoyaku.instance.KoyakuDisplayer(data.suportRoleID);
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
    private string RaceIDChangeText(int cardRace1, int cardRace2, int cardRace3) {
        // 各種族保存用
        string[] raceNum = new string[3];
        // 最大種族数分繰り返す
        for (int i = 0; i < 3; i++) {
            int cardRace = 0;
            // 何番目の種族か判別させる
            switch (i) {
                case 0:
                    cardRace = cardRace1;
                    break;
                case 1:
                    cardRace = cardRace2;
                    break;
                case 2:
                    cardRace = cardRace3;
                    break;
                default:
                    cardRace = -1;
                    break;
            }
            // IDに応じてenumを参照する
            cardRaceID = (CardRaceID)cardRace;
            // それぞれをテキストに変換する
            switch (cardRaceID) {
                case CardRaceID.Robo:
                    raceNum[i] = "ロボ";
                    break;
                case CardRaceID.Warrior:
                    raceNum[i] = "戦士";
                    break;
                case CardRaceID.Human:
                    raceNum[i] = "人";
                    break;
                default:
                    raceNum[i] = null;
                    break;
            }
            // 各種族を合わせていく
            races = raceNum[0];
            if (raceNum[1] != null) {
                races += "・" + raceNum[1];
                if (raceNum[2] != null) {
                    races += "・" + raceNum[2];
                }
            }
        }
        return races;
    }
}
