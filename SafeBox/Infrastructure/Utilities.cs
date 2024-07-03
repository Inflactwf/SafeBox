using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace SafeBox.Infrastructure
{
    internal static class Utilities
    {
        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        internal static char GetCharFromKey(Key key)
        {
            var keyboardState = new byte[256];
            var keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
                return default;

            var virtualKeyCode = KeyInterop.VirtualKeyFromKey(key);
            var scanCode = MapVirtualKey((uint)virtualKeyCode, 0);
            var inputLocaleIdentifier = GetKeyboardLayout(0);
            var stringBuilder = new StringBuilder(2);
            var result = ToUnicodeEx((uint)virtualKeyCode, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0, inputLocaleIdentifier);

            return result switch
            {
                -1 or 0 => '\0',
                _ => stringBuilder[0],
            };
        }
    }
}
