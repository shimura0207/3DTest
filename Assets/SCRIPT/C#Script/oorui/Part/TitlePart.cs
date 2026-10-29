/*
 *  @file   TitlePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
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
        // 基底側の処理を呼ぶ
        await base.Setup();
        start.onClick.AddListener(OnTitleSelected);
        // BGM再生
        AudioManager.instance.PlayBGM(TITLE_BGMNAME);
        // タイトルロゴの点滅を開始
        FlashingImage().Forget();
        await UniTask.CompletedTask;
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
        // BGMを再生

        // 選択状態を初期化
        selectMenu = MainMenuSelect.None;

        // メニューが選択されるまで待機する
        while (selectMenu == MainMenuSelect.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }
        // 再生中のBGMを停止する
        AudioManager.instance.FadeOutBGM(0.2f);
        // メニューが消える前にフェードアウトする
        await FadeManager.Instance.FadeOut(FadeType.Black, 0.5f);
        // メインメニュー画面に遷移
        await PartManager.Instance.TransitionPart(GamePart.MainMenu);
    }

    /// <summary>
    /// 仮：画像を点滅させる
    /// </summary>
    private async UniTask FlashingImage() {

        // 画像の透明度を管理する変数
        float alpha = 1.0f;

        // 透明度を減少・増加させる方向
        bool isFadeOut = true;

        // パートが破棄されるまで点滅を繰り返す
        while (true) {
            // 経過時間を管理する
            float elapsedTime = 0.0f;

            // 現在の透明度を保存する
            float startAlpha = alpha;

            // 目標の透明度を設定する
            float targetAlpha = isFadeOut ? 0.3f : 1.0f;

            // 指定時間をかけて透明度を変化させる
            while (elapsedTime < FLASH_DURATION) {
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
                await UniTask.Yield();
            }

            // 透明・不透明の切り替え
            alpha = targetAlpha;
            isFadeOut = !isFadeOut;
        }
    }

}
