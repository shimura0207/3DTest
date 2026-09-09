/*
 *  @file   MenuManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メニューウィンドウを管理
/// </summary>
public class MenuWindowManager : SystemObject {

    public static MenuWindowManager instance { get; private set; } = null;    // 自身へのインスタンスを参照

    private List<MenuWindowBase> menuWindowList = null;    // メニューウィンドウを管理するリスト

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        instance = this;
        menuWindowList = new List<MenuWindowBase>(256);
        await UniTask.CompletedTask;
    }

    /// <summary>
    /// メニューの取得
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T Get<T>(string name = null) where T : MenuWindowBase {
        // 全メニューを走査
        for (int i = 0, max = menuWindowList.Count; i < max; i++) {
            T menu = menuWindowList[i] as T;
            if (menu == null) continue;
            return menu;
        }
        return Load<T>(name);
    }

    /// <summary>
    /// メニューの読み込み
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    private T Load<T>(string name) where T : MenuWindowBase {
        // 読み込み
        T menu = Resources.Load<T>(name);
        if (menu == null) return null;

        T createMenu = Instantiate(menu, transform);
        if (createMenu == null) return null;

        //// タイトルメニュー以外のメニュー
        //if (!(createMenu is MenuTitle)) {
        //    // ロード時には非表示にしておく
        //    createMenu.gameObject.SetActive(false);
        //}

        menuWindowList.Add(createMenu);
        return createMenu;

    }
}
