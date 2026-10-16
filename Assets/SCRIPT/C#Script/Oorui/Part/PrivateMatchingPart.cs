/*
 *  @file   PrivateMatchingPart
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class PrivateMatchingPart : PartBase {

    // 接続した時の状態
    private ConnectionState conect = ConnectionState.None;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await base.Initialize();
    }

    /// <summary>
    /// 準備処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Setup() {
        await base.Setup();
        // 接続状況を初期化
        //conect = ConnectionState.None;
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Execute() {

        // 接続状態によって処理を変える
        if (conect == ConnectionState.Host) {
            // 処理を抜けたら遷移
            await MenuWindowManager.Instance.RoomMenuOpen(conect);
        }
        else if (conect == ConnectionState.Client) {
            // クライアントとして部屋に参加する
            await MenuWindowManager.Instance.RoomMenuOpen(conect);

        }

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// 片付け処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Teardown() {
        await base.Teardown();
    }


    /// <summary>
    /// 接続状況を設定
    /// </summary>
    /// <param name="state"></param>
    public void SetConnectState(ConnectionState state) {
        conect = state;
    }
}
