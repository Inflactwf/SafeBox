using Konscious.Security.Cryptography;
using SafeBox.Infrastructure;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace SafeBox.Security;

internal class AesCryptographer
{
    /// <summary>
    /// Decrypts a base64-encoded string using the specified secure key and returns the original plaintext.
    /// </summary>
    /// <remarks>This method uses AES encryption and expects the initialization vector (IV) to be embedded in
    /// the input data. Ensure that the key is securely managed and not exposed.</remarks>
    /// <param name="data">The base64-encoded string to decrypt. Cannot be null or empty.</param>
    /// <param name="key">The secure key used for decryption. Cannot be null and must have a length greater than zero.</param>
    /// <returns>The decrypted plaintext string, or null if the input is invalid or an error occurs during decryption.</returns>
    internal static string Decrypt(string data, SecureString key)
    {
        if (string.IsNullOrEmpty(data) || key == null || key.Length == 0)
            return null;

        try
        {
            var combinedArray = Convert.FromBase64String(data);
            var iv = new byte[16];
            var cipherBytes = new byte[combinedArray.Length - iv.Length];

            Array.Copy(combinedArray, 0, iv, 0, iv.Length);
            Array.Copy(combinedArray, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(key, 32);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            Logger.Error($"{Constants.AesLogMark}: An error occurred while decrypting:\n{ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Decrypts the specified encrypted string and returns the result as a SecureString.
    /// </summary>
    /// <remarks>Ensure that the provided key is protected and not exposed to unauthorized access. The caller
    /// is responsible for managing the lifecycle of the returned SecureString.</remarks>
    /// <param name="data">The encrypted string to decrypt. This parameter must not be null or empty.</param>
    /// <param name="key">The SecureString used as the decryption key. This parameter must be properly initialized and kept secure.</param>
    /// <returns>A SecureString containing the decrypted data. Returns null if decryption fails.</returns>
    internal static SecureString DecryptToSecureString(string data, SecureString key) =>
        SecurityHelper.ToSecureString(Decrypt(data, key));

    /// <summary>
    /// Encrypts the specified plaintext data using the provided secure key and returns the encrypted result as a
    /// Base64-encoded string.
    /// </summary>
    /// <remarks>A random initialization vector (IV) is generated for each encryption operation to enhance
    /// security. The IV is prepended to the encrypted data in the returned string, allowing for proper decryption.
    /// Callers must ensure the key meets the required constraints for successful encryption.</remarks>
    /// <param name="data">The plaintext string to be encrypted. Cannot be null or empty.</param>
    /// <param name="key">A secure key used for encryption. Cannot be null and must have a length greater than zero.</param>
    /// <returns>A Base64-encoded string containing the encrypted data with the initialization vector (IV) prepended, or null if
    /// the input data or key is invalid.</returns>
    internal static string Encrypt(string data, SecureString key)
    {
        if (string.IsNullOrEmpty(data) || key == null || key.Length == 0)
            return null;

        var iv = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(iv);

        using var aes = Aes.Create();
        aes.Key = DeriveKeyFromPassword(key, 32);
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs, Encoding.UTF8))
        {
            sw.Write(data);
        }

        var encryptedBytes = ms.ToArray();
        var combined = new byte[iv.Length + encryptedBytes.Length];
        Array.Copy(iv, 0, combined, 0, iv.Length);
        Array.Copy(encryptedBytes, 0, combined, iv.Length, encryptedBytes.Length);

        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Derives a cryptographic key from the specified password using the Argon2id key derivation function.
    /// </summary>
    /// <remarks>This method uses a fixed salt for compatibility and applies Argon2id with a memory cost of
    /// 65536, 3 iterations, and a degree of parallelism set to 4. The fixed salt may impact security; for stronger
    /// protection, consider using a unique salt per password. Ensure that the key length matches the requirements of
    /// the cryptographic algorithm in use.</remarks>
    /// <param name="password">The password from which to derive the key. Must be provided as a SecureString to enhance security and protect
    /// sensitive data in memory.</param>
    /// <param name="keyLength">The desired length, in bytes, of the derived key. Must be a positive integer appropriate for the intended
    /// cryptographic algorithm.</param>
    /// <returns>A byte array containing the derived cryptographic key of the specified length.</returns>
    private static byte[] DeriveKeyFromPassword(SecureString password, int keyLength)
    {
        const int iterations = 3;
        const int memoryCost = 65536;
        const int parallelism = 4;

        var salt = "FixedSaltForCompatibility"u8.ToArray();

        using var argon2 = new Argon2id(GetSecureStringBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            Iterations = iterations,
            MemorySize = memoryCost
        };

        var hash = argon2.GetBytes(keyLength);

        Array.Clear(salt, 0, salt.Length);
        return hash;
    }

    /// <summary>
    /// Converts the specified SecureString to a byte array containing its UTF-8 encoded representation.
    /// </summary>
    /// <remarks>This method securely handles the conversion of a SecureString to prevent sensitive data
    /// exposure. The memory allocated for the string is properly freed after use to minimize the risk of leaking
    /// sensitive information.</remarks>
    /// <param name="secureString">The SecureString instance to convert. Must not be null or empty.</param>
    /// <returns>A byte array representing the UTF-8 encoded bytes of the SecureString. Returns an empty array if the input
    /// SecureString is null or empty.</returns>
    private static byte[] GetSecureStringBytes(SecureString secureString)
    {
        if (secureString == null || secureString.Length == 0)
            return [];

        var ptr = IntPtr.Zero;

        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Encoding.UTF8.GetBytes(Marshal.PtrToStringUni(ptr, secureString.Length));
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }
}