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
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Mks.Common.Ext
{
    /// <summary>
    ///     Extension methods for Assembly
    /// </summary>
    public static class AssemblyExt
    {
        /// <summary>
        ///     Reads a manifest resource and writes it to a file stream
        /// </summary>
        /// <param name="a">The assembly containing the resource</param>
        /// <param name="manifestId">The resource ID</param>
        /// <param name="file">The destination file stream</param>
        /// <param name="log">Optional logger</param>
        /// <returns>True if successful, otherwise false</returns>
        public static bool GetManifestStoreToFile(this Assembly a, string manifestId, FileStream file, ILogger? log)
        {
            if (a == null!)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (file == null!)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using Stream? stream = a.GetManifestResourceStream(manifestId);
            if (stream == null)
            {
                log.TryLogWarning($"[{nameof(AssemblyExt)}]({nameof(GetManifestStoreToFile)}): {manifestId} not found in resource!");
                return false;
            }

            using FileStream? fileStream = file;
            using StreamReader? reader = new StreamReader(stream);

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
            catch (Exception e)
            {
                log.TryLogError($"[{nameof(AssemblyExt)}]({nameof(GetManifestStoreToFile)}): {e}");
                return false;
            }

            return true;
        }
    }
}