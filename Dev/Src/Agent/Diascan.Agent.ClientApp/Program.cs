using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Diascan.Agent.Manager;

namespace Diascan.Agent.ClientApp
{
    public static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int command);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            var currentProcessName = Process.GetCurrentProcess().ProcessName;
            var processesAgent = new List<string>();
            var propertiesToSelect = new[] {"Handle", "ProcessId"};
            var processQuery = new SelectQuery("Win32_Process", $"Name = '{currentProcessName}.exe'", propertiesToSelect);

            using (var searcher = new ManagementObjectSearcher(processQuery))
            using (var processes = searcher.Get())
                foreach (ManagementObject process in processes)
                {
                    var outParams = process.InvokeMethod("GetOwner", null, null);
                    if (outParams == null) continue;
                    var user = (string)outParams["User"];
                    if (processesAgent.Contains(user))
                    {
                        var hwnd = FindWindow(null, "ПО ПДИ");
                        ShowWindow(hwnd, 5);
                        return;
                    }
                    processesAgent.Add(user);
                }

            var agentManager = new AgentManager();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DBForm());
        }
    }
}
