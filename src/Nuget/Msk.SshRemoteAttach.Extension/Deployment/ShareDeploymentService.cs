using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SshRemoteAttach.Core;

namespace SshRemoteAttach.Deployment;

/// <summary>
/// Copies the MSBuild output directory to a local SMB mount that mirrors the remote working directory.
/// </summary>
/// <remarks>
/// The local SMB share (<see cref="SshRemoteAttachProfile.DeployLocalShare"/>) must already be
/// mounted before the command is invoked — the extension does not mount it.
/// </remarks>
internal sealed class ShareDeploymentService : IDeploymentService
{
    /// <inheritdoc/>
    public async Task DeployAsync(
        SshRemoteAttachProfile profile,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!profile.DeployBeforeLaunch || string.IsNullOrEmpty(profile.DeployLocalShare))
            return;

        if (string.IsNullOrEmpty(outputDirectory))
            throw new LaunchException(
                "Could not determine the project output directory. Build the project first.");

        if (!Directory.Exists(outputDirectory))
            throw new LaunchException(
                $"Output directory does not exist: '{outputDirectory}'. Build the project first.");

        var dest = profile.DeployLocalShare;

        try
        {
            await Task.Run(() => CopyDirectory(outputDirectory, dest, cancellationToken), cancellationToken)
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
                var src  = new FileInfo(file);
                var dst  = new FileInfo(destFile);
                if (src.Length == dst.Length && src.LastWriteTimeUtc == dst.LastWriteTimeUtc)
                    continue;
            }

            File.Copy(file, destFile, overwrite: true);
        }
    }
}
