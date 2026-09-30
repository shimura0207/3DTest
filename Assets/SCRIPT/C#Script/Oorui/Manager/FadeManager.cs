/*
 * file FadeManager
 * @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static CommonModule;
/// <summary>
/// フェード管理
/// </summary>
public class FadeManager : SystemObject {
    // 自身への参照
    public static FadeManager Instance { get; private set; }

    // フェード用画像リスト
    [SerializeField] private List<Image> fadeImageList = null;
    // フェード時間の定数
    private const float _DEFAULT_FADE_DURATION = 1f;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        Instance = this;
        if (fadeImageList == null) fadeImageList = new List<Image>();
        // すべてのフェード画像を透明かつ非表示にする
        foreach (var image in fadeImageList) {
            if (image == null) continue;

            Color color = image.color;
            color.a = 0f;
            image.color = color;
            image.gameObject.SetActive(false);
        }
        await UniTask.CompletedTask;
    }


    /// <summary>
    /// フェードアウト（画面を暗くする）
    /// </summary>
    public async UniTask FadeOut(
        FadeType type = FadeType.Black,
        float duration = _DEFAULT_FADE_DURATION) {

        // フェード対象の画像を取得
        var image = GetImageByType(type);
        if (image == null) return;

        // 対象画像のみを有効化
        SetImageActiveOnly(type);

        // 現在のアルファ値からフェードアウトを開始
        await FadeTargetAlpha(image, 1.0f, duration);
    }

    /// <summary>
    /// フェードイン（画面を明るくする）
    /// </summary>
    public async UniTask FadeIn(
        FadeType type = FadeType.Black,
        float duration = _DEFAULT_FADE_DURATION) {

        // フェード対象の画像を取得
        var image = GetImageByType(type);
        if (image == null) return;

        // 対象画像のみを有効化
        SetImageActiveOnly(type);

        // 不透明な状態からフェードインを開始
        Color color = image.color;
        color.a = 1.0f;
        image.color = color;

        // 透明になるまでフェード
        await FadeTargetAlpha(image, 0.0f, duration);

        // フェード完了後に画像を非表示にする
        image.gameObject.SetActive(false);
    }



    /// <summary>
    /// フェード画像を指定の不透明度に変化させる
    /// </summary>
    /// <param name="image"></param>
    /// <param name="targetAlpha"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private async UniTask FadeTargetAlpha(Image image, float targetAlpha, float duration) {
        // 経過時間
        float elapsedTime = 0f;
        float startAlpha = image.color.a;
        Color color = image.color;

        while (elapsedTime < duration) {
            // フレーム経過時間
            elapsedTime += Time.deltaTime;
            // 補間した不透明度をフェード画像に設定
            float t = elapsedTime / duration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            image.color = color;
            // 5フレーム待ち
            await UniTask.DelayFrame(1);
        }
        color.a = targetAlpha;
        image.color = color;
    }

    /// <summary>
    /// フェード対象の選択
    /// </summary>
    /// <param name="activeType"></param>
    private void SetImageActiveOnly(FadeType activeType) {
        for (int i = 0; i < fadeImageList.Count; i++) {
            var image = fadeImageList[i];
            if (image == null) continue;

            image.gameObject.SetActive(i == (int)activeType);
        }
    }

    /// <summary>
    /// 使用するフェード画像の取得
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private Image GetImageByType(FadeType type) {
        int index = (int)type;
        // リストに使えるインデックスか走査
        if (!IsEnableIndex(fadeImageList, index)) return null;
        // 使用するフェード画像を取得
        return fadeImageList[index];
    }
}
