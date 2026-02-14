using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FrinkyEngine.Core.Rendering;

/// <summary>
/// Redirects Raylib's native trace log output to <see cref="FrinkyLog"/>.
/// </summary>
public static unsafe class RaylibLogger
{
    [DllImport("raylib", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetTraceLogCallback(delegate* unmanaged[Cdecl]<int, sbyte*, sbyte*, void> callback);

    [DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl)]
    private static extern int vsnprintf(sbyte* buffer, nuint size, sbyte* format, sbyte* args);

    /// <summary>
    /// Whether Raylib trace log messages are forwarded to <see cref="FrinkyLog"/>. Default is <c>false</c>.
    /// </summary>
    public static bool Enabled { get; set; }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void TraceLogCallback(int logLevel, sbyte* text, sbyte* args)
    {
        if (!Enabled)
            return;

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

    /// <summary>
    /// Installs the Raylib trace log callback. Call once during initialization.
    /// </summary>
    public static void Install()
    {
        SetTraceLogCallback(&TraceLogCallback);
    }
}
