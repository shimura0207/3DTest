/*
 * @file    CardAbilityDatabaseEditor
 * @author  Riku
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

/// <summary>
/// CardAbilityDatabaseのエディター拡張
/// </summary>
[CustomEditor(typeof(CardAbilityDatabase))]
public class CardAbilityDatabaseEditor : Editor {
    // 入力されたカードID
    private int inputCardID = -1;
    // 入力された効果番号
    private int inputAbilityID = -1;

    public override void OnInspectorGUI() {
        CardAbilityDatabase database = (CardAbilityDatabase)target;
        
        // カードID設定
        inputCardID = EditorGUILayout.IntField("CardID", inputCardID);
        // 効果番号設定
        inputAbilityID = EditorGUILayout.IntField("AbilityID", inputAbilityID);


    }
}
