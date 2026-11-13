/*
 *  @file   OptionMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 設定画面で表示するメニュー
/// </summary>
public class OptionMenu : MenuWindowBase {

    // プレイヤー名の入力欄
    [SerializeField]
    private TMP_InputField playerNameInputField;

    // 名前を確定するボタン
    [SerializeField]
    private Button savePlayerNameButton;

    // BGM音量スライダー
    [SerializeField]
    private Slider bgmVolumeSlider;

    // SE音量スライダー
    [SerializeField]
    private Slider seVolumeSlider;

    // BGM音量の数値表示
    [SerializeField]
    private TMP_Text bgmVolumeText;

    // SE音量の数値表示
    [SerializeField]
    private TMP_Text seVolumeText;

    // BGM通常音量ボタン
    [SerializeField]
    private Button bgmVolumeButton;

    // BGM消音ボタン
    [SerializeField]
    private Button bgmMuteButton;

    // SE通常音量ボタン
    [SerializeField]
    private Button seVolumeButton;

    // SE消音ボタン
    [SerializeField]
    private Button seMuteButton;

    // 音量の最大値
    private const int MAX_VOLUME_LEVEL = 10;

    // ミュート前のBGM音量
    private int bgmVolumeBeforeMute = 10;

    // ミュート前のSE音量
    private int seVolumeBeforeMute = 10;

    // BGMがミュートされているか
    private bool isBgmMuted = false;

    // SEがミュートされているか
    private bool isSeMuted = false;
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();

        // 現在保存されているプレイヤー名を入力欄に表示する
        PlayerSaveData saveData = SaveManager.Instance.Load();
        playerNameInputField.text = saveData.playerName;

        // BGMスライダーの範囲を設定する
        bgmVolumeSlider.minValue = 0;
        bgmVolumeSlider.maxValue = MAX_VOLUME_LEVEL;
        bgmVolumeSlider.wholeNumbers = false;

        // SEスライダーの範囲を設定する
        seVolumeSlider.minValue = 0;
        seVolumeSlider.maxValue = MAX_VOLUME_LEVEL;
        seVolumeSlider.wholeNumbers = false;

    }

    /// <summary>
    /// メニューウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // AudioManagerから現在の音量を取得する
        int bgmLevel = Mathf.RoundToInt(AudioManager.instance.BGMVolume * MAX_VOLUME_LEVEL);
        int seLevel = Mathf.RoundToInt(AudioManager.instance.SEVolume * MAX_VOLUME_LEVEL);
        // スライダーに現在の音量を反映する
        bgmVolumeSlider.SetValueWithoutNotify(bgmLevel);
        seVolumeSlider.SetValueWithoutNotify(seLevel);

        // ミュート前の音量を初期化する
        bgmVolumeBeforeMute = bgmLevel > 0 ? bgmLevel : MAX_VOLUME_LEVEL;
        seVolumeBeforeMute = seLevel > 0 ? seLevel : MAX_VOLUME_LEVEL;

        await base.Open();
        // ボタン登録
        ButtonSetup();

        // 音量に応じてボタン表示を更新
        UpdateVolumeButton();

    }

    /// <summary>
    /// 全ボタンの登録
    /// </summary>
    private void ButtonSetup() {
        // プレイヤー名保存ボタンのイベントを登録する
        savePlayerNameButton.onClick.RemoveListener(OnSavePlayerName);
        savePlayerNameButton.onClick.AddListener(OnSavePlayerName);

        // BGMスライダーのイベントを登録する
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        // SEスライダーのイベントを登録する
        seVolumeSlider.onValueChanged.RemoveListener(OnSEVolumeChanged);
        seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);

        // BGM通常音量ボタン
        bgmVolumeButton.onClick.RemoveListener(OnBGMMute);
        bgmVolumeButton.onClick.AddListener(OnBGMMute);

        // BGM消音ボタン
        bgmMuteButton.onClick.RemoveListener(OnBGMMute);
        bgmMuteButton.onClick.AddListener(OnBGMMute);

        // SE通常音量ボタン
        seVolumeButton.onClick.RemoveListener(OnSEMute);
        seVolumeButton.onClick.AddListener(OnSEMute);

        // SE消音ボタン
        seMuteButton.onClick.RemoveListener(OnSEMute);
        seMuteButton.onClick.AddListener(OnSEMute);
    }

    /// <summary>
    /// メニューウィンドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // プレイヤー名保存ボタンのイベントを解除する
        savePlayerNameButton.onClick.RemoveListener(OnSavePlayerName);

        // BGMスライダーのイベントを解除する
        bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

        // SEスライダーのイベントを解除する
        seVolumeSlider.onValueChanged.RemoveListener(OnSEVolumeChanged);


        // BGM通常音量ボタン
        bgmVolumeButton.onClick.RemoveListener(OnBGMMute);

        // BGM消音ボタン
        bgmMuteButton.onClick.RemoveListener(OnBGMMute);

        // SE通常音量ボタン
        seVolumeButton.onClick.RemoveListener(OnSEMute);

        // SE消音ボタン
        seMuteButton.onClick.RemoveListener(OnSEMute);

        await base.Close();
    }

    /// <summary>
    /// 名前の確定ボタンが押されたときの処理
    /// </summary>
    private void OnSavePlayerName() {
        // 入力された名前の前後にある空白を取り除く
        string playerName = playerNameInputField.text.Trim();

        // 名前が空の場合は保存しない
        if (string.IsNullOrWhiteSpace(playerName)) {
            Debug.LogWarning("プレイヤー名を入力してください。");
            return;
        }

        // 禁止ワード一覧をマスターデータから取得する
        var errorWordList = ErrorWordMasterUtility.GetErrorWord();

        // 禁止ワードが取得できなかった場合は保存を中断する
        if (errorWordList == null) {
            Debug.LogError("禁止ワードのマスターデータを取得できませんでした。");
            return;
        }

        // すべての禁止ワードを確認する
        foreach (var errorWord in errorWordList) {

            // データが存在しない場合は次の禁止ワードへ進む
            if (errorWord == null) {
                continue;
            }

            // 禁止ワードの文字列を取得する
            // ※ errorWord.word は実際のデータ定義のフィールド名に合わせる
            string word = errorWord.ErrorWord;

            // 空の禁止ワードは判定対象から除外する
            if (string.IsNullOrWhiteSpace(word)) {
                continue;
            }

            // プレイヤー名に禁止ワードが含まれているか判定する
            if (playerName.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) {

                // 禁止ワードが含まれている場合は保存しない
                Debug.LogWarning($"使用できない文字が含まれています: {word}");
                // InputFieldに入力されている名前をすべて削除する
                playerNameInputField.text = "";

                return;
            }
        }

        // 現在のセーブデータを取得する
        PlayerSaveData saveData = SaveManager.Instance.Load();

        // プレイヤー名だけを更新する
        saveData.playerName = playerName;

        // 更新したセーブデータを保存する
        SaveManager.Instance.Save(saveData);

        Debug.Log($"プレイヤー名を保存しました: {playerName}");
    }

    /// <summary>
    /// BGM音量が変更されたときの処理
    /// </summary>
    private void OnBGMVolumeChanged(float value) {

        // 音量を0～1の範囲に変換する
        AudioManager.instance.SetBGMVolume(
            value / MAX_VOLUME_LEVEL
        );

        // 0より大きい場合はミュート解除用に保存する
        if (value > 0) {
            bgmVolumeBeforeMute = Mathf.RoundToInt(value);
        }

        // 音量に応じてボタン表示を更新する
        UpdateVolumeButton();
    }

    /// <summary>
    /// SE音量が変更されたときの処理
    /// </summary>
    private void OnSEVolumeChanged(float value) {

        // 音量を0～1の範囲に変換する
        AudioManager.instance.SetSEVolume(
            value / MAX_VOLUME_LEVEL
        );

        // 0より大きい場合はミュート解除用に保存する
        if (value > 0) {
            seVolumeBeforeMute = Mathf.RoundToInt(value);
        }

        // 音量に応じてボタン表示を更新する
        UpdateVolumeButton();
    }

    /// <summary>
    /// BGM音量ボタンを押したときの処理
    /// </summary>
    private void OnBGMMute() {

        // 現在の音量が0より大きい場合
        if (bgmVolumeSlider.value > 0) {

            // 現在の音量を保存する
            bgmVolumeBeforeMute = Mathf.RoundToInt(bgmVolumeSlider.value);

            // 音量を0にする
            bgmVolumeSlider.SetValueWithoutNotify(0);

            // BGMを消音にする
            AudioManager.instance.SetBGMVolume(0f);
        }
        else {

            // 消音前の音量に戻す
            bgmVolumeSlider.SetValueWithoutNotify(bgmVolumeBeforeMute);

            // BGM音量を復元する
            AudioManager.instance.SetBGMVolume(
                bgmVolumeBeforeMute / (float)MAX_VOLUME_LEVEL
            );
        }

        // ボタンの表示を更新する
        UpdateVolumeButton();


    }

    /// <summary>
    /// SE音量ボタンを押したときの処理
    /// </summary>
    private void OnSEMute() {

        // 現在の音量が0より大きい場合
        if (seVolumeSlider.value > 0) {

            // 現在の音量を保存する
            seVolumeBeforeMute = Mathf.RoundToInt(seVolumeSlider.value);

            // 音量を0にする
            seVolumeSlider.SetValueWithoutNotify(0);

            // SEを消音にする
            AudioManager.instance.SetSEVolume(0f);
        }
        else {

            // 消音前の音量に戻す
            seVolumeSlider.SetValueWithoutNotify(seVolumeBeforeMute);

            // SE音量を復元する
            AudioManager.instance.SetSEVolume(
                seVolumeBeforeMute / (float)MAX_VOLUME_LEVEL
            );
        }

        // ボタンの表示を更新する
        UpdateVolumeButton();

    }

    /// <summary>
    /// 音量に応じてボタンの表示を切り替える
    /// </summary>
    private void UpdateVolumeButton() {

        // BGM音量が0より大きいか判定する
        bool hasBgmVolume = bgmVolumeSlider.value > 0;

        // SE音量が0より大きいか判定する
        bool hasSeVolume = seVolumeSlider.value > 0;

        // BGMボタンの表示を切り替える
        bgmVolumeButton.gameObject.SetActive(hasBgmVolume);
        bgmMuteButton.gameObject.SetActive(!hasBgmVolume);

        // SEボタンの表示を切り替える
        seVolumeButton.gameObject.SetActive(hasSeVolume);
        seMuteButton.gameObject.SetActive(!hasSeVolume);
    }

}
