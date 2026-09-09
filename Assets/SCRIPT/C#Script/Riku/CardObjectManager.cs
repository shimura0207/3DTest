/*
 * @file    CardObjectManafer.cs
 * @author  Riku
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static CommonModule;

/// <summary>
/// カードオブジェクト管理クラス
/// </summary>
public class CardObjectManager : MonoBehaviour {
    // 自身への参照
    public static CardObjectManager instance { get; private set; } = null;

    // 使用中カードオブジェクトの親オブジェクト
    [SerializeField]
    private Transform useObjectRoot = null;
    // 未使用カードオブジェクトの親オブジェクト
    [SerializeField]
    private Transform unuseObjectRoot = null;
    // カードオブジェクトのオリジナル
    [SerializeField]
    private CardObject originObject = null;

    // 使用中のカードオブジェクトリスト
    private List<CardObject> useObjectList = null;
    // 未使用のカードオブジェクトリスト
    private List<CardObject> unuseObjectList = null;

    // ある程度の生成数
    private const int CARD_OBJECT_MAX = 30;

    // Start is called before the first frame update
    void Start() {
        instance = this;

        // カードオブジェクトをある程度生成して未使用状態にしておく
        useObjectList = new List<CardObject>;
        unuseObjectList = new List<CardObject>;
        for (int i = 0; i < CARD_OBJECT_MAX; i++) {
            unuseObjectList.Add(Instantiate(originObjectm, unuseObjectRoot));
            unuseObjectList[i].Initialize();
        }
    }

    // Update is called once per frame
    void Update() {

    }

    public CardObject UseCardObject() {
        // 使用可能なカードオブジェクトのインスタンスを取得
        CardObject useCard = GetUsableCardObject();
        // 使用可能なIDを取得して使用リストに追加
        int useID = -1;
        for (int i = 0, max = useObjectList.Count; i < useObjectList; i++) {

        }
    }

    /// <summary>
    /// 未使用状態のカードオブジェクト取得
    /// </summary>
    /// <returns></returns>
    private CardObject GetUsableCardObject() {
        if (IsEmpty(unuseObjectList)) return Instantiate(originObject);

        CardObject result = unuseObjectList[0];
        unuseObjectList.Remove(0);
        return result;
    }
}
