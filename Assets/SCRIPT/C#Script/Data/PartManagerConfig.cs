/*
 *  @file   PartManagerConfig
 *  @author oorui
 */

using UnityEngine;

/// <summary>
/// PartManagerが使用する設定データ
/// </summary>
[CreateAssetMenu(
    fileName = "PartManagerConfig", menuName = "Game/PartManagerConfig")]
public class PartManagerConfig : ScriptableObject {

    /// <summary>
    /// 生成・管理するパートのリスト
    /// </summary>
    [SerializeField]
    private PartBase[] partOriginList = null;

    /// <summary>
    /// パートのリストを取得する
    /// </summary>
    public PartBase[] PartOriginList => partOriginList;
}