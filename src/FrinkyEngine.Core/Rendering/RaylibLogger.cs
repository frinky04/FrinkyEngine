using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FrinkyEngine.Core.Rendering;

public static unsafe class RaylibLogger
{
    [DllImport("raylib", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetTraceLogCallback(delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void> callback);

    [DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
    private static extern int vsnprintf(sbyte* buffer, nuint size, sbyte* format, sbyte* args);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void TraceLogCallback(int logLevel, sbyte* text, sbyte* args)
    {
        sbyte* buffer = stackalloc sbyte[1024];
        vsnprintf(buffer, 1024, text, args);
        var message = new string(buffer).TrimEnd('\n', '\r');

        var level = logLevel switch
        {
            5 => LogLevel.Error,   // LOG_ERROR
            6 => LogLevel.Error,   // LOG_FATAL
            4 => LogLevel.Warning, // LOG_WARNING
            _ => LogLevel.Info     // LOG_INFO, LOG_DEBUG, LOG_TRACE
        };

        FrinkyLog.Log(message, level, "Raylib");
    }

    public static void Install()
    {
        SetTraceLogCallback(&TraceLogCallback);
    }
}
