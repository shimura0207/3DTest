using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class EfectManager : MonoBehaviour {
    [Header("通常画像")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Material noiseMaterial;

    private Coroutine noiseCoroutine;


    // =========================================================
    // トランプ
    // 0 = 左
    // 1 = 中
    // 2 = 右
    // =========================================================

    [Header("トランプ画像")]
    [SerializeField] private Image[] trumpImages = new Image[3];

    [Header("トランプ裏面")]
    [SerializeField] private Sprite trumpBackSprite;

    [Header("トランプ表面")]
    [SerializeField] private Sprite[] trumpFrontSprites = new Sprite[3];


    // =========================================================
    // レバーON
    // 裏面トランプ3枚を表示
    // =========================================================
    public void ShowTrumps() {
        for (int i = 0; i < 3; i++) {
            if (trumpImages[i] == null) {
                Debug.LogWarning(
                    "trumpImages[" + i + "] が設定されていません"
                );

                continue;
            }

            // 裏面にする
            trumpImages[i].sprite = trumpBackSprite;

            // 表示
            trumpImages[i].gameObject.SetActive(true);
            trumpImages[i].enabled = true;
        }

        Debug.Log("トランプ3枚を裏面で表示");
    }


    // =========================================================
    // 停止したリールに対応するトランプを表にする
    //
    // 0 = 左
    // 1 = 中
    // 2 = 右
    // =========================================================
    public void ShowTramp(int reelIndex) {
        if (reelIndex < 0 || reelIndex >= 3) {
            Debug.LogWarning(
                "不正なreelIndex : " + reelIndex
            );

            return;
        }

        if (trumpImages[reelIndex] == null) {
            Debug.LogWarning(
                "trumpImages[" + reelIndex + "] が設定されていません"
            );

            return;
        }

        if (trumpFrontSprites[reelIndex] == null) {
            Debug.LogWarning(
                "trumpFrontSprites[" + reelIndex + "] が設定されていません"
            );

            return;
        }

        // 裏面 → 表面へ画像を変更
        trumpImages[reelIndex].sprite =
            trumpFrontSprites[reelIndex];

        Debug.Log(
            "トランプ " + reelIndex + " を表面に変更"
        );
    }


    // =========================================================
    // 全トランプを消す
    // =========================================================
    public void HideTrumps() {
        for (int i = 0; i < 3; i++) {
            if (trumpImages[i] != null) {
                trumpImages[i].gameObject.SetActive(false);
            }
        }
    }


    // =========================================================
    // 既存のノイズ演出
    // =========================================================
    public void ShowNoiseAndHide() {
        targetImage.material = noiseMaterial;

        if (noiseCoroutine != null) {
            StopCoroutine(noiseCoroutine);
        }

        noiseCoroutine = StartCoroutine(NoiseShowCoroutine());
    }


    private IEnumerator NoiseShowCoroutine() {
        RectTransform rect =
            targetImage.GetComponent<RectTransform>();

        Vector2 originalPosition =
            rect.anchoredPosition;

        targetImage.gameObject.SetActive(true);

        float timer = 0f;
        float duration = 0.5f;

        while (timer < duration) {
            timer += Time.deltaTime;

            targetImage.enabled =
                Random.value > 0.2f;

            rect.anchoredPosition =
                originalPosition +
                new Vector2(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f)
                );

            yield return null;
        }

        targetImage.enabled = false;
        rect.anchoredPosition = originalPosition;

        noiseCoroutine = null;
    }


    public void ShowClean() {
        if (noiseCoroutine != null) {
            StopCoroutine(noiseCoroutine);
            noiseCoroutine = null;
        }

        targetImage.gameObject.SetActive(true);
        targetImage.enabled = true;
    }


    public void Hide() {
        if (noiseCoroutine != null) {
            StopCoroutine(noiseCoroutine);
            noiseCoroutine = null;
        }

        targetImage.enabled = false;
    }

    [SerializeField] GameObject start;
    [SerializeField] VideoPlayer startvideo;
    [SerializeField] GameObject Loop;
    public void ShowCutinStart() {
        start.SetActive(true);
        startvideo.loopPointReached += ShowCutinLoop;
    }

    void ShowCutinLoop(VideoPlayer vp) {
        Loop.SetActive(true);
    }

}