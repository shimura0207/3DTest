/*
 *  @file   SaveManager
 *  @author oorui
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// セーブファイルの保存と読み込みを管理するクラス
/// </summary>
public class SaveManager : MonoBehaviour {

    // 自身への参照
    public static SaveManager Instance { get; private set; }

    // プレイヤー関連の暗号化セーブファイル名
    private const string PLAYER_SAVE_FILE_NAME = "PlayerData.dat";

    // 所持カード用の暗号化セーブファイル名
    private const string CARD_SAVE_FILE_NAME = "PossessionCardData.dat";


    // 新しいセーブデータの保存先
    private string PlayerDataSaveFilePath =>
        Path.Combine(Application.persistentDataPath, PLAYER_SAVE_FILE_NAME);

    private string CardDataSaveFilePath =>
        Path.Combine(Application.persistentDataPath, CARD_SAVE_FILE_NAME);

    // カードのポイント変換定数
    private const int CARD_ACQUIRED = 10;

    /// <summary>
    /// SaveManagerの初期化
    /// </summary>
    private void Awake() {

        // 同じシーンに複数のSaveManagerが存在しないようにする
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        // 自身をシングルトンとして登録する
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
            // 保存先フォルダを作成する
            string directoryPath = Path.GetDirectoryName(PlayerDataSaveFilePath);
            Directory.CreateDirectory(directoryPath);

            // セーブデータをJSON形式に変換する
            string json = JsonUtility.ToJson(saveData, true);

            // JSONを暗号化する
            byte[] encryptedData = SaveDataCrypto.Encrypt(json);

            // 暗号化したデータをバイト配列として保存する
            File.WriteAllBytes(PlayerDataSaveFilePath, encryptedData);

            // 保存成功ログを表示する
            Debug.Log($"セーブが完了しました: {PlayerDataSaveFilePath}");
        }
        catch (Exception e) {
            // 保存に失敗した原因を表示する
            Debug.LogError(
                $"セーブに失敗しました。\n" +
                $"保存先: {PlayerDataSaveFilePath}\n" +
                $"原因: {e}"
            );
        }
    }


    /// <summary>
    /// デッキをセーブデータに追加する
    /// </summary>
    /// <returns>作成したデッキのID</returns>
    public int SaveDeck(string deckName, List<int> cardIDs) {

        // 保存データを読み込む
        PlayerSaveData saveData = Load();

        // デッキ一覧が存在しなければ初期化する
        if (saveData.deckList == null) {
            saveData.deckList = new List<DeckSaveData>();
        }

        // 次回発行するIDが不正な値なら初期値に戻す
        if (saveData.nextDeckID < 1) {
            saveData.nextDeckID = 1;
        }

        // 新しいデッキ情報を作成する
        DeckSaveData newDeck = new DeckSaveData {
            deckID = saveData.nextDeckID,
            deckName = deckName,

            // 渡されたカードID一覧をコピーして保持する
            cardIDs = cardIDs != null
                ? new List<int>(cardIDs)
                : new List<int>()
        };

        // デッキ一覧に追加する
        saveData.deckList.Add(newDeck);

        // 次回発行するIDを1つ増やす
        saveData.nextDeckID++;

        // 更新したセーブデータを保存する
        Save(saveData);

        // 作成したデッキのIDを返す
        return newDeck.deckID;
    }


    /// <summary>
    /// デッキIDを指定してデッキ情報を取得する
    /// </summary>
    public DeckSaveData GetDeckData(int deckID) {

        // 保存データを読み込む
        PlayerSaveData saveData = Load();

        // デッキ一覧が存在しなければ取得できない
        if (saveData.deckList == null) {
            return null;
        }

        // 指定されたIDと一致するデッキを検索する
        return saveData.deckList.Find(deck => deck != null && deck.deckID == deckID);
    }


    /// <summary>
    /// デッキIDを指定してカードID一覧を取得する
    /// </summary>
    public List<int> GetDeckCardIDs(int deckID) {

        // 指定されたIDのデッキ情報を取得する
        DeckSaveData deckData = GetDeckData(deckID);

        // デッキが見つからなければnullを返す
        if (deckData == null || deckData.cardIDs == null) {
            return null;
        }

        // 元の保存データを直接変更されないようコピーして返す
        return new List<int>(deckData.cardIDs);
    }


    /// <summary>
    /// 既存デッキにIDを割り振り、次回発行するIDを調整する
    /// </summary>
    /// <returns>セーブデータに変更があったか</returns>
    private bool EnsureDeckIDs(PlayerSaveData saveData) {

        // 保存データが存在しなければ処理を終了する
        if (saveData == null) {
            return false;
        }

        // デッキ一覧が存在しなければ初期化する
        bool isChanged = false;

        if (saveData.deckList == null) {
            saveData.deckList = new List<DeckSaveData>();
            isChanged = true;
        }

        // 使用済みのデッキIDを管理する
        HashSet<int> usedIDs = new HashSet<int>();

        // IDの再設定が必要なデッキを管理する
        List<DeckSaveData> needsIDDecks = new List<DeckSaveData>();

        // 既存デッキのIDを確認する
        foreach (DeckSaveData deck in saveData.deckList) {

            // nullのデッキ情報はスキップする
            if (deck == null) {
                continue;
            }

            // IDが正しく、まだ使用されていなければ登録する
            if (deck.deckID > 0 && usedIDs.Add(deck.deckID)) {
                continue;
            }

            // IDが未設定、または重複している場合は再設定対象にする
            needsIDDecks.Add(deck);
        }

        // 次回発行するIDが不正な場合は初期値にする
        if (saveData.nextDeckID < 1) {
            saveData.nextDeckID = 1;
            isChanged = true;
        }

        // IDが重複しないように次回発行番号を調整する
        foreach (DeckSaveData deck in needsIDDecks) {

            // 使用済みIDと重複する間は番号を進める
            while (usedIDs.Contains(saveData.nextDeckID)) {
                saveData.nextDeckID++;
            }

            // 新しいIDを割り振る
            deck.deckID = saveData.nextDeckID;

            // 割り振ったIDを使用済みに登録する
            usedIDs.Add(deck.deckID);

            // 次回発行するIDを1つ増やす
            saveData.nextDeckID++;

            // セーブデータが変更されたことを記録する
            isChanged = true;
        }

        // 既存の最大IDより小さい番号が次回発行番号にならないようにする
        foreach (int id in usedIDs) {

            if (saveData.nextDeckID <= id) {
                saveData.nextDeckID = id + 1;
                isChanged = true;
            }
        }

        // 変更の有無を返す
        return isChanged;
    }

    /// <summary>
    /// 既存デッキの内容を更新する
    /// </summary>
    /// <param name="deckID">更新するデッキのID</param>
    /// <param name="deckName">新しいデッキ名</param>
    /// <param name="cardIDs">新しいカード一覧</param>
    /// <returns>更新できたかどうか</returns>
    public bool UpdateDeck(int deckID, string deckName, List<int> cardIDs) {

        // セーブデータを読み込む
        PlayerSaveData saveData = Load();

        // デッキ一覧が存在しなければ終了する
        if (saveData.deckList == null) return false;

        // 更新対象のデッキを検索する
        DeckSaveData targetDeck = saveData.deckList.Find(
            deck => deck != null && deck.deckID == deckID
        );

        // 対象デッキが見つからなければ終了する
        if (targetDeck == null) return false;

        // デッキ名を更新する
        targetDeck.deckName = deckName;

        // カード一覧をコピーして更新する
        targetDeck.cardIDs = cardIDs != null
            ? new List<int>(cardIDs)
            : new List<int>();

        // 更新したセーブデータを保存する
        Save(saveData);

        return true;
    }

    /// <summary>
    /// デッキIDを指定してデッキを削除する
    /// </summary>
    /// <param name="deckID">削除するデッキのID</param>
    /// <returns>削除できたかどうか</returns>
    public bool DeleteDeck(int deckID) {

        // セーブデータを読み込む
        PlayerSaveData saveData = Load();

        // デッキ一覧が存在しなければ削除できない
        if (saveData.deckList == null) return false;

        // 指定されたIDのデッキを削除する
        int removedCount = saveData.deckList.RemoveAll(
            deck => deck != null && deck.deckID == deckID
        );

        // 削除対象が見つからなければ終了する
        if (removedCount == 0) return false;

        // 更新したセーブデータを保存する
        Save(saveData);

        return true;
    }

    /// <summary>
    /// セーブファイルからデータを読み込む
    /// </summary>
    public PlayerSaveData Load() {
        // 新しいセーブファイルが存在しない場合
        if (!File.Exists(PlayerDataSaveFilePath)) {
            Debug.Log("既存のセーブファイルが存在しないため、新規データを作成します");
            // 新しいプレイヤーデータを返す
            return new PlayerSaveData();
        }

        try {

            // 暗号化データを読み込む
            byte[] encryptedData = File.ReadAllBytes(PlayerDataSaveFilePath);

            // 改ざん検知後に復号
            string json = SaveDataCrypto.Decrypt(encryptedData);

            // JSONからプレイヤーデータに変換
            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);

            // 読み込み結果を確認
            if (saveData == null) {
                throw new InvalidDataException("セーブデータが不正です");
            }

            // デッキIDの整合性を確認
            if (EnsureDeckIDs(saveData)) {
                // 更新したデータを保存
                Save(saveData);
            }

            return saveData;
        }
        catch (Exception e) {

            // 改ざんや破損を検知した場合
            Debug.LogError($"プレイヤーデータの読み込みに失敗しました: {e}");

            // 元のセーブデータを上書きしない
            throw;
        }
    }


    /// <summary>
    /// 所持カードデータをJSONファイルに保存する
    /// </summary>
    public void SaveCards(PossessionCardSaveData cardSaveData) {

        // 保存するデータが存在しない場合は処理を終了する
        if (cardSaveData == null) {
            Debug.LogError("保存する所持カードデータがありません");
            return;
        }

        try {
            // 保存先フォルダを作成する
            Directory.CreateDirectory(Path.GetDirectoryName(CardDataSaveFilePath));

            // 所持カードデータをJSON形式に変換する
            string json = JsonUtility.ToJson(cardSaveData, true);

            // JSONを暗号化する
            byte[] encryptedData = SaveDataCrypto.Encrypt(json);

            // 暗号化したデータをバイト配列として保存する
            File.WriteAllBytes(CardDataSaveFilePath, encryptedData);

            // 保存成功ログを表示する
            Debug.Log($"所持カードの保存が完了しました: {CardDataSaveFilePath}");
        }
        catch (Exception e) {
            // 保存に失敗した原因を表示する
            Debug.LogError($"所持カードの保存に失敗しました: {e}");
        }
    }

    /// <summary>
    /// カードを1枚入手する
    /// </summary>
    /// <param name="cardID">入手したカードのID</param>
    /// <param name="duplicatePoint">4枚目以降に変換するポイント</param>
    /// <returns>カード入手処理の結果</returns>
    public void AcquireCard(int cardID) {
        // 不正なカードIDを受け付けない
        if (cardID <= 0) return;

        // 所持カードデータを読み込む
        PossessionCardSaveData cardSaveData = LoadCards();

        // 同じカードIDの所持枚数を確認する
        int ownedCount = 0;
        // 取得したカードとセーブデータを使用して走査
        foreach (int id in cardSaveData.cardID) {
            // 入手対象と同じカードIDなら枚数を増やす
            if (id == cardID) {
                ownedCount++;
            }
        }

        // 既に3枚所持している場合はポイントに変換する
        if (ownedCount >= 3) {

            // ポイント管理クラスが存在しなければ終了する
            if (CreatePointManager.Instance == null) return;

            // 4枚目以降は10ポイントに変換する
            CreatePointManager.Instance.AddPoint(CARD_ACQUIRED);

            Debug.Log($"カードID:{cardID} は4枚目以降のため10ポイントに変換しました");
            return;
        }

        // 所持カードにカードIDを1枚追加する
        cardSaveData.cardID.Add(cardID);

        // 所持カードデータを保存する
        SaveCards(cardSaveData);

        // 入手ログを表示する
        Debug.Log($"カードID:{cardID} を入手しました。現在の所持枚数:{ownedCount + 1}");
    }
    /// <summary>
    /// 所持カードのセーブファイルを読み込む
    /// </summary>
    public PossessionCardSaveData LoadCards() {

        // 新しいセーブファイルが存在しない場合
        if (!File.Exists(CardDataSaveFilePath)) {
            Debug.Log("既存の所持カードデータが存在しないため、新規作成します");
            // 空の所持カードデータを返す
            return new PossessionCardSaveData();
        }

        try {

            // 暗号化データを読み込む
            byte[] encryptedData = File.ReadAllBytes(CardDataSaveFilePath);

            // 改ざん検知後に復号
            string json = SaveDataCrypto.Decrypt(encryptedData);

            // JSONから所持カードデータに変換
            PossessionCardSaveData cardSaveData = JsonUtility.FromJson<PossessionCardSaveData>(json);

            // 読み込み結果を確認
            if (cardSaveData == null) {
                throw new InvalidDataException("所持カードデータが不正です");
            }

            // リストがnullの場合は初期化
            if (cardSaveData.cardID == null) {
                cardSaveData.cardID = new List<int>();
            }

            return cardSaveData;
        }
        catch (Exception e) {

            // 改ざんや破損を検知した場合
            Debug.LogError($"所持カードデータの読み込みに失敗しました: {e}");

            // 元のセーブデータを上書きしない
            throw;
        }
    }

    /// <summary>
    /// 所持カードのセーブファイルが存在するか確認する
    /// </summary>
    /// <returns>セーブファイルが存在するか</returns>
    public bool HasCardSaveFile() {

        // 所持カードのセーブファイルが存在するか返す
        return File.Exists(CardDataSaveFilePath);
    }
}