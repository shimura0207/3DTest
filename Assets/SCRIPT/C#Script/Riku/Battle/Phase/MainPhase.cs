/*
 * @file    MainPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;


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

    // 持ち上げ幅
    private readonly float CATCHI_UP = 0.01f;
    
    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        // カード操作
        await CardControl();

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
    /// <param name="selectCard">指定したいカード(指定なしなら自由に操作)</param>
    /// <param name="selectLane">指定したいレーン(指定なしなら自由に操作)</param>
    /// <returns>カードをプレイかレーン移動したらtrueを返す</returns>
    public async UniTask<bool> CardControl(CardObject selectCard = null, FieldLane selectLane = FieldLane.None) {
        // マウスカーソルのスクリーン座標取得
        Vector3 mousePos = Input.mousePosition;
        // スクリーン座標からレイを取得
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        // 左クリックでカードを掴む
        if (Input.GetMouseButtonDown(0)) {
            // 掴む
            CardCatch(ray);
            // 指定カードがあり、そのカードでないなら掴まない
            if (selectCard && selectCard != cardObject) {
                catchObject.transform.position = catchStartPos;
                catchObject = null;
                cardObject = null;
            }
        }

        // 掴んでいる時の処理
        bool cardAction = false;
        if (catchObject) {
            // 掴んでいるオブジェクトをマウスに追従
            CardChaseMouse(ray);

            // 掴んでいるオブジェクトを離す
            if (Input.GetMouseButtonUp(0)) {
                // フィールドレイヤー取得
                int fieldLayer = LayerMask.GetMask("Field");
                // フィールドとのレイキャスト
                if (Physics.Raycast(
                    ray, GameConst.RAY_RANGE_MAX, fieldLayer)) {
                    // カードがレーン上のものかどうか判定
                    if (cardObject.laneID > -1) {
                        // レーン交換
                        if (LaneChange(ray, selectLane)) {
                            cardAction = true;
                        }
                        else {
                            resetPos = true;
                        }
                    }
                    // カードが手札のものだった場合の処理
                    else if (cardObject.handID > -1) {
                        // カードを使用
                        await PlayCard(cardObject, selectLane);
                        cardAction = true;
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

                return cardAction;
            }
        }
        return false;
    }

    /// <summary>
    /// カードの使用
    /// </summary>
    /// <param name="card">使用するオブジェクトのID</param>
    /// <param name="selectLane">指定したいレーン(指定なしなら自由に操作)</param>
    /// <returns></returns>
    private async UniTask PlayCard(CardObject card, FieldLane selectLane = FieldLane.None) {
        if (!card) return;
        // カードタイプによって分岐
        switch (card.cardType) {
            case CardType.Monster:
                // 残りのサイズ取得
                int remainingSize = BattleSystemManager.instance.GetRemainingSize(PlayerType.Self);
                // サイズが上限超過しないか
                if (card.currentStatus[MonsterStatus.SIZE] <= remainingSize) {
                    FieldLane lane = FieldLane.None;
                    // レーンの指定がある場合はそのレーンを選ぶまで繰り返す
                    while (lane == FieldLane.None) {
                        // レーンの指定がある場合は矢印を出す
                        if (selectLane != FieldLane.None) {
                            BattleSystemManager.instance.TutorialLaneArrow(selectLane);
                        }
                        // レーン設置ウィンドウを出す
                        lane = await MenuWindowManager.Instance.EmptyLaneMenuOpen(PlayerType.Self);
                        // 違うレーンを選んだらクリア
                        if (selectLane != FieldLane.None && lane != selectLane) {
                            lane = FieldLane.None;
                        }
                    }
                    // レーンの選択ができていれば
                    if (lane != FieldLane.None) {
                        // モンスター召喚
                        await BattleSystemManager.instance.SummonMonster(PlayerType.Self, card, lane);
                        Debug.Log("モンスター召喚！！");
                    }
                    else {
                        resetPos = false;
                        Debug.Log("レーンの選択ができませんでした");
                    }
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
            GameConst.RAY_RANGE_MAX, cardLayer)
            ) {
            if (!hit.collider) return;
            // 触れているオブジェクトを保存
            catchObject = hit.collider.gameObject;
            // オブジェクトのクラス取得
            cardObject = catchObject.GetComponent<CardObject>();
            // 相手のカードなら掴まない
            // 手札が縮小状態で手札は掴まない
            if (cardObject.playerType != PlayerType.Self ||
                cardObject.handID >= 0 && CardObjectManager.instance.isHandShrink) {
                catchObject = null;
                cardObject = null;
                return;
            }
            // 位置を保存
            catchStartPos = catchObject.transform.position;
            // 少し手前側に出す
            catchObject.transform.position = new Vector3(
                catchStartPos.x,
                catchStartPos.y,
                catchStartPos.z - CATCHI_UP
                );
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
        // レイを出す位置も少し手前に
        ray.origin = new Vector3(ray.origin.x, ray.origin.y, ray.origin.z - CATCHI_UP);
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
    /// <param name="selectLane">指定したいレーン(指定なしなら自由に操作)</param>
    private bool LaneChange(Ray ray, FieldLane selectLane = FieldLane.None) {
        // レーンレイヤー取得
        int laneLayer = LayerMask.GetMask("Lane");
        // レーンのレイキャスト
        if (Physics.Raycast(
        ray, out RaycastHit hit,
        GameConst.RAY_RANGE_MAX, laneLayer)) {
            // レーン番号の取得
            FieldLane lane = CardObjectManager.instance.GetObjectLane(hit.transform);
            // レーンの指定がある場合はそのレーン以外なら交換しない
            if (selectLane != FieldLane.None && selectLane != lane) return false;
            // 移動先が同じレーンなら交換しない
            if ((FieldLane)cardObject.laneID == lane) return false;
            // レーンの位置変更
            BattleSystemManager.instance.LaneChange(PlayerType.Self, (FieldLane)cardObject.laneID, lane);
            return true;
        }
        return false;
    }
}
