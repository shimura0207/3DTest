/*
 *  @file   SaveManager
 *  @author oorui
 */

using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// セーブファイルの保存と読み込みを管理するクラス
/// </summary>
public class SaveManager : MonoBehaviour {
    // 自身への参照
    public static SaveManager Instance { get; private set; }

    /// <summary>
    /// プレイヤー関連のセーブファイル名
    /// </summary>
    private const string PLAYER_SAVE_FILE_NAME = "PlayerData.json";

    /// <summary>
    /// 所持カード用のセーブファイル名
    /// </summary>
    private const string CARD_SAVE_FILE_NAME = "PossessionCardData.json";

    // セーブデータの保存先
    private string PlayerDataSaveFilePath => Path.Combine(@"D:\RG_SaveData", PLAYER_SAVE_FILE_NAME);
    private string CardDataSaveFilePath => Path.Combine(@"D:\RG_SaveData", CARD_SAVE_FILE_NAME);

    // 同じシーンに複数のSaveManagerが存在しないようにする
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // シーンを切り替えてもSaveManagerを維持する
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// セーブデータをJSON形式でファイルに保存する
    /// </summary>
    public void Save(PlayerSaveData saveData) {
        // 保存するデータが存在しない場合は処理を終了する
        if (saveData == null) {
            Debug.LogError("保存するセーブデータがありません");
            return;
        }

        try {
            // セーブファイルを保存するフォルダのパスを取得する
            string directoryPath = Path.GetDirectoryName(PlayerDataSaveFilePath);

            // フォルダが存在しない場合は作成する
            // すでに存在していても問題ない
            Directory.CreateDirectory(directoryPath);

            // SaveDataをJSON形式の文字列に変換する
            string json = JsonUtility.ToJson(saveData, true);

            // JSONファイルにデータを書き込む
            // ファイルが存在しない場合は新しく作成される
            File.WriteAllText(PlayerDataSaveFilePath, json);

            // 保存先のパスを含めて成功ログを表示する
            Debug.Log($"セーブが完了しました: {PlayerDataSaveFilePath}");
        }
        catch (Exception e) {
            // 保存に失敗した原因と保存先を表示する
            Debug.LogError(
                $"セーブに失敗しました。\n" +
                $"保存先: {PlayerDataSaveFilePath}\n" +
                $"原因: {e}"
            );
        }
    }

    /// <summary>
    /// 所持カードデータをJSONファイルに保存する
    /// </summary>
    public void SaveCards(PossessionCardSaveData cardSaveData) {
        // 保存するデータが存在しない場合は終了する
        if (cardSaveData == null) {
            Debug.LogError("保存する所持カードデータがありません");
            return;
        }

        try {
            // 保存先フォルダを作成する
            Directory.CreateDirectory(Path.GetDirectoryName(CardDataSaveFilePath));

            // 所持カードデータをJSON形式に変換する
            string json = JsonUtility.ToJson(cardSaveData, true);

            // カード専用のJSONファイルに保存する
            File.WriteAllText(CardDataSaveFilePath, json);

            Debug.Log($"所持カードの保存が完了しました: {CardDataSaveFilePath}");
        }
        catch (Exception e) {
            // エラー表示
            Debug.LogError($"所持カードの保存に失敗しました: {e}");
        }
    }

    /// <summary>
    /// セーブファイルからデータを読み込む
    /// </summary>
    public PlayerSaveData Load() {
        // セーブファイルが存在しなければ新規データを返す
        if (!File.Exists(PlayerDataSaveFilePath)) {
            Debug.Log("セーブファイルが存在しないため、新規データを作成します");
            return new PlayerSaveData();
        }

        try {
            // ファイルからJSON文字列を読み込む
            string json = File.ReadAllText(PlayerDataSaveFilePath);

            // JSON文字列をSaveDataに変換する
            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);

            // 読み込み結果が不正な場合はエラーにする
            if (saveData == null) {
                throw new InvalidDataException("セーブデータを読み込めませんでした");
            }

            return saveData;
        }
        catch (Exception e) {
            // ファイルの破損などで読み込めない場合は新規データを返す
            Debug.LogError($"ロードに失敗しました: {e.Message}");
            return new PlayerSaveData();
        }
    }

    /// <summary>
    /// 所持カードのセーブファイルを読み込む
    /// </summary>
    public PossessionCardSaveData LoadCards() {
        // ファイルが存在しなければ空の所持カードデータを返す
        if (!File.Exists(CardDataSaveFilePath)) {
            Debug.Log("所持カードのセーブファイルがないため、新規データを作成します");
            return new PossessionCardSaveData();
        }

        try {
            // JSONファイルから文字列を読み込む
            string json = File.ReadAllText(CardDataSaveFilePath);

            // JSON文字列を所持カードデータに変換する
            PossessionCardSaveData cardSaveData =
                JsonUtility.FromJson<PossessionCardSaveData>(json);

            // 読み込み結果が不正な場合はエラーにする
            if (cardSaveData == null) {
                throw new InvalidDataException(
                    "所持カードデータを読み込めませんでした"
                );
            }

            // リストがnullの場合は空のリストを用意する
            if (cardSaveData.cardID == null) {
                cardSaveData.cardID = new System.Collections.Generic.List<int>();
            }

            return cardSaveData;
        }
        catch (Exception e) {
            // 読み込み失敗時はエラーを表示して空データを返す
            Debug.LogError($"所持カードの読み込みに失敗しました: {e.Message}");
            return new PossessionCardSaveData();
        }
    }
}
