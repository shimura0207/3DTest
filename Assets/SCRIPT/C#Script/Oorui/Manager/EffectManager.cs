/*
 * @file    EffectManager
 * @author  oorui
 */

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.VFX;
using static CommonModule;

public class EffectManager : SystemObject {
    /// <summary>
    /// 自身への参照
    /// </summary>
    public static EffectManager Instance { get; private set; } = null;

    // エフェクトオブジェクト
    [SerializeField]
    private EffectObject effectObject = null;

    // 使用中のエフェクトリスト
    private List<List<GameObject>> useEffectList = null;

    // タスク中断用トークン
    private CancellationToken token;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <returns></returns>
    public override async UniTask Initialize() {
        Instance = this;
        // エフェクトオブジェクトの初期化
        effectObject.Initialize();
        // 使用エフェクトリストを適当数生成
        useEffectList = new List<List<GameObject>>((int)EffectType.max);
        for (int i = 0, max = (int)EffectType.max; i < max; i++) {
            useEffectList.Add(new List<GameObject>(EffectObject.GENERATE_OBJECTS_MAX));
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
    public async UniTask PlayEffect(EffectType playEffect, Vector3 playPosition, Transform setParent = null) {
        // エフェクト使用化
        GameObject effect = effectObject.UseEffect(playEffect, playPosition, setParent);
        // SetParent用一時的待ち
        await UniTask.Yield();
        effect.transform.position = playPosition;
        // リストに追加
        useEffectList[(int)playEffect].Add(effect);
        // 終了待ち
        while (effect.GetComponent<ParticleSystem>().isPlaying) {
            await UniTask.DelayFrame(1, PlayerLoopTiming.Update, token);
        }
        // エフェクトの終了処理
        effect.transform.position = Vector3.zero;
        // エフェクト非表示
        useEffectList[(int)playEffect].RemoveAt(0);
        effectObject.UnuseEffect((int)playEffect, 0);
    }
}
