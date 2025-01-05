using SafeBox.Extensions;
using System;
using System.Security;
using System.Security.Cryptography;

namespace SafeBox.Security
{
    internal static class SHACryptographer
    {
        internal static string Encrypt(string data)
        {
            if (data.IsNullOrWhiteSpace())
                return string.Empty;

            using var sha512 = SHA512.Create();
            return Convert.ToBase64String(sha512.ComputeHash(data.GetUTF8Bytes()));
        }

        internal static string Encrypt(SecureString secureString)
        {
            if (secureString.IsNull())
                return string.Empty;

            using var sha512 = SHA512.Create();
            return Convert.ToBase64String(secureString.GetHashBytes(sha512));
        }
    }
}
