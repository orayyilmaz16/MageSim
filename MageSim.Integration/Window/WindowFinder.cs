using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MageSim.Integration.Window
{
    public static class WindowFinder
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        public static IntPtr FindByTitleContains(string contains)
        {
            IntPtr found = IntPtr.Zero;
            EnumWindows((h, p) =>
            {
                var sb = new StringBuilder(512);
                GetWindowText(h, sb, sb.Capacity);
                var title = sb.ToString();
                if (!string.IsNullOrWhiteSpace(title) &&
                    title.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = h;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        public static IntPtr FindByProcess(string processName, int index = 0)
        {
            var procs = Process.GetProcessesByName(processName);
            if (procs.Length == 0 || index < 0 || index >= procs.Length) return IntPtr.Zero;
            var target = procs[index];
            return target.MainWindowHandle; // gerekirse PID ile enumlayıp eşleştir
        }

        public static bool IsValid(IntPtr hWnd) => hWnd != IntPtr.Zero && IsWindow(hWnd);
    }
}
