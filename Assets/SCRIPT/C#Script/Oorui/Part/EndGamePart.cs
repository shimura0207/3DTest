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
    private MainMenuSelect selectMenu = MainMenuSelect.EndGame;

    // スタートボタン
    [SerializeField] private Button endButton;

    public override async UniTask Initialize() {
        // 基底側の処理を呼ぶ
        await base.Initialize();
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        // 基底側の処理を呼ぶ
        await base.Setup();
        endButton.onClick.AddListener(OnEndGameSelected);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnEndGameSelected() {
        // 遷移先をタイトルに設定する
        selectMenu = MainMenuSelect.EndGame;
    }

    public override async UniTask Execute() {
        // 選択状態を初期化する
        selectMenu = MainMenuSelect.None;

        // リザルト結果に応じて変更
        BattleResult(battleResult);

        // メニューが選択されるまで待機する
        while (selectMenu == MainMenuSelect.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
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
                break;

            case BattleState.Lose:
                // 敗北画像表示
                Debug.Log("Lose");
                break;
        }
    }
}
