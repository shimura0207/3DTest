/*
 *  @file   RandomMatchingMenu
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// ランダムマッチの開始と進行状況の表示を管理するメニュー
/// </summary>
public class RandomMatchingMenu : MenuWindowBase {

    // メニューが開いているかどうか
    private bool isMenuOpen = false;

    // 接続中に表示するテキスト
    [SerializeField]
    private TMP_Text matchingText = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        await base.Initialize();

        // 初期状態を設定する
        isMenuOpen = false;
        matchingText.text = "";
    }

    /// <summary>
    /// メニューが開かれたときの処理
    /// </summary>
    public override async UniTask Open() {
        // メニューウィンドウを表示する
        await base.Open();

        // メニューを開いている状態にする
        isMenuOpen = true;

        // マッチング中の表示にする
        matchingText.text = "対戦相手を探しています...";

        // ランダムマッチを開始する
        TitleManager.instance.OnRandomMatchButton();

        // 対戦開始通知を受け取るまで待機する
        await UniTask.WaitUntil(() =>
            !isMenuOpen ||
            CustomNetworkManager.instance == null ||
            CustomNetworkManager.instance.IsMatchStarted
        );

        // メニューが閉じられた場合は後続処理を行わない
        if (!isMenuOpen) return;

        // NetworkManagerがなくなった場合はエラー表示にする
        if (CustomNetworkManager.instance == null) {
            matchingText.text = "通信が終了しました。";
            return;
        }

        // 対戦相手を確認したら表示を更新する
        matchingText.text = "対戦相手が見つかりました。";

        // 少し表示してから閉じる処理
         await UniTask.Delay(1000);

        // マッチング後にメニューを閉じる
        await Close();
    }

    /// <summary>
    /// ウィンドウを閉じるときの処理
    /// </summary>
    public override async UniTask Close() {
        // 待機中の処理を終了させる
        isMenuOpen = false;

        // メニューウィンドウを非表示にする
        await base.Close();

        // 表示内容を初期化する
        matchingText.text = "";
    }
}