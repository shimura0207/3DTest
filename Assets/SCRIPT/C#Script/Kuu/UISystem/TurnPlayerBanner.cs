/*
 * @file    TurnPlayerBanner.cs
 * @author  kuu
 */
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPlayerBanner : MonoBehaviour
{
    // ターン開始バナー
    [SerializeField] Image banner = null;

    // 自分のターンバナー画像
    [SerializeField] Sprite selfBanner = null;

    // 相手のターンバナー画像
    [SerializeField] Sprite opponentBanner = null;

    // 表示する秒数
    private const int DISPLAY_TIME = 1000;

    /// <summary>
    /// 適切なバナーを表示
    /// </summary>
    /// <returns></returns>
    public async UniTask DisplayBanner(PlayerType player) {
        // ターンプレイヤーに合わせてバナーの画像を変える
        if (player == PlayerType.Self) {
            banner.sprite = selfBanner;
        }
        else if (player == PlayerType.Opponent) {
            banner.sprite = opponentBanner;
        }

        // バナーを表示する
        banner.enabled = true;

        // 1秒待って非表示にする
        await UniTask.Delay(DISPLAY_TIME);
        banner.enabled = false;
    }
}
