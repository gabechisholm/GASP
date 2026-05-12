using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PBIScreenshotter
{
    public class HotKeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        public static void Register(Window window, uint key, uint modifiers, int id)
        {
            var helper = new WindowInteropHelper(window);
            RegisterHotKey(helper.Handle, id, modifiers, key);
        }

        public static void Unregister(Window window, int id)
        {
            var helper = new WindowInteropHelper(window);
            UnregisterHotKey(helper.Handle, id);
        }
    }
}
