/*
 *  @file   MenuWindowManagerConfig
 *  @author oorui
 */
using UnityEngine;

/// <summary>
/// MenuWindowManagerが使用する設定データ
/// </summary>
[CreateAssetMenu(fileName = "MenuWindowManagerConfig", menuName = "Game/MenuWindowManagerConfig")]
public class MenuWindowManagerConfig : ScriptableObject {
    /// <summary>
    /// 生成・管理するパートのリスト
    /// </summary>
    [SerializeField]
    private MenuWindowBase[] menuOriginList = null;

    /// <summary>
    /// パートのリストを取得する
    /// </summary>
    public MenuWindowBase[] MenuOriginList => menuOriginList;
}
