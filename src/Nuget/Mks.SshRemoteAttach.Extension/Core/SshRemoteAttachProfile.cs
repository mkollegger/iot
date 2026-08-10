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

namespace Mks.SshRemoteAttach.Extension.Core;

/// <summary>
///     Strongly-typed view over one <c>launchSettings.json</c> profile whose
///     <c>commandName</c> is <c>"SshRemoteAttach"</c>.
/// </summary>
/// <remarks>
///     Properties map to camelCase keys in <c>launchSettings.json</c>.
///     Missing required keys throw <see cref="ArgumentException" /> with the key name,
///     never <see cref="NullReferenceException" />.
/// </remarks>
public class SshRemoteAttachProfile
{
    #region Properties

    // ── Required ─────────────────────────────────────────────────────────────

    /// <summary>Name of this profile (the JSON key).</summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>Hostname or IP of the remote target.</summary>
    public string SshHost { get; set; } = string.Empty;

    /// <summary>SSH login user on the remote target.</summary>
    public string SshUser { get; set; } = string.Empty;

    /// <summary>Absolute remote path to the <c>vsdbg</c> binary.</summary>
    public string RemoteVsDbgPath { get; set; } = string.Empty;

    /// <summary>Absolute remote path to the <c>dotnet</c> binary.</summary>
    public string RemoteDotnetPath { get; set; } = string.Empty;

    /// <summary>Remote working directory containing the application DLL.</summary>
    public string RemoteWorkingDirectory { get; set; } = string.Empty;

    /// <summary>Filename of the main DLL (not full path).</summary>
    public string RemoteAppDll { get; set; } = string.Empty;

    // ── Optional ─────────────────────────────────────────────────────────────

    /// <summary>SSH identity file path (<c>-i</c>); <see langword="null" /> to omit.</summary>
    public string? SshIdentityFile { get; set; }

    /// <summary>Path to SSH client on Windows.</summary>
    public string SshExecutable { get; set; } = @"C:\Windows\System32\OpenSSH\ssh.exe";

    /// <summary>Prefix vsdbg invocation with <c>sudo</c>.</summary>
    public bool UseSudo { get; set; }

    /// <summary>Extra args passed to the remote app; <see langword="null" /> if none.</summary>
    public string? RemoteAppArgs { get; set; }

    /// <summary>UNC path of a local SMB mount mirroring <see cref="RemoteWorkingDirectory" />.</summary>
    public string? DeployLocalShare { get; set; }

    /// <summary>Copy MSBuild output dir to <see cref="DeployLocalShare" />.</summary>
    public bool DeployFromOutputDir { get; set; } = true;

    /// <summary>Run deployment before launching the debugger.</summary>
    public bool DeployBeforeLaunch { get; set; } = true;

    #endregion

    // ── Factory ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Parses a profile from a raw key/value dictionary (the parsed JSON object
    ///     from <c>launchSettings.json</c>).
    /// </summary>
    /// <param name="profileName">The JSON key used for this profile.</param>
    /// <param name="raw">Deserialized profile properties.</param>
    /// <exception cref="ArgumentException">Thrown when a required property is missing or empty.</exception>
    public static SshRemoteAttachProfile Parse(string profileName, IReadOnlyDictionary<string, object?> raw)
    {
        return new SshRemoteAttachProfile
        {
            ProfileName = profileName,
            SshHost = Require(raw, "sshHost", profileName),
            SshUser = Require(raw, "sshUser", profileName),
            RemoteVsDbgPath = Require(raw, "remoteVsDbgPath", profileName),
            RemoteDotnetPath = Require(raw, "remoteDotnetPath", profileName),
            RemoteWorkingDirectory = Require(raw, "remoteWorkingDirectory", profileName),
            RemoteAppDll = Require(raw, "remoteAppDll", profileName),

            SshIdentityFile = Optional(raw, "sshIdentityFile"),
            SshExecutable = Optional(raw, "sshExecutable")
                ?? @"C:\Windows\System32\OpenSSH\ssh.exe",
            UseSudo = OptionalBool(raw, "useSudo"),
            RemoteAppArgs = Optional(raw, "remoteAppArgs"),
            DeployLocalShare = Optional(raw, "deployLocalShare"),
            DeployFromOutputDir = OptionalBool(raw, "deployFromOutputDir", true),
            DeployBeforeLaunch = OptionalBool(raw, "deployBeforeLaunch", true),
        };
    }

    private static string Require(IReadOnlyDictionary<string, object?> d, string key, string profileName)
    {
        if (d.TryGetValue(key, out var v) && v?.ToString() is { } s && !string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        throw new ArgumentException(
            $"Profile '{profileName}' (commandName=SshRemoteAttach): required property '{key}' is missing or empty.",
            key);
    }

    private static string? Optional(IReadOnlyDictionary<string, object?> d, string key)
        => d.TryGetValue(key, out var v) ? v?.ToString() : null;

    private static bool OptionalBool(IReadOnlyDictionary<string, object?> d, string key, bool defaultValue = false)
    {
        if (!d.TryGetValue(key, out var v))
        {
            return defaultValue;
        }

        if (v is bool b)
        {
            return b;
        }

        if (bool.TryParse(v?.ToString(), out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }
}