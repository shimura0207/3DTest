/*
 *  @file   SystemManagerConfig
 *  @author oorui
 */

using UnityEngine;

/// <summary>
/// SystemManagerが使用する設定データ
/// </summary>
[CreateAssetMenu(
    fileName = "SystemManagerConfig", menuName = "Game/SystemManagerConfig")]
    
public class SystemManagerConfig : ScriptableObject {

    /// <summary>
    /// 管理するシステムオブジェクトのリスト
    /// </summary>
    [SerializeField]
    private SystemObject[] systemObjectList = null;

    /// <summary>
    /// システムオブジェクトのリストを取得する
    /// </summary>
    public SystemObject[] SystemObjectList => systemObjectList;
}