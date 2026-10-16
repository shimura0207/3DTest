/*
 *  @file   MenuManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// すべてのメニューウィンドウを管理するクラス
/// </summary>
public class MenuWindowManager : SystemObject {

    // 自身へのインスタンス参照
    public static MenuWindowManager Instance { get; private set; } = null;

    // 生成したメニューを型ごとに管理
    private Dictionary<System.Type, MenuWindowBase> menuWindowDictionary = new Dictionary<System.Type, MenuWindowBase>();

    /// <summary>
    /// メニューの参照先
    /// </summary>
    [SerializeField] MenuWindowManagerConfig menuWindowManagerConfig = null;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override async UniTask Initialize() {
        // 自身をシングルトンのインスタンスに設定
        Instance = this;

        // 設定データが登録されていなければエラーを出して終了
        if (menuWindowManagerConfig == null) {
            Debug.LogError("MenuWindowManagerConfigが設定されていません。");
            return;
        }

        // ScriptableObjectからメニューのPrefab一覧を取得
        MenuWindowBase[] menuPrefabList = menuWindowManagerConfig.MenuOriginList;

        // Prefab一覧が設定されていなければ終了
        if (menuPrefabList == null) return;

        // メニュー管理用の辞書を初期化
        menuWindowDictionary.Clear();

        // すべてのメニューを生成
        foreach (MenuWindowBase prefab in menuPrefabList) {

            // 未設定のPrefabはスキップ
            if (prefab == null) continue;

            // Prefabからメニューを生成
            MenuWindowBase menu = Instantiate(prefab, transform);

            // メニューの型を取得
            System.Type menuType = menu.GetType();

            // 同じ型のメニューが登録済みなら重複生成を破棄
            if (menuWindowDictionary.ContainsKey(menuType)) {
                Destroy(menu.gameObject);
                continue;
            }

            // Managerから参照できるように登録
            menuWindowDictionary.Add(menuType, menu);
        }

        // 初期化処理をまとめて実行するためのリスト
        List<UniTask> taskList =
            new List<UniTask>(menuWindowDictionary.Count);

        // 登録したすべてのメニューの初期化処理を登録
        foreach (MenuWindowBase menu in menuWindowDictionary.Values) {
            taskList.Add(menu.Initialize());
        }

        // すべてのメニューの初期化完了を待つ
        await CommonModule.WaitTask(taskList);
    }

    /// <summary>
    /// 指定した型のメニューを取得
    /// </summary>
    public T Get<T>() where T : MenuWindowBase {

        // 指定した型のメニューが登録されていれば返す
        if (menuWindowDictionary.TryGetValue(typeof(T), out MenuWindowBase menu)) {
            return menu as T;
        }
        return null;
    }

    /// <summary>
    /// 指定した型のメニューを開く
    /// </summary>
    public async UniTask Open<T>() where T : MenuWindowBase {
        // 対象のメニューを取得
        T menu = Get<T>();

        // メニューが存在しなければ終了
        if (menu == null) return;

        // メニューを表示
        await menu.Open();
    }

    /// <summary>
    /// 指定した型のメニューを閉じる
    /// </summary>
    public async UniTask Close<T>() where T : MenuWindowBase {
        // 対象のメニューを取得
        T menu = Get<T>();
        // メニューが存在しなければ終了
        if (menu == null) return;
        // メニューを非表示
        await menu.Close();
    }

    /// <summary>
    /// レーン選択画面を開き、選択結果を返す
    /// </summary>
    public async UniTask<FieldLane> LaneMenuOpen() {

        // 管理しているLaneWindowを取得
        LaneWindow laneWindow = Get<LaneWindow>();

        // メニューが見つからない場合は処理を終了
        if (laneWindow == null) {
            return default;
        }

        // レーンを選択するまで待機し、結果を返す
        return await laneWindow.OpenAndWaitForSelection();
    }

    /// <summary>
    /// 確認画面を表示し、はい・いいえの結果を返す
    /// </summary>
    /// <param name="message">確認画面に表示するメッセージ</param>
    /// <returns>はいならtrue、いいえならfalse</returns>
    public async UniTask<bool> Confirm(string message) {

        // 管理している確認画面を取得
        GenericCheckMenu menu = Get<GenericCheckMenu>();

        // メニューがなくてもいいえが押された状態とする
        if (menu == null) return false;

        // 選択結果を返す
        return await menu.Open(message);
    }

    /// <summary>
    /// バトル設定を行うメニュー
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async UniTask BattleSettingsMenuOpen(int id) {
        // 管理しているメニューを取得
        BattleSettingsMenu menu = Get<BattleSettingsMenu>();

        // メニューがない場合は処理しない 
        if (menu == null) return;

        // メニューにIDを渡して開く
        await menu.Open(id);
    }

    /// <summary>
    /// ホストかクライアントか選択されるメニューを表示
    /// </summary>
    /// <param name="connect"></param>
    /// <returns></returns>
    public async UniTask<ConnectionState> ConnectMenuOpen(ConnectionState connect) {
        HostORClientMenu menu = Get<HostORClientMenu>();

        if (menu == null) return ConnectionState.None;

        return await menu.Open(connect);
    }

    /// <summary>
    /// クライアント接続の確認画面を表示し、選択結果を返す
    /// </summary>
    public async UniTask<bool> ClientConnectionConfirm() {

        // 管理している確認画面を取得
        ClientConectionMenu menu = Get<ClientConectionMenu>();

        // メニューが取得できなければ接続を許可しない
        if (menu == null) {
            return false;
        }

        // 確認画面を開き、選択結果を返す
        return await menu.OpenAndWaitForSelection();
    }

    /// <summary>
    /// 接続役割を設定して共通のルーム画面を開く
    /// </summary>
    public async UniTask RoomMenuOpen(ConnectionState state) {
        // 管理中のルーム画面を取得する
        RoomMenu menu = Get<RoomMenu>();

        // ルーム画面が存在しない場合は終了する
        if (menu == null) return;

        // 表示する前にホスト・クライアントの役割を渡す
        menu.SetConnectionState(state);

        // 共通のルーム画面を表示する
        await menu.Open();
    }
}