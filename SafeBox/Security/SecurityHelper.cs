using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SafeBox.Security
{
    internal static class SecurityHelper
    {
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

        internal static SecureString ToSecureString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return new();

            var secureStr = new SecureString();

            foreach (var c in str)
                secureStr.AppendChar(c);

            secureStr.MakeReadOnly();

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
                    for (int i = 0; i < str.Length; i++)
                    {
                        chars[i] = (char)0;
                    }
                }
            }

            str = null;
        }
    }
}
