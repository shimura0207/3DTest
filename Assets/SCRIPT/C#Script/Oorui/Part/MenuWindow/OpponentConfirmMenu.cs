/*
 *  @file   OpponentConfirmMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 対戦相手を確認するメニュー
/// </summary>
public class OpponentConfirmMenu : MenuWindowBase {

    // テーマIDと表示するオブジェクトを紐づけるクラス
    [Serializable]
    private class ThemeObjectData {

        // テーマID
        public int themeID;

        // 表示するオブジェクト
        public GameObject targetObject;
    }

    // 自分のテーマ表示リスト
    [SerializeField]
    private List<ThemeObjectData> ownThemeList = new List<ThemeObjectData>();

    // 対戦相手のテーマ表示リスト
    [SerializeField]
    private List<ThemeObjectData> opponentThemeList = new List<ThemeObjectData>();

    // 自分の名前を表示するテキスト
    [SerializeField]
    private TMP_Text ownNameText = null;

    // 対戦相手の名前を表示するテキスト
    [SerializeField]
    private TMP_Text opponentNameText = null;

    // 自分が先攻か後攻かを表示するテキスト
    [SerializeField]
    private TMP_Text ownTurnText = null;

    // 対戦相手が先攻か後攻かを表示するテキスト
    [SerializeField]
    private TMP_Text opponentTurnText = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();

        // 名前の表示を初期化する
        ownNameText.text = "";
        opponentNameText.text = "";
    }

    /// <summary>
    /// メニューが開かれた時の処理
    /// </summary>
    public override async UniTask Open() {
        await base.Open();

        // 自分の名前を表示する
        ownNameText.text = PlayerNameManager.ownName;

        // 対戦相手の名前を表示する
        opponentNameText.text = PlayerNameManager.opponentName;

        // 自分のテーマIDを取得する
        int ownThemeID = (int)PlayerDeckInfoManager.ownTheme;

        // 対戦相手のテーマIDを取得する
        int opponentThemeID = (int)PlayerDeckInfoManager.opponentTheme;

        // 自分のテーマ画像を切り替える
        RefreshThemeObject(ownThemeList, ownThemeID);

        // 対戦相手のテーマ画像を切り替える
        RefreshThemeObject(opponentThemeList, opponentThemeID);

        // 自分が先攻か後攻かを取得する
        bool isOwnFirst = FirstPlayerManager.firstPlayer == PlayerType.Self;

        // 自分の先攻・後攻を表示する
        ownTurnText.text = isOwnFirst ? "先攻" : "後攻";

        // 対戦相手は自分と反対の先攻・後攻になる
        opponentTurnText.text = isOwnFirst ? "後攻" : "先攻";

        // 何かキーが押されるまで待機する
        await UniTask.Delay(3000);

        await Close();
    }

    /// <summary>
    /// テーマIDに一致するオブジェクトだけを表示する
    /// </summary>
    /// <param name="themeList">テーマ表示リスト</param>
    /// <param name="themeID">表示するテーマID</param>
    private void RefreshThemeObject(List<ThemeObjectData> themeList, int themeID) {

        // リスト内のオブジェクトをすべて非表示にする
        foreach (ThemeObjectData data in themeList) {

            if (data.targetObject == null) {
                continue;
            }

            data.targetObject.SetActive(false);
        }

        // テーマIDが一致するオブジェクトを探す
        foreach (ThemeObjectData data in themeList) {

            if (data.targetObject == null) {
                continue;
            }

            // IDが一致したオブジェクトを表示する
            if (data.themeID == themeID) {
                data.targetObject.SetActive(true);
                break;
            }
        }
    }

    /// <summary>
    /// メニューが閉じられた時の処理
    /// </summary>
    public override async UniTask Close() {
        await base.Close();
    }
}