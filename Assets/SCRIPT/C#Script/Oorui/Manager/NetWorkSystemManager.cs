/*
 *  @file NetWorkSystemManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// ネットワーク関連の外部コードをSystemObjectとして扱うSystemObject
/// </summary>
public class NetWorkSystemManager : SystemObject {

    // 自身への参照
    public static NetWorkSystemManager Instance { get; private set; }

    // UnityTransportへの参照
    private UnityTransport unityTransport;

    /// <summary>
    /// UnityTransportを取得する
    /// </summary>
    public UnityTransport UnityTransport => unityTransport;


    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        Instance = this;
        // NetworkManagerが存在するか確認
        if (NetWorkSystemManager.Instance == null) return;
        // NetWorkSystemManagerからUnityTransportを取得
        unityTransport = NetWorkSystemManager.Instance.GetComponent<UnityTransport>();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// UnityTransportを取得する
    /// </summary>
    /// <returns>UnityTransport</returns>
    public UnityTransport GetUnityTransport() {
        // 保持しているUnityTransportを返す
        return unityTransport;
    }

}
