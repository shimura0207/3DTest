/*
 *  @file   SystemManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using UnityEngine;


/// <summary>
/// ゲーム全体の機能の管理
/// </summary>
public class SystemManager : MonoBehaviour {

    /// <summary>
    /// 管理するシステムオブジェクトのリスト
    /// </summary>
    [SerializeField]
    private SystemObject[] systemObjectList = null;

    private void Start() {
        UniTask task = Initialize();
        Application.targetFrameRate = 60;
    }


    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    private async UniTask Initialize() {
        // 全システムオブジェクトの生成、初期化
        for (int i = 0, max = systemObjectList.Length; i < max; i++) {
            SystemObject origin = systemObjectList[i];
            if (origin == null) continue;
            // システムオブジェクト生成
            SystemObject createObject = Instantiate(origin, transform);
            // 初期化
            await createObject.Initialize();
        }
        // スタンバイパートの実行
        UniTask task = PartManager.Instance.TransitionPart(GamePart.Standby);

    }



}
