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

    // セーブファイル名
    private const string SAVE_FILE_NAME = "SaveData.json";

    // セーブファイルの保存先
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

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
    public void Save(SaveData saveData) {
        if (saveData == null) {
            Debug.LogError("保存するセーブデータがありません");
            return;
        }

        try {
            // SaveDataをJSON文字列に変換する
            string json = JsonUtility.ToJson(saveData, true);

            // JSON文字列をファイルに書き込む
            File.WriteAllText(SaveFilePath, json);

            Debug.Log("セーブが完了しました");
        }
        catch (Exception e) {
            // 保存に失敗した原因をコンソールに表示する
            Debug.LogError($"セーブに失敗しました: {e.Message}");
        }
    }

    /// <summary>
    /// セーブファイルからデータを読み込む
    /// </summary>
    public SaveData Load() {
        // セーブファイルが存在しなければ新規データを返す
        if (!File.Exists(SaveFilePath)) {
            Debug.Log("セーブファイルが存在しないため、新規データを作成します");
            return new SaveData();
        }

        try {
            // ファイルからJSON文字列を読み込む
            string json = File.ReadAllText(SaveFilePath);

            // JSON文字列をSaveDataに変換する
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // 読み込み結果が不正な場合はエラーにする
            if (saveData == null) {
                throw new InvalidDataException("セーブデータを読み込めませんでした");
            }

            // 古いセーブデータなどでリストがnullの場合に備える
            if (saveData.ownedCardIDs == null) {
                saveData.ownedCardIDs = new System.Collections.Generic.List<int>();
            }

            return saveData;
        }
        catch (Exception e) {
            // ファイルの破損などで読み込めない場合は新規データを返す
            Debug.LogError($"ロードに失敗しました: {e.Message}");
            return new SaveData();
        }
    }
}
