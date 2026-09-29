/*
 *  @file   CardImageDetabaseEditor.cs
 *  @author Riku
 */
using UnityEngine;
using UnityEditor;

/// <summary>
/// CardImageDatabaseのエディター拡張
/// </summary>
[CustomEditor(typeof(CardImageDatabase))]
public class CardImageDatabaseEditor : Editor {
    // 入力されたID
    private int inputID = -1;
    // 入力されたスプライト
    private Sprite inputSprite = null;

    public override void OnInspectorGUI() {
        CardImageDatabase database = (CardImageDatabase)target;

        // ID設定
        inputID = EditorGUILayout.IntField("CardID", inputID);

        // Sprite設定
        inputSprite = (Sprite)EditorGUILayout.ObjectField(
            "Image", 
            inputSprite, 
            typeof(Sprite), 
            false
            );

        // 追加ボタン
        if (GUILayout.Button("AddList")) {
            // 設定したデータを追加
            database.AddCardImageDataList(inputID, inputSprite);

            // 設定した値をリセット
            inputID = -1;
            inputSprite = null;
        }
    }
}
