using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Diascan.Agent.ErrorHandler
{
    internal enum Veh : long
    {
        ExceptionContinueSearch = 0,
        ExceptionExecuteHandler = 1
    }

    public class AgentErrorHandler
    {
        public const uint Err = 0xf00000fd;

        private const uint StatusInPageError = 3221225478;
        private const uint StatusAccessViolation = 3221225477;
        
        public static void TryCatch(Action action) => TryCatch<object>(() => { action(); return null; });

        public static unsafe void TryCatch<T>(Func<T> action)
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

        private static unsafe Veh Handler(ref ExceptionPointers exceptionPointers)
        {
            if (exceptionPointers.ExceptionRecordPtr == null)
                return Veh.ExceptionContinueSearch;

            var record = exceptionPointers.ExceptionRecordPtr;
            if (record->ExceptionCode != StatusAccessViolation && record->ExceptionCode != StatusInPageError)
                return Veh.ExceptionContinueSearch;
            record->ExceptionCode = Err;
            return Veh.ExceptionExecuteHandler;
        }
    }

    internal delegate Veh PvectoredExceptionHandler(ref ExceptionPointers exceptionPointers);
    
    internal static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr AddVectoredExceptionHandler(int firstHandler, PvectoredExceptionHandler vectoredHandler);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr RemoveVectoredExceptionHandler(IntPtr installedHandler);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool SetThreadStackGuarantee(int* stackSizeInBytes);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ExceptionPointers
    {
        public ExceptionRecord* ExceptionRecordPtr;
        public IntPtr Context;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ExceptionRecord
    {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public ExceptionRecord* ExceptionRecordPtr;
        public IntPtr ExceptionAddress;
        public uint NumberParameters;
        public IntPtr* ExceptionInformation;
    }
}
