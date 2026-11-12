/*
 * @file    CardDatailMenu
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailMenu : MenuWindowBase {
    // 受け取ったカードID
    private int cardID = -1;

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
    [SerializeField] private Text pullPointText;    // 消費ポイントテキスト
    [SerializeField] private Text havePointText;    // 現在の所持ポイント
    [SerializeField] private Text haveCardNumText;  // 現在のカードの所持数


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
    }

    /// <summary>
    /// メニューが表示された時の処理
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async UniTask Open(int id) {
        // メンバ変数でIDを受け取る
        cardID = id;
        // テキストの初期化を行う
        AllTextSetup();
        // ID指定のカードの取得を行う
        var cardData = CardDataMasterUtility.GetCardMaster(id);
        // すべてのテキストを更新する
        AllTextUpdate(cardData);
        // 対応役画像の色を更新
        SuportRoleImageUpdate(cardData.suportRoleID);
        // カード画像を取得
        Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(id);
        // カード画像を設定する
        if (cardImage != null) {

            // カード画像を設定する
            cardImage.sprite = cardSprite;

            // カード画像の縦横比を維持する
            cardImage.preserveAspect = true;

            // 所持カードをセーブデータから取得
            PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

            // 指定IDのカードを所持しているか確認
            bool isOwned = saveData != null &&
                           saveData.cardID != null &&
                           saveData.cardID.Contains(id);

            // 所持していなければ灰色、所持していれば通常色
            cardImage.color = isOwned ? Color.white : Color.gray;
        }
        await base.Open();
        // ボタンが押されるまで待機
        completionSource = new UniTaskCompletionSource<bool>();
        // ボタンが押されるまでウィンドウを開いたままにする
        await completionSource.Task;

        // ボタンが押されたらウィンドウを閉じる
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
    }

    private void AllTextUpdate(Entity_CardData.Param cardData) {
        cardNameText.text = cardData.name;
        cardTypeText.text = cardData.CardType == 0 ? "モンスター" : "スペル";
        cardRaceText.text = RaceIDChangeText(cardData.FirstRaceID, cardData.SecondRaceID, cardData.ThildRaceID);
        cardStatusText.text = $"ATK : {cardData.ATK} / HP : {cardData.HP} / SIZE : {cardData.SIZE}";
        cardSuportRoleText.text = SuportRoleChangeText(cardData.suportRoleID);
        cardAbilittyText.text = cardData.ability;
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
    /// 閉じられた時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        await base.Close();
        // カードの初期化
        cardID = -1;
    }
}
