using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Canvas直下の専用RushRootに付ける、約2秒のRUSH突入演出。
/// RushRootは有効のまま使用。表示はCanvasGroupで制御します。
/// 子Imageの順番: Background / LightLine / RushLogo / Flash
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class RGRushIntro : MonoBehaviour
{
    [Header("専用のUIを設定")]
    [SerializeField] private Image background;
    [SerializeField] private Image lightLine;
    [SerializeField] private Image rushLogo;
    [SerializeField] private Image flash;

    [Header("音声は任意")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip impactSound;

    [Header("調整")]
    [SerializeField, Min(0f)] private float shakeStrength = 16f;
    [SerializeField] private bool playOnStart;
    [SerializeField] private UnityEvent onCompleted = new UnityEvent();

    public bool IsPlaying { get; private set; }
    private CanvasGroup group;
    private Coroutine routine;
    private Vector2 logoPosition;
    private Vector3 logoScale;
    private Vector3 lineScale;
    private Color backgroundColor, lineColor, logoColor, flashColor;
    private bool ready;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        if (!background || !lightLine || !rushLogo || !flash)
        {
            Debug.LogError("RGRushIntro: 4つのImageをInspectorで設定してください。", this);
            return;
        }

        logoPosition = rushLogo.rectTransform.anchoredPosition;
        logoScale = rushLogo.rectTransform.localScale;
        lineScale = lightLine.rectTransform.localScale;
        backgroundColor = background.color;
        lineColor = lightLine.color;
        logoColor = rushLogo.color;
        flashColor = flash.color;
        // 演出中もUI入力を遮らない。ゲーム操作の停止は呼び出し側で行う。
        background.raycastTarget = lightLine.raycastTarget = false;
        rushLogo.raycastTarget = flash.raycastTarget = false;
        ready = true;
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    // ButtonのOnClick、または他スクリプトから呼び出せます。
    public void Play()
    {
        if (!ready || !isActiveAndEnabled) return;
        Cancel();
        routine = StartCoroutine(Animate());
    }

    public void Cancel()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        IsPlaying = false;
        if (group) group.alpha = 0f;
        if (!ready) return;
        rushLogo.rectTransform.anchoredPosition = logoPosition;
        rushLogo.rectTransform.localScale = logoScale;
        lightLine.rectTransform.localScale = lineScale;
        background.color = backgroundColor;
        lightLine.color = lineColor;
        rushLogo.color = logoColor;
        flash.color = flashColor;
    }

    private void OnDisable() { Cancel(); }

    private IEnumerator Animate()
    {
        IsPlaying = true;
        group.alpha = 1f;
        PlaySound(chargeSound);
        bool hit = false;
        float time = 0f;

        while (time < 2f)
        {
            // 0.00-0.25: 暗転。1.65-2.00: 全体を消す。
            group.alpha = 1f - Progress(time, 1.65f, 2f);
            SetAlpha(background, backgroundColor,
                Mathf.Lerp(0f, 0.9f, Progress(time, 0f, 0.25f)));

            // 0.25-0.55: 中央から光の帯を横へ伸ばす。
            float lineProgress = Progress(time, 0.25f, 0.55f);
            lightLine.rectTransform.localScale = Vector3.Scale(lineScale,
                new Vector3(Mathf.Lerp(0.02f, 1f, lineProgress), 1f, 1f));
            SetAlpha(lightLine, lineColor,
                lineProgress * (1f - Progress(time, 0.55f, 0.85f)));

            // 0.55: 衝撃音と一度だけの短いフラッシュ。
            if (!hit && time >= 0.55f)
            {
                hit = true;
                PlaySound(impactSound);
            }
            SetAlpha(flash, flashColor, time < 0.55f ? 0f :
                0.65f * (1f - Progress(time, 0.55f, 0.70f)));

            // 0.55-0.75: 2.5倍から縮小。0.75-0.85: 少し跳ね返る。
            float scale = time < 0.75f
                ? Mathf.Lerp(2.5f, 0.94f, EaseOut(Progress(time, 0.55f, 0.75f)))
                : Mathf.Lerp(0.94f, 1f, Progress(time, 0.75f, 0.85f));
            rushLogo.rectTransform.localScale = logoScale * scale;
            SetAlpha(rushLogo, logoColor, Progress(time, 0.55f, 0.62f));

            // 文字だけを揺らすので、既存のゲームUIには影響しません。
            float shake = time >= 0.55f && time < 0.85f
                ? shakeStrength * (1f - Progress(time, 0.55f, 0.85f)) : 0f;
            rushLogo.rectTransform.anchoredPosition = logoPosition +
                new Vector2(Mathf.Sin(time * 170f), Mathf.Cos(time * 139f)) * shake;

            yield return null;
            time += Time.unscaledDeltaTime;
        }

        routine = null;
        Cancel();
        onCompleted.Invoke();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

    private static float Progress(float time, float start, float end)
    {
        return Mathf.Clamp01((time - start) / (end - start));
    }

    private static float EaseOut(float t) { return 1f - Mathf.Pow(1f - t, 3f); }

    private static void SetAlpha(Image image, Color original, float alpha)
    {
        original.a *= alpha;
        image.color = original;
    }
}
