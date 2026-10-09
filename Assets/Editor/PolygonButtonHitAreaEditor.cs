using UnityEditor;
using UnityEngine;

/// <summary>
/// Sceneビューで頂点を編集する希望
/// </summary>
[CustomEditor(typeof(PolygonButtonHitArea))]
public class PolygonButtonHitAreaEditor : Editor {
    // 頂点ハンドルと輪郭線の表示色
    private static readonly Color HandleColor = new Color(1f, 0.25f, 0.2f, 1f);
    private static readonly Color FillColor = new Color(1f, 0.5f, 0.2f, 0.12f);


    /// <summary>
    /// Inspectorに頂点編集用の操作ボタンを追加する
    /// </summary>
    public override void OnInspectorGUI() {
        // points配列など、通常のInspector項目を表示
        DrawDefaultInspector();

        PolygonButtonHitArea area = (PolygonButtonHitArea)target;

        EditorGUILayout.Space();

        // 頂点を追加するボタン
        if (GUILayout.Button("頂点を追加")) {
            Undo.RecordObject(area, "Add Polygon Point");

            Vector2[] oldPoints = area.Points;
            Vector2[] newPoints = new Vector2[oldPoints.Length + 1];

            // 既存の頂点を維持
            for (int i = 0; i < oldPoints.Length; i++) {
                newPoints[i] = oldPoints[i];
            }

            // 最後の頂点の近くに新しい頂点を追加
            newPoints[newPoints.Length - 1]
                = oldPoints.Length > 0
                ? oldPoints[oldPoints.Length - 1]
                + new Vector2(20f, -20f)
                : Vector2.zero;

            // SerializedObject経由で配列を更新する
            SerializedProperty pointsProperty = serializedObject.FindProperty("points");
            pointsProperty.arraySize = newPoints.Length;

            for (int i = 0; i < newPoints.Length; i++) {
                pointsProperty.GetArrayElementAtIndex(i).vector2Value = newPoints[i];
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(area);
        }

        // 頂点を削除するボタン
        if (GUILayout.Button("最後の頂点を削除")) {
            SerializedProperty pointsProperty = serializedObject.FindProperty("points");

            if (pointsProperty.arraySize > 0) {
                Undo.RecordObject(area, "Remove Polygon Point");
                pointsProperty.arraySize--;
                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(area);
            }
        }


        EditorGUILayout.HelpBox(
            "Sceneヴューで赤い頂点をどらっぎして形を調整できます。\n"
            + "頂点を3個以上必要です。",
            MessageType.Info
        );
    }

    private void OnSceneGUI() {
        PolygonButtonHitArea area = (PolygonButtonHitArea)target;

        RectTransform rectTransform = area.transform as RectTransform;

        if (rectTransform == null || area.Points == null) return;
        Vector2[] points = area.Points;

        if (points.Length == 0) return;

        // 頂点のローカル座標をワールド座標に変換する
        Vector3[] worldPoints = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++) {
            worldPoints[i] = rectTransform.TransformPoint(points[i]);
        }

        if (points.Length >= 3) {
            Handles.color = FillColor;

            // 多角形の輪郭を線で描画する
            Handles.DrawAAConvexPolygon(worldPoints);
        }

        // 多角形の輪郭を赤い線で描画する
        Handles.color = HandleColor;

        if (points.Length >= 2) {
            Vector3[] closePoints = new Vector3[worldPoints.Length + 1];

            for (int i = 0; i < worldPoints.Length; i++) {
                closePoints[i] = worldPoints[i];
            }

            // 最後の頂点から最初の頂点へ線をつなぐ。
            closePoints[closePoints.Length - 1] = worldPoints[0];

            Handles.DrawAAPolyLine(3f, closePoints);
        }

        for (int i = 0; i < points.Length; i++) {
            EditorGUI.BeginChangeCheck();

            Handles.color = HandleColor;

            Vector3 movedWorldPoint = Handles.FreeMoveHandle(
                worldPoints[i],
                HandleUtility.GetHandleSize(worldPoints[i]) * 0.08f,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(area, "MovePolygon Point");

                points[i] = rectTransform.InverseTransformPoint(movedWorldPoint);

                // 頂点の変更を保存する
                SerializedProperty pointsProperty = serializedObject.FindProperty("points");

                pointsProperty.GetArrayElementAtIndex(i).vector2Value = points[i];

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(area);
            }
            // 頂点番号をSceneビューに表示する。
            Handles.Label(
                worldPoints[i] + Vector3.up * 10f,
                $"P{i}");
        }
    }

}
