using SafeBox.Extensions;
using SafeBox.Infrastructure;
using System;
using System.Security;
using System.Security.Cryptography;

namespace SafeBox.Security;

internal static class DpapiCryptographer
{
    private static readonly byte[] Entropy = [20];

    internal static string Encrypt(SecureString secureString)
    {
        try
        {
            var encryptedData = ProtectedData.Protect(
                SecurityHelper.SecureStringToString(secureString).GetUtf8Bytes(),
                Entropy,
                DataProtectionScope.LocalMachine);
                
            return Convert.ToBase64String(encryptedData);
        }
        catch (Exception ex)
        {
            Logger.Error($"{Constants.DpapiLogMark}: An error occurred while encrypting:\n{ex.Message}\n{ex.StackTrace}");
            return string.Empty;
        }
    }

    internal static SecureString Decrypt(string data)
    {
        try
        {
            return SecurityHelper.ToSecureString(
                ProtectedData.Unprotect(Convert.FromBase64String(data),
                    Entropy,
                    DataProtectionScope.LocalMachine));
        }
        catch (Exception ex)
        {
            Logger.Error($"{Constants.DpapiLogMark}: An error occurred while decrypting:\n{ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }
}