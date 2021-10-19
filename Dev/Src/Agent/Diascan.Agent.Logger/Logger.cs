using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace Diascan.Agent.Logger
{
    public static class Logger
    {
        public static string Domain { get; private set; }
        public static string Username { get; private set; }
        public static string PathFile { get; private set; }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private enum TypeOfMessage
        {
            Info = 1,
            Warn,
            Error,
            Fatal
        }

        private static readonly object locking = new object();

        public static void InitLogger(string pathToExecuteFile)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Fatal);
            if (!Directory.Exists(pathToExecuteFile))
                Directory.CreateDirectory(pathToExecuteFile);
            PathFile = pathToExecuteFile + @"\templog.log";
            UserIdentification();
            Info($"{Domain}/{Username} - начало работы");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(3);
            return sf.GetMethod().Name;
        }

        //  формат сообщения: {дата}|{тип сообщения}|{вызывающий метод}|{сообщение}
        private static string GenerateLogString(TypeOfMessage typeOfMessage, object message)
        {
            return $"{DateTime.Now:F}|{typeOfMessage}|{GetCurrentMethod()}|{message}";
        }

        private static void WriteToLogFile(string message)
        {
            lock (locking)
            {
                using (var sw = new StreamWriter(PathFile, true))
                {
                    sw.WriteLine(message);
                }
            }
        }

        public static void Info(object message)
        {
            WriteToLogFile(GenerateLogString(TypeOfMessage.Info, message));
        }

        public static void Warn(object message)
        {
            WriteToLogFile(GenerateLogString(TypeOfMessage.Info, message));
        }

        public static void Error(object message)
        {
            WriteToLogFile(GenerateLogString(TypeOfMessage.Info, message));
        }

        private static void Fatal(object sender, UnhandledExceptionEventArgs e)
        {
            WriteToLogFile(GenerateLogString(TypeOfMessage.Info, $"Критическая ошибка: {e.ExceptionObject}"));
            MessageBox.Show($"Возникла критическая ошибка, приложение будет закрыто: {e.ExceptionObject}", 
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            Process.Start(PathFile);
            Environment.Exit(0);
        }

        public static string GetLogFile()
        {
            lock (locking)
            {
                using (var sr = new StreamReader(PathFile))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static string GenerateXlsFileName(string namePdi)
        {
            var shortDateFormat = DateTime.Now.ToString("yy-MM-dd");
            var shortTimeFormat = DateTime.Now.ToString("T").Replace(":", "-");
            return $"{namePdi}_{Username}_{shortDateFormat}_{shortTimeFormat}.xls";
        }

        public static void UserIdentification()
        {
            var process = Process.GetCurrentProcess();
            var processHandle = IntPtr.Zero;
            try
            {
                OpenProcessToken(process.Handle, 8, out processHandle);
                var wi = new WindowsIdentity(processHandle);
                Domain = wi.Name.Split('\\').First();
                Username = wi.Name.Split('\\').Last();
            }
            catch
            {
                Warn("Не удалось определить пользователя");
                return;
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
