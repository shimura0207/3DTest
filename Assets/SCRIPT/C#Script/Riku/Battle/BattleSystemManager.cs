/*
 * @file    BattleSystemManager.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using Edgegap;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

using static GameConst;

/// <summary>
/// バトルシステム管理クラス
/// </summary>
public class BattleSystemManager : MonoBehaviour {
    // 自身への参照
    public static BattleSystemManager instance { get; private set; } = null;
    // 先攻のプレイヤー
    private PlayerType firstPlayer = PlayerType.None;
    // 現在のターンプレイヤー
    public PlayerType turnPlayer { get; private set; } = PlayerType.None;
    // 各フェイズのリスト
    [SerializeField]
    private PhaseBase[] phaseList = null;
    // 現在のフェイズ
    public BattlePhase currentPhase { get; private set; } = BattlePhase.None;
    // 対戦の状態
    public BattleState battleState { get; private set; } = BattleState.None;
    // プレイヤーの体力
    public Dictionary<PlayerType, int> playerHP { get; private set; } = null;
    // プレイヤーのサイズ上限
    public Dictionary<PlayerType, int> playerSizeLimit { get; private set; } = null;
    // 現在のターン
    private int currentTurn = -1;
    // スペルを唱えた回数
    public int spellCastCount { get; private set; } = -1;
    // バチスロ移行フラグ
    private bool pachisuroGo = false;
    // パチスロボタンが有効かどうか
    public bool pachisuroGoActive { get; private set; } = false;
    // ターンエンドフラグ
    private bool turnEnd = false;
    // ターンエンドボタンが有効かどうか
    public bool turnEndActive { get; private set; } = false;

    // チュートリアルの矢印のオリジナル
    [SerializeField]
    private GameObject tutorialArrowPrefab = null;
    // チュートリアルの矢印の親
    [SerializeField]
    private Transform tutorialArrowRoot = null;
    // チュートリアルの矢印
    private ArrowCreate tutorialArrow = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        instance = this;

        playerHP = new Dictionary<PlayerType, int>();
        playerSizeLimit = new Dictionary<PlayerType, int>();
        firstPlayer = PlayerType.None;
        turnPlayer = PlayerType.None;
        currentPhase = BattlePhase.None;
        battleState = BattleState.None;
        currentTurn = -1;
        spellCastCount = -1;
        pachisuroGo = false;
        pachisuroGoActive = false;
        turnEnd = false;
        pachisuroGoActive = false;

        tutorialArrow = new ArrowCreate(tutorialArrowPrefab, tutorialArrowRoot);
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    public async UniTask Execute() {
        // 各フェイズのターン処理
        switch (turnPlayer) {
            case PlayerType.Self:
                // 自分のターン
                await phaseList[(int)currentPhase].SelfExecute();
                break;
            case PlayerType.Opponent:
                // 相手のターン
                await phaseList[(int)currentPhase].OpponentExecute();
                break;
        }

        // パチスロフェイズへの移行
        if (pachisuroGo) {
            // メインフェイズを次のフェイズへ
            phaseList[(int)currentPhase].NextPhase();
            pachisuroGo = false;
            pachisuroGoActive = false;
            turnEndActive = false;
        }
        // ターンエンド
        if (turnEnd) {
            // フェイズの片付け
            phaseList[(int)currentPhase].Teardown();
            // フェイズをエンドフェイズに
            currentPhase = BattlePhase.EndPhase;
            // フェイズの準備
            phaseList[(int)currentPhase].Setup();
            turnEnd = false;
            pachisuroGoActive = false;
            turnEndActive = false;
        }

        // 次のフェイズへの移行
        if (phaseList[(int)currentPhase].nextPhase) {
            // 移行処理
            NextPhaseSetup();
        }
    }

    /// <summary>
    /// 次のフェイズへの移行
    /// </summary>
    private void NextPhaseSetup() {
        // フェイズの片付け
        phaseList[(int)currentPhase].Teardown();
        // 今回がエンドフェイズならターンを交代しスタートフェイズへ
        if (currentPhase == BattlePhase.EndPhase) {
            // ターンプレイヤー交代
            ChangeTurnPlayer();
            // スペルの使用回数リセット
            spellCastCount = 0;
            // スタートフェイズに戻る
            currentPhase = BattlePhase.StartPhase;

            Debug.Log("スタートフェイズ");
        }
        else {
            // フェイズを一つ進める
            currentPhase++;

            // ボタンの有効化無効化
            if (turnPlayer == PlayerType.Self &&
                currentPhase == BattlePhase.MainPhase) {
                pachisuroGoActive = true;
                turnEndActive = true;
            }

            switch (currentPhase) {
                case BattlePhase.StartPhase:
                    Debug.Log("スタートフェイズ");
                    break;
                case BattlePhase.MainPhase:
                    Debug.Log("メインフェイズ");
                    break;
                case BattlePhase.PachisuroPhase:
                    Debug.Log("パチスロフェイズ");
                    break;
                case BattlePhase.AttackPhase:
                    Debug.Log("アタックフェイズ");
                    break;
                case BattlePhase.EndPhase:
                    Debug.Log("エンドフェイズ");
                    break;
            }
        }
        // フェイズの準備
        phaseList[(int)currentPhase].Setup();
    }

    /// <summary>
    /// バトル準備
    /// </summary>
    /// <param name="setUseDeckList">使用デッキリスト</param>
    /// <param name="setFirsetPlayer">先攻プレイヤー</param>
    /// <returns></returns>
    public async UniTask BattleSetup(List<int> setUseDeckList, PlayerType setFirsetPlayer) {
        // 自身の使用デッキ登録
        AreaCardManager.instance.SetDeck(PlayerType.Self, setUseDeckList);
        // デッキのシャッフル
        AreaCardManager.instance.ShuffleDeck();
        // 相手にデッキの内容を送る
        var deck = AreaCardManager.instance.DeckPack();
        CustomNetworkManager.instance.SendStringToOpponent(deck);
        // 先攻プレイヤー登録
        firstPlayer = setFirsetPlayer;
        // 現在のターンプレイヤーを先攻プレイヤーに
        turnPlayer = firstPlayer;
        // フェイズをスタートに
        currentPhase = BattlePhase.StartPhase;
        // 対戦中状態に切り替え
        battleState = BattleState.InProgress;
        // 現在のターンを1に
        currentTurn = 1;

        // 相手のデッキを受け取るまで待つ
        while (!AreaCardManager.instance.catchOpponentDeck) {
            await UniTask.Yield();
        }
        AreaCardManager.instance.SetCatchOpponentDeck(false);

        // 各プレイヤーの初期化
        for (PlayerType i = 0; i < PlayerType.Max; i++) {
            // HPの初期化
            playerHP[i] = PLAYER_HP;
            // サイズ上限を最小値に
            playerSizeLimit[i] = SIZE_LIMIT_MIN;

            // 手札5枚用意
            for (int f = 0; f < START_HAND_COUNT; f++) {
                // ターンプレイヤーから先に手札を用意
                if (firstPlayer == PlayerType.Self) {
                    DrawCard(i);
                }
                else {
                    if (i == PlayerType.Self)
                        DrawCard(PlayerType.Opponent);
                    if (i == PlayerType.Opponent)
                        DrawCard(PlayerType.Self);
                }
            }
        }

        // ログ
        switch (turnPlayer) {
            case PlayerType.Self:
                Debug.Log("先攻：自分のターン");
                break;
            case PlayerType.Opponent:
                Debug.Log("先攻：相手のターン");
                break;
        }
    }

    /// <summary>
    /// 対戦片付け処理
    /// </summary>
    public void BattleTeardown() {
        // このクラスの初期化処理
        Initialize();
        // 各エリアのカード管理クラスの初期化処理
        AreaCardManager.instance.Initialize();
        // カードオブジェクトの未使用化と初期化
        CardObjectManager.instance.AllUnuseCardObject();
        CardObjectManager.instance.Initialize();
    }

    /// <summary>
    /// ターンプレイヤーを交代する
    /// </summary>
    private void ChangeTurnPlayer() {
        switch (turnPlayer) {
            case PlayerType.Self:
                turnPlayer = PlayerType.Opponent;
                Debug.Log("相手のターン");
                break;
            case PlayerType.Opponent:
                turnPlayer = PlayerType.Self;
                Debug.Log("自分のターン");
                break;
        }
        // 交代したプレイヤーが先攻プレイヤーならターンを進める
        if (firstPlayer == turnPlayer) {
            currentTurn++;
        }
        // プレイヤーのサイズ上限を上げる
        // 1ターン目はスキップ
        if (currentTurn <= 1) return;
        if (playerSizeLimit[turnPlayer] + SIZE_LIMIT_UP >= SIZE_LIMIT_MAX) {
            playerSizeLimit[turnPlayer] = SIZE_LIMIT_MAX;
        }
        else {
            playerSizeLimit[turnPlayer] += SIZE_LIMIT_UP;
        }
    }

    /// <summary>
    /// プレイヤーに指定のHPを付与
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="giveHP">付与量</param>
    public void PlayerGiveHP(PlayerType player, int giveHP) {
        // HP付与
        playerHP[player] += giveHP;

        // ダメージをくらったら表示
        if (giveHP < 0) {
            UIManager.instance.GenerateDamageUI(giveHP, null, player);
        }

        // HPが0以下なら勝敗がつく
        if (playerHP[player] > 0) return;

        playerHP[player] = 0;
        switch (player) {
            case PlayerType.Self:
                // 自身のHPが0なら敗北
                battleState = BattleState.Lose;
                break;
            case PlayerType.Opponent:
                // 相手のHPが0なら勝利
                battleState = BattleState.Win;
                break;
        }
    }

    /// <summary>
    /// カードのドロー(オブジェクト操作含め)
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    public void DrawCard(PlayerType player) {
        // 手札上限なら手札ではなく墓地へ送る
        int handCount = AreaCardManager.instance.outDeckCards[player].handCards.Count;
        if (HAND_COUNT_MAX <= handCount) {
            // 山札トップから墓地へ
            AreaCardManager.instance.DeckToGrave(player);
        }
        else {
            // 山札からIDを抜く
            int card = AreaCardManager.instance.DrawCard(player);
            // オブジェクトの使用化
            CardObject cardObject = CardObjectManager.instance.UseCardObject(card, player);
            // オブジェクトを手札の位置に移動
            CardObjectManager.instance.MoveToHand(player, cardObject.objectID);
        }
    }

    /// <summary>
    /// モンスター召喚
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="card">召喚するカードのオブジェクト</param>
    /// <param name="lane">召喚するカードのレーン</param>
    /// <param name="isAbility">効果発動中かどうか</param>
    public async UniTask SummonMonster(PlayerType player, CardObject card, FieldLane lane, bool isAbility = false) {
        int objectID = card.objectID;
        if (card.handID >= 0) {
            // 手札から指定されたレーンにカードを移動
            AreaCardManager.instance.HandToField(player, card.handID, (int)lane);
        }
        // 指定されたレーンにカードを設置
        CardObjectManager.instance.MoveToField(player, objectID, lane);
        // 召喚時効果発動確認
        foreach (var ability in card.abilityList) {
            if (ability.abilityTiming != AbilityTiming.Play) continue;
            // 見つかった効果を積む
            AbilityManager.instance.SetAblityQueue(card, ability);
        }

        // 自分の処理なら相手に送る
        if (player == PlayerType.Self) {
            var pack = NetworkValueConverter.Pack(NetworkUseID.PlayCard, objectID, lane);
            CustomNetworkManager.instance.SendStringToOpponent(pack);
        }

        // 効果発動中ならここで終了
        if (isAbility) await UniTask.CompletedTask;

        // 積まれた効果があれば発動
        await AbilityManager.instance.AbilityExecute();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// スペルを唱える
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="card">唱えるカードのオブジェクト</param>
    public void CastSpell(PlayerType player, CardObject card) {
        int objectID = card.objectID;
        // スペルの効果発動確認
        foreach (var ability in card.abilityList) {
            if (ability.abilityTiming != AbilityTiming.Play) continue;
            // 見つかった効果を積む
            AbilityManager.instance.SetAblityQueue(card, ability);
        }
        // 発動したスペルは手札から捨てる
        HandDestruction(player, card);
        // スペルの回数カウント
        spellCastCount++;

        // 自身の処理なら相手に送る
        if (player == PlayerType.Self) {
            var pack = NetworkValueConverter.Pack(NetworkUseID.PlayCard, objectID, FieldLane.None);
            CustomNetworkManager.instance.SendStringToOpponent(pack);
        }

        // 積まれた効果があれば発動
        UniTask task = AbilityManager.instance.AbilityExecute();
    }


    /// <summary>
    /// 残りのサイズ取得
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <returns></returns>
    public int GetRemainingSize(PlayerType player) {
        var fieldObject = CardObjectManager.instance.fieldObjectList[player];
        int size = 0;
        // フィールドの合計サイズ計算
        foreach (var obj in fieldObject) {
            if (!obj.Value) continue;
            size += obj.Value.currentStatus[MonsterStatus.SIZE];
        }
        // サイズ上限から合計サイズを引いた値を返す
        return playerSizeLimit[player] - size;
    }

    /// <summary>
    /// レーン交換
    /// </summary>
    /// <param name="player"></param>
    /// <param name="laneA"></param>
    /// <param name="laneB"></param>
    public void LaneChange(PlayerType player, FieldLane laneA, FieldLane laneB) {
        // IDリストを交換
        AreaCardManager.instance.LaneChange(player, (int)laneA, (int)laneB);
        // オブジェクトを交換
        CardObjectManager.instance.LaneChange(player, laneA, laneB);

        // 自身の処理なら相手に送る
        if (player != PlayerType.Self) return;
        var pack = NetworkValueConverter.Pack(NetworkUseID.LaneCange, laneA, laneB);
        CustomNetworkManager.instance.SendStringToOpponent(pack);
    }

    /// <summary>
    /// パチスロフェイズへ移行
    /// </summary>
    public void PachisuroGo() {
        pachisuroGo = true;
    }

    /// <summary>
    /// ターンエンド
    /// </summary>
    public void TurnEnd() {
        turnEnd = true;
    }

    /// <summary>
    /// 相手のプレイカード受け取り
    /// </summary>
    /// <param name="playCard"></param>
    public void ReceiveOpponentPlayCard(string[] playCard) {
        // 2つ目の情報がカードのオブジェクトIDなのでintに変換
        int objectID = -1;
        NetworkValueConverter.TryGetInt(playCard[1], out objectID);
        // 3つ目の情報がレーンIDなのでenumに変換
        FieldLane laneID = FieldLane.None;
        NetworkValueConverter.TryGetEnum(playCard[2], out laneID);

        // カード使用
        CardObject obj = CardObjectManager.instance.GetUseCardObject(objectID);
        switch (obj.cardType) {
            case CardType.Monster:
                UniTask task = SummonMonster(PlayerType.Opponent, obj, laneID);
                break;
            case CardType.Spell:
                CastSpell(PlayerType.Opponent, obj);
                break;
        }
    }

    /// <summary>
    /// 相手のレーン交換受け取り
    /// </summary>
    /// <param name="changeLane"></param>
    public void ReceiveOpponentLaneChange(string[] changeLane) {
        // 2,3つ目の情報が交換するレーンなのでenumに変換
        FieldLane laneA = FieldLane.None;
        NetworkValueConverter.TryGetEnum(changeLane[1], out laneA);
        FieldLane laneB = FieldLane.None;
        NetworkValueConverter.TryGetEnum(changeLane[2], out laneB);

        // 受け取った2つのレーンを交換
        LaneChange(PlayerType.Opponent, laneA, laneB);
    }

    /// <summary>
    /// 手札を捨てる
    /// </summary>
    /// <param name="player">どちらのプレイヤーか</param>
    /// <param name="card">捨てるカード</param>
    public void HandDestruction(PlayerType player, CardObject card) {
        // カードを手札から墓地に移動
        AreaCardManager.instance.HandToGrave(player, card.handID);
        // カードオブジェクトを未使用状態に
        CardObjectManager.instance.UnuseCardObject(card);
        // 手札の整列
        CardObjectManager.instance.ArrangeHand(player);
    }

    /// <summary>
    /// チュートリアル準備
    /// </summary>
    /// <returns></returns>
    public async UniTask TutorialSetup() {
        // 自身ののチュートリアルデッキを取得
        List<int> deck = DeckDataMasterUtility.GetDeckMaster(1);
        // チュートリアルデッキが取得できなければ処理を終了する
        if (deck == null) return;
        // 自身の使用デッキ登録
        AreaCardManager.instance.SetDeck(PlayerType.Self, deck);
        // 相手のチュートリアルデッキを取得
        deck = DeckDataMasterUtility.GetDeckMaster(2);
        // チュートリアルデッキが取得できなければ処理を終了する
        if (deck == null) return;
        // 相手の使用デッキ登録
        AreaCardManager.instance.SetDeck(PlayerType.Opponent, deck);
        // 先攻プレイヤーは自身
        firstPlayer = PlayerType.Self;
        // 現在のターンプレイヤーを先攻プレイヤーに
        turnPlayer = firstPlayer;
        // フェイズをスタートに
        currentPhase = BattlePhase.StartPhase;
        // 対戦中状態に切り替え
        battleState = BattleState.InProgress;
        // 現在のターンを1に
        currentTurn = 1;

        // 各プレイヤーの初期化
        for (PlayerType i = 0; i < PlayerType.Max; i++) {
            // HPの初期化
            playerHP[i] = PLAYER_HP;
            // サイズ上限を最小値に
            playerSizeLimit[i] = SIZE_LIMIT_MIN;

            // 手札5枚用意
            for (int f = 0; f < START_HAND_COUNT; f++) {
                DrawCard(i);
            }
        }
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// チュートリアルの矢印をレーンの位置に出す
    /// </summary>
    /// <param name="lane">出すレーン</param>
    public void TutorialLaneArrow(FieldLane lane) {
        switch (lane) {
            case FieldLane.Lane1:
                tutorialArrow.CreateVerticalArrow(new Vector2(460, 560));
                break;
            case FieldLane.Lane2:
                tutorialArrow.CreateVerticalArrow(new Vector2(710, 560));
                break;
            case FieldLane.Lane3:
                tutorialArrow.CreateVerticalArrow(new Vector2(960, 560));
                break;
            case FieldLane.Lane4:
                tutorialArrow.CreateVerticalArrow(new Vector2(1210, 560));
                break;
            case FieldLane.Lane5:
                tutorialArrow.CreateVerticalArrow(new Vector2(1460, 560));
                break;
        }
    }

    /// <summary>
    /// チュートリアルの実行処理
    /// </summary>
    /// <returns></returns>
    public async UniTask TutorialExecute() {
        await UniTask.DelayFrame(30);
        string text = "このゲームはパチスロとカードゲームで\n相手のHPをゼロにすることで勝利するゲームです！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "早速始めよう！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);

        // 自身のスタートフェイズ
        currentPhase = BattlePhase.StartPhase;
        await phaseList[(int)currentPhase].SelfExecute();
        text = "カードの右下のマークが対応役だ！\n後で出てくるから覚えておこう。";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "対応役がSのカードはスペルだ！\n使えば様々な効果が発動する！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "対応役が設定されているカードがモンスターだ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "右クリックでカードの詳細が確認できるから\n困ったらチェックだ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "早速モンスターを出そう！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "手札のモンスターを左クリックで掴んで、\nフィールドまで移動させると召喚できるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        NextPhaseSetup();

        // 自身のメインフェイズ
        MainPhase mainPhase = (MainPhase)phaseList[(int)currentPhase];
        CardObject raymond = CardObjectManager.instance.GetUseCardObject(0);
        // 矢印出す
        tutorialArrow.CreateVerticalArrow(new Vector2(570, 250), 180);
        // Professor:Raymondを出すまで待つ
        while (true) {
            if (await mainPhase.CardControl(raymond, FieldLane.Lane3))
                break;
            await UniTask.Yield();
        }
        // 矢印消す
        tutorialArrow.DeleteArrow();
        text = "フィールドにはサイズ上限があるから、\nモンスターを出すときは注意するんだ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "そのターン中に出したばかりのモンスターは疲労するぞ！\n疲労中は攻撃ができないから注意だ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "ボタンを押してターンを終わろう。";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // 矢印出す
        tutorialArrow.CreateHorizontalArrow(new Vector2(1520, 490), 90);
        // ターンエンドボタン
        turnEndActive = true;
        while (!turnEnd) {
            await UniTask.Yield();
        }
        turnEndActive = false;
        turnEnd = false;
        // フェイズの片付け
        phaseList[(int)currentPhase].Teardown();
        // フェイズをエンドフェイズに
        currentPhase = BattlePhase.EndPhase;
        // フェイズの準備
        phaseList[(int)currentPhase].Setup();
        // 矢印消す
        tutorialArrow.DeleteArrow();

        // 自身のエンドフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        NextPhaseSetup();

        // 相手のスタートフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        NextPhaseSetup();

        // 相手のメインフェイズ
        // 出されたレーンの対面に出す
        FieldLane frontLane = FieldLane.None;
        if (raymond.laneID == 0) {
            frontLane = FieldLane.Lane5;
        }
        else {
            frontLane = (FieldLane)((int)FieldLane.Max % raymond.laneID + 1);
        }
        // リヴァイブドラゴンを出す
        CardObject revive = CardObjectManager.instance.GetUseCardObject(5);
        await SummonMonster(PlayerType.Opponent, revive, frontLane);
        text = "相手もモンスターを出してきたね。\n相手のターン終了！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // フェイズの片付け
        phaseList[(int)currentPhase].Teardown();
        // フェイズをエンドフェイズに
        currentPhase = BattlePhase.EndPhase;
        // フェイズの準備
        phaseList[(int)currentPhase].Setup();

        // 相手のエンドフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        NextPhaseSetup();

        // 自身のスタートフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        NextPhaseSetup();

        // 自身のメインフェイズ
        text = "このターンからさっき出したモンスターは攻撃可能だ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "ターンが進むとフィールドに出せるサイズも大きくなるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "2体目のモンスターを出そう";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        CardObject lank = CardObjectManager.instance.GetUseCardObject(4);
        // RG:LANKを出すまで待つ
        // 矢印出す
        tutorialArrow.CreateVerticalArrow(new Vector2(960, 250), 180);
        while (true) {
            if (await mainPhase.CardControl(lank, FieldLane.Lane4))
                break;
            await UniTask.Yield();
        }
        // 矢印消す
        tutorialArrow.DeleteArrow();
        text = "攻撃するときはボタンを押してパチスロGO！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "引いた役とモンスターの対応役が一致していたら攻撃するよ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "Professor:Raymondの対応役はハズレだから、\nハズレを引けると攻撃ができるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // 矢印出す
        tutorialArrow.CreateHorizontalArrow(new Vector2(1520, 600), 90);
        // パチスロボタン
        pachisuroGoActive = true;
        while (!pachisuroGo) {
            await UniTask.Yield();
        }
        pachisuroGoActive = false;
        pachisuroGo = false;
        // 矢印消す
        tutorialArrow.DeleteArrow();
        NextPhaseSetup();

        // 自身のパチスロフェイズ
        text = "パチスロを回そう！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "Ctrlでbet！上矢印でリールを回せるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "基本的には横か斜めに絵柄が揃うと役の成立だ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // ハズレを引く
        text = "今回は何も絵柄がそろっていないのでハズレ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "これでパチスロフェイズの終わりに攻撃ができるね。";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // 弱チェを引く
        text = "チェリーは左のリールに一つでも止まったら\n役として成立するよ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // スイカを引く
        text = "レアな役を引いたみたいだね。レアな役は出にくいぶん、\n対応モンスターの破壊力はすごいぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // チャンス目を引く
        text = "おっとチャンス目だ！この役はチェリーとスイカ\n両方の役が成立したとみなされるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // ハズレを引く
        text = "また対応役を引いたね！\nこれで2回攻撃ができるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        NextPhaseSetup();

        // 自身のアタックフェイズ
        AttackPhase attackPhase = (AttackPhase)phaseList[(int)BattlePhase.AttackPhase];
        text = "対応役を引いたので攻撃ができるぞ！\n目の前のモンスターに攻撃だ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // 途中で解説を挟みたいので直接処理
        // 当選役取得
        List<PachisuroSymbolKoyakuEnum> koyakuList = SlotReelController.Instance.GetRoles();
        // フィールドのカード取得
        var fieldCards = CardObjectManager.instance.fieldObjectList[PlayerType.Self];
        // 当選役の数分の対応役チェック
        bool firstAttack = true;
        foreach (var koyaku in koyakuList) {
            // Professor:Raymondの役チェック
            attackPhase.CardAttack(PlayerType.Opponent, koyaku, raymond,
                SlotReelController.Instance.AttackRush,
                SlotReelController.Instance.AttackNextRush);

            // UIに反映
            PlayerHPUIControllere hp; //呼ぶスクリプトにあだなつける
            GameObject player = GameObject.Find("Player2HP");
            hp = player.GetComponent<PlayerHPUIControllere>(); //付いているスクリプトを取得
            hp.SetCurrentHP(playerHP[PlayerType.Opponent]);
            await UniTask.DelayFrame(10);
            if (koyaku == PachisuroSymbolKoyakuEnum.Miss) {
                if (firstAttack) {
                    // 1回目の攻撃
                    text = "やった！ダメージだ！\n相手のモンスターを破壊したぞ！";
                    await MenuWindowManager.Instance.TutorialTextWindow(text);
                    firstAttack = false;
                } else {
                    text = "続けて2回目の攻撃！目の前のモンスターは\nいなくなったからプレイヤーに攻撃だ！";
                    await MenuWindowManager.Instance.TutorialTextWindow(text);
                }
            }
        }
        NextPhaseSetup();

        // 自身のエンドフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        NextPhaseSetup();

        // 相手のスタートフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        NextPhaseSetup();

        // 相手のメインフェイズ
        // 出されたレーンの対面に出す
        frontLane = FieldLane.None;
        if (lank.laneID == 0) {
            frontLane = FieldLane.Lane5;
        }
        else {
            frontLane = (FieldLane)((int)FieldLane.Max % lank.laneID + 1);
        }
        // リーンドラゴンを出す
        CardObject rein = CardObjectManager.instance.GetUseCardObject(7);
        await SummonMonster(PlayerType.Opponent, rein, frontLane);
        text = "強力なモンスターが出たぞ！\n今出たリーンドラゴンは疲労しない効果を持っている。";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "つまり出たターンからいきなり攻撃できるんだ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        NextPhaseSetup();

        // 相手のパチスロフェイズ
        // ハズレ、ハズレ、リプ、ベル、リプを引く
        NextPhaseSetup();

        // 相手のアタックフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        text = "モンスターが1体やられてしまったね。";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        NextPhaseSetup();

        // 相手のエンドフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        NextPhaseSetup();

        // 自身のスタートフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        NextPhaseSetup();

        // 自身のメインフェイズ
        text = "このままだと相手の攻撃を受けてしまうね。\nそんなときはモンスターを移動させて攻撃を防ごう！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "移動させたいモンスターを左クリックで掴み、\n移動させたい場所まで持っていけば移動できるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // 矢印出す
        tutorialArrow.CreateHorizontalArrow(new Vector2(1075, 390), 90);
        // モンスターを移動させるまで待つ
        while (true) {
            if (await mainPhase.CardControl(raymond, FieldLane.Lane4))
                break;
            await UniTask.Yield();
        }
        // 矢印消す
        tutorialArrow.DeleteArrow();
        text = "移動させたモンスターは疲労してしまうので注意だ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "もし移動先にモンスターがいる場合は\nそのモンスターと位置を交換するぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "両方のモンスターが疲労してしまうから\n慎重に判断するべきだな！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "サイズに空きがあるね。\n少し強力なモンスターを出そう！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // RG:CRASSAを出すまで待つ
        CardObject crassa = CardObjectManager.instance.GetUseCardObject(3);
        // 矢印出す
        tutorialArrow.CreateVerticalArrow(new Vector2(880, 250), 180);
        while (true) {
            if (await mainPhase.CardControl(crassa, FieldLane.Lane3))
                break;
            await UniTask.Yield();
        }
        // 矢印消す
        tutorialArrow.DeleteArrow();
        text = "さらにスペルを使ってみよう！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "PERFECT・STOPは自分のモンスターの\n疲労をいやすことができるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // PERFECT・STOPを使うまで待つ
        CardObject perfect = CardObjectManager.instance.GetUseCardObject(1);
        // 矢印出す
        tutorialArrow.CreateVerticalArrow(new Vector2(650, 250), 180);
        while (true) {
            if (await mainPhase.CardControl(perfect))
                break;
            await UniTask.Yield();
        }
        // 矢印消す
        tutorialArrow.DeleteArrow();
        text = "これでみんな攻撃できるね！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        NextPhaseSetup();

        // 自身のパチスロフェイズ
        // リプ、弱チェ、ベル、強チェを引く
        text = "特別なチェリーを引いたね！\nチェリーが揃ったら2倍の攻撃力で攻撃するぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // 7を引く
        text = "すごい演出が出て7が揃ったね。\n7が揃うと全員で攻撃できるぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        text = "さらに、ラッシュに当選！\n次のパチスロフェイズで7が揃う確率が大幅にアップするぞ！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        NextPhaseSetup();

        // 自身のアタックフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        text = "相手に大きなダメージを与えたね！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        NextPhaseSetup();

        // 自身のエンドフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        NextPhaseSetup();

        // 相手のスタートフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        NextPhaseSetup();

        // 相手のメインフェイズ
        // 出されたレーンの対面に出す
        frontLane = FieldLane.None;
        if (raymond.laneID == 0) {
            frontLane = FieldLane.Lane5;
        }
        else {
            frontLane = (FieldLane)((int)FieldLane.Max % crassa.laneID + 1);
        }
        // ジングルドラゴンを出す
        CardObject jingle = CardObjectManager.instance.GetUseCardObject(9);
        await SummonMonster(PlayerType.Opponent, jingle, frontLane);
        NextPhaseSetup();

        // 相手のパチスロフェイズ
        // スイカ、リプ、リプ、リプ、ハズレを引く
        NextPhaseSetup();

        // 相手のアタックフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        NextPhaseSetup();

        // 相手のエンドフェイズ
        await phaseList[(int)currentPhase].OpponentExecute();
        NextPhaseSetup();

        // 自身のスタートフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        NextPhaseSetup();

        // 自身のメインフェイズ
        text = "今はラッシュ中で相手のモンスターは少ない。\nこのまま押し切っちゃおう！";
        await MenuWindowManager.Instance.TutorialTextWindow(text);
        // RG:REIを2体出す
        CardObject rei = CardObjectManager.instance.GetUseCardObject(11);
        // 矢印出す
        tutorialArrow.CreateVerticalArrow(new Vector2(1030, 250), 180);
        while (true) {
            if (await mainPhase.CardControl(rei, FieldLane.Lane5))
                break;
            await UniTask.Yield();
        }
        rei = CardObjectManager.instance.GetUseCardObject(13);
        // 矢印出す
        tutorialArrow.CreateVerticalArrow(new Vector2(1170, 250), 180);
        while (true) {
            if (await mainPhase.CardControl(rei, FieldLane.Lane2))
                break;
            await UniTask.Yield();
        }
        NextPhaseSetup();

        // 自身のパチスロフェイズ
        // 7、弱チェ、ハズレ、7、7を引く
        NextPhaseSetup();

        // 自身のアタックフェイズ
        await phaseList[(int)currentPhase].SelfExecute();
        NextPhaseSetup();
    }
}
