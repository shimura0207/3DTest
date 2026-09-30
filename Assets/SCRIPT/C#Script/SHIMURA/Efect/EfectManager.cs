using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EfectManager : MonoBehaviour {
    [SerializeField] private Image targetImage;
    [SerializeField] private Material noiseMaterial;
    private Coroutine noiseCoroutine;

    // =========================================================
    // ノイズっぽく表示 → 0.5秒後に消す
    // =========================================================
    public void ShowNoiseAndHide() {
        targetImage.material = noiseMaterial;
        if (noiseCoroutine != null) {
            StopCoroutine(noiseCoroutine);
        }

        noiseCoroutine = StartCoroutine(NoiseShowCoroutine());
    }

    private IEnumerator NoiseShowCoroutine() {
        RectTransform rect = targetImage.GetComponent<RectTransform>();

        // 元の状態を保存
        Vector2 originalPosition = rect.anchoredPosition;

        targetImage.gameObject.SetActive(true);

        float timer = 0f;
        float duration = 0.5f;

        while (timer < duration) {
            timer += Time.deltaTime;

            // 高速で表示・非表示を繰り返す
            targetImage.enabled = Random.value > 0.2f;

            // 少しだけ位置をランダムにずらす
            rect.anchoredPosition = originalPosition +
                                     new Vector2(
                                         Random.Range(-5f, 5f),
                                         Random.Range(-5f, 5f)
                                     );

            yield return null;
        }

        // 元に戻して消す
        targetImage.enabled = false;
        rect.anchoredPosition = originalPosition;

        noiseCoroutine = null;
    }


    // =========================================================
    // ノイズなしで表示
    // 消す関数を呼ぶまで表示し続ける
    // =========================================================
    public void ShowClean() {
        // ノイズ演出が動いていたら停止
        if (noiseCoroutine != null) {
            StopCoroutine(noiseCoroutine);
            noiseCoroutine = null;
        }

        targetImage.gameObject.SetActive(true);
        targetImage.enabled = true;
    }


    // =========================================================
    // 消す
    // =========================================================
    public void Hide() {
        if (noiseCoroutine != null) {
            StopCoroutine(noiseCoroutine);
            noiseCoroutine = null;
        }

        targetImage.enabled = false;
    }
}