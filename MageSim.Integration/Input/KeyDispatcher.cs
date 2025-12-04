using System;
using System.Runtime.InteropServices;

namespace MageSim.Integration.Input
{
    public sealed class KeyDispatcher
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void SendKey(IntPtr hWnd, byte vk)
        {
            if (hWnd == IntPtr.Zero) return;
            SetForegroundWindow(hWnd);
            keybd_event(vk, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
