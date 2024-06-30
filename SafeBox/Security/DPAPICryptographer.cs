using SafeBox.Interfaces;
using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace SafeBox.Security
{
    internal class DPAPICryptographer : ICryptographer<SecureString>
    {
        private static readonly byte[] Entropy = [20];

        public string Encrypt(string data, string key = null)
        {
            try
            {
                var encryptedData = ProtectedData.Protect(
                    Encoding.Unicode.GetBytes(data),
                    Entropy,
                    DataProtectionScope.LocalMachine);

                return Convert.ToBase64String(encryptedData);
            }
            catch
            {
                return string.Empty;
            }
        }

        public SecureString Decrypt(string data, string key = null)
        {
            try
            {
                var decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(data),
                    Entropy,
                    DataProtectionScope.LocalMachine);

                return SecurityHelper.ToSecureString(Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return null;
            }
        }
    }
}
