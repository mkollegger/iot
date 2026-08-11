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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Mks.SshRemoteAttach.Extension.Commands;

namespace Mks.SshRemoteAttach.Extension.Core;

/// <summary>
///     Reads and parses <c>Properties/launchSettings.json</c>, returning all profiles whose
///     <c>commandName</c> equals <c>"SshRemoteAttach"</c>.
/// </summary>
internal sealed class LaunchSettingsReader
{
    private readonly StartSshRemoteDebugCommand _startSshRemoteDebugCommand;
    private readonly SelectedProfileService _selectedProfileService;
    private const string CommandName = "SshRemoteAttach";
    
    //public LaunchSettingsReader(SelectedProfileService selectedProfileService, StartSshRemoteDebugCommand startSshRemoteDebugCommand)
    //{
    //    Debugger.Break();


    //    _selectedProfileService = selectedProfileService ?? throw new ArgumentNullException(nameof(selectedProfileService));
    //    _startSshRemoteDebugCommand = startSshRemoteDebugCommand ?? throw new ArgumentNullException(nameof(startSshRemoteDebugCommand));
    //}

    /// <summary>
    ///     Parses all <c>SshRemoteAttach</c> profiles from the file at <paramref name="launchSettingsPath" />.
    /// </summary>
    /// <exception cref="LaunchException">Thrown when the file is missing, unreadable, or contains invalid JSON.</exception>
    public IReadOnlyList<SshRemoteAttachProfile> ReadProfiles(string launchSettingsPath)
    {
        if (!File.Exists(launchSettingsPath))
        {
            throw new LaunchException(
                $"launchSettings.json not found: '{launchSettingsPath}'.\n" +
                "Add a profile with commandName: \"SshRemoteAttach\" to Properties/launchSettings.json.");
        }

        string json;
        try
        {
            json = File.ReadAllText(launchSettingsPath, Encoding.UTF8);
        }
        catch (IOException ex)
        {
            throw new LaunchException($"Could not read '{launchSettingsPath}': {ex.Message}", ex);
        }

        var r = ParseProfiles(json, launchSettingsPath);
        return r;
    }

    // ── Parsing ──────────────────────────────────────────────────────────────

    private static IReadOnlyList<SshRemoteAttachProfile> ParseProfiles(string json, string filePath)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new LaunchException($"Invalid JSON in '{filePath}': {ex.Message}", ex);
        }

        using (doc)
        {
            var results = new List<SshRemoteAttachProfile>();

            if (!doc.RootElement.TryGetProperty("profiles", out var profilesElement))
            {
                return results;
            }

            foreach (var entry in profilesElement.EnumerateObject())
            {
                var el = entry.Value;

                if (!el.TryGetProperty("commandName", out var cmdEl))
                {
                    continue;
                }

                if (!CommandName.Equals(cmdEl.GetString(), StringComparison.Ordinal))
                {
                    continue;
                }

                var raw = ToRawDict(el);
                try
                {
                    results.Add(SshRemoteAttachProfile.Parse(entry.Name, raw));
                }
                catch (ArgumentException ex)
                {
                    throw new LaunchException(ex.Message, ex);
                }
            }

            return results;
        }
    }

    private static Dictionary<string, object?> ToRawDict(JsonElement element)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Number => prop.Value.TryGetInt64(out var l)
                    ? (object?) l
                    : prop.Value.GetDouble(),
                _ => prop.Value.GetString(),
            };
        }

        return dict;
    }
}