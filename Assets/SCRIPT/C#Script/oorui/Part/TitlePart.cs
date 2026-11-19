/*
 *  @file   TitlePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトルパート
/// </summary>
public class TitlePart : PartBase {


    // スタートボタン
    [SerializeField] private Button start;

    // 選択された遷移先
    private MainMenuSelect selectMenu = MainMenuSelect.Title;
    // 流すBGM
    private const string TITLE_BGMNAME = "TitleBGM";
    // タイトルロゴ
    [SerializeField] private Image titleLogo;

    // 点滅速度
    private const float FLASH_DURATION = 0.85f;
    // タイトルロゴの点滅処理を管理するキャンセルトークン
    private CancellationTokenSource flashTokenSource;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 基底側の処理を呼ぶ
        await base.Initialize();
    }

    /// <summary>
    /// 使用前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        // 基底クラスの処理
        await base.Setup();

        // ボタンイベントの重複登録を防ぐ
        start.onClick.RemoveListener(OnTitleSelected);
        start.onClick.AddListener(OnTitleSelected);

        // BGM再生
        AudioManager.instance.PlayBGM(TITLE_BGMNAME);

        // フェードイン
        await FadeManager.Instance.FadeIn(FadeType.Black);

        // 点滅処理用のキャンセルトークンを作成
        flashTokenSource?.Cancel();
        flashTokenSource?.Dispose();
        flashTokenSource = new CancellationTokenSource();

        // タイトルロゴの点滅を開始
        FlashingImage(flashTokenSource.Token).Forget();
    }


    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnTitleSelected() {
        // 遷移先をタイトルに設定する
        selectMenu = MainMenuSelect.Title;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // 選択状態を初期化
        selectMenu = MainMenuSelect.None;

        // 入力待ちを繰り返す
        while (true) {

            // Escapeキーの入力状態
            bool isEscapePressed = false;

            // スタートボタンが押されるか、Escapeキーが押されるまで待機
            await UniTask.WaitUntil(() => {

                // Escapeキーの入力を確認
                isEscapePressed = Input.GetKeyDown(KeyCode.Escape);

                return selectMenu != MainMenuSelect.None || isEscapePressed;
            });

            // Escapeキーが押された場合
            if (isEscapePressed) {

                // ゲームを終了するか確認する
                bool isGameEnd = await MenuWindowManager.Instance.Confirm("ゲームを終了しますか？");

                // 「いいえ」が選択された場合
                if (!isGameEnd) continue;
                // 再生中のBGMを停止する
                AudioManager.instance.FadeOutBGM(0.2f);

                // 点滅処理を停止する
                flashTokenSource?.Cancel();

                // ゲームを終了する
                QuitGame();

                return;
            }

            // スタートボタンが押された場合

            // 再生中のBGMを停止する
            AudioManager.instance.FadeOutBGM(0.2f);

            // ロゴの点滅処理を停止する
            flashTokenSource?.Cancel();

            // 画面遷移前にフェードアウトする
            await FadeManager.Instance.FadeOut(FadeType.Black, 0.5f);

            // メインメニュー画面へ遷移する
            await PartManager.Instance.TransitionPart(GamePart.MainMenu);

            return;
        }
    }

    /// <summary>
    /// 画像を点滅させる
    /// </summary>
    /// <param name="token">キャンセルトークン</param>
    private async UniTask FlashingImage(CancellationToken token) {

        // 画像の透明度を管理する変数
        float alpha = 1.0f;

        // 透明度を減少・増加させる方向
        bool isFadeOut = true;

        // キャンセルされるまで点滅を繰り返す
        while (!token.IsCancellationRequested && titleLogo != null) {

            // 経過時間を管理する
            float elapsedTime = 0.0f;

            // 現在の透明度を保存する
            float startAlpha = alpha;

            // 目標の透明度を設定する
            float targetAlpha = isFadeOut ? 0.3f : 1.0f;

            // 指定時間をかけて透明度を変化させる
            while (elapsedTime < FLASH_DURATION &&
                   !token.IsCancellationRequested &&
                   titleLogo != null) {

                elapsedTime += Time.deltaTime;

                // 0～1の割合で透明度を補間する
                float progress = Mathf.Clamp01(elapsedTime / FLASH_DURATION);

                // 現在の透明度を計算する
                alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

                // 画像に透明度を反映する
                Color color = titleLogo.color;
                color.a = alpha;
                titleLogo.color = color;

                // 次のフレームまで待機する
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // キャンセルまたは画像が破棄された場合は終了する
            if (token.IsCancellationRequested || titleLogo == null) {
                break;
            }

            // 透明・不透明の切り替え
            alpha = targetAlpha;
            isFadeOut = !isFadeOut;
        }
    }

    /// <summary>
    /// ゲームを終了する
    /// </summary>
    private void QuitGame() {

        // ゲームを終了する
        Application.Quit();

#if UNITY_EDITOR
        // Unityエディター上の場合は再生モードを終了する
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    public override async UniTask Teardown() {

        // ロゴの点滅処理を停止する
        if (flashTokenSource != null) {

            // 非同期処理をキャンセルする
            flashTokenSource.Cancel();

            // トークンソースを破棄する
            flashTokenSource.Dispose();
            flashTokenSource = null;
        }

        // ボタンイベントを解除する
        start.onClick.RemoveListener(OnTitleSelected);

        // 基底クラスの片付け処理
        await base.Teardown();
    }

}
