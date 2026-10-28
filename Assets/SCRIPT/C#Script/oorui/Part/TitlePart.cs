/*
 *  @file   TitlePart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// タイトルパート
/// </summary>
public class TitlePart : PartBase {


    // スタートボタン
    [SerializeField] private Button start;

    // 選択された遷移先
    private MainMenuSelect selectMenu = MainMenuSelect.Title;
    // 流すBGM
    private const string TITLE_BGMNAME = "TitleBGM";
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        // 基底側の処理を呼ぶ
        await base.Initialize();
    }

    /// <summary>
    /// 使用前準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        // 基底側の処理を呼ぶ
        await base.Setup();
        start.onClick.AddListener(OnTitleSelected);
        // BGM再生
        AudioManager.instance.PlayBGM(TITLE_BGMNAME);
        await UniTask.CompletedTask;
    }


    /// <summary>
    /// マッチングメニューが選択された時の処理
    /// </summary>
    private void OnTitleSelected() {
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

        // メニューが選択されるまで待機する
        while (selectMenu == MainMenuSelect.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }
        // 再生中のBGMを停止する
        AudioManager.instance.FadeOutBGM(0.2f);
        // メニューが消える前にフェードアウトする
        await FadeManager.Instance.FadeOut(FadeType.Black, 0.5f);
        // メインメニュー画面に遷移
        await PartManager.Instance.TransitionPart(GamePart.MainMenu);
    }


}
