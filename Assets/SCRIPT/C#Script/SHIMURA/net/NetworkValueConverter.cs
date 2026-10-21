using System;
using System.Globalization;
using System.Text;

/// <summary>
/// 複数の値を通信向けの文字列にまとめたり、分解したりするクラス。
///
/// 【使用例】
/// string message = NetworkValueConverter.Pack("01", "02", 500, true);
/// → "01,02,500,true"
///
/// string[] values = NetworkValueConverter.Unpack(message);
/// → ["01", "02", "500", "true"]
///
/// 【特徴】
/// ・引数の数と型を自由に指定できます。
/// ・文字列にカンマや改行が含まれていても分解できます。
/// ・PCの言語設定による小数点表記の違いを防ぎます。
/// ・GameObjectへのアタッチは不要です。
///
/// 【注意】
/// ・型情報は送信しません。受信側で必要な型へ変換してください。
/// ・nullは空文字になります。nullと空文字の区別は保存しません。
/// ・配列や独自クラスの内部データを自動で保存する機能ではありません。
/// ・Packの結果は、必ずUnpackで分解してください。
/// </summary>
public static class NetworkValueConverter {
    /// <summary>
    /// 数値を送受信する際の表記を統一します。
    /// 例えば、小数の1.5が環境によって「1,5」になることを防ぎます。
    /// </summary>
    private static readonly CultureInfo Culture =
        CultureInfo.InvariantCulture;

    // =========================================================
    // 送信用：複数の値を1つの文字列へ
    // =========================================================

    /// <summary>
    /// 渡された値を、カンマ区切りの文字列にまとめます。
    ///
    /// params：
    /// 引数を好きな数だけ指定できます。
    ///
    /// object：
    /// string、int、float、boolなど異なる型を指定できます。
    ///
    /// 使用例：
    /// Pack("01", "02")
    /// Pack("攻撃", 2, 500)
    /// Pack("演出", true, 1.5f)
    ///
    /// ※最低1項目は必要です。
    /// ※IDの先頭のゼロを残す場合は、"01"のように文字列で指定します。
    /// </summary>
    public static string Pack(params object[] values) {
        if (values == null) {
            throw new ArgumentNullException(nameof(values));
        }

        // 項目0個と「空文字が1個」を区別できなくなるため、
        // この形式では最低1項目を必須にしています。
        if (values.Length == 0) {
            throw new ArgumentException(
                "Packには1個以上の値を指定してください。",
                nameof(values));
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < values.Length; i++) {
            // 最初の項目以外は、直前に区切りのカンマを入れます。
            if (i > 0) {
                builder.Append(',');
            }

            // 値を文字列へ変換します。
            string text = ConvertToText(values[i]);

            // 値の中のカンマなどが区切りとして扱われないように変換します。
            builder.Append(Escape(text));
        }

        return builder.ToString();
    }

    /// <summary>
    /// 型に応じて値を文字列へ変換します。
    /// </summary>
    private static string ConvertToText(object value) {
        // nullは空文字として送ります。
        if (value == null) {
            return string.Empty;
        }

        // boolの表記を小文字に統一します。
        if (value is bool boolValue) {
            return boolValue ? "true" : "false";
        }

        // R形式は、floatの値を復元できる桁数で文字列化します。
        if (value is float floatValue) {
            return floatValue.ToString("R", Culture);
        }

        // doubleも同様に、復元できる形式で文字列化します。
        if (value is double doubleValue) {
            return doubleValue.ToString("R", Culture);
        }

        // 日付を渡す場合は、日時情報を保持する形式にします。
        if (value is DateTime dateTimeValue) {
            return dateTimeValue.ToString("O", Culture);
        }

        // int、long、decimal、enumなどを文字列化します。
        if (value is IFormattable formattable) {
            return formattable.ToString(null, Culture) ?? string.Empty;
        }

        // stringなど、それ以外の型はToStringを使います。
        // 独自クラスの場合、内部フィールドが自動保存されるわけではありません。
        return value.ToString() ?? string.Empty;
    }

    // =========================================================
    // 受信用：文字列を項目ごとに分解
    // =========================================================

    /// <summary>
    /// Packでまとめた文字列を、項目ごとに分解します。
    ///
    /// 戻り値の順番は、Packに渡した引数の順番と同じです。
    ///
    /// 使用例：
    /// Unpack("01,02,500")
    /// → values[0] = "01"
    /// → values[1] = "02"
    /// → values[2] = "500"
    ///
    /// 空文字の項目も削除せず保持します。
    /// Unpack("01,,500")のvalues[1]は空文字です。
    /// </summary>
    public static string[] Unpack(string message) {
        if (message == null) {
            throw new ArgumentNullException(nameof(message));
        }

        // 空文字も1つの項目として残して分解します。
        string[] values = message.Split(
            new[] { ',' },
            StringSplitOptions.None);

        for (int i = 0; i < values.Length; i++) {
            // エスケープしたカンマや改行を元に戻します。
            values[i] = Unescape(values[i]);
        }

        return values;
    }

    /// <summary>
    /// nullの可能性がある受信データを、例外を出さずに分解します。
    ///
    /// 成功：true。valuesに分解結果が入ります。
    /// 失敗：false。valuesは空配列になります。
    ///
    /// ※項目数やIDなど、メッセージの内容が正しいかは別途確認してください。
    /// </summary>
    public static bool TryUnpack(string message, out string[] values) {
        values = Array.Empty<string>();

        if (message == null) {
            return false;
        }

        values = Unpack(message);
        return true;
    }

    // =========================================================
    // 分解した文字列を必要な型へ変換
    // =========================================================

    /// <summary>
    /// intへ変換します。
    /// 例："500" → 500
    ///
    /// 数値でない場合や、intの範囲外の場合はfalseを返します。
    /// </summary>
    public static bool TryGetInt(string text, out int value) {
        return int.TryParse(
            text,
            NumberStyles.Integer,
            Culture,
            out value);
    }

    /// <summary>
    /// longへ変換します。
    /// intでは収まらない大きな整数を扱う場合に使います。
    /// </summary>
    public static bool TryGetLong(string text, out long value) {
        return long.TryParse(
            text,
            NumberStyles.Integer,
            Culture,
            out value);
    }

    /// <summary>
    /// floatへ変換します。
    /// 例："1.5" → 1.5f
    ///
    /// NaNやInfinityなど、通常の計算値ではないものは拒否します。
    /// </summary>
    public static bool TryGetFloat(string text, out float value) {
        if (float.TryParse(
                text,
                NumberStyles.Float,
                Culture,
                out value)
            && !float.IsNaN(value)
            && !float.IsInfinity(value)) {
            return true;
        }

        value = 0f;
        return false;
    }

    /// <summary>
    /// doubleへ変換します。
    /// floatより高い精度で小数を扱う場合に使います。
    ///
    /// NaNやInfinityは拒否します。
    /// </summary>
    public static bool TryGetDouble(string text, out double value) {
        if (double.TryParse(
                text,
                NumberStyles.Float,
                Culture,
                out value)
            && !double.IsNaN(value)
            && !double.IsInfinity(value)) {
            return true;
        }

        value = 0d;
        return false;
    }

    /// <summary>
    /// decimalへ変換します。
    /// </summary>
    public static bool TryGetDecimal(string text, out decimal value) {
        return decimal.TryParse(
            text,
            NumberStyles.Float,
            Culture,
            out value);
    }

    /// <summary>
    /// boolへ変換します。
    /// "true" → true
    /// "false" → false
    ///
    /// 大文字・小文字は区別しません。
    /// "1"や"0"はboolとして受け付けません。
    /// </summary>
    public static bool TryGetBool(string text, out bool value) {
        return bool.TryParse(text, out value);
    }

    /// <summary>
    /// enumへ変換します。
    ///
    /// 使用例：
    /// TryGetEnum&lt;PachisuroSymbolKoyakuEnum&gt;(text, out var role)
    ///
    /// enumに定義されていない値は拒否します。
    /// Flags属性の組み合わせ値も、定義されていなければ拒否します。
    /// </summary>
    public static bool TryGetEnum<TEnum>(string text, out TEnum value)
        where TEnum : struct, Enum {
        if (Enum.TryParse<TEnum>(text, false, out value)
            && Enum.IsDefined(typeof(TEnum), value)) {
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Packで文字列化したDateTimeを復元します。
    /// </summary>
    public static bool TryGetDateTime(string text, out DateTime value) {
        return DateTime.TryParseExact(
            text,
            "O",
            Culture,
            DateTimeStyles.RoundtripKind,
            out value);
    }

    // =========================================================
    // 区切り文字を安全に扱うための内部処理
    // =========================================================

    /// <summary>
    /// 項目の中にある特殊文字を置き換えます。
    ///
    /// %     → %25
    /// ,     → %2C
    /// 改行  → %0D / %0A
    ///
    /// %を最初に変換することで、
    /// 元の文字列に「%2C」が含まれていても区別できます。
    /// </summary>
    private static string Escape(string text) {
        return text
            .Replace("%", "%25")
            .Replace(",", "%2C")
            .Replace("\r", "%0D")
            .Replace("\n", "%0A");
    }

    /// <summary>
    /// Escapeで置き換えた文字を元に戻します。
    ///
    /// %は最後に戻します。
    /// 順番を変えたり繰り返し復号すると、元の文字列が壊れる場合があります。
    /// </summary>
    private static string Unescape(string text) {
        return text
            .Replace("%2C", ",")
            .Replace("%0D", "\r")
            .Replace("%0A", "\n")
            .Replace("%25", "%");
    }
}