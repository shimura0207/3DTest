using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM・SEをまとめて管理するクラス。
///
/// 【準備】
/// ・専用の空GameObjectにアタッチする。
/// ・InspectorのBGM一覧／SE一覧に名前とAudioClipを登録する。
/// ・AudioSourceは自動生成されるため追加不要。
///
/// 【呼び出し例】
/// AudioManager.Instance.PlayBGM("SlotBGM");
/// AudioManager.Instance.PlaySE("Lever");
/// </summary>
[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour {

    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class SoundData {

        [Tooltip("呼び出し用の名前。同じ一覧内では重複させないでください。")]
        public string soundName;

        [Tooltip("再生する音声ファイル")]
        public AudioClip clip;

        [Range(0f, 1f)]
        [Tooltip("この音声自体の音量")]
        public float volume = 1f;
    }

    [Header("シーン設定")]
    [Tooltip("シーンを切り替えても、このManagerと再生中の音を維持する")]
    [SerializeField] private bool persistAcrossScenes = true;

    [Header("BGM一覧")]
    [SerializeField] private SoundData[] bgmList = new SoundData[0];

    [Header("SE一覧")]
    [SerializeField] private SoundData[] seList = new SoundData[0];

    [Header("全体音量")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Header("種類別音量")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float seVolume = 1f;

    [Header("ミュート")]
    [SerializeField] private bool isMuted = false;

    private AudioSource bgmSource;
    private AudioSource seSource;

    // 名前から音声を取得するための辞書。
    private readonly Dictionary<string, SoundData> bgmTable =
        new Dictionary<string, SoundData>(StringComparer.Ordinal);

    private readonly Dictionary<string, SoundData> seTable =
        new Dictionary<string, SoundData>(StringComparer.Ordinal);

    private SoundData currentBgm;
    private bool isBgmPaused;

    // フェード用の倍率。通常時は1。
    private float bgmFadeVolume = 1f;
    private Coroutine bgmFadeCoroutine;

    public string CurrentBGMName =>
        currentBgm != null ? currentBgm.soundName : string.Empty;

    public bool IsBGMPlaying =>
        bgmSource != null && bgmSource.isPlaying;

    public bool IsBGMPaused => isBgmPaused;

    public float MasterVolume => masterVolume;
    public float BGMVolume => bgmVolume;
    public float SEVolume => seVolume;
    public bool IsMuted => isMuted;

    // =========================================================
    // 初期化
    // =========================================================

    private void Awake() {

        // シーン移動先に別のAudioManagerがあっても重複させない。
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes) {
            // DontDestroyOnLoadを使えるように最上位へ移動する。
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        bgmSource = CreateAudioSource(true);
        seSource = CreateAudioSource(false);

        RebuildSoundTables();
        ApplyVolumes();
    }

    private AudioSource CreateAudioSource(bool loop) {
        AudioSource source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;

        // 距離や向きで音量が変わらない2D音声。
        source.spatialBlend = 0f;

        source.volume = 1f;
        source.pitch = 1f;

        return source;
    }

    /// <summary>
    /// 音声一覧を辞書へ登録し直す。
    /// 再生中にInspectorの登録内容を変えた場合にも利用できます。
    /// </summary>
    [ContextMenu("音声一覧を再登録")]
    public void RebuildSoundTables() {
        BuildSoundTable(bgmList, bgmTable, "BGM");
        BuildSoundTable(seList, seTable, "SE");
    }

    private void BuildSoundTable(
        SoundData[] sounds,
        Dictionary<string, SoundData> table,
        string category) {

        table.Clear();

        if (sounds == null) return;

        for (int i = 0; i < sounds.Length; i++) {
            SoundData sound = sounds[i];

            if (sound == null ||
                string.IsNullOrWhiteSpace(sound.soundName)) {

                Debug.LogWarning(
                    $"[AudioManager] {category}一覧の要素{i}に名前がありません。",
                    this
                );
                continue;
            }

            if (sound.clip == null) {
                Debug.LogWarning(
                    $"[AudioManager] {category}「{sound.soundName}」にClipがありません。",
                    this
                );
                continue;
            }

            if (table.ContainsKey(sound.soundName)) {
                Debug.LogWarning(
                    $"[AudioManager] {category}名「{sound.soundName}」が重複しています。" +
                    "先に登録された音声を使用します。",
                    this
                );
                continue;
            }

            table.Add(sound.soundName, sound);
        }
    }

    private bool TryGetSound(
        Dictionary<string, SoundData> table,
        string soundName,
        string category,
        out SoundData sound) {

        sound = null;

        if (string.IsNullOrWhiteSpace(soundName)) {
            Debug.LogWarning(
                $"[AudioManager] 呼び出す{category}名が空です。",
                this
            );
            return false;
        }

        if (!table.TryGetValue(soundName, out sound) ||
            sound == null ||
            sound.clip == null) {

            Debug.LogWarning(
                $"[AudioManager] {category}「{soundName}」が未登録、またはClipがありません。",
                this
            );
            return false;
        }

        return true;
    }

    // =========================================================
    // BGM
    // =========================================================

    /// <summary>
    /// BGMを再生する。
    ///
    /// loop：ループするか。
    /// restart：同じBGMでも先頭から再生し直すか。
    ///
    /// 同じBGMが一時停止中なら、restart=falseで続きから再開します。
    /// </summary>
    public void PlayBGM(
        string soundName,
        bool loop = true,
        bool restart = false) {

        if (!TryGetSound(bgmTable, soundName, "BGM", out SoundData sound)) {
            return;
        }

        CancelBGMFade();

        bgmSource.loop = loop;

        bool sameBgm =
            currentBgm == sound &&
            bgmSource.clip == sound.clip;

        if (sameBgm && !restart) {

            if (isBgmPaused) {
                bgmSource.UnPause();
                isBgmPaused = false;
                ApplyVolumes();
                return;
            }

            // 同じBGMを毎回先頭へ戻さない。
            if (bgmSource.isPlaying) {
                ApplyVolumes();
                return;
            }
        }

        bgmSource.Stop();

        currentBgm = sound;
        isBgmPaused = false;

        bgmSource.clip = sound.clip;

        ApplyVolumes();
        bgmSource.Play();
    }

    /// <summary>
    /// BGMを停止する。次の再生は先頭から。
    /// </summary>
    public void StopBGM() {
        CancelBGMFade();

        bgmSource.Stop();
        bgmSource.clip = null;

        currentBgm = null;
        isBgmPaused = false;
    }

    /// <summary>
    /// BGMを一時停止する。
    /// </summary>
    public void PauseBGM() {
        if (!bgmSource.isPlaying) return;

        CancelBGMFade();

        bgmSource.Pause();
        isBgmPaused = true;
    }

    /// <summary>
    /// 一時停止したBGMを続きから再開する。
    /// </summary>
    public void ResumeBGM() {
        if (!isBgmPaused || bgmSource.clip == null) return;

        bgmSource.UnPause();
        isBgmPaused = false;
    }

    /// <summary>
    /// 指定した秒数でBGMをフェードアウトして停止する。
    /// Time.timeScaleが0でも進行します。
    /// </summary>
    public void FadeOutBGM(float duration = 1f) {

        if (duration <= 0f ||
            !bgmSource.isPlaying ||
            !isActiveAndEnabled) {

            StopBGM();
            return;
        }

        // 既存のフェードを止め、現在の音量から引き継ぐ。
        CancelBGMFade(false);

        bgmFadeCoroutine = StartCoroutine(
            FadeOutBGMCoroutine(duration)
        );
    }

    private IEnumerator FadeOutBGMCoroutine(float duration) {
        float elapsed = 0f;
        float startVolume = bgmFadeVolume;

        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;

            bgmFadeVolume = Mathf.Lerp(
                startVolume,
                0f,
                Mathf.Clamp01(elapsed / duration)
            );

            ApplyVolumes();

            yield return null;
        }

        // 自分自身をStopCoroutineしないよう、先に参照を外す。
        bgmFadeCoroutine = null;
        StopBGM();
    }

    private void CancelBGMFade(bool restoreVolume = true) {
        if (bgmFadeCoroutine != null) {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        if (restoreVolume) {
            bgmFadeVolume = 1f;
            ApplyVolumes();
        }
    }

    // =========================================================
    // SE
    // =========================================================

    /// <summary>
    /// SEを1回鳴らす。複数のSEを重ねて再生可能。
    ///
    /// volumeScale：今回だけの音量倍率（0～1）。
    /// </summary>
    public void PlaySE(string soundName, float volumeScale = 1f) {

        if (!TryGetSound(seTable, soundName, "SE", out SoundData sound)) {
            return;
        }

        float volume =
            Mathf.Clamp01(sound.volume) *
            Mathf.Clamp01(volumeScale);

        seSource.PlayOneShot(sound.clip, volume);
    }

    /// <summary>
    /// このManagerで再生したSEをすべて停止する。
    /// </summary>
    public void StopSE() {
        seSource.Stop();
    }

    /// <summary>
    /// BGMとSEをすべて停止する。
    /// </summary>
    public void StopAll() {
        StopBGM();
        StopSE();
    }

    // =========================================================
    // 音量・ミュート
    // =========================================================

    public void SetMasterVolume(float volume) {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetBGMVolume(float volume) {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetSEVolume(float volume) {
        seVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    /// <summary>
    /// trueで消音、falseで解除。再生位置は進み続けます。
    /// </summary>
    public void SetMute(bool mute) {
        isMuted = mute;
        ApplyVolumes();
    }

    private void ApplyVolumes() {
        if (bgmSource != null) {
            float clipVolume = currentBgm != null
                ? Mathf.Clamp01(currentBgm.volume)
                : 1f;

            bgmSource.volume =
                Mathf.Clamp01(masterVolume) *
                Mathf.Clamp01(bgmVolume) *
                clipVolume *
                bgmFadeVolume;

            bgmSource.mute = isMuted;
        }

        if (seSource != null) {
            seSource.volume =
                Mathf.Clamp01(masterVolume) *
                Mathf.Clamp01(seVolume);

            seSource.mute = isMuted;
        }
    }

    private void Update() {
        // 再生中のInspector音量変更も反映する。
        ApplyVolumes();
    }

    private void OnDisable() {
        if (Instance != this) return;

        // 無効化時にフェード処理を残さない。
        CancelBGMFade();

        if (bgmSource != null) {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        if (seSource != null) {
            seSource.Stop();
        }

        currentBgm = null;
        isBgmPaused = false;
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }
}