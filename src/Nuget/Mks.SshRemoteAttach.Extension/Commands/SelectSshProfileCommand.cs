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
using System.Collections.Generic;
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
using Mks.SshRemoteAttach.Extension.Core;
using Command = Microsoft.VisualStudio.Extensibility.Commands.Command;

namespace Mks.SshRemoteAttach.Extension.Commands;

[VisualStudioContribution]
internal sealed class SelectSshProfileCommand : Command
{
    private readonly AsyncServiceProviderInjection<DTE, DTE2> _dteInjection;
    private readonly LaunchSettingsReader _reader;
    private readonly SelectedProfileService _selected;

    public SelectSshProfileCommand(
        VisualStudioExtensibility extensibility,
        AsyncServiceProviderInjection<DTE, DTE2> dteInjection,
        LaunchSettingsReader reader,
        SelectedProfileService selected)
        : base(extensibility)
    {
        _dteInjection = dteInjection ?? throw new ArgumentNullException(nameof(dteInjection));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _selected = selected ?? throw new ArgumentNullException(nameof(selected));
    }

    #region Properties

    public override CommandConfiguration CommandConfiguration => new("Select SSH Profile...")
    {
        EnabledWhen = ActivationConstraint.SolutionState(SolutionState.Exists),
    };

    #endregion

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var dte = await _dteInjection.GetServiceAsync();
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        IReadOnlyList<SshRemoteAttachProfile> profiles;
        try
        {
            var launchSettingsPath = FindLaunchSettingsPath(dte);
            profiles = _reader.ReadProfiles(launchSettingsPath);
        }
        catch (LaunchException ex)
        {
            await ShowErrorAsync(ex.Message, cancellationToken);
            return;
        }

        if (profiles.Count == 0)
        {
            await ShowErrorAsync(
                "No SshRemoteAttach profile found in launchSettings.json.\n" +
                "Add a profile with commandName: \"SshRemoteAttach\".",
                cancellationToken);
            return;
        }

        if (profiles.Count == 1)
        {
            _selected.SelectedProfileName = profiles[0].ProfileName;
            return;
        }

        var choices = new ChoiceResultCollection<int>();
        var defaultIndex = 0;

        for (var i = 0; i < profiles.Count; i++)
        {
            var profile = profiles[i];
            choices.Add(new ChoiceDescription(profile.ProfileName), i);
            if (string.Equals(_selected.SelectedProfileName, profile.ProfileName, StringComparison.Ordinal))
            {
                defaultIndex = i;
            }
        }

        var promptOptions = new PromptOptions<int>(
            choices,
            defaultIndex,
            -1);
        
        var selectedIndex = await Extensibility.Shell().ShowPromptAsync(
            "Select the SSH profile used for build, deploy, and remote debug.",
            promptOptions,
            cancellationToken);

        if (selectedIndex >= 0 && selectedIndex < profiles.Count)
        {
            _selected.SelectedProfileName = profiles[selectedIndex].ProfileName;
        }
    }

    private static string FindLaunchSettingsPath(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var startupProject = FindStartupProject(dte);

        var projectDir = Path.GetDirectoryName(startupProject.FullName)
            ?? throw new LaunchException(
                $"Could not determine directory of '{startupProject.FullName}'.");

        return Path.Combine(projectDir, "Properties", "launchSettings.json");
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

    private static Project? FindInProject(Project project, string uniqueName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

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

    private Task<bool> ShowErrorAsync(string message, CancellationToken ct)
        => Extensibility.Shell().ShowPromptAsync(message, PromptOptions.OKCancel, ct);
}