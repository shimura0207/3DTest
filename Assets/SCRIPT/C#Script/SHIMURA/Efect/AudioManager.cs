using Cysharp.Threading.Tasks;
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
public class AudioManager : SystemObject {
    // 自身への参照
    public static AudioManager instance { get; private set; }

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
    // AudioManagerが使用する設定データ
    [Header("音声設定")]
    [SerializeField] private AudioManagerConfig audioManagerConfig = null;

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

    public override async UniTask Initialize() {
        // シングルトンを自身に設定
        instance = this;
        // BGMをコンポーネントに追加
        bgmSource = CreateAudioSource(true);
        // SEをコンポーネントに追加
        seSource = CreateAudioSource(false);

        // 音声一覧を辞書へ登録し直す
        RebuildSoundTables();
        ApplyVolumes();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// ゲームオブジェクトにAudioSourceコンポーネントを追加
    /// </summary>
    /// <param name="loop"></param>
    /// <returns></returns>
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
        // 設定データが登録されていなければ終了する
        if (audioManagerConfig == null) {
            // 古い音声データが残らないように辞書を空にする
            bgmTable.Clear();
            seTable.Clear();
            return;
        }

        // ScriptableObjectからBGM一覧を取得して辞書を構築する
        BuildSoundTable(audioManagerConfig.BGMList, bgmTable, "BGM");
        // ScriptableObjectからSE一覧を取得して辞書を構築する
        BuildSoundTable(audioManagerConfig.SEList, seTable, "SE");
    }

    /// <summary>
    /// 音声データを受け取り、音声名から検索できる辞書を作る
    /// </summary>
    /// <param name="sounds">登録する音声データの配列</param>
    /// <param name="table">登録先の辞書</param>
    /// <param name="category">BGMや効果音などの分類名</param>
    private void BuildSoundTable(
        SoundData[] sounds,
        Dictionary<string, SoundData> table,
        string category) {

        // 登録内容を削除
        table.Clear();
        // 音声データ配列がなければ処理を抜ける
        if (sounds == null) return;

        // 音声データ配列を走査
        for (int i = 0; i < sounds.Length; i++) {
            // 要素ごとに取得
            SoundData sound = sounds[i];

            // 音がないか、名前が空なら警告を出す
            if (sound == null || string.IsNullOrWhiteSpace(sound.soundName)) {
                // 警告を出す
                Debug.LogWarning($"[AudioManager] {category}一覧の要素{i}に名前がありません。", this);
                continue;
            }
            // 再生する音声ファイルが未設定なら警告を出す
            if (sound.clip == null) {
                // 警告を出す
                Debug.LogWarning(
                    $"[AudioManager] {category}「{sound.soundName}」にClipがありません。",
                    this
                );
                continue;
            }

            // 同じ名前がすでに辞書に登録されているか確認する
            if (table.ContainsKey(sound.soundName)) {
                // 警告を出す
                Debug.LogWarning(
                    $"[AudioManager] {category}名「{sound.soundName}」が重複しています。" +
                    "先に登録された音声を使用します。",
                    this
                );
                continue;
            }
            // 配列に追加
            table.Add(sound.soundName, sound);
        }
    }

    /// <summary>
    /// 音声データを検索、取得できたかどうかを返す
    /// </summary>
    /// <param name="table">検索対象の辞書</param>
    /// <param name="soundName">検索する音声名</param>
    /// <param name="category">BGMか効果音</param>
    /// <param name="sound">取得した音声データを返す</param>
    /// <returns></returns>
    private bool TryGetSound(
        Dictionary<string, SoundData> table,
        string soundName,
        string category,
        out SoundData sound) {

        // 初期値をnullにする。
        sound = null;

        // 音声名がnull・空文字・空白だけなら警告を出して終了する
        if (string.IsNullOrWhiteSpace(soundName)) {
            // 警告を出す
            Debug.LogWarning(
                $"[AudioManager] 呼び出す{category}名が空です。",
                this
            );
            return false;
        }

        // 指定名の音声データが存在し、再生用Clipも設定されているか確認する
        if (!table.TryGetValue(soundName, out sound) ||
            sound == null ||
            sound.clip == null) {
            // 警告を出す
            Debug.LogWarning(
                $"[AudioManager] {category}「{soundName}」が未登録、またはClipがありません。",
                this
            );
            return false;
        }
        // 音声データとClipが正常に取得できたことを返す
        return true;
    }

    // =========================================================
    // BGM
    // =========================================================

    /// <summary>
    /// 指定したBGMを再生
    /// </summary>
    /// <param name="soundName">音声の名前</param>
    /// <param name="loop">ループ再生するかどうか</param>
    /// <param name="restart">先頭から再生</param>
    public void PlayBGM(
        string soundName,
        bool loop = true,
        bool restart = false) {

        // 指定されたBGMが登録されていなければ処理を終了する
        if (!TryGetSound(bgmTable, soundName, "BGM", out SoundData sound)) return;
        // 実行中のBGMフェード処理をキャンセルする
        CancelBGMFade();
        // ループ設定をAudioSourceに反映する
        bgmSource.loop = loop;

        // 登録データと再生中のClipが両方一致するか確認する
        bool sameBgm = currentBgm == sound && bgmSource.clip == sound.clip;

        // 同じBGMで、先頭からの再生指定がない場合に処理する
        if (sameBgm && !restart) {
            // 一時停止中なら、停止位置から再生を再開する
            if (isBgmPaused) {
                bgmSource.UnPause();
                isBgmPaused = false;
                ApplyVolumes();
                return;
            }

            // 同じBGMがすでに再生中なら、先頭に戻さず維持する。
            if (bgmSource.isPlaying) {
                ApplyVolumes();
                return;
            }
        }
        // 現在のBGMを停止する
        bgmSource.Stop();

        // 新しいBGMデータを管理対象に設定する
        currentBgm = sound;
        isBgmPaused = false;

        // 再生するClipを設定する
        bgmSource.clip = sound.clip;
        // 音量を適用して、BGMを再生する
        ApplyVolumes();
        bgmSource.Play();
    }

    /// <summary>
    /// BGMを停止する。次の再生は先頭から。
    /// </summary>
    public void StopBGM() {
        // 実行中のBGMフェード処理をキャンセルする。
        CancelBGMFade();

        // 現在のBGM再生を停止する。
        bgmSource.Stop();

        // AudioSourceに設定されている音声データを解除する。
        bgmSource.clip = null;

        // 現在のBGMとして管理しているデータを解除する。
        currentBgm = null;

        // 一時停止中を示すフラグを解除する。
        isBgmPaused = false;
    }

    /// <summary>
    /// BGMを一時停止する。
    /// </summary>
    public void PauseBGM() {
        // BGMが再生中でなければ、何もせず終了する。
        if (!bgmSource.isPlaying) return;

        // 実行中のBGMフェード処理をキャンセルする。
        CancelBGMFade();

        // BGMを現在の再生位置で一時停止する。
        bgmSource.Pause();

        // 一時停止中であることを記録する。
        isBgmPaused = true;
    }

    /// <summary>
    /// 一時停止したBGMを続きから再開する。
    /// </summary>
    public void ResumeBGM() {
        // 一時停止中でない場合、またはClipがない場合は終了する。
        if (!isBgmPaused || bgmSource.clip == null) return;

        // 一時停止した位置からBGMを再開する。
        bgmSource.UnPause();

        // 一時停止状態を解除する。
        isBgmPaused = false;
    }

    /// <summary>
    /// 指定した秒数でBGMをフェードアウトして停止する。
    /// Time.timeScaleが0でも進行します。
    /// </summary>
    public void FadeOutBGM(float duration = 1f) {

        // 時間が0以下、BGMが再生中でない、またはコンポーネントが無効なら停止する。
        if (duration <= 0f || !bgmSource.isPlaying || !isActiveAndEnabled) {
            // BGMを停止
            StopBGM();
            return;
        }

        // 既存のフェードを止め、現在の音量を引き継ぐ。
        CancelBGMFade(false);

        // 指定時間で音量を下げるコルーチンを開始する。
        bgmFadeCoroutine = StartCoroutine(FadeOutBGMCoroutine(duration)
        );
    }

    /// <summary>
    /// BGMの音量を指定された時間をかけて徐々に下げ、最後にBGMを停止するコルーチン
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator FadeOutBGMCoroutine(float duration) {
        // フェード開始からの経過時間を0秒にする。
        float elapsed = 0f;
        // フェード開始時点の音量を保存する。
        float startVolume = bgmFadeVolume;

        // 指定されたフェード時間が経過するまで繰り返す。
        while (elapsed < duration) {
            // Time.timeScaleの影響を受けずに経過時間を加算する
            elapsed += Time.unscaledDeltaTime;
            // 経過時間に応じて、開始音量から0へ徐々に変化させる
            bgmFadeVolume = Mathf.Lerp(
                startVolume,
                0f,
                Mathf.Clamp01(elapsed / duration)
            );
            // 変更した音量をAudioSourceなどに反映する
            ApplyVolumes();
            // 次のフレームまで待機する
            yield return null;
        }

        // 自分自身をStopCoroutineしないよう、先に参照を外す。
        bgmFadeCoroutine = null;
        StopBGM();
    }

    /// <summary>
    /// BGMのフェード処理をキャンセルする
    /// </summary>
    /// <param name="restoreVolume"></param>
    private void CancelBGMFade(bool restoreVolume = true) {

        // フェード処理が実行中か確認する。
        if (bgmFadeCoroutine != null) {

            // 実行中のフェード用コルーチンを停止する。
            StopCoroutine(bgmFadeCoroutine);

            // コルーチンの参照を解除する。
            bgmFadeCoroutine = null;
        }

        // 音量を元に戻す指定がある場合に実行する。
        if (restoreVolume) {

            // フェード用の音量を通常値の1.0に戻す。
            bgmFadeVolume = 1f;

            // 変更した音量をAudioSourceなどに反映する。
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
        if (instance != this) return;

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
        if (instance == this) {
            instance = null;
        }
    }
}