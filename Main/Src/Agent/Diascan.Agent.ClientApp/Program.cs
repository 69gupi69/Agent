using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Diascan.Agent.Manager;

namespace Diascan.Agent.ClientApp
{
    static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int Command);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Process.GetProcessesByName("Diascan.Agent.ClientApp").Length > 1)
            {
                var hwnd = FindWindow(null, "Состояние БД");
                ShowWindow(hwnd, 5);
                return;
            }
            var agentManager = new AgentManager();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DBForm());
        }
    }
}
