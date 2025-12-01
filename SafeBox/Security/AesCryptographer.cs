using SafeBox.Extensions;
using SafeBox.Infrastructure;
using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;

namespace SafeBox.Security;

internal class AesCryptographer
{
    /// <summary>
    /// Decrypts data using <seealso cref="SecureString"/> as a key and <seealso cref="Aes"/> as a crypto provider.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns>Successful result is <seealso cref="string"/>, otherwise <see langword="default"/>.</returns>
    internal static string Decrypt(string data, SecureString key)
    {
        if (data.IsNull() || key.IsNull())
            return null;

        try
        {
            var combinedArray = Convert.FromBase64String(data);
            var iv = new byte[16];
            var buffer = new byte[combinedArray.Length - iv.Length];

            Array.Copy(combinedArray, 0, iv, 0, iv.Length);
            Array.Copy(combinedArray, iv.Length, buffer, 0, buffer.Length);

            using var aes = Aes.Create();
            aes.Key = GetFixedLengthKey(key, 32);
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch (Exception ex)
        {
            Logger.Error($"{Constants.AesLogMark}: An error occurred while decrypting:\n{ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    internal static SecureString DecryptToSecureString(string data, SecureString key) =>
        SecurityHelper.ToSecureString(Decrypt(data, key));

    internal static string Encrypt(string data, SecureString key)
    {
        if (data.IsNull() || key.IsNull())
            return null;

        var iv = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(iv);

        var fixedLengthKey = GetFixedLengthKey(key, 32);

        using var aes = Aes.Create();
        aes.Key = fixedLengthKey;
        aes.IV = iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

        using (var streamWriter = new StreamWriter(cryptoStream))
            streamWriter.Write(data);

        var array = memoryStream.ToArray();

        var combinedArray = new byte[iv.Length + array.Length];
        Array.Copy(iv, 0, combinedArray, 0, iv.Length);
        Array.Copy(array, 0, combinedArray, iv.Length, array.Length);

        return Convert.ToBase64String(combinedArray);
    }

    private static byte[] GetFixedLengthKey(SecureString key, int length)
    {
        using var sha512 = SHA512.Create();

        var hashBytes = key.GetHashBytes(sha512);
        var fixedLengthKey = new byte[length];
        Array.Copy(hashBytes, fixedLengthKey, length);

        return fixedLengthKey;
    }
}