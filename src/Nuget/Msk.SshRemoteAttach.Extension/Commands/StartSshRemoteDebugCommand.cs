using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using SshRemoteAttach.Core;
using SshRemoteAttach.Deployment;
using Process = System.Diagnostics.Process;

namespace SshRemoteAttach.Commands;

/// <summary>
/// Menu command that reads <c>launchSettings.json</c>, optionally deploys the build output,
/// and starts a remote debug session via <c>DebugAdapterHost.Launch</c>.
/// </summary>
/// <remarks>
/// Mirrors the manual workflow:
/// <code>
/// DebugAdapterHost.Launch /LaunchJson:attach_vs202x.json
/// </code>
/// </remarks>
[VisualStudioContribution]
internal sealed class StartSshRemoteDebugCommand : Microsoft.VisualStudio.Extensibility.Commands.Command
{
    private readonly AsyncServiceProviderInjection<DTE, DTE2> _dteInjection;
    private readonly LaunchSettingsReader _reader;
    private readonly IDeploymentService _deployment;

    public StartSshRemoteDebugCommand(
        VisualStudioExtensibility extensibility,
        AsyncServiceProviderInjection<DTE, DTE2> dteInjection,
        LaunchSettingsReader reader,
        IDeploymentService deployment)
        : base(extensibility)
    {
        _dteInjection  = dteInjection ?? throw new ArgumentNullException(nameof(dteInjection));
        _reader        = reader       ?? throw new ArgumentNullException(nameof(reader));
        _deployment    = deployment   ?? throw new ArgumentNullException(nameof(deployment));
    }

    /// <inheritdoc/>
    public override CommandConfiguration CommandConfiguration => new("Start SSH Remote Debug")
    {
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        EnabledWhen = ActivationConstraint.SolutionState(SolutionState.Exists),
        
    };

    /// <inheritdoc/>
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        // ── 1. Acquire DTE and switch to the UI thread ───────────────────────
        var dte = await _dteInjection.GetServiceAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        // ── 2. Resolve paths from the running solution (UI thread) ───────────
        string launchSettingsPath;
        string outputDir;
        try
        {
            launchSettingsPath = FindLaunchSettingsPath(dte);
            outputDir          = ResolveOutputDirectory(dte);
        }
        catch (LaunchException ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
            return;
        }

        // ── 3. Read profiles and deploy (thread pool) ────────────────────────
        IReadOnlyList<SshRemoteAttachProfile> profiles;
        SshRemoteAttachProfile profile;
        string tempPath;

        try
        {
            profiles = _reader.ReadProfiles(launchSettingsPath);
                                                                                
            if (profiles.Count == 0)
            {
                await ShowErrorAsync(
                    "No SshRemoteAttach profile found in launchSettings.json.\n" +
                    "Add a profile with commandName: \"SshRemoteAttach\".",
                    cancellationToken);
                return;
            }

            profile = profiles[0];

            await _deployment.DeployAsync(profile, outputDir, cancellationToken)
                             .ConfigureAwait(false);

            var json = AttachJsonBuilder.Build(profile);
            tempPath = WriteTempJson(json, profile.SshHost);
        }
        catch (LaunchException ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
            return;
        }

        // ── 4. Launch the debug adapter (UI thread) ──────────────────────────
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        try
        {

            dte.ExecuteCommand("DebugAdapterHost.Launch", $"/LaunchJson:\"{tempPath}\"");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await ShowErrorAsync($"DebugAdapterHost.Launch failed: {ex.Message}", cancellationToken);
        }
        finally
        {
            TryDelete(tempPath);
        }
    }

    // ── DTE helpers (must be called on the UI thread) ───────────────────────

    /// <summary>Returns the absolute path to <c>Properties/launchSettings.json</c> for the startup project.</summary>
    private static string FindLaunchSettingsPath(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var startupProject = FindStartupProject(dte);

        var projectDir = Path.GetDirectoryName(startupProject.FullName)
            ?? throw new LaunchException(
                $"Could not determine directory of '{startupProject.FullName}'.");

        return Path.Combine(projectDir, "Properties", "launchSettings.json");
    }

    /// <summary>Returns the MSBuild output directory for the active configuration of the startup project.</summary>
    private static string ResolveOutputDirectory(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            var startupProject = FindStartupProject(dte);
            var projectDir     = Path.GetDirectoryName(startupProject.FullName) ?? string.Empty;

            var outputPath = startupProject
                .ConfigurationManager
                .ActiveConfiguration
                .Properties
                .Item("OutputPath")
                .Value as string ?? string.Empty;

            return Path.Combine(projectDir, outputPath).TrimEnd('\\', '/');
        }
        catch (LaunchException)
        {
            throw;
        }
        catch
        {
            return string.Empty; // deployment service handles the empty case
        }
    }

    private static Project FindStartupProject(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (dte.Solution?.SolutionBuild?.StartupProjects is not object[] startups || startups.Length == 0)
            throw new LaunchException(
                "No startup project is set. Right-click a project and choose 'Set as Startup Project'.");

        var startupName = startups[0] as string
            ?? throw new LaunchException("Could not determine the startup project name.");

        // Enumerate top-level and solution-folder items.
        foreach (Project p in dte.Solution.Projects)
        {
            var found = FindInProject(p, startupName);
            if (found != null)
                return found;
        }

        throw new LaunchException(
            $"Startup project '{startupName}' was not found in the solution.");
    }

    private static Project? FindInProject(Project project, string uniqueName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        // Solution folder — recurse into children.
        if (project.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.SubProject == null) continue;
                var found = FindInProject(item.SubProject, uniqueName);
                if (found != null) return found;
            }
            return null;
        }

        return string.Equals(project.UniqueName, uniqueName, StringComparison.OrdinalIgnoreCase)
               ? project
               : null;
    }

    // ── Static helpers ───────────────────────────────────────────────────────

    private static string WriteTempJson(string json, string sshHost)
    {
        var name = $"ssh_remote_attach_{SanitizeFileName(sshHost.Replace('.', '_'))}_{Process.GetCurrentProcess().Id}.json";
        var path = Path.Combine(Path.GetTempPath(), name);
        File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        return path;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    private Task<bool> ShowErrorAsync(string message, CancellationToken ct)
        => Extensibility.Shell().ShowPromptAsync(message, PromptOptions.OKCancel, ct);
}
