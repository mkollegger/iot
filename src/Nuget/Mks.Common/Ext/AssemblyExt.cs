using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Mks.Common.Ext
{
    public static class AssemblyExt
    {
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
