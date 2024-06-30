using SafeBox.Extensions;
using SafeBox.Interfaces;
using System;
using System.Security.Cryptography;

namespace SafeBox.Security
{
    internal class SHACryptographer : ICryptographer<string>
    {
        public string Decrypt(string data, string key = null) =>
            throw new NotImplementedException();

        public string Encrypt(string data, string key = null)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(data.GetUTF8Bytes()));
        }
    }
}
