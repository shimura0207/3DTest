/*
 *  @file   StandbyPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 準備パート
/// </summary>
public class StandbyPart : PartBase {
    public override async UniTask Execute() {

        // マスターデータの読み込み
        MasterDataManager.LoadAllData();

        // 所持カードセーブデータにレンタルデッキを登録する
        AddPossessionCardData();

        // デバッグログを表示する
        Debug.Log("StandbyPart通過");
        // タイトルパートへ遷移し、遷移処理が完了するまで待機する
        await PartManager.Instance.TransitionPart(GamePart.Title);
    }


    /// <summary>
    /// 所持カードセーブデータにカードを登録していく
    /// </summary>
    private void AddPossessionCardData() {
        // レンタルデッキを取得
        List<int> rental = DeckDataMasterUtility.GetDeckMaster(1);

        // 現在の所持カードセーブデータの取得
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // どちらかがなければ、処理を終わる
        if (rental == null || saveData == null) return;

        // カードIDのリストが初期化されていない場合は新しく作成する
        if (saveData.cardID == null) {
            saveData.cardID = new List<int>();
        }

        // レンタルデッキのカードIDを所持カードリストに追加する
        if (saveData.cardID.Count == 0) {
            saveData.cardID.AddRange(rental);

            // 初回登録した所持カードを保存する
            SaveManager.Instance.SaveCards(saveData);
        }

        // 所持カードのセーブデータをファイルに保存する
        SaveManager.Instance.SaveCards(saveData);

        // デバッグログを表示する
        Debug.Log("StandbyPart通過");


    }

}
