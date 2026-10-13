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
    private const string SAVE_FILE_NAME = "PlayerData.json";

    // CドライブのProgram Files内にある保存先
    private string SaveFilePath =>
        Path.Combine(
            @"D:\RG_SaveData",
            SAVE_FILE_NAME
        );

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
        // 保存するデータが存在しない場合は処理を終了する
        if (saveData == null) {
            Debug.LogError("保存するセーブデータがありません");
            return;
        }

        try {
            // セーブファイルを保存するフォルダのパスを取得する
            string directoryPath = Path.GetDirectoryName(SaveFilePath);

            // フォルダが存在しない場合は作成する
            // すでに存在していても問題ない
            Directory.CreateDirectory(directoryPath);

            // SaveDataをJSON形式の文字列に変換する
            string json = JsonUtility.ToJson(saveData, true);

            // JSONファイルにデータを書き込む
            // ファイルが存在しない場合は新しく作成される
            File.WriteAllText(SaveFilePath, json);

            // 保存先のパスを含めて成功ログを表示する
            Debug.Log($"セーブが完了しました: {SaveFilePath}");
        }
        catch (Exception e) {
            // 保存に失敗した原因と保存先を表示する
            Debug.LogError(
                $"セーブに失敗しました。\n" +
                $"保存先: {SaveFilePath}\n" +
                $"原因: {e}"
            );
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
