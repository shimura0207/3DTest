/*
 *  @file TextWindowMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class TextWindowMenu : MenuWindowBase {
    // メニューを閉じるボタン
    [SerializeField] private Button closeWindowButton;
    // 受けっとたメッセージを表示するテキスト
    [SerializeField] private TMP_Text messageText;
    // クリックを促す矢印画像
    [SerializeField] private Image clickArrowImage;

    // 1文字を表示する間隔
    private float characterInterval = 0.05f;

    // テキストの表示が完了したか
    private bool isTextComplete = false;

    // 矢印の上下移動幅
    [SerializeField] private float arrowMoveDistance = 10f;

    // 上下移動にかかる時間
    [SerializeField] private float arrowMoveDuration = 0.5f;

    // 矢印のRectTransform
    private RectTransform arrowRectTransform;

    // 矢印の初期位置
    private Vector2 arrowDefaultPosition;

    // 矢印アニメーションのキャンセル用トークン
    private CancellationTokenSource arrowAnimationTokenSource;

    // ボタンが選択されるまで待機するための完了通知
    private UniTaskCompletionSource<bool> completionSource;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // 処理登録
        closeWindowButton.onClick.RemoveListener(OnClickCloseButton);
        closeWindowButton.onClick.AddListener(OnClickCloseButton);

        // 矢印のRectTransformを取得
        arrowRectTransform = clickArrowImage.GetComponent<RectTransform>();

        // 初期位置を保存
        arrowDefaultPosition = arrowRectTransform.anchoredPosition;
    }

    /// <summary>
    /// ウィンドウ表示
    /// </summary>
    /// <param name="message">表示したいテキスト</param>
    /// <returns></returns>
    public async UniTask Open(string message) {
        // ウィンドウを表示
        await base.Open();

        // テキストを初期化
        messageText.text = string.Empty;

        // 表示完了フラグを初期化
        isTextComplete = false;

        // クリック矢印を非表示
        clickArrowImage.gameObject.SetActive(false);

        // 1文字ずつテキストを表示
        foreach (char character in message) {

            // 1文字追加
            messageText.text += character;

            // 指定時間待機
            await UniTask.Delay(TimeSpan.FromSeconds(characterInterval));
        }

        // テキスト表示完了
        isTextComplete = true;

        // クリック矢印を表示
        clickArrowImage.gameObject.SetActive(true);
        // 既存のアニメーションを停止
        arrowAnimationTokenSource?.Cancel();
        arrowAnimationTokenSource?.Dispose();

        // 新しいキャンセルトークンを作成
        arrowAnimationTokenSource = new CancellationTokenSource();

        // 矢印のアニメーションを開始
        ArrowAnimation(arrowAnimationTokenSource.Token).Forget();

        // ボタンが押されるまで待機
        completionSource = new UniTaskCompletionSource<bool>();
        // ボタンが押されるまでウィンドウを開いたままにする
        await completionSource.Task;

        // ボタンが押されたらウィンドウを閉じる
        await Close();
    }

    /// <summary>
    /// ウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override UniTask Close() {
        // 矢印アニメーションを停止
        arrowAnimationTokenSource?.Cancel();
        arrowAnimationTokenSource?.Dispose();
        arrowAnimationTokenSource = null;

        // 矢印を非表示
        clickArrowImage.gameObject.SetActive(false);

        // 待機中の処理があれば終了
        completionSource?.TrySetResult(false);

        return base.Close();
    }

    /// <summary>
    /// 画面をクリックされた時の処理
    /// </summary>
    private void OnClickCloseButton() {

        // テキスト表示中は何もしない
        if (!isTextComplete) {
            return;
        }

        // 待機処理を終了
        completionSource?.TrySetResult(false);
    }

    /// <summary>
    /// 矢印を上下に移動させる処理
    /// </summary>
    private async UniTask ArrowAnimation(CancellationToken token) {

        try {
            while (!token.IsCancellationRequested &&
                   clickArrowImage != null &&
                   clickArrowImage.gameObject.activeSelf) {

                // 下から上へ移動
                await MoveArrow(arrowDefaultPosition.y + arrowMoveDistance, token);

                // 上から下へ移動
                await MoveArrow(arrowDefaultPosition.y - arrowMoveDistance, token);
            }
        }
        catch (OperationCanceledException) {
            // キャンセルされた場合は正常終了
        }
    }


    /// <summary>
    /// 指定したY座標まで矢印を移動させる
    /// </summary>
    private async UniTask MoveArrow(float targetY, CancellationToken token) {

        // 移動開始位置
        Vector2 startPosition = arrowRectTransform.anchoredPosition;

        // 経過時間
        float elapsedTime = 0f;

        while (elapsedTime < arrowMoveDuration) {

            // キャンセルされた場合は終了
            token.ThrowIfCancellationRequested();

            // オブジェクトが破棄されていたら終了
            if (arrowRectTransform == null) {
                return;
            }

            // 経過時間を更新
            elapsedTime += Time.deltaTime;

            // 移動割合を計算
            float progress = Mathf.Clamp01(elapsedTime / arrowMoveDuration);

            // なめらかに移動
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // 矢印の位置を更新
            arrowRectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                new Vector2(startPosition.x, targetY),
                smoothProgress
            );

            await UniTask.Yield(token);
        }
    }


    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() {

        // 実行中のアニメーションを停止
        arrowAnimationTokenSource?.Cancel();
        arrowAnimationTokenSource?.Dispose();
        arrowAnimationTokenSource = null;
    }
}
