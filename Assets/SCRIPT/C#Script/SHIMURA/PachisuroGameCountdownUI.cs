using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// パチスロフェイズの残りG表示と、レバーオン時の縮小・フェード演出を担当します。
/// 残りG数の増減は既存のゲーム進行側で管理してください。
/// </summary>
public class PachisuroGameCountdownUI : MonoBehaviour {
    [Header("通常の残りG表示（別CanvasでもOK）")]
    [SerializeField] private TextMeshProUGUI remainingGamesText;

    [Header("液晶中央のカウントダウン演出")]
    [SerializeField] private TextMeshProUGUI effectNumberText;
    [SerializeField] private CanvasGroup effectCanvasGroup;
    [SerializeField] private bool appendGToEffect = false;

    [Header("縮小・フェード設定")]
    [Min(0.01f)]
    [SerializeField] private float animationDuration = 0.8f;
    [Min(0f)]
    [SerializeField] private float startingScale = 2.5f;
    [Min(0f)]
    [SerializeField] private float endingScale = 0.65f;
    [Range(0f, 0.99f)]
    [SerializeField] private float fadeStart = 0.7f;
    [SerializeField] private bool useUnscaledTime = false;

    private Coroutine animationCoroutine;
    private int remainingGames;

    private void Awake() {
        if (effectCanvasGroup != null) {
            // この演出でボタンなどのクリックを邪魔しないようにする。
            effectCanvasGroup.interactable = false;
            effectCanvasGroup.blocksRaycasts = false;
        }

        HideEffectVisual();
    }

    /// <summary>フェイズ開始時に呼びます（例：5G）。</summary>
    public void Initialize(int games) {
        SetRemainingGames(games);
        HideCountdown();
    }

    /// <summary>残りGの常設テキストだけを更新します。</summary>
    public void SetRemainingGames(int games) {
        remainingGames = Mathf.Max(0, games);

        if (remainingGamesText != null) {
            remainingGamesText.text = $"残り：{remainingGames}G";
        }
    }

    /// <summary>
    /// レバーオンが実際に成立したときに1回呼びます。
    /// 引数はその時点で表示すべき残りG数です。
    /// </summary>
    public void PlayLeverOn(int games) {
        SetRemainingGames(games);
        HideCountdown();

        if (effectNumberText == null || effectCanvasGroup == null) {
            Debug.LogWarning("カウントダウン演出用のTMPまたはCanvasGroupが未設定です。", this);
            return;
        }

        if (!isActiveAndEnabled) {
            return;
        }

        effectNumberText.text = appendGToEffect ? $"{remainingGames}G" : remainingGames.ToString();
        animationCoroutine = StartCoroutine(AnimateCountdown());
    }

    /// <summary>再生中の演出だけを消します（フェイズ終了時にも使用可）。</summary>
    public void HideCountdown() {
        if (animationCoroutine != null) {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        HideEffectVisual();
    }

    /// <summary>フェイズ終了時。常設テキストも非表示にします。</summary>
    public void Clear() {
        HideCountdown();
        remainingGames = 0;

        if (remainingGamesText != null) {
            remainingGamesText.text = string.Empty;
        }
    }

    private IEnumerator AnimateCountdown() {
        RectTransform rect = effectNumberText.rectTransform;
        float duration = Mathf.Max(0.01f, animationDuration);
        float elapsed = 0f;

        effectCanvasGroup.alpha = 1f;
        rect.localScale = Vector3.one * startingScale;

        while (elapsed < duration) {
            float t = Mathf.Clamp01(elapsed / duration);

            // 最初は速く、最後はゆっくりと縮小。
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            float scale = Mathf.Lerp(startingScale, endingScale, easeOut);
            rect.localScale = Vector3.one * scale;

            // fadeStart (例: 0.7) 以降、最後までに透明にする。
            float fadeT = Mathf.InverseLerp(fadeStart, 1f, t);
            effectCanvasGroup.alpha = 1f - fadeT;

            yield return null;
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        HideEffectVisual();
        animationCoroutine = null;
    }

    private void HideEffectVisual() {
        if (effectCanvasGroup != null) {
            effectCanvasGroup.alpha = 0f;
        }

        if (effectNumberText != null) {
            effectNumberText.rectTransform.localScale = Vector3.one * endingScale;
        }
    }

    private void OnDisable() {
        if (animationCoroutine != null) {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        HideEffectVisual();
    }
}
