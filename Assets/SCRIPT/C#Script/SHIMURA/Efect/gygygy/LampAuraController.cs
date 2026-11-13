using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World Space Canvas下端 → 縦中央 → 対応Image。
/// CanvasのRectサイズを基準にするため、100x100でも1920x1080でも使用可能。
/// </summary>
[DisallowMultipleComponent]
public class LampAuraController : MonoBehaviour {
    public enum LampColor { White, Blue, Yellow, Red, Green }

    [System.Serializable]
    public class LampSetting {
        [Tooltip("発生するX位置の基準となるランプ")]
        public Transform source;
        [Tooltip("到着先のUI Image")]
        public Image target;
        public Color auraColor = Color.white;
    }

    [Header("画面全体を覆うWorld Space Canvas本体")]
    [SerializeField] private Canvas targetCanvas;

    [Header("各色のSource・Target")]
    public LampSetting white = new LampSetting { auraColor = Color.white };
    public LampSetting blue = new LampSetting { auraColor = new Color(0.15f, 0.45f, 1f) };
    public LampSetting yellow = new LampSetting { auraColor = new Color(1f, 0.85f, 0.05f) };
    public LampSetting red = new LampSetting { auraColor = new Color(1f, 0.08f, 0.12f) };
    public LampSetting green = new LampSetting { auraColor = new Color(0.15f, 1f, 0.35f) };

    [Header("移動時間（秒）")]
    [SerializeField, Min(0.01f)] private float riseDuration = 0.1f;
    [SerializeField, Min(0f)] private float centerHoldDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float targetMoveDuration = 0.2f;
    [SerializeField, Min(0.01f)] private float arrivalDuration = 0.18f;

    [Header("発生位置（Canvasの高さに対する割合）")]
    [Tooltip("0なら下端そのもの。0.02なら下端から高さの2%上")]
    [SerializeField, Range(0f, 0.2f)] private float bottomInsetRatio = 0.02f;

    [Header("半透明オーラ")]
    [SerializeField, Range(1, 60)] private int auraLayers = 16;
    [Tooltip("Canvas短辺に対する粒の基本サイズ。0.12なら短辺の12%")]
    [SerializeField, Range(0.005f, 0.4f)] private float auraSizeRatio = 0.12f;
    [SerializeField, Range(0f, 0.95f)] private float auraOpacity = 0.45f;
    [Tooltip("粒の横方向の広がり。基本サイズに対する割合")]
    [SerializeField, Range(0f, 0.5f)] private float auraSpread = 0.22f;
    [SerializeField, Range(1f, 3f)] private float auraVerticalStretch = 1.6f;

    [Header("動作")]
    [SerializeField] private bool useUnscaledTime;
    [SerializeField] private bool playAllOnStart;
    [SerializeField, Min(1)] private int maxParticles = 400;

    private RectTransform canvasRect;
    private RectTransform effectRoot;
    private Sprite glowSprite;
    private Texture2D glowTexture;

    // 1回の演出（Sevenの場合は5色全体）をひとまとめにして完了を通知。
    private class CompletionGroup {
        public int remaining;
        public bool cancelled;
        public System.Action onCompleted;
    }

    private readonly List<CompletionGroup> completedGroups = new List<CompletionGroup>();

    private class AuraParticle {
        public CompletionGroup group;
        public Image image;
        public RectTransform rect;
        public RectTransform target;
        public Vector2 start;
        public Vector2 middle;
        public Color color;
        public float age, size, width, height;
        public float rise, hold, move, fade;
        public int createdFrame;
    }

    private readonly List<AuraParticle> particles = new List<AuraParticle>();

    private void Start() {
        if (playAllOnStart) PlayAll();
    }

    private bool Prepare() {
        if (targetCanvas == null) targetCanvas = GetComponentInParent<Canvas>();
        if (targetCanvas == null || targetCanvas.renderMode != RenderMode.WorldSpace) {
            Debug.LogError("Target CanvasにWorld Space Canvas本体を指定してください。", this);
            return false;
        }
        canvasRect = targetCanvas.transform as RectTransform;
        if (effectRoot == null) {
            GameObject go = new GameObject("LampAuraEffects", typeof(RectTransform), typeof(LayoutElement));
            effectRoot = go.GetComponent<RectTransform>();
            effectRoot.SetParent(canvasRect, false);
            effectRoot.anchorMin = Vector2.zero;
            effectRoot.anchorMax = Vector2.one;
            effectRoot.pivot = new Vector2(0.5f, 0.5f);
            effectRoot.offsetMin = Vector2.zero;
            effectRoot.offsetMax = Vector2.zero;
            go.GetComponent<LayoutElement>().ignoreLayout = true;
        }
        else if (effectRoot.parent != canvasRect) {
            StopAllEffects();
            effectRoot.SetParent(canvasRect, false);
            effectRoot.offsetMin = Vector2.zero;
            effectRoot.offsetMax = Vector2.zero;
        }
        effectRoot.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
        if (glowSprite == null) CreateGlowSprite();
        return true;
    }

    public void Play(LampColor lampColor) {
        TryPlayWithCompletion(lampColor, null);
    }

    /// <summary>自然終了した場合だけ1回通知。中断・設定不足では通知しない。</summary>
    public bool TryPlayWithCompletion(LampColor lampColor, System.Action onCompleted) {
        CompletionGroup group = new CompletionGroup { onCompleted = onCompleted };
        return SpawnAura(lampColor, group);
    }

    /// <summary>5色を1セットとして再生し、全色の自然終了後に1回だけ通知。</summary>
    public bool TryPlayAllWithCompletion(System.Action onCompleted) {
        if (!Application.isPlaying || !isActiveAndEnabled || !Prepare()) return false;
        LampColor[] colors = { LampColor.White, LampColor.Blue, LampColor.Yellow, LampColor.Red, LampColor.Green };
        foreach (LampColor color in colors) {
            LampSetting setting = GetSetting(color);
            if (setting == null || setting.source == null || setting.target == null) {
                Debug.LogWarning("全色オーラ：" + color + "のSource/Targetが未設定です。", this);
                return false;
            }
        }
        // 5色すべての粒数を確保。途中までしか出ないセットは開始しない。
        if (maxParticles - particles.Count < Mathf.Clamp(auraLayers, 1, 60) * colors.Length) {
            Debug.LogWarning("全色オーラ：Max Particlesの上限に達しています。", this);
            return false;
        }
        CompletionGroup group = new CompletionGroup { onCompleted = onCompleted };
        foreach (LampColor color in colors) {
            if (!SpawnAura(color, group)) {
                group.cancelled = true;
                return false;
            }
        }
        return true;
    }

    private bool SpawnAura(LampColor lampColor, CompletionGroup group) {
        if (!Application.isPlaying) {
            Debug.LogWarning("Unityの再生中にテストしてください。", this);
            return false;
        }
        if (!isActiveAndEnabled || !Prepare()) return false;

        LampSetting setting = GetSetting(lampColor);
        if (setting == null || setting.source == null || setting.target == null) {
            Debug.LogWarning(lampColor + "のSourceとTargetを登録してください。", this);
            return false;
        }

        Rect bounds = canvasRect.rect;
        if (bounds.width <= 0f || bounds.height <= 0f) return false;
        Vector3 sourceLocal = canvasRect.InverseTransformPoint(GetWorldCenter(setting.source));
        float x = Mathf.Clamp(sourceLocal.x, bounds.xMin, bounds.xMax);
        float y = bounds.yMin + bounds.height * bottomInsetRatio;
        Vector2 start = CanvasToEffect(new Vector2(x, y));
        Vector2 middle = CanvasToEffect(new Vector2(x, bounds.center.y));

        int count = Mathf.Min(Mathf.Clamp(auraLayers, 1, 60), Mathf.Max(0, maxParticles - particles.Count));
        if (count == 0) return false;
        float baseSize = Mathf.Min(bounds.width, bounds.height) * Mathf.Max(0.001f, auraSizeRatio);

        // 複数の粒を重ねても、全体が不透明になりすぎないよう補正する。
        float combinedAlpha = Mathf.Clamp(auraOpacity * setting.auraColor.a, 0f, 0.95f);
        float layerAlpha = 1f - Mathf.Pow(1f - combinedAlpha, 1f / count);

        for (int i = 0; i < count; i++) {
            GameObject go = new GameObject("Aura_" + lampColor,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(effectRoot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            Image image = go.GetComponent<Image>();
            image.sprite = glowSprite;
            image.raycastTarget = false;

            // 下端のYは変えず、横だけに薄い層を広げる。
            float offset = Random.Range(-1f, 1f) * baseSize * auraSpread;
            offset = Mathf.Clamp(offset, bounds.xMin - x, bounds.xMax - x);
            Vector2 layerOffset = new Vector2(offset, 0f);
            Color color = setting.auraColor;
            color.a = layerAlpha;

            AuraParticle p = new AuraParticle {
                group = group,
                image = image,
                rect = rect,
                target = setting.target.rectTransform,
                start = start + layerOffset,
                middle = middle + layerOffset,
                color = color,
                age = 0f,
                createdFrame = Time.frameCount,
                size = baseSize * Random.Range(0.8f, 1.25f),
                width = Random.Range(0.65f, 1f),
                height = auraVerticalStretch * Random.Range(0.85f, 1.15f),
                rise = Mathf.Max(0.01f, riseDuration),
                hold = Mathf.Max(0f, centerHoldDuration),
                move = Mathf.Max(0.01f, targetMoveDuration),
                fade = Mathf.Max(0.01f, arrivalDuration)
            };
            group.remaining++;
            particles.Add(p);
            Draw(p, p.start, 1f, 1f, 0f);
        }
        return true;
    }

    private void Update() {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        for (int i = particles.Count - 1; i >= 0; i--) {
            AuraParticle p = particles[i];
            if (p.image == null || p.target == null || effectRoot == null) {
                RemoveParticle(i);
                continue;
            }
            // 呼び出し順によらず、生成フレームには下端を描画する。
            if (Time.frameCount == p.createdFrame) continue;
            p.age += dt;
            float holdEnd = p.rise + p.hold;
            float moveEnd = holdEnd + p.move;

            if (p.age < p.rise) {
                float t = Mathf.Clamp01(p.age / p.rise);
                float eased = t * t;
                Draw(p, Vector2.Lerp(p.start, p.middle, eased), Mathf.Lerp(1f, 0.85f, eased), 1f, 0f);
            }
            else if (p.age < holdEnd) {
                Draw(p, p.middle, 0.85f, 1f, 0f);
            }
            else if (p.age < moveEnd) {
                float t = Mathf.Clamp01((p.age - holdEnd) / p.move);
                float eased = t * t;
                Draw(p, Vector2.Lerp(p.middle, WorldToEffect(GetWorldCenter(p.target)), eased),
                    Mathf.Lerp(0.85f, 0.6f, eased), 1f, 0f);
            }
            else {
                float t = (p.age - moveEnd) / p.fade;
                if (t >= 1f) { RemoveParticle(i, true); continue; }
                Draw(p, WorldToEffect(GetWorldCenter(p.target)), Mathf.Lerp(0.6f, 1.5f, t), 1f - t, t);
            }
        }
        // 粒の走査が終わってから通知。コールバック内の再生・停止にも対応。
        while (completedGroups.Count > 0) {
            CompletionGroup group = completedGroups[0];
            completedGroups.RemoveAt(0);
            if (group.cancelled) continue;
            System.Action callback = group.onCompleted;
            group.onCompleted = null;
            try { callback?.Invoke(); }
            catch (System.Exception exception) { Debug.LogException(exception, this); }
        }
    }

    private void Draw(AuraParticle p, Vector2 position, float scale, float alpha, float roundness) {
        p.rect.anchoredPosition = position;
        p.rect.sizeDelta = new Vector2(Mathf.Lerp(p.width, 1f, roundness),
            Mathf.Lerp(p.height, 1f, roundness)) * p.size * scale;
        Color color = p.color;
        color.a *= alpha;
        p.image.color = color;
    }

    private static Vector3 GetWorldCenter(Transform target) {
        RectTransform rect = target as RectTransform;
        return rect != null ? rect.TransformPoint(rect.rect.center) : target.position;
    }

    private Vector2 WorldToEffect(Vector3 world) {
        Vector3 local = effectRoot.InverseTransformPoint(world);
        return new Vector2(local.x, local.y);
    }

    private Vector2 CanvasToEffect(Vector2 point) {
        return WorldToEffect(canvasRect.TransformPoint(new Vector3(point.x, point.y, 0f)));
    }

    private LampSetting GetSetting(LampColor color) {
        switch (color) {
            case LampColor.Blue: return blue;
            case LampColor.Yellow: return yellow;
            case LampColor.Red: return red;
            case LampColor.Green: return green;
            default: return white;
        }
    }

    private void CreateGlowSprite() {
        const int size = 128;
        glowTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        glowTexture.name = "GeneratedLampAura";
        glowTexture.wrapMode = TextureWrapMode.Clamp;
        glowTexture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++) {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;
                float radius = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(radius));
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        glowTexture.SetPixels(pixels);
        glowTexture.Apply(false, true);
        glowSprite = Sprite.Create(glowTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void RemoveParticle(int index, bool completedNormally = false) {
        CompletionGroup group = particles[index].group;
        if (particles[index].image != null) {
            particles[index].image.gameObject.SetActive(false);
            Destroy(particles[index].image.gameObject);
        }
        particles.RemoveAt(index);
        if (group != null) {
            if (!completedNormally) group.cancelled = true;
            group.remaining--;
            if (group.remaining == 0 && !group.cancelled)
                completedGroups.Add(group);
        }
    }

    public void StopAllEffects() {
        for (int i = particles.Count - 1; i >= 0; i--) RemoveParticle(i);
        completedGroups.Clear();
    }

    private void OnDisable() { StopAllEffects(); }

    private void OnDestroy() {
        StopAllEffects();
        if (effectRoot != null) Destroy(effectRoot.gameObject);
        if (glowSprite != null) Destroy(glowSprite);
        if (glowTexture != null) Destroy(glowTexture);
    }

    public void PlayWhite() { Play(LampColor.White); }
    public void PlayBlue() { Play(LampColor.Blue); }
    public void PlayYellow() { Play(LampColor.Yellow); }
    public void PlayRed() { Play(LampColor.Red); }
    public void PlayGreen() { Play(LampColor.Green); }

    [ContextMenu("テスト：全色を再生")]
    public void PlayAll() {
        TryPlayAllWithCompletion(null);
    }
}
