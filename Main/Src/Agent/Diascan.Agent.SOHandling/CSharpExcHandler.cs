using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Diascan.Agent.SOHandling
{
    public enum VEH : long
    {
        EXCEPTION_CONTINUE_SEARCH = 0,
        EXCEPTION_EXECUTE_HANDLER = 1,
        EXCEPTION_CONTINUE_EXECUTION = -1
    }

    public class CSharpExcHandler
    {
        private const uint STATUS_IN_PAGE_ERROR = 3221225478;
        private const uint STATUS_ACCESS_VIOLATION = 3221225477;
        private const uint Err = 0xf00000fd;
        
        public static void HandleSO(Action action)
            => HandleSO<object>(() => { action(); return null; });

        public static unsafe void HandleSO<T>(Func<T> action)
        {
            var handler = Kernel32.AddVectoredExceptionHandler(1, Handler);
            
            if (handler == IntPtr.Zero)
                throw new Win32Exception("AddVectoredExceptionHandler failed");

            var size = 32768;
            if (!Kernel32.SetThreadStackGuarantee(&size))
                throw new InsufficientExecutionStackException("SetThreadStackGuarantee failed", new Win32Exception());
            var result = action();
            
            if (handler != IntPtr.Zero)
                Kernel32.RemoveVectoredExceptionHandler(handler);
        }

        private static unsafe VEH Handler(ref EXCEPTION_POINTERS exceptionPointers)
        {
            if (exceptionPointers.ExceptionRecord == null)
                return VEH.EXCEPTION_CONTINUE_SEARCH;

            var record = exceptionPointers.ExceptionRecord;
            if (record->ExceptionCode != STATUS_ACCESS_VIOLATION && record->ExceptionCode != STATUS_IN_PAGE_ERROR)
                return VEH.EXCEPTION_CONTINUE_SEARCH;
            record->ExceptionCode = Err;
            return VEH.EXCEPTION_EXECUTE_HANDLER;
        }
    }

    public delegate VEH PVECTORED_EXCEPTION_HANDLER(ref EXCEPTION_POINTERS exceptionPointers);

    public static class Msvcrt
    {
        [DllImport("msvcrt.dll", SetLastError = true)]
        public static extern int _resetstkoflw();
    }

    public static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr AddVectoredExceptionHandler(Int32 FirstHandler, PVECTORED_EXCEPTION_HANDLER VectoredHandler);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr RemoveVectoredExceptionHandler(IntPtr InstalledHandler);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool SetThreadStackGuarantee(int* StackSizeInBytes);
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EXCEPTION_POINTERS
    {
        public EXCEPTION_RECORD* ExceptionRecord;
        public IntPtr Context;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EXCEPTION_RECORD
    {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public EXCEPTION_RECORD* ExceptionRecord;
        public IntPtr ExceptionAddress;
        public uint NumberParameters;
        public IntPtr* ExceptionInformation;
    }
}
