using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;


/// <summary>
/// ScriptableObjectからデータを取得する管理クラス
/// </summary>
public class ScriptableObjectManager : SystemObject {
    // 自身への参照
    public static ScriptableObjectManager instance { get; private set; }

    // カードのイラストデータ
    [SerializeField]
    private CardImageDatabase cardImageDatabase = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        instance = this;

    }


    public Sprite GetCardImage(int cardID) {
        cardImageDatabase.GetCardImage(cardID);
        return null;
    }


}
