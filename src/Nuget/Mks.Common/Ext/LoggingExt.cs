using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Mks.Common.Ext
{
    public static class LoggingExt
    {
        public static void TryLog(this ILogger? log, LogLevel level, string message)
        {
            if (log != null!)
            {
                log.Log(level, message);
            }
            else
            {
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine($"[Mks-{level}]{message}");
                }
            }
        }
        public static void TryLogTrace(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Trace, message);
        }
        public static void TryLogInformation(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Information, message);
        }
        public static void TryLogInfo(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Information, message);
        }

        public static void TryLogWarning(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Warning, message);
        }
        public static void TryLogError(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Error, message);
        }
        public static void TryLogCritical(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Critical, message);
        }
    }
}
