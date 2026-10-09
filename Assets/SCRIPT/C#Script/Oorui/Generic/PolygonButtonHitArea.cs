using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// 多角形の内側だけをクリック可能にするUI判定クラス。
/// 頂点はRectTransformのローカル座標で管理する。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PolygonButtonHitArea : MonoBehaviour, ICanvasRaycastFilter {
    // 頂点座標データ配列
    [SerializeField]
    private Vector2[] points ={
        // 初期状態は四角形
        new Vector2(-100f, -50f),
        new Vector2(-100f,  50f),
        new Vector2( 100f,  50f),
        new Vector2( 100f, -50f)
    };

    /// <summary>
    /// InspectorやEditorから頂点配列を取得する。
    /// </summary>
    public Vector2[] Points => points;

    /// <summary>
    /// 指定された画面座標がクリック可能な領域内か判定する。
    /// </summary>
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
        // UIの画面座標をRectTransformのローカル座標に変換する。
        RectTransform rectTransform = (RectTransform)transform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            screenPoint,
            eventCamera,
            out Vector2 localPoint)) {
            return false;
        }

        // 頂点が3個未満なら有効な多角形ではない。
        if (points == null || points.Length < 3) {
            return false;
        }

        // 多角形の内側だけをクリック可能にする。
        return IsInsidePolygon(localPoint);
    }

    /// <summary>
    /// レイキャスティング法で点が多角形の内側か判定する。
    /// </summary>
    private bool IsInsidePolygon(Vector2 point) {
        bool inside = false;

        for (int i = 0, j = points.Length - 1; i < points.Length; j = i++) {
            Vector2 a = points[i];
            Vector2 b = points[j];

            // 頂点間の辺が、判定点の高さを横切るか調べる。
            if ((a.y > point.y) != (b.y > point.y)) {
                // 辺と水平線が交差するX座標を求める。
                float crossingX = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;

                // 交差回数を数えて内側・外側を判定する。
                if (point.x < crossingX) {
                    inside = !inside;
                }
            }
        }

        return inside;
    }
}
