/*
 * @file    CardTextUI.cs
 * @autor   Kuu
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードテキストUIクラス
/// </summary>
public class NewBehaviourScript : MonoBehaviour
{
    // レイが当たったオブジェクトの情報
    private RaycastHit hitInfo;

    // Update is called once per frame
    void Update()
    {
        // マウスのスクリーン座標を取得
        Vector3 mousePosition = Input.mousePosition;

        // スクリーン座標をワールド空間に変換
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        // レイキャストを行い、何かに当たった場合
        if (Physics.Raycast(ray, out hitInfo))
        {
            // 当たったオブジェクトの名前を表示
            Debug.Log("当たったオブジェクト: " + hitInfo.collider.gameObject.name);
        }
        else
        {
            // 何も当たっていない場合
            Debug.Log("何も当たっていません");
        }


        if (Input.GetKeyDown(KeyCode.D)) {
            // ターン開始時ドロー
            RGDeckController slot; //呼ぶスクリプトにあだなつける
            GameObject obj = GameObject.Find("DeckContlol"); //Mangerっていうオブジェクトを探す
            slot = obj.GetComponent<RGDeckController>(); //付いているスクリプトを取得
            slot.DrawCardToHand();
        }
    }
}