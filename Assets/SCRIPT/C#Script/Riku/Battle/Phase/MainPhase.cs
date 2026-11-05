/*
 * @file    MainPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Collections;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
//using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// メインフェイズ
/// </summary>
public class MainPhase : PhaseBase {
    // つかんだオブジェクトを保持
    private GameObject catchObject = null;
    // オブジェクトのクラス
    private CardObject cardObject = null;
    // 掴んだ時の位置
    private Vector3 catchStartPos = Vector3.zero;
    // 座標の初期化フラグ
    private bool resetPos = false;

    // レイキャストの最大距離
    private const float RAY_RANGE_MAX = 10.0f;
    
    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        // カード操作
        CardControl();

        // 積まれた効果があれば発動
        await AbilityManager.instance.AbilityExecute();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask OpponentExecute() {
        // 積まれた効果があれば発動
        await AbilityManager.instance.AbilityExecute();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// カード操作
    /// </summary>
    private void CardControl() {
        // マウスカーソルのスクリーン座標取得
        Vector3 mousePos = Input.mousePosition;
        // スクリーン座標からレイを取得
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        // 左クリックでカードを掴む
        if (Input.GetMouseButtonDown(0)) {
            // 掴む
            CardCatch(ray);
        }

        // 掴んでいる時の処理
        if (catchObject) {
            // 掴んでいるオブジェクトをマウスに追従
            CardChaseMouse(ray);

            // 掴んでいるオブジェクトを離す
            if (Input.GetMouseButtonUp(0)) {
                // フィールドレイヤー取得
                int fieldLayer = LayerMask.GetMask("Field");
                // フィールドとのレイキャスト
                if (Physics.Raycast(
                    ray, RAY_RANGE_MAX, fieldLayer)) {
                    // カードがレーン上のものかどうか判定
                    if (cardObject.laneID > -1) {
                        // レーン交換
                        if (!LaneChange(ray))
                            resetPos = true;
                    }
                    // カードが手札のものだった場合の処理
                    else if (cardObject.handID > -1) {
                        // カードを使用
                        UniTask task = PlayCard(cardObject);
                    }
                }
                // フィールド外で離したら元の位置に戻る
                else {
                    resetPos = true;
                }

                // 座標を初期化
                if (resetPos) {
                    catchObject.transform.position = catchStartPos;
                    resetPos = false;
                }

                // 掴んだ時の情報をリセット
                catchObject = null;
                cardObject = null;
                catchStartPos = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// カードの使用
    /// </summary>
    /// <param name="card">使用するオブジェクトのID</param>
    /// <returns></returns>
    private async UniTask PlayCard(CardObject card) {
        if (!card) return;
        // カードタイプによって分岐
        switch (card.cardType) {
            case CardType.Monster:
                // 残りのサイズ取得
                int remainingSize = BattleSystemManager.instance.GetRemainingSize(PlayerType.Self);
                // サイズが上限超過しないか
                if (card.currentStatus[MonsterStatus.SIZE] <= remainingSize) {
                    // レーン設置ウィンドウを出す
                    FieldLane lane = await MenuWindowManager.Instance.LaneMenuOpen();
                    // モンスター召喚
                    await BattleSystemManager.instance.SummonMonster(PlayerType.Self, card, lane);
                    Debug.Log("モンスター召喚！！");
                }
                else {
                    resetPos = true;
                    Debug.Log("サイズが超過しています");
                }
                break;
            case CardType.Spell:
                // スペルの回数上限に引っかからないか
                if (BattleSystemManager.instance.spellCastCount < GameConst.SPELL_CAST_MAX) {
                    // スペルを唱える
                    BattleSystemManager.instance.CastSpell(PlayerType.Self, card);
                    Debug.Log("スペル発動！！");
                }
                else {
                    resetPos = true;
                    Debug.Log("サイズが超過しています");
                }
                break;
        }
    }

    /// <summary>
    /// カードを掴む
    /// </summary>
    /// <param name="ray"></param>
    private void CardCatch(Ray ray) {
        // カードオブジェクトレイヤー取得
        int cardLayer = LayerMask.GetMask("CardObject");
        // レイキャストでカードを触れているか判定
        if (Physics.Raycast(
            ray, out RaycastHit hit,
            RAY_RANGE_MAX, cardLayer)
            ) {
            if (!hit.collider) return;
            // 触れているオブジェクトを保存
            catchObject = hit.collider.gameObject;
            // オブジェクトのクラス取得
            cardObject = catchObject.GetComponent<CardObject>();
            // 相手のカードなら掴まない
            if (cardObject.playerType != PlayerType.Self) {
                catchObject = null;
                cardObject = null;
                return;
            }
            // 位置を保存
            catchStartPos = catchObject.transform.position;
        }
    }

    /// <summary>
    /// カードがマウスを追う
    /// </summary>
    /// <param name="ray"></param>
    private void CardChaseMouse(Ray ray) {
        // カードのある位置にXY平面の仮想板を作る
        Plane plane = new Plane(
            Vector3.forward,
            catchObject.transform.position);
        // 仮想板とレイの判定
        if (plane.Raycast(ray, out float distance)) {
            // 衝突した位置を取得
            Vector3 target = ray.GetPoint(distance);
            // カードの位置を変更
            catchObject.transform.position = target;
        }
    }

    /// <summary>
    /// レーン交換
    /// </summary>
    /// <param name="ray"></param>
    private bool LaneChange(Ray ray) {
        // レーンレイヤー取得
        int laneLayer = LayerMask.GetMask("Lane");
        // レーンのレイキャスト
        if (Physics.Raycast(
        ray, out RaycastHit hit,
        RAY_RANGE_MAX, laneLayer)) {
            // レーン番号の取得
            FieldLane lane = CardObjectManager.instance.GetObjectLane(hit.transform);
            // レーンの位置変更
            BattleSystemManager.instance.LaneChange(PlayerType.Self, (FieldLane)cardObject.laneID, lane);
            return true;
        }
        return false;
    }
}
