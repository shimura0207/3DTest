/*
 *  @file   CardListPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カード一覧パート
/// </summary>
public class CardListPart : PartBase {

    // 所持カード一覧の表示を管理するクラス
    [SerializeField]
    private PossessionCardListView possessionCardListView = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        // 基底クラスの初期化処理
        await base.Initialize();
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    public override async UniTask Setup() {
        // 基底クラスの準備処理
        await base.Setup();

        // 表示クラスが設定されているか確認する
        if (possessionCardListView == null) return;

        // 所持カード一覧を生成して表示する
        possessionCardListView.RefreshCards();

        Debug.Log("カード一覧画面表示中");
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    public override async UniTask Execute() {
        // 現時点では追加の実行処理はない
        await UniTask.CompletedTask;
    }
}
