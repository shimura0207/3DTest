/*
 *  @file NetWorkSystemManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// ネットワーク関連の外部コードをSystemObjectとして扱うSystemObject
/// </summary>
public class NetWorkSystemManager : SystemObject {

    // 自身への参照
    public static NetWorkSystemManager Instance { get; private set; }

    // NetworkManagerへの参照
    private NetworkManager networkManager;

    // UnityTransportへの参照
    private UnityTransport unityTransport;

    /// <summary>
    /// NetworkManagerを取得する
    /// </summary>
    public NetworkManager NetworkManager =>
        networkManager;

    /// <summary>
    /// UnityTransportを取得する
    /// </summary>
    public UnityTransport UnityTransport =>
        unityTransport;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {

        // 自身をSingletonとして登録
        Instance = this;

        // Scene上のNetworkManagerをSingletonから取得
        networkManager = NetworkManager.Singleton;

        // NetworkManagerが取得できなかった場合
        if (networkManager == null) {

            // エラーを表示
            Debug.LogError(
                "NetWorkSystemManager : " +
                "NetworkManagerが取得できませんでした。"
            );

            return;
        }

        // NetworkManagerと同じGameObjectからUnityTransportを取得
        unityTransport = networkManager.GetComponent<UnityTransport>();

        // UnityTransportが取得できなかった場合
        if (unityTransport == null) {

            // エラーを表示
            Debug.LogError(
                "NetWorkSystemManager : " +
                "UnityTransportがNetworkManagerに設定されていません。"
            );

            return;
        }

        // 初期化処理を完了
        await UniTask.CompletedTask;
    }
}