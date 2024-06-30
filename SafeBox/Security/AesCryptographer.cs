using SafeBox.Extensions;
using SafeBox.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeBox.Security
{
    internal class AesCryptographer : ICryptographer<string>
    {
        public string Decrypt(string data, string key)
        {
            if (data.IsNullOrWhiteSpace() || key.IsNullOrWhiteSpace())
                return default;

            //var combinedArray = Convert.FromBase64String(data);
            //var iv = new byte[16];
            //var buffer = new byte[combinedArray.Length - iv.Length];

            //Array.Copy(combinedArray, 0, iv, 0, iv.Length);
            //Array.Copy(combinedArray, iv.Length, buffer, 0, buffer.Length);

            //using var aes = Aes.Create();
            //aes.Key = GetFixedLengthKey(key, 32);
            //aes.IV = iv;

            //var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            //using var memoryStream = new MemoryStream(buffer);
            //using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            //using var streamReader = new StreamReader(cryptoStream);

            //return streamReader.ReadToEnd();

            byte[] combinedArray = Convert.FromBase64String(data);
            byte[] iv = new byte[16];
            byte[] buffer = new byte[combinedArray.Length - iv.Length];

            Array.Copy(combinedArray, 0, iv, 0, iv.Length);
            Array.Copy(combinedArray, iv.Length, buffer, 0, buffer.Length);

            byte[] fixedLengthKey = GetFixedLengthKey(key, 32); // Используем 32 байта для AES-256

            using (Aes aes = Aes.Create())
            {
                aes.Key = fixedLengthKey;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string Encrypt(string data, string key)
        {
            if (data.IsNullOrWhiteSpace() || key.IsNullOrWhiteSpace())
                return default;

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            byte[] fixedLengthKey = GetFixedLengthKey(key, 32); // Используем 32 байта для AES-256
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = fixedLengthKey;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(data);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }

            byte[] combinedArray = new byte[iv.Length + array.Length];
            Array.Copy(iv, 0, combinedArray, 0, iv.Length);
            Array.Copy(array, 0, combinedArray, iv.Length, array.Length);

            return Convert.ToBase64String(combinedArray);

            //var iv = new byte[16];
            //using var rng = RandomNumberGenerator.Create();
            //rng.GetBytes(iv);

            //var fixedLengthKey = GetFixedLengthKey(key, 32);

            //using var aes = Aes.Create();
            //aes.Key = fixedLengthKey;
            //aes.IV = iv;

            //var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            //using var memoryStream = new MemoryStream();
            //using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            //using (var streamWriter = new StreamWriter(cryptoStream))
            //    streamWriter.Write(data);

            //var array = memoryStream.ToArray();

            //var combinedArray = new byte[iv.Length + array.Length];
            //Array.Copy(iv, 0, combinedArray, 0, iv.Length);
            //Array.Copy(array, 0, combinedArray, iv.Length, array.Length);

            //return Convert.ToBase64String(combinedArray);
        }

        private static byte[] GetFixedLengthKey(string key, int length)
        {
            using var sha512 = SHA512.Create();

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hashBytes = sha512.ComputeHash(keyBytes);
            var fixedLengthKey = new byte[length];
            Array.Copy(hashBytes, fixedLengthKey, length);

            return fixedLengthKey;
        }
    }
}
