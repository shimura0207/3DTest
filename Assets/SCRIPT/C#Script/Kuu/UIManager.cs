/*
 * @file    UIManager.cs
 * @author  kuu
 */
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

using static CommonModule;

/// <summary>
/// UI管理クラス
/// </summary>
public class UIManager : SystemObject {
    // 自身への参照
    public static UIManager instance { get; private set; }

    // つかんだオブジェクトを保持
    private GameObject catchObject = null;

    // オブジェクトのクラス
    private CardObject cardObject = null;

    // 参照カードのデータを保存
    public Entity_CardData.Param cardData { get; private set; } = null;

    // 省略用
    private BattleSystemManager battleSystem = BattleSystemManager.instance;

    // 現在の最大サイズ
    public int currentMaxSize = 0;

    // 対戦相手の最大サイズ
    public int opponentMaxSize = 0;

    // 残りのサイズ
    public int restSize = 0;

    // 対戦相手の残りサイズ
    public int opponentRestSize = 0;

    // マウスの左右クリック判定
    public bool mouseButton { get; private set; } = true;

    // ダメージUI
    [SerializeField] private GameObject damageUI = null;

    // 自分のHPTextの位置
    private Vector3 SELFHP_POSITION = new Vector3(-0.263f, 1.123f, -0.41f);

    // 自分のHPTextの位置
    private Vector3 OPPONENTHP_POSITION = new Vector3(-0.263f, 1.42f, -0.41f);

    // ダメージの値を渡すために一時的に保存する用
    public int damageValue = 0;

    public override async UniTask Initialize() {
        instance = this;

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    public void Execute() {
        // バトル準備が完了していなければ更新しない
        if (battleSystem == null || battleSystem.battleState != BattleState.InProgress) {
            return;
        }
        if (battleSystem.playerSizeLimit == null ||
            !battleSystem.playerSizeLimit.ContainsKey(PlayerType.Self) ||
            !battleSystem.playerSizeLimit.ContainsKey(PlayerType.Opponent)) {
            return;
        }

        // マウスカーソルのスクリーン座標取得
        Vector3 mousePos = Input.mousePosition;
        // スクリーン座標からレイを取得
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // 左クリックでカードを掴む
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            // マウスボタンの判定を一旦左にする
            mouseButton = true;
            // カードオブジェクトレイヤー取得
            int cardLayer = LayerMask.GetMask("CardObject");
            // レイキャストでカードを触れているか判定
            if (Physics.Raycast(
                ray, out RaycastHit hit,
                GameConst.RAY_RANGE_MAX, cardLayer)
                ) {
                if (!hit.collider) return;
                // 触れているオブジェクトを保存
                catchObject = hit.collider.gameObject;
                // オブジェクトのクラス取得
                cardObject = catchObject.GetComponent<CardObject>();
                // IDからステータスを取得
                cardData = CardDataMasterUtility.GetCardMaster(cardObject.cardID);
                // 裏向きならデータは要らない
                if (cardObject.SpriteRenderer.sprite == ScriptableObjectManager.instance.GetCardImage(0)) {
                    cardData = null;
                }
            }
            else {
                cardData = null;
            }
            // 右クリックだったら判定を右にする
            if (Input.GetMouseButtonDown(1)) {
                mouseButton = false;
            }
        }

        // 手札を左クリックで拡大縮小
        if (Input.GetMouseButtonDown(0)) {
            int handLayer = LayerMask.GetMask("Hand");
            // レイキャストで手札に触れているか判定
            if (Physics.Raycast(
                ray, out RaycastHit hit,
                GameConst.RAY_RANGE_MAX, handLayer)
                ) {
                if (!hit.collider) return;

                // 拡大
                if (CardObjectManager.instance.isHandShrink) {
                    CardObjectManager.instance.ScaleUpHand();
                }
            }
            // 手札以外でカードを掴んでいないなら縮小
            else {
                if (!CardObjectManager.instance.isHandShrink && cardData == null) {
                    CardObjectManager.instance.ScaleDownHand();
                }
            }
        }

        // 現在のサイズ状況更新
        currentMaxSize = battleSystem.playerSizeLimit[PlayerType.Self];
        opponentMaxSize = battleSystem.playerSizeLimit[PlayerType.Opponent];
        restSize = battleSystem.GetRemainingSize(PlayerType.Self);
        opponentRestSize = battleSystem.GetRemainingSize(PlayerType.Opponent);
    }

    /// <summary>
    /// ダメージUIを生成する
    /// </summary>
    /// <param name="damage">表示したいダメージ</param>
    /// <param name="card">表示したいカード(プレイヤー指定ならnull)</param>
    /// <param name="player">表示したいプレイヤー</param>
    public void GenerateDamageUI(int damage, CardObject card, PlayerType player = PlayerType.None) {
        // 生成する位置
        Vector3 position = Vector3.zero;
        // カードオブジェクトがないならプレイヤータイプで位置を合わせる
        if (card == null) {
            // 自分
            if (player == PlayerType.Self) {
                position = SELFHP_POSITION;
            }
            // 相手
            else if (player == PlayerType.Opponent) {
                position = OPPONENTHP_POSITION;
            }
            // 情報が足りない場合は抜ける
            else {
                return;
            }
        }
        // カードのポジションに合わせる
        else {
            position = card.transform.position;
        }
        // ダメージを保存する
        damageValue = damage;
        // 生成する
        Instantiate(damageUI, position, Quaternion.identity);
    }
}

