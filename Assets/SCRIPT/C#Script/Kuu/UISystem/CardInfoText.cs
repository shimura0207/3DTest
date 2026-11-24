/*
 * @file    CardInfo.cs
 * @autor   Kuu
 */
using Cysharp.Threading.Tasks;
using Mirror.Examples.CCU;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInfoText : MonoBehaviour {
    // 簡易カードテキスト
    [SerializeField] private Canvas simpleText = null;

    // 詳細カードテキスト
    [SerializeField] private Canvas detaileText = null;

    [SerializeField] private Canvas status = null;

    // ログウィンドウ
    [SerializeField] private GameObject logWindow = null;

    // ログの名前
    [SerializeField] private TMP_Text logName = null;

    // ログの効果
    [SerializeField] private TMP_Text logAbility = null;

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

    // 対応役テキスト
    [SerializeField] private TMP_Text koyakuText = null;

    // カード画像
    [SerializeField] private Image cardImage = null;

    // 対応役アイコン
    [SerializeField] private DisplayKoyaku displayKoyaku = null;



    // RG:ISUKA Lv.2の名前
    private const string ISUKA_LV2_NAME = "RG:ISUKA LV2";
    // RG:REACHE Lv.2の名前
    private const string REACHE_LV2_NAME = "RG:REACHE Lv.2";
    // RG:REACHE Lv.3の名前
    private const string REACHE_LV3_NAME = "RG:REACHE Lv.3";

    // データ保存用
    private Entity_CardData.Param data = null;

    // 種族enum
    private CardRaceID cardRaceID = CardRaceID.None;

    // 対応役enum
    private CardSuportRole koyakuID = CardSuportRole.None;

    // カードタイプテキスト保存用
    private string type = null;

    // 種族テキスト保存用
    private string races = null;

    // 対応役テキスト保存用
    private string suportRole = null;

    // マウスの右クリックと左クリックを判別する用
    private bool mouseButton = true;

    // ウィンドウに触れているか判定を保存する用
    private bool touchWindow = false;

    // ログウィンドウを表示する秒数
    private const int DISPLAY_TIME = 3;


    // Start is called before the first frame update
    void Start() {
        simpleText.enabled = false;
        detaileText.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        data = UIManager.instance.cardData;
        
        // データがなければ非表示
        if (data == null) {
            simpleText.enabled = false;
            detaileText.enabled = false;
            return;
        }

        mouseButton = UIManager.instance.mouseButton;
        touchWindow = UIManager.instance.touchWindow;
        // マウスボタン左右判定
        if (mouseButton) {
            // ウィンドウに触っていないなら簡易ウィンドウ表示
            if (!touchWindow) {
                simpleText.enabled = true;
                detaileText.enabled = false;
            }
        }
        else {
            simpleText.enabled = false;
            detaileText.enabled = true;
            }

            // 各データをテキストにする
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
            if (!mouseButton) {
                status.enabled = true;
                HPText.text = "なし";
                ATKText.text = "";
                SIZEText.text = "";
            }
            else {
                if (!touchWindow) {
                    status.enabled = false;
                }
            }
        }
        abilityText.text = data.ability;
        displayKoyaku.KoyakuDisplayer(data.suportRoleID);
        // 詳細ウィンドウのみ表示
        if (!mouseButton) {
            // 対応役テキスト
            koyakuText.enabled = true;
            koyakuText.text = SuportRoleIDChangeText(data.suportRoleID);
            // カード画像
            cardImage.enabled = true;
            cardImage.sprite = ScriptableObjectManager.instance.GetCardImage(data.cardID);
        } else {
            if (!touchWindow) {
                koyakuText.enabled = false;
                cardImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// リンク判定
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData) {
        // クリックした場所にリンクがあるか調べる
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            abilityText,
            eventData.position,
            eventData.pressEventCamera
        );

        // リンクがなければ終了
        if (linkIndex == -1)
            return;

        // クリックされたリンクの情報を取得
        TMP_LinkInfo linkInfo = abilityText.textInfo.linkInfo[linkIndex];

        // リンクIDとリンク文字を取得
        string linkId = linkInfo.GetLinkID();
        string linkText = linkInfo.GetLinkText();

        Debug.Log("クリックされたリンクID: " + linkId);
        Debug.Log("クリックされた文字: " + linkText);

        // 名前からID取得用
        int cardID;
        // リンクIDによって処理を分ける
        switch (linkId) {
            // RG:ISUKA Lv.2の詳細表示
            case "ISUKALV2":
                cardID = CardDataMasterUtility.GetCardNameData(ISUKA_LV2_NAME).cardID;
                data = CardDataMasterUtility.GetCardMaster(cardID);
                break;
            // RG:REACHE Lv.2の詳細表示
            case "REACHELV2":
                cardID = CardDataMasterUtility.GetCardNameData(REACHE_LV2_NAME).cardID;
                data = CardDataMasterUtility.GetCardMaster(105);
                break;
            // RG:REACHE Lv.3の詳細表示
            case "REACHELV3":
                cardID = CardDataMasterUtility.GetCardNameData(REACHE_LV3_NAME).cardID;
                data = CardDataMasterUtility.GetCardMaster(106);
                break;
        }
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

    /// <summary>
    /// 対応役IDをテキストに変換
    /// </summary>
    /// <param name="suportRoleID"></param>
    /// <returns></returns>
    private string SuportRoleIDChangeText(int suportRoleID) {
            // IDに応じてenumを参照する
            koyakuID = (CardSuportRole)suportRoleID;

        // それぞれをテキストに変換する
        switch (koyakuID) {
            case CardSuportRole.NULL:
                suportRole = "なし";
                break;
            case CardSuportRole.Miss:
                suportRole = "ハズレ";
                break;
            case CardSuportRole.Replay:
                suportRole = "リプレイ";
                break;
            case CardSuportRole.Cherry:
                suportRole = "チェリー";
                break;
            case CardSuportRole.Bell:
                suportRole = "ベル";
                break;
            case CardSuportRole.Suika:
                suportRole = "スイカ";
                break;
            case CardSuportRole.ALL:
                suportRole = "全て";
                break;
            case CardSuportRole.Cherry_Suika:
                suportRole = "チェリー・スイカ";
                break;
            default:
                suportRole = "";
                break;
        }
        return suportRole;
    }

    /// <summary>
    /// ログを表示する
    /// </summary>
    /// <returns></returns>
    public void DisplayLog(int cardID) {
        // IDがなければ表示しない
        if (cardID <= 0) {
            return;
        }

        // IDからステータスを取得
        var logData = CardDataMasterUtility.GetCardMaster(cardID);

        // 名前と効果をそれぞれ入れる
        logName.text = logData.name;
        logAbility.text = logData.ability;
        // 表示する
        logWindow.SetActive(true);
    }

    /// <summary>
    /// ログを非表示にする
    /// </summary>
    public void HideLog() {
        logWindow.SetActive(false);
    }
}