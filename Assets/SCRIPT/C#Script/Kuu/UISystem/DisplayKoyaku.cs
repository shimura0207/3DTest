/*
 * @file    DisplayKoyaku.cs
 * @autor   Kuu
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayKoyaku : MonoBehaviour {
    // 通常5種の役
    [SerializeField] private Canvas normalKoyaku = null;

    // 通常5種の役の影
    [SerializeField] private Canvas shadow = null;

    // ハズレ
    [SerializeField] private Image miss = null;

    // リプレイ
    [SerializeField] private Image replay = null;

    // ベル
    [SerializeField] private Image bell = null;

    // スイカ
    [SerializeField] private Image suika = null;

    // チェリー
    [SerializeField] private Image cherry = null;

    // 全て
    [SerializeField] private Image all = null;

    // スペル
    [SerializeField] private Image spell = null;

    // カードの対応役enum
    private CardSuportRole cardSuportRole;

    // Start is called before the first frame update
    void Start() {
        normalKoyaku.enabled = false;
        shadow.enabled = false;
        all.enabled = false;
        spell.enabled = false;
    }

    /// <summary>
    /// 対応役に応じたアイコンを表示
    /// </summary>
    /// <param name="koyaku"></param>
    public void KoyakuDisplayer(int koyaku) {
        // 表示リセット
        normalKoyaku.enabled = false;
        shadow.enabled = false;
        all.enabled = false;
        spell.enabled = false;

        cardSuportRole = (CardSuportRole)koyaku;
        // それぞれ必要なものを表示する
        switch (cardSuportRole) {
            // スペル
            case CardSuportRole.NULL:
                spell.enabled = true;
                break;
            // ハズレ
            case CardSuportRole.Miss:
                shadow.enabled = true;
                normalKoyaku.enabled = true;
                miss.enabled = true;
                break;
            // リプレイ
            case CardSuportRole.Replay:
                shadow.enabled = true;
                normalKoyaku.enabled = true;
                replay.enabled = true;
                break;
            // チェリー
            case CardSuportRole.Cherry:
                shadow.enabled = true;
                normalKoyaku.enabled = true;
                cherry.enabled = true;
                break;
            // ベル
            case CardSuportRole.Bell:
                shadow.enabled = true;
                normalKoyaku.enabled = true;
                bell.enabled = true;
                break;
            // スイカ
            case CardSuportRole.Suika:
                shadow.enabled = true;
                normalKoyaku.enabled = true;
                suika.enabled = true;
                break;
            // 全て
            case CardSuportRole.ALL:
                all.enabled = true;
                break;
            default:
                normalKoyaku.enabled = false;
                shadow.enabled = false;
                all.enabled = false;
                spell.enabled = false;
                break;
        }

    }
}
