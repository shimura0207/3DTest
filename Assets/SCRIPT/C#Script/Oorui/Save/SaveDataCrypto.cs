/*
 *  @file   SaveDataCrypto
 *  @author oorui
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// セーブデータの暗号化・復号を管理するクラス
/// </summary>
public class SaveDataCrypto {

    // AES暗号化用の鍵
    // ※32Byte
    private static readonly byte[] AES_KEY = Convert.FromBase64String("d5szokR14wWUK+WQvvxTNg3qouz8iqkojb0LvBuN1jE=");

    // HMAC改ざん検知用の鍵
    // ※32Byte
    private static readonly byte[] HMAC_KEY =Convert.FromBase64String("oul6FxQFxTRMIV8zslP0Uqsv3KHqvkON9PL9TDYjtT4=");

    // AESのIVサイズ
    private const int IV_SIZE = 16;

    // HMACの署名サイズ
    private const int HMAC_SIZE = 32;

    /// <summary>
    /// JSON文字列を暗号化する
    /// </summary>
    public static byte[] Encrypt(string plainText) {

        // JSON文字列をバイト配列に変換
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        // 毎回異なるIVを生成
        byte[] iv = new byte[IV_SIZE];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(iv);
        }

        byte[] encryptedBytes;

        // AES暗号化
        using (Aes aes = Aes.Create()) {

            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = AES_KEY;
            aes.IV = iv;

            using (ICryptoTransform encryptor = aes.CreateEncryptor()) {
                encryptedBytes = encryptor.TransformFinalBlock(
                    plainBytes, 0, plainBytes.Length
                );
            }
        }

        // IVと暗号文を結合
        byte[] payload = new byte[IV_SIZE + encryptedBytes.Length];

        Buffer.BlockCopy(iv, 0, payload, 0, IV_SIZE);
        Buffer.BlockCopy(
            encryptedBytes, 0,
            payload, IV_SIZE,
            encryptedBytes.Length
        );

        // IVと暗号文からHMAC署名を生成
        byte[] signature;

        using (HMACSHA256 hmac = new HMACSHA256(HMAC_KEY)) {
            signature = hmac.ComputeHash(payload);
        }

        // 署名 + IV + 暗号文の順番で保存データを作成
        byte[] result = new byte[HMAC_SIZE + payload.Length];

        Buffer.BlockCopy(signature, 0, result, 0, HMAC_SIZE);
        Buffer.BlockCopy(
            payload, 0,
            result, HMAC_SIZE,
            payload.Length
        );

        return result;
    }

    /// <summary>
    /// 暗号化されたデータを復号する
    /// </summary>
    public static string Decrypt(byte[] encryptedData) {

        // データサイズを確認
        if (encryptedData == null ||
            encryptedData.Length < HMAC_SIZE + IV_SIZE + 16) {

            throw new InvalidDataException("セーブデータの形式が不正です。");
        }

        // HMAC署名を取り出す
        byte[] savedSignature = new byte[HMAC_SIZE];

        Buffer.BlockCopy(
            encryptedData, 0,
            savedSignature, 0,
            HMAC_SIZE
        );

        // IVと暗号文を取り出す
        int payloadLength = encryptedData.Length - HMAC_SIZE;

        byte[] payload = new byte[payloadLength];

        Buffer.BlockCopy(
            encryptedData, HMAC_SIZE,
            payload, 0,
            payloadLength
        );

        // 保存データからHMAC署名を再計算
        byte[] calculatedSignature;

        using (HMACSHA256 hmac = new HMACSHA256(HMAC_KEY)) {
            calculatedSignature = hmac.ComputeHash(payload);
        }

        // 署名を比較して改ざんされていないか確認
        if (!IsEqual(savedSignature, calculatedSignature)) {
            throw new InvalidDataException(
                "セーブデータの改ざんを検知しました。"
            );
        }

        // IVを取り出す
        byte[] iv = new byte[IV_SIZE];

        Buffer.BlockCopy(payload, 0, iv, 0, IV_SIZE);

        // 暗号文を取り出す
        int cipherLength = payload.Length - IV_SIZE;

        byte[] cipherBytes = new byte[cipherLength];

        Buffer.BlockCopy(
            payload, IV_SIZE,
            cipherBytes, 0,
            cipherLength
        );

        byte[] decryptedBytes;

        // AES復号
        using (Aes aes = Aes.Create()) {

            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = AES_KEY;
            aes.IV = iv;

            using (ICryptoTransform decryptor = aes.CreateDecryptor()) {
                decryptedBytes = decryptor.TransformFinalBlock(
                    cipherBytes, 0, cipherBytes.Length
                );
            }
        }

        // バイト配列をJSON文字列に戻す
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// 署名を安全に比較する
    /// </summary>
    private static bool IsEqual(byte[] a, byte[] b) {

        // 配列サイズが異なる場合は不一致
        if (a == null || b == null || a.Length != b.Length) {
            return false;
        }

        // 全バイトを比較する
        int result = 0;

        for (int i = 0; i < a.Length; i++) {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}