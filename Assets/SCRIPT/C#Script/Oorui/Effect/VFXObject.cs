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
public class VFXObject : MonoBehaviour {
    // エフェクトのオリジナルリスト
    [SerializeField]
    private List<GameObject> originVFXtList = null;

    // 使用中のエフェクトのオブジェクトリスト
    private List<List<GameObject>> useVFXObjectsList = null;

    // 未使用のエフェクトオブジェクトリスト
    private List<List<GameObject>> unuseVFXObjectsList = null;

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
        unuseVFXObjectsList = new List<List<GameObject>>((int)VFXType.max);
        for (int i = 0, max = (int)VFXType.max; i < max; i++) {
            unuseVFXObjectsList.Add(new List<GameObject>(GENERATE_OBJECTS_MAX));
            for (int obectNum = 0; obectNum < GENERATE_OBJECTS_MAX; obectNum++) {
                unuseVFXObjectsList[i].Add(Instantiate(originVFXtList[i], unuseObjectRoot));
            }
        }
        useVFXObjectsList = new List<List<GameObject>>((int)VFXType.max);
        for (int i = 0, max = (int)VFXType.max; i < max; i++) {
            useVFXObjectsList.Add(new List<GameObject>(GENERATE_OBJECTS_MAX));
        }
    }


    /// <summary>
    /// エフェクトを使用状態にする
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public GameObject UseEffect(VFXType effect, Vector3 setPosition, Transform setParent) {
        // setParentがnullなら_useObjectRootに
        Transform effectParent = setParent ?? useObjectRoot;
        // 使用可能なエフェクトの取得
        GameObject useObject = GetUsableEffectObject(effect);
        useVFXObjectsList[(int)effect].Add(useObject);
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
        GameObject result = useVFXObjectsList[effect][number];
        unuseVFXObjectsList[effect].Add(result);
        useVFXObjectsList[effect].RemoveAt(number);
        result.transform.SetParent(unuseObjectRoot);

    }

    /// <summary>
    /// 未使用状態のエフェクト取得
    /// </summary>
    /// <param name="effectType"></param>
    /// <returns></returns>
    private GameObject GetUsableEffectObject(VFXType effectType) {
        int effect = (int)effectType;
        if (IsEmpty(unuseVFXObjectsList[effect])) return Instantiate(originVFXtList[effect]);

        GameObject result = unuseVFXObjectsList[effect][0];
        unuseVFXObjectsList[effect].RemoveAt(0);
        return result;
    }

}
