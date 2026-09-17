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
    /// システムオブジェクトの参照設定
    /// </summary>
    [SerializeField]
    private SystemManagerConfig systemManagerConfig = null;

    /// <summary>
    /// 開始時の初期化処理を実行する
    /// </summary>
    private void Start() {

        // ゲームのフレームレートを設定する
        Application.targetFrameRate = 60;

        // 非同期の初期化処理を開始する
        Initialize().Forget();
    }

    /// <summary>
    /// システムオブジェクトの生成と初期化を行う
    /// </summary>
    /// <returns>初期化処理のUniTask</returns>
    private async UniTask Initialize() {

        // 設定アセットが割り当てられていない場合は処理を終了
        if (systemManagerConfig == null) return;

        // システムオブジェクトのリストを取得
        SystemObject[] systemObjectList = systemManagerConfig.SystemObjectList;

        // システムオブジェクトのリストが存在しない場合は処理を終了
        if (systemObjectList == null) return;

        // 全システムオブジェクトの生成、初期化
        for (int i = 0, max = systemObjectList.Length; i < max; i++) {

            // システムオブジェクトを取得
            SystemObject origin = systemObjectList[i];

            // 参照が設定されていない場合はスキップ
            if (origin == null) continue;

            // システムオブジェクトの生成
            SystemObject createObject = Instantiate(origin, transform);

            // システムオブジェクトの初期化
            await createObject.Initialize();
        }

        // 全システムオブジェクトの初期化完了後にスタンバイへ遷移
        if (PartManager.Instance == null) return;

        // スタンバイパートへ遷移
        await PartManager.Instance.TransitionPart(GamePart.Standby);
    }
}