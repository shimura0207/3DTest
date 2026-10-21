/*
 *  @file   CardListPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// カード一覧パート
/// </summary>
public class CardListPart : PartBase {

    // 所持カード一覧の表示を管理するクラス
    [SerializeField]
    private PossessionCardListView possessionCardListView = null;
    // レアリティ順に順番を変更するボタン
    [SerializeField] private Button sort_Rearity;
    // 並び順を初期値に変更するボタン
    [SerializeField] private Button sort_Reset;
    // 所持枚数順に順番を変更するボタン
    [SerializeField] private Button sort_OwnerCard;
    // ひとつ前の画面に戻るボタン
    [SerializeField] private Button returnButton;

    // 選択された遷移先
    private CardRelationMenuSelect selectMenu = CardRelationMenuSelect.None;
    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        // 基底クラスの初期化処理
        await base.Initialize();
        selectMenu = CardRelationMenuSelect.None;
    }

    /// <summary>
    /// 使用前準備
    /// </summary>
    public override async UniTask Setup() {
        // 基底クラスの準備処理
        await base.Setup();
        // 遷移先を初期化
        selectMenu = CardRelationMenuSelect.None;
        // 追加と初期化をまとめて行う
        ListenerALL();

        // 表示クラスが設定されているか確認する
        if (possessionCardListView == null) return;

        // 所持カード一覧を生成して表示する
        possessionCardListView.RefreshCards();

        Debug.Log("カード一覧画面表示中");
    }

    /// <summary>
    /// イベント登録、解除をまとめて行う
    /// </summary>
    private void ListenerALL() {
        // レアリティ順変更イベント
        sort_Rearity.onClick.RemoveListener(possessionCardListView.RealitySort);
        sort_Rearity.onClick.AddListener(possessionCardListView.RealitySort);
        // 順番初期化イベント
        sort_Reset.onClick.RemoveListener(possessionCardListView.ResetSort);
        sort_Reset.onClick.AddListener(possessionCardListView.ResetSort);
        // 所持枚数順変更イベント
        sort_OwnerCard.onClick.RemoveListener(possessionCardListView.OwnerCountSort);
        sort_OwnerCard.onClick.AddListener(possessionCardListView.OwnerCountSort);
        // 画面遷移イベント
        returnButton.onClick.RemoveListener(OnReturnSelected);
        returnButton.onClick.AddListener(OnReturnSelected);
    }

    /// <summary>
    /// カード一覧画面を戻る先に選択
    /// </summary>
    private void OnReturnSelected() {
        selectMenu = CardRelationMenuSelect.CardRelation;
    }

    /// <summary>
    /// 実行処理
    /// </summary>
    public override async UniTask Execute() {
        // メニューが選択されるまで待機する
        while (selectMenu == CardRelationMenuSelect.None) {
            // 次のフレームまで待機する
            await UniTask.DelayFrame(1);
        }

        // カード関連画面に遷移
        await PartManager.Instance.TransitionPart(GamePart.CardRelation);

        // 現時点では追加の実行処理はない
        await UniTask.CompletedTask;
    }

    public override async UniTask Teardown() {
        await base.Teardown();
        selectMenu = CardRelationMenuSelect.None;
    }
}
