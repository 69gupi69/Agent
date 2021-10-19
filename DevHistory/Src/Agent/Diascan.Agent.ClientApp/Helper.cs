using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace Diascan.Agent.ClientApp
{
    static class Helper
    {
        private const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        private const int REMOTE_NAME_INFO_LEVEL = 0x00000002;

        private const int ERROR_MORE_DATA = 234;
        private const int NOERROR = 0;

        [DllImport("mpr.dll", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int WNetGetUniversalName(
                    string lpLocalPath,
                    [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
                    IntPtr lpBuffer,
                    [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        public static string GetUniversalName(string path)
        {
            if (String.IsNullOrEmpty(path)) return String.Empty;
            try
            {
                var uri = new Uri(path);
                if (!uri.IsUnc)
                {
                    uri = new Uri(GetUNC(path));
                }

                if (uri.HostNameType == UriHostNameType.IPv4)
                {
                    var hostEntry = Dns.GetHostEntry(uri.Host);
                    var uriBuilder = new UriBuilder(uri.Scheme, hostEntry.HostName, uri.Port, uri.AbsolutePath);
                    uri = uriBuilder.Uri;
                }

                return uri.LocalPath;
            }
            catch (Exception)
            {
                return path;
            }
        }

        private static string GetUNC(string path)
        {
            var buffer = IntPtr.Zero;
            try
            {
                var size = 0;
                var apiRetVal = WNetGetUniversalName(path, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size,
                    ref size);

                if (apiRetVal != ERROR_MORE_DATA)
                    return path;

                buffer = Marshal.AllocCoTaskMem(size);
                apiRetVal = WNetGetUniversalName(path, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

                if (apiRetVal != NOERROR)
                    throw new Win32Exception(apiRetVal);

                return Marshal.PtrToStringAnsi(new IntPtr(buffer.ToInt64() + IntPtr.Size));
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }
    }
}
