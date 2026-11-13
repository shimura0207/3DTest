/*
 * @file    CardDatailMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailMenu : MenuWindowBase {
    // 受け取ったカードID
    private int cardID = -1;
    // 消費するポイント
    private int cardPointNum = 0;

    // カード名を表示するテキスト
    [SerializeField] private TMP_Text cardNameText;
    // カードタイプを表示するテキスト
    [SerializeField] private TMP_Text cardTypeText;
    // カードの種族を表示するテキスト
    [SerializeField] private TMP_Text cardRaceText;
    // カードステータスを表示するテキスト
    [SerializeField] private TMP_Text cardStatusText;
    // カードの対応役を表示するテキスト
    [SerializeField] private TMP_Text cardSuportRoleText;
    // カードの効果を表示するテキスト
    [SerializeField] private TMP_Text cardAbilittyText;
    // 閉じるようのボタン
    [SerializeField] private Button closeButton;
    // カードの画像を取得
    [SerializeField] private Image cardImage;

    // 対応役の画像
    [SerializeField] private Image missImage;   // ミス
    [SerializeField] private Image replayImage; // リプレイ
    [SerializeField] private Image cherryImage; // チェリー
    [SerializeField] private Image bellImage;   // ベル
    [SerializeField] private Image suikaImage;  // スイカ

    // カード生成関連UI
    [SerializeField] private Button createButton;   // カード生成ボタン
    [SerializeField] private TMP_Text pullPointText;    // 消費ポイントテキスト
    [SerializeField] private TMP_Text havePointText;    // 現在の所持ポイント
    [SerializeField] private TMP_Text haveCardNumText;  // 現在のカードの所持数


    // ボタンが選択されるまで待機するための完了通知
    private UniTaskCompletionSource<bool> completionSource;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // 処理登録
        closeButton.onClick.RemoveListener(OnCloseButton);
        closeButton.onClick.AddListener(OnCloseButton);
        createButton.onClick.RemoveListener(OnCreateButton);
        createButton.onClick.AddListener(OnCreateButton);
    }

    /// <summary>
    /// メニューが表示された時の処理
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async UniTask Open(int id) {

        // 表示するカードIDを設定する
        cardID = id;

        // テキストを初期化する
        AllTextSetup();

        // マスターデータからカード情報を取得する
        var cardData = CardDataMasterUtility.GetCardMaster(id);

        // カード情報を画面に反映する
        AllTextUpdate(cardData);

        // 対応役画像を更新する
        SuportRoleImageUpdate(cardData.suportRoleID);

        // カード画像を取得する
        Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(id);

        // カード画像を設定する
        if (cardImage != null) {
            cardImage.sprite = cardSprite;
            cardImage.preserveAspect = true;

            // 所持枚数を取得する
            int ownedCount = GetOwnedCardCount(id);

            // 所持状態に応じて画像の色を変更する
            cardImage.color = ownedCount > 0 ? Color.white : Color.gray;

            // 3枚以上所持している場合は生成ボタンを無効化する
            createButton.interactable = ownedCount < 3;
        }

        // メニューを表示する
        await base.Open();

        // ボタンが押されるまで待機する
        completionSource = new UniTaskCompletionSource<bool>();
        await completionSource.Task;

        // メニューを閉じる
        await Close();

    }

    /// <summary>
    /// カードテキスト全体の初期化
    /// </summary>
    private void AllTextSetup() {
        cardNameText.text = string.Empty;
        cardTypeText.text = string.Empty;
        cardRaceText.text = string.Empty;
        cardStatusText.text = string.Empty;
        cardSuportRoleText.text = string.Empty;
        cardAbilittyText.text = string.Empty;
        pullPointText.text = string.Empty;
        havePointText.text = string.Empty;
        haveCardNumText.text = string.Empty;
    }

    /// <summary>
    /// テキストの更新
    /// </summary>
    /// <param name="cardData"></param>
    private void AllTextUpdate(Entity_CardData.Param cardData) {
        cardNameText.text = cardData.name;
        cardTypeText.text = cardData.CardType == 0 ? "モンスター" : "スペル";
        cardRaceText.text = RaceIDChangeText(cardData.FirstRaceID, cardData.SecondRaceID, cardData.ThildRaceID);
        cardStatusText.text = $"ATK : {cardData.ATK} / HP : {cardData.HP} / SIZE : {cardData.SIZE}";
        cardSuportRoleText.text = SuportRoleChangeText(cardData.suportRoleID);
        cardAbilittyText.text = cardData.ability;

        // テキストの更新
        pullPointText.text = CardCreatePoint(cardData);
        havePointText.text = HavePoint();
        haveCardNumText.text = HaveCardNum(cardData.cardID);
    }

    /// <summary>
    /// 種族IDをテキストに変換
    /// </summary>
    /// <param name="cardRace"></param>
    /// <returns></returns>
    private string RaceIDChangeText(int cardRace1, int cardRace2, int cardRace3) {
        // 種族IDと表示名の対応表
        Dictionary<CardRaceID, string> raceNameList = new Dictionary<CardRaceID, string>() {
        { CardRaceID.Robo, "ロボ" },
        { CardRaceID.Warrior, "戦士" },
        { CardRaceID.Human, "人" } };

        // 種族IDを配列にまとめる
        int[] raceIDs = { cardRace1, cardRace2, cardRace3 };

        // 表示する種族名を保存
        List<string> raceNames = new List<string>();

        // 種族数分繰り返す
        foreach (int raceID in raceIDs) {

            // IDをenumに変換
            CardRaceID race = (CardRaceID)raceID;

            // 対応する種族名が存在する場合のみ追加
            if (raceNameList.TryGetValue(race, out string raceName)) {
                raceNames.Add(raceName);
            }
        }

        // 種族名を連結して返す
        return string.Join("・", raceNames);
    }

    /// <summary>
    /// 対応役IDをテキストに変換
    /// </summary>
    private string SuportRoleChangeText(int roleID) {

        // 対応役と表示名の対応表
        Dictionary<CardSuportRole, string> roleNameList = new Dictionary<CardSuportRole, string>() {
        { CardSuportRole.NULL, "" },
        { CardSuportRole.Miss, "ミス" },
        { CardSuportRole.Replay, "リプレイ" },
        { CardSuportRole.Cherry, "チェリー" },
        { CardSuportRole.Bell, "ベル" },
        { CardSuportRole.Suika, "スイカ" },
        { CardSuportRole.ALL, "全て" },
        { CardSuportRole.Cherry_Suika, "チェリー・スイカ" }};

        // int型をenumに変換
        CardSuportRole role = (CardSuportRole)roleID;

        // 対応する表示名が存在すれば返す
        if (roleNameList.TryGetValue(role, out string roleName)) {
            return roleName;
        }

        // 該当する対応役がない場合
        return string.Empty;
    }

    /// <summary>
    /// ボタンが押された時の処理
    /// </summary>
    private void OnCloseButton() {
        // 待機処理を終了
        completionSource?.TrySetResult(false);
    }

    /// <summary>
    /// カード生成ボタンが押された時の処理
    /// </summary>
    private void OnCreateButton() {
        // カード生成を行う
        SaveManager.Instance.AcquireCard(cardID);

        // UIを更新する
        CreateUIUpdate();

    }



    /// <summary>
    /// 対応役の画像を初期化する
    /// </summary>
    private void AllSuportRoleImageSetup() {

        // すべての対応役画像をグレーにする
        Image[] roleImages = {
        missImage,
        replayImage,
        cherryImage,
        bellImage,
        suikaImage
    };

        foreach (Image image in roleImages) {

            // 画像が設定されている場合のみグレーにする
            if (image != null) {
                image.color = Color.gray;
            }
        }
    }

    /// <summary>
    /// 対応役IDに応じて画像の色を変更する
    /// </summary>
    /// <param name="roleID">対応役ID</param>
    private void SuportRoleImageUpdate(int roleID) {

        // 最初にすべての画像をグレーにする
        AllSuportRoleImageSetup();

        // 対応役IDをEnumに変換
        CardSuportRole role = (CardSuportRole)roleID;

        // 対応役に応じて該当画像を通常色にする
        switch (role) {

            case CardSuportRole.Miss:
                missImage.color = Color.white;
                break;

            case CardSuportRole.Replay:
                replayImage.color = Color.white;
                break;

            case CardSuportRole.Cherry:
                cherryImage.color = Color.white;
                break;

            case CardSuportRole.Bell:
                bellImage.color = Color.white;
                break;

            case CardSuportRole.Suika:
                suikaImage.color = Color.white;
                break;

            case CardSuportRole.ALL:

                // すべての対応役画像を通常色にする
                missImage.color = Color.white;
                replayImage.color = Color.white;
                cherryImage.color = Color.white;
                bellImage.color = Color.white;
                suikaImage.color = Color.white;
                break;

            case CardSuportRole.Cherry_Suika:

                // チェリーとスイカのみ通常色にする
                cherryImage.color = Color.white;
                suikaImage.color = Color.white;
                break;
        }
    }

    /// <summary>
    /// カード生成のポイントを決定する
    /// </summary>
    /// <param name="cardData"></param>
    private string CardCreatePoint(Entity_CardData.Param cardData) {

        CardRearity realityID = (CardRearity)cardData.rarityID;
        switch (realityID) {
            case CardRearity.N:
                cardPointNum = 5;
                break;
            case CardRearity.R:
                cardPointNum = 10;
                break;
            case CardRearity.SR:
                cardPointNum = 50;
                break;
            case CardRearity.LR:
                cardPointNum = 100;
                break;
        }

        // 値をstring型で返す
        return cardPointNum.ToString();
    }

    /// <summary>
    /// 現在所持しているポイント数
    /// </summary>
    /// <returns></returns>
    private string HavePoint() {
        // プレイヤーのセーブデータを取得
        var player = SaveManager.Instance.Load();
        // セーブデータからポイントを取得
        int point = player.ownedPoint;
        return point.ToString();
    }

    /// <summary>
    /// ID番の所持しているカード数の取得
    /// </summary>
    /// <param name="cardID"></param>
    /// <returns></returns>
    private string HaveCardNum(int cardID) {
        // 所持カードデータを読み込む
        PossessionCardSaveData cardSaveData = SaveManager.Instance.LoadCards();
        // 同じカードIDの取得枚数を確認する
        int ownerCount = 0;
        // 取得したカードとセーブデータを使用して走査
        foreach (int id in cardSaveData.cardID) {
            // 入手対象と同じカードIDなら枚数を増やす
            if (id == cardID) {
                ownerCount++;
            }
        }

        // 登録枚数 / 最大枚数を表示する
        return $"{ownerCount}/{3}";

    }

    /// <summary>
    /// カード生成関連UIを更新する
    /// </summary>
    private void CreateUIUpdate() {

        // 現在の所持ポイントを更新
        havePointText.text = HavePoint();

        // 現在のカード所持枚数を更新
        haveCardNumText.text = HaveCardNum(cardID);

        // カードを所持済みの色に変更
        cardImage.color = Color.white;
    }

    /// <summary>
    /// 指定したカードの所持枚数を取得する
    /// </summary>
    private int GetOwnedCardCount(int targetCardID) {

        // 所持カードデータを取得する
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // セーブデータが存在しない場合は0枚を返す
        if (saveData == null || saveData.cardID == null) {
            return 0;
        }

        // 対象カードの所持枚数を数える
        int count = 0;

        foreach (int id in saveData.cardID) {

            // カードIDが一致した場合に枚数を増やす
            if (id == targetCardID) {
                count++;
            }
        }

        // 所持枚数を返す
        return count;
    }

    /// <summary>
    /// 閉じられた時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        await base.Close();
        // カードの初期化
        cardID = -1;
    }
}
