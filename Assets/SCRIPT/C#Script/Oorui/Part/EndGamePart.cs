/*
 *  @file   EndGamePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using Mirror.BouncyCastle.Math.EC.Multiplier;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルト画面
/// </summary>
public class EndGamePart : PartBase {

    // 自分視点の対戦結果
    private BattleState battleResult = BattleState.None;

    // 選択された遷移先
    private ResultSelectMenu selectMenu = ResultSelectMenu.None;

    // リザルト画面（Canvas1）
    [SerializeField] private GameObject resultCanvas;

    // 遷移先選択画面（Canvas2）
    [SerializeField] private GameObject selectCanvas;

    // 勝利時に表示する画像
    [SerializeField] private GameObject winImage;
    // 勝利時に表示するテキスト
    [SerializeField] private TMP_Text resultText;

    // 敗北時に表示する画像
    [SerializeField] private GameObject loseImage;
    // メインメニューに戻るボタン
    [SerializeField] private Button returnMenuButton;

    // 再マッチングボタン
    [SerializeField] private Button matchingButton;

    // デッキ変更ボタン
    [SerializeField] private Button deckChangeButton;

    // 報酬を付与済みかどうか
    private bool isRewardGranted = false;
    // 獲得カード表示用Prefab
    [SerializeField] private PossessionCardItem rewardCardPrefab;

    // 獲得カードを配置する親オブジェクト
    [SerializeField] private Transform rewardCardParent;

    // 獲得したカード情報
    private Dictionary<int, int> rewardCards = new Dictionary<int, int>();

    // 生成したカードオブジェクトを管理
    private List<PossessionCardItem> rewardCardItems = new List<PossessionCardItem>();

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {

        // 基底側の処理
        await base.Initialize();

        // Canvasを非表示にする
        resultCanvas.SetActive(false);
        selectCanvas.SetActive(false);

        // 勝敗画像を非表示にする
        winImage.SetActive(false);
        loseImage.SetActive(false);
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    public override async UniTask Setup() {

        // 基底側の処理
        await base.Setup();

        // Canvasを非表示にする
        resultCanvas.SetActive(false);
        selectCanvas.SetActive(false);

        // ボタンのイベントを登録する
        // 重複登録を防ぐため、先に解除する
        returnMenuButton.onClick.RemoveListener(OnReturnMenuSelected);
        matchingButton.onClick.RemoveListener(OnMatchingSelected);
        deckChangeButton.onClick.RemoveListener(OnDeckChangeSelected);

        returnMenuButton.onClick.AddListener(OnReturnMenuSelected);
        matchingButton.onClick.AddListener(OnMatchingSelected);
        deckChangeButton.onClick.AddListener(OnDeckChangeSelected);

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メインメニュー選択
    /// </summary>
    private void OnReturnMenuSelected() {
        // 遷移先をメインメニューに設定
        selectMenu = ResultSelectMenu.MainMenu;
    }

    /// <summary>
    /// 再マッチング選択
    /// </summary>
    private void OnMatchingSelected() {
        // 遷移先をマッチング画面に設定
        selectMenu = ResultSelectMenu.Matching;
    }

    /// <summary>
    /// デッキ変更選択
    /// </summary>
    private void OnDeckChangeSelected() {
        // 遷移先をデッキ編成画面に設定
        selectMenu = ResultSelectMenu.DeckBuilding;
    }

    /// <summary>
    /// メイン処理
    /// </summary>
    public override async UniTask Execute() {

        // 選択状態を初期化
        selectMenu = ResultSelectMenu.None;

        // 生成済みのカード表示を削除
        ClearRewardCards();

        // 報酬がまだ付与されていない場合
        if (!isRewardGranted) {

            // 報酬付与済みにする
            isRewardGranted = true;

            // 対戦結果に応じて報酬を付与し、獲得カード情報を取得
            rewardCards = BattleRewardManager.GrantRewards(battleResult);
        }

        // 獲得カードを画面に表示する
        DisplayRewardCards();

        // Canvas1を表示する
        resultCanvas.SetActive(true);

        // Canvas2を非表示にする
        selectCanvas.SetActive(false);

        // 勝敗に応じた画像を表示
        BattleResult(battleResult);

        // 数秒待つ
        await UniTask.Delay(1500);

        // Canvas1を非表示にする
        resultCanvas.SetActive(false);

        // Canvas2を表示する
        selectCanvas.SetActive(true);

        // ボタンが選択されるまで待機
        await UniTask.WaitUntil(() => selectMenu != ResultSelectMenu.None);

        // 重複カードをポイントに変換する
        SaveManager.Instance.ConvertExcessCardsToPoints();

        // 選択されたボタンに応じて処理を切り替える
        switch (selectMenu) {

            case ResultSelectMenu.MainMenu:
                // マッチング中であれば終了処理
                CancelMatching();
                // メインメニューに遷移
                await PartManager.Instance.TransitionPart(GamePart.MainMenu);
                break;

            case ResultSelectMenu.Matching:
                // マッチング中であれば終了処理
                CancelMatching();
                // マッチング画面に遷移
                await PartManager.Instance.TransitionPart(GamePart.Matchmaking);
                break;

            case ResultSelectMenu.DeckBuilding:
                // マッチング中であれば終了処理
                CancelMatching();
                // デッキ編成画面に遷移
                await PartManager.Instance.TransitionPart(GamePart.DeckBuild);
                break;
        }
    }

    /// <summary>
    /// マッチング終了処理
    /// </summary>
    private void CancelMatching() {

        // ネットワーク管理クラスが存在する場合
        if (MatchingNetMatch.instance != null &&
            CustomNetworkManager.instance != null &&
            CustomNetworkManager.instance.IsMatchStarted) {

            // マッチングを終了する
            MatchingNetMatch.instance.CancelRandomMatch();
        }
    }

    /// <summary>
    /// 対戦結果を設定する
    /// </summary>
    public void SetBattleResult(BattleState result) {

        // 対戦結果を保存
        battleResult = result;

        // 新しい対戦の報酬付与状態を初期化
        isRewardGranted = false;
    }

    /// <summary>
    /// 対戦結果を表示する
    /// </summary>
    public void BattleResult(BattleState result) {

        // 最初に両方の画像を非表示にする
        winImage.SetActive(false);
        loseImage.SetActive(false);

        // 対戦結果に応じて表示を切り替える
        switch (result) {

            case BattleState.Win:

                // 勝利画像を表示
                winImage.SetActive(true);
                resultText.text = "WIN";
                Debug.Log("Win");
                break;

            case BattleState.Lose:

                // 敗北画像を表示
                loseImage.SetActive(true);
                resultText.text = "LOSE";
                Debug.Log("Lose");
                break;
        }
    }

    /// <summary>
    /// 獲得したカードを画面に表示する
    /// </summary>
    private void DisplayRewardCards() {

        // 獲得カードが存在しない場合は終了
        if (rewardCards == null || rewardCards.Count == 0) return;

        // 報酬カードの種類ごとに処理する
        foreach (var reward in rewardCards) {

            // カードIDを取得する
            int cardID = reward.Key;

            // 同じカードの獲得枚数を取得する
            int count = reward.Value;

            // カード画像を取得する
            Sprite cardSprite = ScriptableObjectManager.instance.GetCardImage(cardID);

            // カード画像が存在しない場合はスキップする
            if (cardSprite == null) continue;

            // 獲得枚数分だけカードを生成する
            for (int i = 0; i < count; i++) {

                // カード表示用Prefabを生成する
                PossessionCardItem item = Instantiate(rewardCardPrefab, rewardCardParent);

                // カードを1枚として設定する
                item.SetCard(cardID, cardSprite, 1);

                // 生成したカードを管理リストに追加する
                rewardCardItems.Add(item);
            }
        }
    }



    /// <summary>
    /// 生成した獲得カードを削除する
    /// </summary>
    private void ClearRewardCards() {

        // 生成したカードをすべて削除
        foreach (PossessionCardItem cardItem in rewardCardItems) {
            // オブジェクトが存在する場合
            if (cardItem != null) {
                // カードオブジェクトを削除
                Destroy(cardItem.gameObject);
            }
        }

        // 管理リストを初期化
        rewardCardItems.Clear();
    }
}