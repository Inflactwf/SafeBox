using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SafeBox.Security;

internal static class SecurityHelper
{
    internal static SecureString TryGetSecureKeyAsSecureStringOrNull()
    {
        var encryptedKey = ConfigurationHandler.SecureKey;

        if (encryptedKey.IsNull())
            return null;

        var secureKey = DPAPICryptographer.Decrypt(encryptedKey);

        return secureKey.IsNull()
            ? null
            : secureKey;
    }

    internal static string SecureStringToString(SecureString secureString)
    {
        var unmanagedString = IntPtr.Zero;

        try
        {
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(unmanagedString);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
        }
    }

    internal static SecureString ToSecureString(byte[] byteArray)
    {
        if (byteArray == null || byteArray.Length == 0)
        {
            Logger.Warn($"{Constants.GeneralLogMark}: Input byte array was null or empty while converting bytes to secure string.");
            return null;
        }

        var secureString = new SecureString();

        try
        {
            foreach (var b in byteArray)
                secureString.AppendChar((char)b);

            secureString.MakeReadOnly();
        }
        finally
        {
            Array.Clear(byteArray, 0, byteArray.Length);
        }

        return secureString;
    }

    internal static SecureString ToSecureString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            Logger.Warn($"{Constants.GeneralLogMark}: Input string was null or empty while converting string to secure string.");
            return null;
        }

        var secureStr = new SecureString();

        foreach (var c in str)
            secureStr.AppendChar(c);

        secureStr.MakeReadOnly();

        DecomposeString(ref str);

        return secureStr;
    }

    internal static void DecomposeString(ref string str)
    {
        if (str == null)
            return;

        unsafe
        {
            fixed (char* chars = str)
            {
                for (var i = 0; i < str.Length; i++)
                {
                    chars[i] = (char)0;
                }
            }
        }

        str = null;
    }
}