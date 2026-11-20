/*
 * @file    CardEffectAnimationManager.cs
 * @author  Riku
 */

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// カード演出管理クラス
/// </summary>
public class CardEffectAnimationManager : MonoBehaviour {
    // 自身への参照
    public static CardEffectAnimationManager instance { get; private set; } = null;

    // 上昇位置
    private readonly Vector3 UP_POSITION = new Vector3(0.0f, 0.01f, -0.04f);
    // 上昇補完値
    private const float UP_INTERPOLATION_RATE = 0.2f;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize() {
        instance = this;
    }

    /// <summary>
    /// 効果発動演出再生
    /// </summary>
    /// <param name="playCard">発動カード</param>
    /// <returns></returns>
    public async UniTask PlayAbilityExecute(CardObject playCard) {
        // カード発光
        UniTask task = EffectManager.instance.PlayEffect(
            EffectType.CardFlash, playCard.transform.position, playCard.transform);

        // カード上昇
        while(true) {
            // 線形補完で移動
            Vector3 pos = playCard.transform.localPosition;
            pos = Vector3.Lerp(pos, UP_POSITION, UP_INTERPOLATION_RATE);
            playCard.transform.localPosition = pos;

            // 移動完了とみなされたら終了
            if (Vector3.Distance(
                playCard.transform.localPosition, UP_POSITION) <= 0.0001f) {
                playCard.transform.localPosition = UP_POSITION;
                break;
            }
            await UniTask.Yield();
        }

        // カード下降
        while (true) {
            // 線形補完で移動
            Vector3 pos = playCard.transform.localPosition;
            pos = Vector3.Lerp(playCard.transform.localPosition, Vector3.zero, UP_INTERPOLATION_RATE);
            playCard.transform.localPosition = pos;

            // 移動完了とみなされたら終了
            if (Vector3.Distance(
                playCard.transform.localPosition, Vector3.zero) <= 0.0001f) {
                playCard.transform.localPosition = Vector3.zero;
                break;
            }
            await UniTask.Yield();
        }
    }
}
