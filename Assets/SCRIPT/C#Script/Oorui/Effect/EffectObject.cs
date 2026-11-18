/*
 * @file    EffectObject
 * @author  oorui
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static CommonModule;

/// <summary>
/// エフェクト毎の処理
/// </summary>
public class EffectObject : MonoBehaviour {
    // エフェクトのオリジナルリスト
    [SerializeField]
    private List<GameObject> originEffectList = null;

    // 使用中のエフェクトのオブジェクトリスト
    private List<List<GameObject>> useEffectObjectsList = null;

    // 未使用のエフェクトオブジェクトリスト
    private List<List<GameObject>> unuseEffectObjectsList = null;

    // 使用オブジェクトの親オブジェクト
    [SerializeField]
    private Transform useObjectRoot = null;
    // 未使用オブジェクトの親オブジェクト
    [SerializeField]
    private Transform unuseObjectRoot = null;

    // エフェクトのオブジェクトの生成数
    public const int GENERATE_OBJECTS_MAX = 16;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize() {
        // 各エフェクトオブジェクトの準備
        unuseEffectObjectsList = new List<List<GameObject>>((int)EffectType.max);
        for (int i = 0, max = (int)EffectType.max; i < max; i++) {
            unuseEffectObjectsList.Add(new List<GameObject>(GENERATE_OBJECTS_MAX));
            for (int obectNum = 0; obectNum < GENERATE_OBJECTS_MAX; obectNum++) {
                unuseEffectObjectsList[i].Add(Instantiate(originEffectList[i], unuseObjectRoot));
            }
        }
        useEffectObjectsList = new List<List<GameObject>>((int)EffectType.max);
        for (int i = 0, max = (int)EffectType.max; i < max; i++) {
            useEffectObjectsList.Add(new List<GameObject>(GENERATE_OBJECTS_MAX));
        }
    }


    /// <summary>
    /// エフェクトを使用状態にする
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public GameObject UseEffect(EffectType effect, Vector3 setPosition, Transform setParent) {
        // setParentがnullなら_useObjectRootに
        Transform effectParent = setParent ?? useObjectRoot;
        // 使用可能なエフェクトの取得
        GameObject useObject = GetUsableEffectObject(effect);
        useEffectObjectsList[(int)effect].Add(useObject);
        useObject.transform.position = setPosition;
        useObject.transform.SetParent(effectParent);
        return useObject;
    }


    /// <summary>
    /// エフェクトを未使用状態にする
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public void UnuseEffect(int effect, int number) {
        GameObject result = useEffectObjectsList[effect][number];
        unuseEffectObjectsList[effect].Add(result);
        useEffectObjectsList[effect].RemoveAt(number);
        result.transform.SetParent(unuseObjectRoot);

    }

    /// <summary>
    /// 未使用状態のエフェクト取得
    /// </summary>
    /// <param name="effectType"></param>
    /// <returns></returns>
    private GameObject GetUsableEffectObject(EffectType effectType) {
        int effect = (int)effectType;
        if (IsEmpty(unuseEffectObjectsList[effect])) return Instantiate(originEffectList[effect]);

        GameObject result = unuseEffectObjectsList[effect][0];
        unuseEffectObjectsList[effect].RemoveAt(0);
        return result;
    }

}
