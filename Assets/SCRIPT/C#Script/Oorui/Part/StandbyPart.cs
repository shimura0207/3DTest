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

        // 現在の所持カードセーブデータを取得
        PossessionCardSaveData saveData = SaveManager.Instance.LoadCards();

        // どちらかがなければ処理を終了する
        if (rental == null || saveData == null) return;

        // 所持カードリストを初期化する
        saveData.cardID = new List<int>();

        // レンタルデッキのカードIDをすべて追加する
        saveData.cardID.AddRange(rental);

        // 所持カードのセーブデータを保存する
        SaveManager.Instance.SaveCards(saveData);

        // デバッグログを表示する
        Debug.Log($"レンタルデッキ登録完了：{saveData.cardID.Count}枚");


    }

}
