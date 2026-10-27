/*
 *  @file   AudioManagerConfig
 *  @author oorui
 */

using UnityEngine;

/// <summary>
/// AudioManagerが使用する設定データ
/// </summary>
[CreateAssetMenu(fileName = "AudioManagerConfig", menuName = "Game/AudioManagerConfig")]
public class AudioManagerConfig : ScriptableObject {

    /// <summary>
    /// BGMのデータ一覧
    /// </summary>
    [SerializeField]
    private AudioManager.SoundData[] bgmList = new AudioManager.SoundData[0];

    /// <summary>
    /// SEのデータ一覧
    /// </summary>
    [SerializeField]
    private AudioManager.SoundData[] seList = new AudioManager.SoundData[0];

    /// <summary>
    /// BGMのリストを取得する
    /// </summary>
    public AudioManager.SoundData[] BGMList => bgmList;

    /// <summary>
    /// SEのリストを取得する
    /// </summary>
    public AudioManager.SoundData[] SEList => seList;
}