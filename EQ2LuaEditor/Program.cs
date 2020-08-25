using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace EQ2LuaEditor
{
    [StructLayout(LayoutKind.Sequential)]
    struct COPYDATASTRUCT
    {
        public int dwData;
        public int cbData;
        public int lpData;
    }

    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        private const int WM_COPYDATA = 0x4A;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew = true;
            using (Mutex mutex = new Mutex(true, "EQ2LuaEditor", out createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new EQ2LuaEditor(args));
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            if (args.Length > 0)
                            {
                                for (int i = 0; i < args.Length; i++)
                                {
                                    COPYDATASTRUCT cds;
                                    cds.dwData = 0;
                                    cds.lpData = (int)Marshal.StringToHGlobalAnsi(args[i]);
                                    cds.cbData = args[i].Length;
                                    SendMessage(process.MainWindowHandle, WM_COPYDATA, IntPtr.Zero, ref cds);
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}
