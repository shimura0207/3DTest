/*
 *  @file NetWorkSystemManager
 *  @author oorui
 */

using Cysharp.Threading.Tasks;

/// <summary>
/// ネットワーク関連の外部コードをSystemObjectとして扱うSystemObject
/// </summary>
public class NetWorkSystemManager : SystemObject {
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        await UniTask.CompletedTask;
    }

}
