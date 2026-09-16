/*
 *  @file   MattchingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// マッチングパート
/// </summary>
public class MatchingPart : PartBase {
    public bool isReturnPart = false;       // 一つ前のシーンに戻る
    public bool isMatchingClick = false;    // マッチングボタンを押したかどうか

    // Inspectorから戻るボタンを設定する
    [SerializeField] private Button returnButton;

    // Inspectorからマッチングボタンを設定する
    [SerializeField] private Button matchingButton;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    public override async UniTask Setup() {
        await base.Setup();
        Debug.Log("マッチング画面表示中");
        // 戻るボタンが押された時の処理を登録する
        returnButton.onClick.AddListener(OnClickReturnButton);

        // マッチングボタンが押された時の処理を登録する
        matchingButton.onClick.AddListener(OnClickMatchingButton);
        // フラグを初期化する
        isReturnPart = false;
        isMatchingClick = false;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {

        // 戻るボタンまたはマッチングボタンが押されるまで待機する
        await UniTask.WaitUntil(() => isReturnPart || isMatchingClick);


        // ←が押されたらひとつ前のパートに戻る
        if (isReturnPart) {
            // メインメニューパートに戻る
            Debug.Log("メインメニューに戻りました。");
            await PartManager.Instance.TransitionPart(eGamePart.MainMenu);
            return;
        }

        // マッチングボタンが押されたらマッチング
        if (isMatchingClick) {
            // マッチング開始

            // マッチしたらメインゲームパートに遷移
            Debug.Log("マッチングしました");
            await PartManager.Instance.TransitionPart(eGamePart.MainGame);
        }


    }


    /// <summary>
    /// 戻るボタンが押された時の処理
    /// </summary>
    private void OnClickReturnButton() {
        // 一つ前のパートへ戻るフラグをONにする
        isReturnPart = true;
    }


    /// <summary>
    /// マッチングボタンが押された時の処理
    /// </summary>
    private void OnClickMatchingButton() {
        // マッチング開始フラグをONにする
        isMatchingClick = true;
    }


    /// <summary>
    /// 片付けそりー
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        // フラグを初期化
        // ボタンイベントを解除して重複登録を防ぐ
        returnButton.onClick.RemoveListener(OnClickReturnButton);
        matchingButton.onClick.RemoveListener(OnClickMatchingButton);
        // フラグを初期化する
        isMatchingClick = false;
        isReturnPart = false;
        await base.Teardown();
    }
}
