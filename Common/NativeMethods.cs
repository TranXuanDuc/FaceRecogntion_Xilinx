using System;
using System.Text;

using System.Runtime.InteropServices;



namespace HD2
{

    static class NativeMethods
    {
        #region Dllimport
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr h);
        //check lock file
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string lpFileName, System.UInt32 dwDesiredAccess, System.UInt32 dwShareMode, IntPtr pSecurityAttributes, System.UInt32 dwCreationDisposition, System.UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        public static readonly uint GENERIC_WRITE = 0x40000000;
        public static readonly uint OPEN_EXISTING = 3;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetCursorPos(int x, int y);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int cButtons, uint dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int WHEEL_DELTA = 120;
        [DllImport("kernel32")]
        public static extern uint WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        public static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);
        #endregion Dllimport
    }
}
