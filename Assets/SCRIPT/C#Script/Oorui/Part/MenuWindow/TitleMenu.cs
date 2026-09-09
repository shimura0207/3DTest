/*
 * @file    TitleMenu.cs
 * @author  oorui
 */

using Cysharp.Threading.Tasks;

/// <summary>
/// タイトルで表示するメニューウィンドウ
/// </summary>
public class TitleMenu : MenuWindowBase {
    public bool isCloseMenu = false;    // 入力取得用
    
    /// <summary>
    /// 初期化処理　
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        // ウィンドウオブジェクトを表示する
        gameObject.SetActive(true);

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューウィンドウ表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Open() {
        // 入力フラグをfalseにしておく
        isCloseMenu = false;

        await base.Open();
        // 入力されるのを待つ
        while (true) {
            // 入力されたときの処理
            if (isCloseMenu) {
                // SE再生

                // エフェクト再生

                break;
            }
            // 1フレーム待つ
            await UniTask.DelayFrame(1);
        }
        // メニューウィンドウを閉じる
        await Close();
    }

    /// <summary>
    /// メニューウィンドウが非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        await base.Close();
        // 入力処理をfalseにしておく
        isCloseMenu = false;
        await UniTask.CompletedTask;
    }

}
