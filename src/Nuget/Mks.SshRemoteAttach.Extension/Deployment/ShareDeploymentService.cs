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

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mks.SshRemoteAttach.Extension.Core;

namespace Mks.SshRemoteAttach.Extension.Deployment;

/// <summary>
///     Copies the MSBuild output directory to a local SMB mount that mirrors the remote working directory.
/// </summary>
/// <remarks>
///     The local SMB share (<see cref="SshRemoteAttachProfile.DeployLocalShare" />) must already be
///     mounted before the command is invoked — the extension does not mount it.
/// </remarks>
internal sealed class ShareDeploymentService : IDeploymentService
{
    // ── Helpers ─────────────────────────────────────────────────────────────

    // Path.GetRelativePath is not available in net472 — implement with Uri.
    private static string GetRelativePath(string basePath, string fullPath)
    {
        var baseUri = new Uri(basePath.TrimEnd('\\', '/') + Path.DirectorySeparatorChar);
        var fullUri = new Uri(fullPath);
        return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString())
            .Replace('/', Path.DirectorySeparatorChar);
    }

    private static void CopyDirectory(string source, string dest, CancellationToken ct)
    {
        Directory.CreateDirectory(dest);

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();

            var relative = GetRelativePath(source, file);
            var destFile = Path.Combine(dest, relative);

            Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);

            // Skip unchanged files (same size and mtime) for fast incremental deploys.
            if (File.Exists(destFile))
            {
                var src = new FileInfo(file);
                var dst = new FileInfo(destFile);
                if (src.Length == dst.Length && src.LastWriteTimeUtc == dst.LastWriteTimeUtc)
                {
                    continue;
                }
            }

            File.Copy(file, destFile, true);
        }
    }

    #region Interface Implementations

    /// <inheritdoc />
    public async Task DeployAsync(
        SshRemoteAttachProfile profile,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!profile.DeployBeforeLaunch || string.IsNullOrEmpty(profile.DeployLocalShare))
        {
            return;
        }

        if (string.IsNullOrEmpty(outputDirectory))
        {
            throw new LaunchException(
                "Could not determine the project output directory. Build the project first.");
        }

        if (!Directory.Exists(outputDirectory))
        {
            throw new LaunchException(
                $"Output directory does not exist: '{outputDirectory}'. Build the project first.");
        }

        var dest = profile.DeployLocalShare;

        try
        {
            await Task.Run(() => CopyDirectory(outputDirectory, dest!, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not LaunchException)
        {
            throw new LaunchException(
                $"Deployment failed copying '{outputDirectory}' to '{dest}': {ex.Message}", ex);
        }
    }

    #endregion
}