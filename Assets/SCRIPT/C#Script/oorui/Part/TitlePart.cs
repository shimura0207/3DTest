/*
 *  @file   TitlePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;

/// <summary>
/// タイトルパート
/// </summary>
public class TitlePart : PartBase {

    // タイトル画面の階層パス
    private const string _MENUWINDOW_TITLE = "Prefab/Part/MenuWindow/TitleMenu";

    // タイトルメニュー
    private TitleMenu title;

    // 選択された遷移先
    private MainMenuSelect selectMenu = MainMenuSelect.Title;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 基底側の処理を呼ぶ
        await base.Initialize();
        // タイトルメニューを取得する
        title = MenuWindowManager.instance.Get<TitleMenu>(_MENUWINDOW_TITLE);
        // タイトルメニューを初期化する
        await title.Initialize();

        // タイトルメニューの選択イベントを登録する
        RegisterMenuEvent();
    }
    
    /// <summary>
    /// 使用前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        // 基底側の処理を呼ぶ
        await base.Setup();
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューイベントを登録する
    /// </summary>
    private void RegisterMenuEvent() {
        // タイトルメニューが選択された時の処理を登録
        title.OnSelected += OnTitleSelected;
    }

    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnTitleSelected(MenuWindowBase menu) {

        // 遷移先をタイトルに設定する
        selectMenu = MainMenuSelect.Title;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {
        // BGMを再生

        // 選択状態を初期化
        selectMenu = MainMenuSelect.None;

        // タイトルメニューを表示
        await title.Open();

        // メニューが選択されるまで待機する
        while(selectMenu == MainMenuSelect.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }
        // タイトルメニューを閉じる
        await title.Close();
        // メインメニュー画面に遷移
        await PartManager.Instance.TransitionPart(GamePart.MainMenu);
    }


}
