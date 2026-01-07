#region License

// #region License
// MIT License
// 
// Copyright (C) 2026 Michael Kollegger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// #endregion

#endregion

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Mks.Common.Ext
{
    /// <summary>
    ///     Extension methods for ILogger to safely log messages even if the logger is null
    /// </summary>
    public static class LoggingExt
    {
        /// <summary>
        ///     Tries to log a message at the specified level. If logger is null and debugger is attached, writes to Debug output.
        /// </summary>
        /// <param name="log">The logger instance (can be null)</param>
        /// <param name="level">The log level</param>
        /// <param name="message">The message to log</param>
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

        /// <summary>
        ///     Tries to log a Trace message
        /// </summary>
        /// <param name="log">The logger instance</param>
        /// <param name="message">The message</param>
        public static void TryLogTrace(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Trace, message);
        }

        /// <summary>
        ///     Tries to log an Information message
        /// </summary>
        /// <param name="log">The logger instance</param>
        /// <param name="message">The message</param>
        public static void TryLogInformation(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Information, message);
        }

        /// <summary>
        ///     Tries to log an Information message (alias for TryLogInformation)
        /// </summary>
        /// <param name="log">The logger instance</param>
        /// <param name="message">The message</param>
        public static void TryLogInfo(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Information, message);
        }

        /// <summary>
        ///     Tries to log a Warning message
        /// </summary>
        /// <param name="log">The logger instance</param>
        /// <param name="message">The message</param>
        public static void TryLogWarning(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Warning, message);
        }

        /// <summary>
        ///     Tries to log an Error message
        /// </summary>
        /// <param name="log">The logger instance</param>
        /// <param name="message">The message</param>
        public static void TryLogError(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Error, message);
        }

        /// <summary>
        ///     Tries to log a Critical message
        /// </summary>
        /// <param name="log">The logger instance</param>
        /// <param name="message">The message</param>
        public static void TryLogCritical(this ILogger? log, string message)
        {
            log.TryLog(LogLevel.Critical, message);
        }
    }
}