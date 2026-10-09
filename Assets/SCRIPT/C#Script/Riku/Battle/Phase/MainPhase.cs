/*
 * @file    MainPhase.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// メインフェイズ
/// </summary>
public class MainPhase : PhaseBase {
    // つかんだオブジェクトを保持
    private GameObject catchObject = null;

    // レイキャストの最大距離
    private const float RAY_RANGE_MAX = 10.0f;
    
    /// <summary>
    /// 自身のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask SelfExecute() {
        // マウスカーソルのスクリーン座標取得
        Vector3 mousePos = Input.mousePosition;
        // スクリーン座標からレイを取得
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        // 左クリックでカードを掴む
        if (Input.GetMouseButtonDown(0)) {
            // カードオブジェクトレイヤー取得
            int cardLayer = LayerMask.GetMask("CardObject");
            // レイキャストでカードを触れているか判定
            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                RAY_RANGE_MAX,
                cardLayer)) {
                if (!hit.collider) return;
                // 触れているオブジェクトを保存
                catchObject = hit.collider.gameObject;
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * RAY_RANGE_MAX, Color.red);

        // 掴んでいる時の処理
        if (catchObject) {
            // 掴んでいるオブジェクトをマウスに追従
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

            // 掴んでいるオブジェクトを離す
            if (Input.GetMouseButtonUp(0)) {
                catchObject.transform.localPosition = Vector3.zero;
                catchObject = null;
            }
        }
            //await UniTask.DelayFrame(30);
            //GameObject hand = GameObject.Find("HANDCanvas"); //Mangerっていうオブジェクトを探す
            //GameObject deck = GameObject.Find("DeckContlol"); //Mangerっていうオブジェクトを探す
            //
            //if (hand != null && deck != null) {
            //    hand.SetActive(true);
            //    deck.SetActive(true);
            //}
            //
            nextPhase = RGDeckController.i;
        nextPhase = Input.GetKeyDown(KeyCode.Space);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 相手のターン処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask OpponentExecute() {
        // 次のフェイズへ
        nextPhase = true;

        await UniTask.DelayFrame(30);
        await UniTask.CompletedTask;
    }
}
