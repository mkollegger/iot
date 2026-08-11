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

using Microsoft.VisualStudio.Extensibility;
using Mks.SshRemoteAttach.Extension.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mks.SshRemoteAttach.Extension.Core;

internal sealed class SelectedProfileService
{
    private const string PersistedProfileMoniker = "Mks.SshRemoteAttach.SelectedProfileName";

    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private bool _isLoaded;
    private string? _selectedProfileName;

    #region Properties

    public string? SelectedProfileName
    {
        get => _selectedProfileName;
        set
        {
            if (string.Equals(_selectedProfileName, value, StringComparison.Ordinal))
            {
                return;
            }

            _selectedProfileName = value;
            Changed?.Invoke(this, EventArgs.Empty);

        }
    }

    #endregion

    public event EventHandler? Changed;

    public async Task EnsureLoadedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        if (_isLoaded)
        {
            return;
        }

        await _loadGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_isLoaded)
            {
                return;
            }

            var persisted = await extensibility.Configuration()
                .GetPersistedStateAsync<string>(PersistedProfileMoniker, null, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(persisted))
            {
                SelectedProfileName = persisted;
            }

            _isLoaded = true;
        }
        finally
        {
            _loadGate.Release();
        }
    }

    public Task PersistAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        return extensibility.Configuration()
            .WritePersistedStateAsync(PersistedProfileMoniker, _selectedProfileName ?? string.Empty, cancellationToken);
    }

    public SshRemoteAttachProfile ResolveSelected(IReadOnlyList<SshRemoteAttachProfile> profiles)
    {
        if (profiles is null)
        {
            throw new ArgumentNullException(nameof(profiles));
        }

        if (profiles.Count == 0)
        {
            throw new ArgumentException("At least one profile is required.", nameof(profiles));
        }

        if (!string.IsNullOrWhiteSpace(_selectedProfileName))
        {
            foreach (var profile in profiles)
            {
                if (string.Equals(profile.ProfileName, _selectedProfileName, StringComparison.Ordinal))
                {
                    return profile;
                }
            }
        }

        var selected = profiles[0];
        SelectedProfileName = selected.ProfileName;
        return selected;
    }
}