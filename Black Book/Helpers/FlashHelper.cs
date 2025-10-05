// BlackBook/Helpers/FlashHelper.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Attention Flash Helper
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BlackBook.Helpers;

public static class FlashHelper {
    [StructLayout(LayoutKind.Sequential)]
    private struct FLASHWINFO {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    private const uint FLASHW_STOP = 0;
    private const uint FLASHW_CAPTION = 0x00000001;
    private const uint FLASHW_TRAY = 0x00000002;
    private const uint FLASHW_TIMERNOFG = 0x0000000C;

    [DllImport("user32.dll")]
    private static extern bool FlashWindowEx (ref FLASHWINFO pwfi);

    public static void Flash (Window w, bool trayToo = true, uint count = 3) {
        var hwnd = new WindowInteropHelper(w).EnsureHandle();
        var flags = FLASHW_CAPTION | (trayToo ? FLASHW_TRAY : 0) | FLASHW_TIMERNOFG;
        var info = new FLASHWINFO {
            cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
            hwnd = hwnd,
            dwFlags = flags,
            uCount = count,
            dwTimeout = 0
        };
        FlashWindowEx(ref info);
    }

    public static void Stop (Window w) {
        var hwnd = new WindowInteropHelper(w).EnsureHandle();
        var info = new FLASHWINFO {
            cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
            hwnd = hwnd,
            dwFlags = FLASHW_STOP
        };
        FlashWindowEx(ref info);
    }
}
