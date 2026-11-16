/*
 *  @file   ArrowCreate
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 指定位置に矢印を出すクラス
/// </summary>
public class ArrowCreate {

    // 矢印の移動距離
    private const float MOVE_DISTANCE = 15f;

    // 矢印の移動速度
    private const float MOVE_SPEED = 2f;

    // 矢印Prefab
    private GameObject arrowPrefab = null;

    // 矢印の生成先
    private Transform arrowParent = null;

    // 生成した矢印オブジェクト
    private GameObject arrowObject = null;

    // アニメーションのキャンセルトークン
    private CancellationTokenSource cancellationTokenSource = null;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="prefab">矢印Prefab</param>
    /// <param name="parent">矢印の生成先</param>
    public ArrowCreate(GameObject prefab, Transform parent) {
        arrowPrefab = prefab;
        arrowParent = parent;
    }

    /// <summary>
    /// 上下に動く矢印を生成する
    /// </summary>
    /// <param name="localPosition">生成位置</param>
    /// <param name="rotationZ">Z軸の回転角度</param>
    public void CreateVerticalArrow(Vector2 localPosition, float rotationZ = 0f) {
        CreateArrow(localPosition, Vector2.up, rotationZ);
    }

    /// <summary>
    /// 左右に動く矢印を生成する
    /// </summary>
    /// <param name="localPosition">生成位置</param>
    /// <param name="rotationZ">Z軸の回転角度</param>
    public void CreateHorizontalArrow(Vector2 localPosition, float rotationZ = 0f) {
        CreateArrow(localPosition, Vector2.right, rotationZ);
    }
    /// <summary>
    /// 矢印を生成する共通処理
    /// </summary>
    private void CreateArrow(Vector2 localPosition, Vector2 direction, float rotationZ) {
        // 既存の矢印を削除
        DeleteArrow();

        // 親を指定して矢印を生成
        arrowObject = Object.Instantiate(arrowPrefab, arrowParent, false);

        // RectTransformを取得
        RectTransform rectTransform = arrowObject.GetComponent<RectTransform>();

        // UIの表示位置を設定
        rectTransform.anchoredPosition = localPosition;

        // スケールを初期化
        rectTransform.localScale = Vector3.one;

        // 指定された角度で回転
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        // 最前面に表示
        arrowObject.transform.SetAsLastSibling();

        // アニメーション開始
        cancellationTokenSource = new CancellationTokenSource();

        MoveArrowAsync(
            rectTransform,
            direction,
            cancellationTokenSource.Token
        ).Forget();
    }

    /// <summary>
    /// 矢印の移動アニメーション
    /// </summary>
    private async UniTask MoveArrowAsync(
        RectTransform rectTransform,
        Vector2 direction,
        CancellationToken token) {

        // 初期位置を取得
        Vector2 startPosition = rectTransform.anchoredPosition;

        try {

            while (!token.IsCancellationRequested) {

                // RectTransformが破棄されていたら終了
                if (rectTransform == null) break;

                // サインによって矢印を往復移動させる
                float offset = Mathf.Sin(Time.unscaledTime * MOVE_SPEED) * MOVE_DISTANCE;

                // UI座標を更新
                rectTransform.anchoredPosition = startPosition + direction * offset;

                // 次のフレームまで待機
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (System.OperationCanceledException) {

        }
    }

    /// <summary>
    /// 矢印を削除する
    /// </summary>
    public void DeleteArrow() {

        // アニメーションを停止
        if (cancellationTokenSource != null) {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        // 矢印を削除
        if (arrowObject != null) {
            Object.Destroy(arrowObject);
            arrowObject = null;
        }
    }
}