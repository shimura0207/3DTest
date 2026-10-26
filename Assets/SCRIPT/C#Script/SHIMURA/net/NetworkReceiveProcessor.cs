using UnityEngine;

/// <summary>
/// 受信した情報を処理するクラス。
/// CustomNetworkManagerから直接呼び出します。
/// </summary>
public static class NetworkReceiveProcessor {
    /// <summary>
    /// 攻撃力の加算。
    /// 形式：atk,対象カードID,加算値
    /// </summary>
    public static void ReceiveAttack(string[] values) {
        if (values.Length != 3) {
            Debug.LogWarning("atkの項目数が不正です。");
            return;
        }

        string cardID = values[1];

        if (!NetworkValueConverter.TryGetInt(
            values[2], out int addAttack)) {
            Debug.LogWarning("攻撃力の加算値が不正です。");
            return;
        }

        // ここで対象カードを取得し、加算する関数を呼びます。
        // 受信処理から送信関数を呼ばないようにしてください。
        Debug.Log($"カード{cardID}：攻撃力に{addAttack}加算");
    }

    /// <summary>
    /// HP関連の受信処理。
    /// 項目の形式が決まったら実装してください。
    /// </summary>
    public static void ReceiveHP(string[] values) {
        Debug.Log("HP情報を受信しました。");
    }

    /// <summary>
    /// ドロー関連の受信処理。
    /// </summary>
    public static void ReceiveDraw(string[] values) {
        Debug.Log("ドロー情報を受信しました。");
    }

    /// <summary>
    /// 召喚関連の受信処理。
    /// </summary>
    public static void ReceiveSummon(string[] values) {
        Debug.Log("召喚情報を受信しました。");
    }
}