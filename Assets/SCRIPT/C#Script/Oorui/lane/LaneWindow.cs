/*
 *  @file   LaneWindow
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// レーンとして表示するウィンドウ
/// </summary>
public class LaneWindow : MenuWindowBase {

    // レーンに対応したボタン
    [SerializeField] private Button Lane1Button;
    [SerializeField] private Button Lane2Button;
    [SerializeField] private Button Lane3Button;
    [SerializeField] private Button Lane4Button;
    [SerializeField] private Button Lane5Button;

    // レーンに対応したボタン
    [SerializeField] private Button Lane1Button_Opponent;
    [SerializeField] private Button Lane2Button_Opponent;
    [SerializeField] private Button Lane3Button_Opponent;
    [SerializeField] private Button Lane4Button_Opponent;
    [SerializeField] private Button Lane5Button_Opponent;

    // ボタンが選択されるまで待機するための完了通知
    private UniTaskCompletionSource<FieldLane> completionSource;

    // 自分側のレーンボタン一覧
    private Button[] selfLaneButtonList;

    // 敵側のレーンボタン一覧
    private Button[] opponentLaneButtonList;

    // レーンボタン一覧
    private Button[] laneButtonList;

    // レーン一覧
    private readonly FieldLane[] fieldLaneList = {
    FieldLane.Lane1,
    FieldLane.Lane2,
    FieldLane.Lane3,
    FieldLane.Lane4,
    FieldLane.Lane5
};
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();

        // 自分側のレーンボタンを配列にまとめる
        selfLaneButtonList = new Button[] {
        Lane1Button,
        Lane2Button,
        Lane3Button,
        Lane4Button,
        Lane5Button
    };

        // 敵側のレーンボタンを配列にまとめる
        opponentLaneButtonList = new Button[] {
        Lane1Button_Opponent,
        Lane2Button_Opponent,
        Lane3Button_Opponent,
        Lane4Button_Opponent,
        Lane5Button_Opponent
    };

        // 自分側のボタンにイベントを登録
        Lane1Button.onClick.AddListener(OnClickLane1Button);
        Lane2Button.onClick.AddListener(OnClickLane2Button);
        Lane3Button.onClick.AddListener(OnClickLane3Button);
        Lane4Button.onClick.AddListener(OnClickLane4Button);
        Lane5Button.onClick.AddListener(OnClickLane5Button);

        // 敵側のボタンにもイベントを登録
        Lane1Button_Opponent.onClick.AddListener(OnClickLane1Button);
        Lane2Button_Opponent.onClick.AddListener(OnClickLane2Button);
        Lane3Button_Opponent.onClick.AddListener(OnClickLane3Button);
        Lane4Button_Opponent.onClick.AddListener(OnClickLane4Button);
        Lane5Button_Opponent.onClick.AddListener(OnClickLane5Button);
    }

    /// <summary>
    /// 空いているレーンを表示する
    /// </summary>
    public void SetupEmptyLane(PlayerType playerType) {
        // 空いているレーンのみ表示
        Setup(playerType, false);
    }

    /// <summary>
    /// カードが置かれているレーンを表示する
    /// </summary>
    public void SetupPlacedLane(PlayerType playerType) {
        // カードが置かれているレーンのみ表示
        Setup(playerType, true);
    }

    /// <summary>
    /// レーンボタンの表示状態を設定
    /// </summary>
    private void Setup(PlayerType playerType, bool isPlaced) {

        // CardObjectManagerが存在しなければ終了
        if (CardObjectManager.instance == null) {
            Debug.LogError("CardObjectManagerが存在しません。");
            return;
        }

        // 対象プレイヤーのフィールド情報を取得
        var fieldObjectList =
            CardObjectManager.instance.fieldObjectList[playerType];

        // プレイヤーに応じて使用するボタンを切り替える
        laneButtonList = playerType == PlayerType.Self
            ? selfLaneButtonList
            : opponentLaneButtonList;

        // 自分側・敵側のボタン表示を切り替える
        foreach (Button button in selfLaneButtonList) {
            button.gameObject.SetActive(playerType == PlayerType.Self);
        }

        foreach (Button button in opponentLaneButtonList) {
            button.gameObject.SetActive(playerType == PlayerType.Opponent);
        }

        // 各レーンの表示状態を設定
        for (int i = 0; i < laneButtonList.Length; i++) {

            // 配列から対応するレーンを取得
            FieldLane lane = fieldLaneList[i];

            // カードが存在するか確認
            bool hasCard = fieldObjectList[lane] != null;

            // 表示条件に応じてボタンを表示
            laneButtonList[i].gameObject.SetActive(
                isPlaced ? hasCard : !hasCard
            );
        }
    }

    /// <summary>
    /// 押された位置を返す
    /// </summary>
    /// <returns></returns>
    public async UniTask<FieldLane> OpenAndWaitForSelection() {
        // ボタン選択結果を受け取るための待機処理を作成
        completionSource = new UniTaskCompletionSource<FieldLane>();

        // 基底クラスの処理でウィンドウを表示
        await base.Open();

        // ボタンが押されるまで待機
        FieldLane result = await completionSource.Task;

        // 選択後にウィンドウを閉じる
        await base.Close();

        return result;
    }

    /// <summary>
    /// ウィドウ非表示時の処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Close() {
        // 基底側でメニューを非表示にする
        await base.Close();
    }

    private void OnClickLane1Button() {
        // Lane1が押されたことを返す
        completionSource.TrySetResult(FieldLane.Lane1);
    }

    private void OnClickLane2Button() {
        // Lane1が押されたことを返す
        completionSource.TrySetResult(FieldLane.Lane2);
    }

    private void OnClickLane3Button() {
        // Lane1が押されたことを返す
        completionSource.TrySetResult(FieldLane.Lane3);
    }

    private void OnClickLane4Button() {
        // Lane1が押されたことを返す
        completionSource.TrySetResult(FieldLane.Lane4);
    }

    private void OnClickLane5Button() {
        // Lane1が押されたことを返す
        completionSource.TrySetResult(FieldLane.Lane5);
    }

    /// <summary>
    /// 指定したプレイヤーのレーンに空きがあるか確認
    /// </summary>
    public bool HasEmptyLane(PlayerType playerType) {

        // CardObjectManagerが存在しなければ選択できない
        if (CardObjectManager.instance == null) return false;

        // 対象プレイヤーのフィールドを取得
        var fieldObjectList =
            CardObjectManager.instance.fieldObjectList[playerType];

        // 空いているレーンがあるか確認
        foreach (var card in fieldObjectList.Values) {
            if (card == null) return true;
        }

        return false;
    }

    /// <summary>
    /// 指定したプレイヤーのレーンにカードがあるか確認
    /// </summary>
    public bool HasPlacedLane(PlayerType playerType) {

        // CardObjectManagerが存在しなければ選択できない
        if (CardObjectManager.instance == null) return false;

        // 対象プレイヤーのフィールドを取得
        var fieldObjectList =
            CardObjectManager.instance.fieldObjectList[playerType];

        // カードが置かれているレーンがあるか確認
        foreach (var card in fieldObjectList.Values) {
            if (card != null) return true;
        }

        return false;
    }


}
