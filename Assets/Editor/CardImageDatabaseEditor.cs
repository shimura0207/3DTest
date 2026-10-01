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
            if (inputID != -1 && inputSprite) {

                // 設定したデータを追加
                database.AddCardImageDataList(inputID, inputSprite);

                // 変更をアセットに保存
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();

                // 設定した値をリセット
                inputID = -1;
                inputSprite = null;
            }
            else {
                // IDかSpriteが入っていなければ警告
                Debug.LogWarning("入力データが十分ではありません。");
            }
        }

        // 追加ボタンとリスト表示を分けるバー表示
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        int index = -1;
        bool delete = false;
        foreach (var data in database.CardImageDataList) {
            // 登録されているカードのIDと見た目を表示
            EditorGUILayout.ObjectField(
                "ID：" + data.cardID.ToString(),
                data.cardImage,
                typeof(Sprite),
                false);

            // 登録されているデータを削除するボタン
            index++;
            if (GUILayout.Button("DeleteData")) {
                // データ削除予約
                delete = true;
                break;
            }
        }
        // 削除
        if (delete) {
            database.DeleteCardImageData(index);
            // 変更をアセットに保存
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }
}
