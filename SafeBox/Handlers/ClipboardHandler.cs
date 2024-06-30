using System;
using System.Runtime.InteropServices;

namespace SafeBox.Handlers
{
    internal static class ClipboardHandler
    {
        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool SetClipboardData(uint uFormat, IntPtr data);

        private const uint CF_UNICODETEXT = 13;

        internal static bool CopyTextToClipboard(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                return false;
            }

            SetClipboardData(CF_UNICODETEXT, Marshal.StringToHGlobalUni(text));
            CloseClipboard();

            return true;
        }
    }
}
