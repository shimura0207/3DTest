/*
 * @file    VFXManager
 * @author oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.VFX;
using static CommonModule;

public class VFXManager : SystemObject {
    /// <summary>
    /// 自身への参照
    /// </summary>
    public static VFXManager Instance { get; private set; } = null;

    // エフェクトオブジェクト
    [SerializeField]
    private VFXObject vfxObject = null;

    // 使用中のエフェクトリスト
    private List<List<GameObject>> useVFXList = null;

    // タスク中断用トークン
    private CancellationToken token;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        Instance = this;
        // エフェクトオブジェクトの初期化
        vfxObject.Initialize();
        // 使用エフェクトリストを適当数生成
        useVFXList = new List<List<GameObject>>((int)VFXType.max);
        for (int i = 0, max = (int)VFXType.max; i < max; i++) {
            useVFXList.Add(new List<GameObject>(VFXObject.GENERATE_OBJECTS_MAX));
        }
        // オブジェクト破棄時に処理されるタスク中断用トークンを取得
        token = this.GetCancellationTokenOnDestroy();

        await UniTask.CompletedTask;
    }

    /// <summary>
    /// エフェクトの再生を行う
    /// </summary>
    /// <param name="playEffect"></param>
    /// <param name="playPosition"></param>
    /// <param name="setParent"></param>
    /// <returns></returns>
    public async UniTask PlayEffect(VFXType playEffect, Vector3 playPosition, Transform setParent = null) {
        // エフェクト使用化
        GameObject vfx = vfxObject.UseEffect(playEffect, playPosition, setParent);
        // SetParent用一時的待ち
        await UniTask.Yield();
        vfx.transform.position = playPosition;
        // リストに追加
        useVFXList[(int)playEffect].Add(vfx);
        // 終了待ち
        while (vfx.GetComponent<ParticleSystem>().isPlaying) {
            await UniTask.DelayFrame(1, PlayerLoopTiming.Update, token);
        }
        // エフェクトの終了処理
        vfx.transform.position = Vector3.zero;
        // エフェクト非表示
        useVFXList[(int)playEffect].RemoveAt(0);
        vfxObject.UnuseEffect((int)playEffect, 0);
    }
}
