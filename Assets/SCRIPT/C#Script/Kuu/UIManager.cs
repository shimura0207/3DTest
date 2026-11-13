/*
 * @file    UIManager.cs
 * @author  kuu
 */
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    // 手札の拡大縮小フラグ
    private bool handFlag = false;

    // カードを掴んでいるか判定
    private bool cardHold = false;

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
                // カードを掴んだ状態
                cardHold = true;
            }
            else {
                cardData = null;
            }
            // 右クリックだったら判定を右にする
            if (Input.GetMouseButtonDown(1)) {
                mouseButton = false;
            }
        }
        // カードを離す
        if (Input.GetMouseButtonUp(0)) {
            cardHold = false;
        }

        // 手札を左クリックで拡大縮小
        if (Input.GetMouseButtonDown(0)) {
            // カードを掴んでいたらダメ
            if (cardHold) return;
            int handLayer = LayerMask.GetMask("Hand");
            // レイキャストで手札に触れているか判定
            if (Physics.Raycast(
                ray, out RaycastHit hit,
                GameConst.RAY_RANGE_MAX, handLayer)
                ) {
                if (!hit.collider) return;
                // 手札の拡大縮小処理
                if (!handFlag) {
                    // 拡大
                    CardObjectManager.instance.ScaleUpHand();
                    handFlag = true;
                }
                else {
                    // 縮小
                    CardObjectManager.instance.ScaleDownHand();
                    handFlag = false;
                }
            }
        }

        // 現在のサイズ状況更新
        currentMaxSize = battleSystem.playerSizeLimit[PlayerType.Self];
        opponentMaxSize = battleSystem.playerSizeLimit[PlayerType.Opponent];
        restSize = battleSystem.GetRemainingSize(PlayerType.Self);
        opponentRestSize = battleSystem.GetRemainingSize(PlayerType.Opponent);
    }
}

