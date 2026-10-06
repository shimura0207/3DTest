
/*
 *  @file   MattchingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// マッチングパート
/// </summary>
public class MatchingPart : PartBase {
    private bool isRandomMatch = false;     // ランダムマッチが選択されたか
    private bool isPrivateMatch = false;    // プライベートマッチが選択されたか
    private bool isReturn = false;          // メニューに戻るが選択されたか

    // Inspectorからランダムマッチボタンを設定する
    [SerializeField] private Button randomButton;

    // Inspectorからプライベートマッチボタンを設定する
    [SerializeField] private Button privateButton;

    // Inspectorからメニューに戻るボタンを設定する
    [SerializeField] private Button returnButton;

    // メニュー
    private const string _HOSTORCLIENT_MENU = "Prefab/Part/MenuWindow/HostORClientMenu";
    private HostORClientMenu hostORClientMenu;

    // 接続状況
    private ConnectionState conection = ConnectionState.None;
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
        // 接続状況の初期化
        conection = ConnectionState.None;


        // メニューの取得
        hostORClientMenu = MenuWindowManager.instance.Get<HostORClientMenu>(_HOSTORCLIENT_MENU);
        // メニューの初期化
        await hostORClientMenu.Initialize();
        await hostORClientMenu.Setup();
        // ランダムボタンが押された時の処理を登録する
        randomButton.onClick.AddListener(OnClickRandomButton);
        // マッチングボタンが押された時の処理を登録する
        privateButton.onClick.AddListener(OnClickMatchingButton);
        // 戻るボタンが押された時の処理を登録
        returnButton.onClick.AddListener(OnClickReturnButton);

        // フラグを初期化する
        isRandomMatch = false;
        isPrivateMatch = false;
        isReturn = false;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが選択されるまで繰り返し待機する
        while (true) {
            // ランダムマッチまたはプライベートマッチが選択されるまで待機
            await UniTask.WaitUntil(() => isRandomMatch || isPrivateMatch || isReturn);

            // ランダムマッチが選択された場合
            if (isRandomMatch) {
                // ランダムマッチパートに遷移
                Debug.Log("ランダムマッチを選択");



                await PartManager.Instance.TransitionPart(GamePart.MainGame, 1);
                return;
            }

            // プライベートマッチが選択された場合
            if (isPrivateMatch) {
                // ホストまたはクライアントを選択するメニューを開く
                Debug.Log("プライベートマッチを選択");
                conection = await hostORClientMenu.Open(conection);

                // キャンセルされた場合
                if (conection == ConnectionState.None) {
                    // プライベートマッチの選択状態を解除
                    isPrivateMatch = false;

                    // ランダムマッチの選択状態も初期化
                    isRandomMatch = false;

                    // 再度ボタン選択を待つ
                    continue;
                }

                // ホストまたはクライアントが選択された場合
                // プライベートマッチパートに遷移
                await PartManager.Instance.TransitionPart(GamePart.PrivateMatch, conection);
                return;
            }

            // メインメニューに戻るが選択された時
            if (isReturn) {
                // メインメニューに遷移する
                await PartManager.Instance.TransitionPart(GamePart.MainMenu);
            }
        }
    }


    /// <summary>
    /// ランダムマッチボタンが押された時の処理
    /// </summary>
    private void OnClickRandomButton() {
        // ランダムマッチフラグをONにする
        isRandomMatch = true;
    }


    /// <summary>
    /// プライベートマッチボタンが押された時の処理
    /// </summary>
    private void OnClickMatchingButton() {
        // プライベートマッチフラグをONにする
        isPrivateMatch = true;
    }

    /// <summary>
    /// メニューに戻るボタンが押された時の処理
    /// </summary>
    private void OnClickReturnButton() {
        // メニューに戻るフラグをONにする
        isReturn = true;
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        // フラグを初期化
        // ボタンイベントを解除して重複登録を防ぐ
        randomButton.onClick.RemoveListener(OnClickRandomButton);
        privateButton.onClick.RemoveListener(OnClickMatchingButton);
        returnButton.onClick.RemoveListener(OnClickReturnButton);
        // フラグを初期化する
        isPrivateMatch = false;
        isRandomMatch = false;
        isReturn = false;
        await base.Teardown();
    }
}
