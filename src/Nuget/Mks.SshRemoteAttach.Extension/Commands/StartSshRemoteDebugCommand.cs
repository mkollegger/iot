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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Mks.SshRemoteAttach.Extension.Core;
using Mks.SshRemoteAttach.Extension.Services;
using Command = Microsoft.VisualStudio.Extensibility.Commands.Command;
using Process = System.Diagnostics.Process;

namespace Mks.SshRemoteAttach.Extension.Commands;

/// <summary>
///     Menu command that reads <c>launchSettings.json</c>, optionally deploys the build output,
///     and starts a remote debug session via <c>DebugAdapterHost.Launch</c>.
/// </summary>
/// <remarks>
///     Mirrors the manual workflow:
///     <code>
/// DebugAdapterHost.Launch /LaunchJson:attach_vs202x.json
/// </code>
/// </remarks>
[VisualStudioContribution]
internal sealed class StartSshRemoteDebugCommand : Command
{
    private readonly IDeploymentService _deployment;
    private readonly AsyncServiceProviderInjection<DTE, DTE2> _dteInjection;
    private readonly LaunchSettingsReader _reader;
    private readonly SelectedProfileService _selected;

    public StartSshRemoteDebugCommand(
        VisualStudioExtensibility extensibility,
        AsyncServiceProviderInjection<DTE, DTE2> dteInjection,
        LaunchSettingsReader reader,
        IDeploymentService deployment,
        SelectedProfileService selected)
        : base(extensibility)
    {
        _dteInjection = dteInjection ?? throw new ArgumentNullException(nameof(dteInjection));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _deployment = deployment ?? throw new ArgumentNullException(nameof(deployment));
        _selected = selected ?? throw new ArgumentNullException(nameof(selected));

        _selected.Changed += (_, _) => DisplayName = _selected.SelectedProfileName;
        DisplayName = _selected.SelectedProfileName;
        _ = InitializeSelectedProfileAsync(extensibility);
    }

    #region Properties

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("Start SSH Remote Debug")
    {
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        EnabledWhen = ActivationConstraint.SolutionState(SolutionState.Exists),
        Icon = new(ImageMoniker.KnownValues.Run, IconSettings.IconAndText),
    };

    #endregion

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await _selected.EnsureLoadedAsync(Extensibility, cancellationToken).ConfigureAwait(false);

        // ── 1. Acquire DTE and switch to the UI thread ───────────────────────
        var dte = await _dteInjection.GetServiceAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        // ── 2. Resolve paths from the running solution (UI thread) ───────────
        string launchSettingsPath;
        string outputDir;
        try
        {
            var startupProject = FindStartupProject(dte);
            launchSettingsPath = FindLaunchSettingsPath(dte);
            outputDir = ResolveOutputDirectory(dte);
            BuildStartupProject(dte, startupProject.UniqueName);
        }
        catch (LaunchException ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
            return;
        }

        // ── 3. Read profiles and deploy (thread pool) ────────────────────────
        string tempPath;

        try
        {
            var profiles = _reader.ReadProfiles(launchSettingsPath);

            if (profiles.Count == 0)
            {
                await ShowErrorAsync(
                    "No SshRemoteAttach profile found in launchSettings.json.\n" +
                    "Add a profile with commandName: \"SshRemoteAttach\".",
                    cancellationToken);
                return;
            }

            var profile = _selected.ResolveSelected(profiles);
            await _selected.PersistAsync(Extensibility, cancellationToken).ConfigureAwait(false);

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

    private async Task InitializeSelectedProfileAsync(VisualStudioExtensibility extensibility)
    {
        try
        {
            await _selected.EnsureLoadedAsync(extensibility, CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Ignore restore errors; fallback profile selection still works.
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
            var projectDir = Path.GetDirectoryName(startupProject.FullName) ?? string.Empty;

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
        {
            throw new LaunchException(
                "No startup project is set. Right-click a project and choose 'Set as Startup Project'.");
        }

        var startupName = startups[0] as string
            ?? throw new LaunchException("Could not determine the startup project name.");

        // Enumerate top-level and solution-folder items.
        foreach (Project p in dte.Solution.Projects)
        {
            var found = FindInProject(p, startupName);
            if (found != null)
            {
                return found;
            }
        }

        throw new LaunchException(
            $"Startup project '{startupName}' was not found in the solution.");
    }

    private static void BuildStartupProject(DTE2 dte, string startupProjectUniqueName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var solutionBuild = dte.Solution?.SolutionBuild
            ?? throw new LaunchException("Could not access solution build services.");

        var activeConfigurationName = solutionBuild.ActiveConfiguration?.Name;
        if (string.IsNullOrWhiteSpace(activeConfigurationName))
        {
            throw new LaunchException("Could not determine the active solution configuration.");
        }

        solutionBuild.BuildProject(activeConfigurationName, startupProjectUniqueName, true);

        if (solutionBuild.LastBuildInfo != 0)
        {
            throw new LaunchException("Build failed. Fix build errors and try again.");
        }
    }

    private static Project? FindInProject(Project project, string uniqueName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        // Solution folder — recurse into children.
        if (project.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.SubProject == null)
                {
                    continue;
                }

                var found = FindInProject(item.SubProject, uniqueName);
                if (found != null)
                {
                    return found;
                }
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
        File.WriteAllText(path, json, Encoding.UTF8);
        return path;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name;
    }

    private Task<bool> ShowErrorAsync(string message, CancellationToken ct)
        => Extensibility.Shell().ShowPromptAsync(message, PromptOptions.OKCancel, ct);
}