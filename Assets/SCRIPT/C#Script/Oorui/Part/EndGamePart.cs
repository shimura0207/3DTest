/*
 *  @file   EndGamePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルト画面
/// </summary>
public class EndGamePart : PartBase {
    // 自分視点の対戦結果を保持する
    private BattleState battleResult = BattleState.None;

    // 選択された遷移先
    private ResultSelectMenu selectMenu = ResultSelectMenu.None;

    // 勝利時に表示する画像オブジェクト
    [SerializeField] private GameObject winImage;
    // 敗北時に表示する画像オブジェクト
    [SerializeField] private GameObject loseImage;

    // メニューに戻るボタン
    [SerializeField] private Button returnMenuButton;

    public override async UniTask Initialize() {
        // 基底側の処理を呼ぶ
        await base.Initialize();
        // オブジェクトを非表示
        winImage.SetActive(false);
        loseImage.SetActive(false);
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        // 基底側の処理を呼ぶ
        await base.Setup();
        // オブジェクトを非表示
        winImage.SetActive(false);
        loseImage.SetActive(false);
        returnMenuButton.onClick.AddListener(OnReturnMenuSelected);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnReturnMenuSelected() {
        // 遷移先をリザルトに設定する
        selectMenu = ResultSelectMenu.MainMenu;
    }

    public override async UniTask Execute() {
        // 選択状態を初期化する
        selectMenu = ResultSelectMenu.None;

        // リザルト結果に応じて変更
        BattleResult(battleResult);

        // メニューが選択されるまで待機する
        while (selectMenu == ResultSelectMenu.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }

        // マッチング中であれば終了する
        if(CustomNetworkManager.instance != null && CustomNetworkManager.instance.IsMatchStarted) {
            // マッチング終了
            CustomNetworkManager.instance.LeaveRoom();
        }

        // メインメニュー画面に遷移する
        await PartManager.Instance.TransitionPart(GamePart.MainMenu);
    }

    /// <summary>
    /// 対戦結果を設定する
    /// </summary>
    /// <param name="result">自分視点の対戦結果</param>
    public void SetBattleResult(BattleState result) {
        // 受け取った対戦結果を保存する
        battleResult = result;
    }

    /// <summary>
    /// 対戦結果を設定する
    /// </summary>
    /// <param name="result">対戦結果</param>
    public void BattleResult(BattleState result) {
        // 対戦結果に応じて表示内容を切り替える
        switch (result) {
            case BattleState.Win:
                // 勝利画像表示
                Debug.Log("Win");
                winImage.SetActive(true);
                break;

            case BattleState.Lose:
                // 敗北画像表示
                Debug.Log("Lose");
                loseImage.SetActive(true);
                break;
        }
    }
}
