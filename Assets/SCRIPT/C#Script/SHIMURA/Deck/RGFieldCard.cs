
using UnityEngine;

/*
 * @file   RGFieldCard.cs
 * @author simura
 */

/// <summary>
/// フィールド上に存在するカードを管理するスクリプト。
/// RGCardDataからカードの基本ステータスを取得します。
/// </summary>
public class RGFieldCard : MonoBehaviour {
    //============================================================
    // カードデータ
    //============================================================

    /// <summary>
    /// このカードが参照しているScriptableObject。
    /// </summary>
    public RGCardData CardData { get; private set; }


    //============================================================
    // 現在HP
    //============================================================

    /// <summary>
    /// ゲーム中の現在HP。
    /// </summary>
    public int CurrentHp { get; private set; }


    //============================================================
    // カードデータ設定
    //============================================================

    /// <summary>
    /// RGCardDataを設定します。
    /// </summary>
    public void SetCardData(RGCardData data) {
        if (data == null) {
            Debug.LogWarning(
                $"{gameObject.name}：RGCardDataがnullです。"
            );

            return;
        }

        CardData = data;

        // ScriptableObjectのHPを現在HPとして設定
        CurrentHp = data.Hp;
    }


    //============================================================
    // ステータス取得
    //============================================================

    /// <summary>
    /// このカードのATKを取得します。
    /// </summary>
    public int GetAtk() {
        if (CardData == null) {
            Debug.LogWarning(
                $"{gameObject.name}：CardDataが設定されていません。"
            );

            return 0;
        }

        return CardData.Atk;
    }


    /// <summary>
    /// このカードのHPを取得します。
    /// </summary>
    public int GetHp() {
        if (CardData == null) {
            Debug.LogWarning(
                $"{gameObject.name}：CardDataが設定されていません。"
            );

            return 0;
        }

        return CardData.Hp;
    }
    /// <summary>
    /// CARDDATE
    /// </summary>
    /// <returns></returns>
    public RGCardData GetCardDate() {
        if (CardData == null) {
            Debug.LogWarning(
                $"{gameObject.name}：CardDataが設定されていません。"
            );

            return null;
        }

        return CardData;
    }
    /// <summary>
    /// このカードの現在HPを取得します。
    /// ダメージを受けた後のHPを取得したい場合はこちらを使用します。
    /// </summary>
    public int GetCurrentHp() {
        return CurrentHp;
    }
}

