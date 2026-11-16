using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 台の3D装飾ランプを常時点滅。BPMで点滅タイミング、音楽の低音で強度を制御。
/// Standard / URP Lit向け。再生時専用のMaterialを使い、共有アセットを変更しない。
/// </summary>
[DisallowMultipleComponent]
public class MusicLampFlashController : MonoBehaviour {
    public enum FlashPattern { All, Alternating, Chase }

    // 生成されたAudioManager.instanceから取得。Inspector指定は不要。
    private AudioSource musicSource;

    [Header("ランプ部分だけを登録。子のRendererも対象")]
    [SerializeField] private GameObject[] lampObjects;

    [Header("点滅リズム（BPMは曲に合わせて手動設定）")]
    [SerializeField] private FlashPattern pattern = FlashPattern.Alternating;
    [SerializeField, Range(30f, 300f)] private float bpm = 140f;
    [Tooltip("1拍の分割数。4なら16分音符ごとの点滅")]
    [SerializeField, Range(1, 8)] private int flashesPerBeat = 4;
    [Tooltip("曲の最初の拍の位置。音楽の開始に無音がある場合などに調整")]
    [SerializeField] private float beatOffsetSeconds;
    [SerializeField, Range(0.05f, 0.9f)] private float litFraction = 0.4f;
    [Tooltip("BGM未再生中も設定BPMで点滅を続ける")]
    [SerializeField] private bool flashWhenMusicStopped = true;

    [Header("点灯・消灯フェード（秒）")]
    [Tooltip("暗い状態から最大輝度までの時間。0なら即点灯")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.03f;
    [Tooltip("最大輝度から消灯までの時間。0なら即消灯")]
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.06f;

    [Header("明るさ")]
    [SerializeField, Min(0f)] private float minimumEmission = 3f;
    [SerializeField, Min(0f)] private float musicEmissionBoost = 12f;
    [SerializeField, Range(0f, 1f)] private float darkBrightness = 0.015f;
    [SerializeField] private bool overrideLampColor;
    [SerializeField, ColorUsage(false, true)] private Color lampColor = Color.white;

    [Header("低音への反応")]
    [SerializeField] private Vector2 bassFrequencyRange = new Vector2(40f, 220f);
    [SerializeField, Min(0.01f)] private float bassSensitivity = 12f;
    [Tooltip("低音が急に強くなった瞬間に全灯フラッシュを追加")]
    [SerializeField] private bool bassHitFlash = true;
    [SerializeField, Range(0.01f, 1f)] private float bassRiseThreshold = 0.12f;
    [SerializeField, Min(0.02f)] private float hitFlashDuration = 0.045f;
    [SerializeField, Min(0.05f)] private float hitCooldown = 0.12f;

    [Header("実行中の確認用")]
    [SerializeField, Range(0f, 1f)] private float currentBassLevel;

    private class LampMaterial {
        public Material material;
        public int baseId;
        public bool hasBase, hasEmission;
        public Color originalColor;
        public Color lightColor;
    }
    private class LampRenderer {
        public Renderer renderer;
        public Material[] originals;
        public Material[] instances;
        public readonly List<LampMaterial> materials = new List<LampMaterial>();
        public int group;
        public float brightness;
        public float emissionIntensity;
    }

    private readonly List<LampRenderer> renderers = new List<LampRenderer>();
    private readonly float[] spectrumLeft = new float[1024];
    private readonly float[] spectrumRight = new float[1024];
    private float clock;
    private float bassAverage;
    private float lastHitTime = -100f;
    private float hitUntil = -1f;
    private int groupCount;
    private bool wasPlaying;
    private AudioClip previousClip;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private void OnEnable() {
        clock = 0f;
        bassAverage = currentBassLevel = 0f;
        lastHitTime = -100f;
        hitUntil = -1f;
        wasPlaying = false;
        previousClip = null;
        BuildMaterials();
    }

    private void Update() {
        // Managerの生成順に依存せず、生成・再生成後のBGMSourceを自動取得する。
        AudioManager manager = AudioManager.instance;
        AudioSource source = manager != null ? manager.BGMSource : null;
        if (musicSource != source) {
            musicSource = source;
            wasPlaying = false;
            previousClip = null;
            bassAverage = currentBassLevel = 0f;
            lastHitTime = -100f;
            hitUntil = -1f;
        }
        clock += Time.unscaledDeltaTime;
        bool playing = musicSource != null && musicSource.isPlaying;
        if (!playing && !flashWhenMusicStopped) {
            foreach (LampRenderer lamp in renderers) Apply(lamp, false, 0f);
            wasPlaying = false;
            return;
        }

        if (playing && (!wasPlaying || previousClip != musicSource.clip)) {
            bassAverage = currentBassLevel = 0f;
            hitUntil = -1f;
        }
        previousClip = musicSource != null ? musicSource.clip : null;
        wasPlaying = playing;
        float bass = playing ? ReadBass() : 0f;
        bool hit = playing && bassHitFlash && bass - bassAverage >= bassRiseThreshold
            && clock - lastHitTime >= hitCooldown;
        if (hit) {
            lastHitTime = clock;
            hitUntil = clock + hitFlashDuration;
        }
        bassAverage = Mathf.Lerp(bassAverage, bass, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 8f));
        currentBassLevel = bass;

        // 音楽再生位置を基準にするので、一時停止・再開・曲のループに追従する。
        double seconds = playing
            ? (musicSource.clip != null
                ? (double)musicSource.timeSamples / Mathf.Max(1, musicSource.clip.frequency)
                : musicSource.time)
            : clock;
        double ticks = System.Math.Max(0.0, seconds - beatOffsetSeconds)
            * Mathf.Max(1f, bpm) / 60.0 * Mathf.Max(1, flashesPerBeat);
        long step = (long)System.Math.Floor(ticks);
        bool pulse = ticks - System.Math.Floor(ticks) < litFraction;
        float intensity = minimumEmission + bass * musicEmissionBoost;
        foreach (LampRenderer lamp in renderers) {
            bool selected = pattern == FlashPattern.All
                || (pattern == FlashPattern.Alternating && lamp.group % 2 == step % 2)
                || (pattern == FlashPattern.Chase && lamp.group == step % Mathf.Max(1, groupCount));
            Apply(lamp, (selected && pulse) || (playing && clock < hitUntil), intensity);
        }
    }

    private float ReadBass() {
        musicSource.GetSpectrumData(spectrumLeft, 0, FFTWindow.BlackmanHarris);
        bool stereo = musicSource.clip != null && musicSource.clip.channels > 1;
        if (stereo) musicSource.GetSpectrumData(spectrumRight, 1, FFTWindow.BlackmanHarris);
        float binHz = Mathf.Max(1, AudioSettings.outputSampleRate) * 0.5f / spectrumLeft.Length;
        float low = Mathf.Max(0f, Mathf.Min(bassFrequencyRange.x, bassFrequencyRange.y));
        float high = Mathf.Max(low, Mathf.Max(bassFrequencyRange.x, bassFrequencyRange.y));
        int start = Mathf.Clamp(Mathf.FloorToInt(low / binHz), 1, spectrumLeft.Length - 1);
        int end = Mathf.Clamp(Mathf.CeilToInt(high / binHz), start, spectrumLeft.Length - 1);
        float total = 0f;
        for (int i = start; i <= end; i++)
            total += stereo ? (spectrumLeft[i] + spectrumRight[i]) * 0.5f : spectrumLeft[i];
        return Mathf.Clamp01(Mathf.Sqrt(total / (end - start + 1)) * bassSensitivity);
    }

    private void BuildMaterials() {
        ReleaseMaterials();
        if (lampObjects == null) return;
        HashSet<Renderer> seen = new HashSet<Renderer>();
        groupCount = 0;
        foreach (GameObject root in lampObjects) {
            if (root == null) continue;
            int group = groupCount;
            bool found = false;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) {
                if (!seen.Add(renderer)) continue;
                LampRenderer lamp = new LampRenderer {
                    renderer = renderer,
                    originals = renderer.sharedMaterials,
                    group = group
                };
                lamp.instances = (Material[])lamp.originals.Clone();
                for (int i = 0; i < lamp.originals.Length; i++) {
                    Material original = lamp.originals[i];
                    if (original == null) continue;
                    Material instance = new Material(original);
                    instance.name = original.name + " (MusicLamp Runtime)";
                    lamp.instances[i] = instance;
                    int baseId = instance.HasProperty(BaseColorId) ? BaseColorId : ColorId;
                    bool hasBase = instance.HasProperty(baseId);
                    bool hasEmission = instance.HasProperty(EmissionId);
                    Color baseColor = hasBase ? instance.GetColor(baseId) : Color.white;
                    Color emission = hasEmission ? instance.GetColor(EmissionId) : Color.black;
                    Color lightColor = baseColor;
                    if (emission.maxColorComponent > 0.001f)
                        lightColor = emission / emission.maxColorComponent;
                    if (hasEmission) instance.EnableKeyword("_EMISSION");
                    if (!hasBase && !hasEmission)
                        Debug.LogWarning(renderer.name + "：Standard / URP Lit等の色変更可能なShaderを使用してください。", this);
                    lamp.materials.Add(new LampMaterial {
                        material = instance,
                        baseId = baseId,
                        hasBase = hasBase,
                        hasEmission = hasEmission,
                        originalColor = baseColor,
                        lightColor = lightColor
                    });
                }
                renderer.sharedMaterials = lamp.instances;
                renderers.Add(lamp);
                // 初回描画も暗い状態から開始する。
                WriteLamp(lamp);
                found = true;
            }
            if (found) groupCount++;
        }
    }

    private void Apply(LampRenderer lamp, bool on, float intensity) {
        if (lamp.renderer == null) return;
        float duration = on ? fadeInDuration : fadeOutDuration;
        lamp.brightness = duration <= 0f
            ? (on ? 1f : 0f)
            : Mathf.MoveTowards(lamp.brightness, on ? 1f : 0f,
                Time.unscaledDeltaTime / duration);

        // 消灯中は直前の発光強度を保持し、フェード量だけを下げる。
        // BGM停止時にもEmissionが一瞬で0にならない。
        if (on) lamp.emissionIntensity = Mathf.Max(0f, intensity);
        WriteLamp(lamp);
    }

    private void WriteLamp(LampRenderer lamp) {
        // 両端を滑らかにして、短いフェードでも硬い切り替わりを抑える。
        float level = lamp.brightness * lamp.brightness * (3f - 2f * lamp.brightness);
        foreach (LampMaterial slot in lamp.materials) {
            if (slot.material == null) continue;
            Color chosen = overrideLampColor ? lampColor : slot.lightColor;
            Color color = chosen * Mathf.Lerp(darkBrightness, 1f, level);
            color.a = slot.originalColor.a;
            if (slot.hasBase) slot.material.SetColor(slot.baseId, color);
            if (slot.hasEmission)
                slot.material.SetColor(EmissionId, chosen * (lamp.emissionIntensity * level));
        }
    }

    private void ReleaseMaterials() {
        foreach (LampRenderer lamp in renderers) {
            if (lamp.renderer != null) {
                Material[] current = lamp.renderer.sharedMaterials;
                for (int i = 0; i < current.Length && i < lamp.instances.Length; i++)
                    if (current[i] == lamp.instances[i]) current[i] = lamp.originals[i];
                lamp.renderer.sharedMaterials = current;
            }
            foreach (LampMaterial slot in lamp.materials)
                if (slot.material != null) Destroy(slot.material);
        }
        renderers.Clear();
    }

    private void OnDisable() { ReleaseMaterials(); }
    private void OnDestroy() { ReleaseMaterials(); }
}
