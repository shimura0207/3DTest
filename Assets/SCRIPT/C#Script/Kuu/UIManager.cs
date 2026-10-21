/*
 * @file    UIManager.cs
 * @author  kuu
 */
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
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

    public bool mouseButton { get; private set; } = true;

    // レイキャストの最大距離
    private const float RAY_RANGE_MAX = 10.0f;

    public override async UniTask Initialize() {
        instance = this;

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    public void Execute() {
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
                ray, out RaycastHit hit,
                RAY_RANGE_MAX, cardLayer)
                ) {
                if (!hit.collider) return;
                // 触れているオブジェクトを保存
                catchObject = hit.collider.gameObject;
                // オブジェクトのクラス取得
                cardObject = catchObject.GetComponent<CardObject>();
                // IDからステータスを取得
                cardData = CardDataMasterUtility.GetCardMaster(cardObject.cardID);
            }
            else {
                cardData = null;
            }
        }
    }
}

