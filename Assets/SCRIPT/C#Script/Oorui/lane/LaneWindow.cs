/*
 *  @file   LaneWindow
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System;
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

    // ボタンが選択されるまで待機するための完了通知
    private UniTaskCompletionSource<FieldLane> completionSource;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
        Lane1Button.onClick.AddListener(OnClickLane1Button);
        Lane2Button.onClick.AddListener(OnClickLane2Button);
        Lane3Button.onClick.AddListener(OnClickLane3Button);
        Lane4Button.onClick.AddListener(OnClickLane4Button);
        Lane5Button.onClick.AddListener(OnClickLane5Button);
    }

    /// <summary>
    /// 準備処理
    /// </summary>
    /// <returns></returns>
    public async UniTask Setup() {
        // フィールド上のカード情報を取得
        var fieldObjectList = CardObjectManager.instance.fieldObjectList;

        // 自分のフィールドにカードがあるレーンのボタンを非表示にする
        Lane1Button.gameObject.SetActive(fieldObjectList[PlayerType.Self][FieldLane.Lane1] == null);

        Lane2Button.gameObject.SetActive(fieldObjectList[PlayerType.Self][FieldLane.Lane2] == null);

        Lane3Button.gameObject.SetActive(fieldObjectList[PlayerType.Self][FieldLane.Lane3] == null);

        Lane4Button.gameObject.SetActive(fieldObjectList[PlayerType.Self][FieldLane.Lane4] == null);

        Lane5Button.gameObject.SetActive(fieldObjectList[PlayerType.Self][FieldLane.Lane5] == null);

        await UniTask.CompletedTask;
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

    /// 空いているレーンが存在するか確認する
    /// </summary>
    /// <returns>空きレーンがあればtrue</returns>
    public bool HasEmptyLane() {

        // CardObjectManagerが存在しなければ選択できない
        if (CardObjectManager.instance == null) return false;

        // 自分のフィールドの各レーンを確認
        var fieldObjectList = CardObjectManager.instance.fieldObjectList[PlayerType.Self];

        foreach (var card in fieldObjectList.Values) {

            // カードが存在しないレーンがあれば空きあり
            if (card == null) return true;
        }

        // すべてのレーンにカードがある
        return false;
    }

}
