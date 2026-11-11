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
        // すでに所持カードのセーブファイルが存在する場合は終了する
        if (SaveManager.Instance.HasCardSaveFile()) {
            Debug.Log("既存の所持カードデータが存在するため登録をスキップします");
            return;
        }

        // レンタルデッキを取得
        List<int> rental = DeckDataMasterUtility.GetDeckMaster(1);

        // レンタルデッキが取得できなければ処理を終了する
        if (rental == null) return;

        // 新規の所持カードセーブデータを取得
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // セーブデータが存在しなければ処理を終了する
        if (saveData == null) return;

        // レンタルデッキのカードIDをすべて追加する
        saveData.cardID.AddRange(rental);

        // 所持カードのセーブデータを保存する
        SaveManager.Instance.SaveCards(saveData);

        // デバッグログを表示する
        Debug.Log($"初回レンタルデッキ登録完了：{rental.Count}枚");


    }

}
