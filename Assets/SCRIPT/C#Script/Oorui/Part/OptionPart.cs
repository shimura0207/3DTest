/*
 *  @file   OptionPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// 設定パート
/// </summary>
public class OptionPart : PartBase {

    // 選択された遷移先
    private OptionMenuSelect selectMenu = OptionMenuSelect.None;

    // 確認画面のメニュー
    private GenericCheckMenu menu;
    // 確認画面のメッセージ
    private const string _GAME_END_MESSAGE = "ゲームを終了しますか？";

    // ゲーム終了ボタン
    [SerializeField] private Button gameEnd;
    // メインメニュー復帰ボタン
    [SerializeField] private Button returnMenu;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        await UniTask.CompletedTask;
    }

    public override async UniTask Setup() {
        await base.Setup();
        Debug.Log("設定画面表示中");
        // 確認画面の取得
        menu = MenuWindowManager.instance.Get<GenericCheckMenu>("Prefab/Part/MenuWindow/GenericCheckMenu");

        // メニューの初期化
        await menu.Initialize();
        await menu.Setup();

        // 選択状態を初期化する
        selectMenu = OptionMenuSelect.None;
        // イベント登録
        gameEnd.onClick.AddListener(OnGameEndSelected);
        returnMenu.onClick.AddListener(OnReturnMenuSelected);
    }

    /// <summary>
    /// メインメニューが選択された時の処理
    /// </summary>
    /// <param name="menu"></param>
    private void OnReturnMenuSelected() {
        // 遷移先をメインメニューに設定する
        selectMenu = OptionMenuSelect.MainMenu;
    }

    /// <summary>
    /// ゲーム終了ボタンが押された時の処理
    /// </summary>
    private void OnGameEndSelected() {
        // ゲーム終了を選択状態にする
        selectMenu = OptionMenuSelect.GameEnd;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // ボタンが選択されるまで繰り返し待機
        while (true) {
            // いずれかのボタンが押されるまで待機
            await UniTask.WaitUntil(() => selectMenu != OptionMenuSelect.None);

            // 選択されたボタンによって処理を分岐
            switch (selectMenu) {
                case OptionMenuSelect.GameEnd: {
                        // ゲーム終了の確認画面を表示
                        bool isGameEnd = await menu.Open(_GAME_END_MESSAGE);

                        // 「はい」が選択された場合
                        if (isGameEnd) {
                            // ゲームを終了
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif
                            return;
                        }

                        // 「いいえ」が選択された場合
                        // 設定画面に留まり、再度ボタン入力を待つ
                        selectMenu = OptionMenuSelect.None;
                        continue;
                    }

                case OptionMenuSelect.MainMenu:
                    // メインメニューパートへ遷移
                    await PartManager.Instance.TransitionPart(GamePart.MainMenu);
                    return;
            }
        }
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
        // 選択状態を初期化する
        selectMenu = OptionMenuSelect.None;
    }
}
