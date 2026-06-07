using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SshRemoteAttach.Core;

/// <summary>
/// Reads and parses <c>Properties/launchSettings.json</c>, returning all profiles whose
/// <c>commandName</c> equals <c>"SshRemoteAttach"</c>.
/// </summary>
internal sealed class LaunchSettingsReader
{
    private const string CommandName = "SshRemoteAttach";

    /// <summary>
    /// Parses all <c>SshRemoteAttach</c> profiles from the file at <paramref name="launchSettingsPath"/>.
    /// </summary>
    /// <exception cref="LaunchException">Thrown when the file is missing, unreadable, or contains invalid JSON.</exception>
    public IReadOnlyList<SshRemoteAttachProfile> ReadProfiles(string launchSettingsPath)
    {
        if (!File.Exists(launchSettingsPath))
            throw new LaunchException(
                $"launchSettings.json not found: '{launchSettingsPath}'.\n" +
                "Add a profile with commandName: \"SshRemoteAttach\" to Properties/launchSettings.json.");

        string json;
        try
        {
            json = File.ReadAllText(launchSettingsPath, System.Text.Encoding.UTF8);
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
                return results;

            foreach (var entry in profilesElement.EnumerateObject())
            {
                var el = entry.Value;

                if (!el.TryGetProperty("commandName", out var cmdEl))
                    continue;
                if (!CommandName.Equals(cmdEl.GetString(), StringComparison.Ordinal))
                    continue;

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
                JsonValueKind.True   => (object?)true,
                JsonValueKind.False  => false,
                JsonValueKind.Null   => null,
                JsonValueKind.Number => prop.Value.TryGetInt64(out var l)
                                        ? (object?)l
                                        : prop.Value.GetDouble(),
                _                    => prop.Value.GetString(),
            };
        }
        return dict;
    }
}
