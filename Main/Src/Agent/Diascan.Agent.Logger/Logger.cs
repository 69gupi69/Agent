using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;


namespace Diascan.Agent.Logger
{
    public static class Logger
    {
        private static string domain;
        private static string username;

        public static string Domain => domain;
        public static string Username => username;


        //private static LiteCollection<LogCollection> logCollection;
        private static string pathFile;
        public static string PathFileLog => pathFile;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        public static void InitLogger(string pathToExecuteFile)
        {
            if  ( !Directory.Exists( pathToExecuteFile ) )
                Directory.CreateDirectory( pathToExecuteFile );

            pathFile = pathToExecuteFile + @"\templog.log";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(2);
            return sf.GetMethod().Name;
        }

        public static void Info(object message)
        {
            var strLog = $"{Environment.NewLine}[{DateTime.Now:F}] " +
                         $"[{GetCurrentMethod()}] " +
                         $"[{GetProcessUser()}] " +
                         $"[{message.ToString()}]";
            try
            {
                File.AppendAllText(pathFile, strLog, Encoding.UTF8);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static string GenerateXlsFileName(string NamePDI)
        {
            var shortDateFormat = DateTime.Now.ToString("yy-MM-dd");
            var shortTimeFormat = DateTime.Now.ToString("T").Replace(":", "-");
            return $"{NamePDI}_{GetProcessUser()}_{shortDateFormat}_{shortTimeFormat}.xls";
        }

        public static string GetProcessUser()
        {
            var process = Process.GetProcessesByName("explorer").FirstOrDefault();
            if (process == null)
                return WindowsIdentity.GetCurrent().Name;
            var processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                var wi = new WindowsIdentity(processHandle);

                domain = wi.Name.Split('\\').First();
                username = wi.Name.Split('\\').Last();

                return username;
            }
            catch
            {
                return "Не удалось определить пользователя";
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }
        }
    }
}
