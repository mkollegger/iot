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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mks.SshRemoteAttach.Extension.Core;

/// <summary>
///     Builds the JSON document consumed by <c>DebugAdapterHost.Launch /LaunchJson:…</c>.
/// </summary>
/// <remarks>
///     Produces the same content as the user's hand-crafted <c>attach_vs202x.json</c>:
///     https://github.com/mkollegger/iot/raw/refs/heads/main/samples/hellopi/attach_vs202x.json
/// </remarks>
internal static class AttachJsonBuilder
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    #region Nested Types

    // ── JSON model ──────────────────────────────────────────────────────────

    private sealed class AttachDoc
    {
        #region Properties

        [JsonPropertyName("version")]
        public string Version { get; } = "0.2.0";

        [JsonPropertyName("adapter")]
        public string Adapter { get; set; } = "";

        [JsonPropertyName("adapterArgs")]
        public string AdapterArgs { get; set; } = "";

        [JsonPropertyName("languageMappings")]
        public Dictionary<string, LangMapping> LanguageMappings { get; set; } = [];

        [JsonPropertyName("exceptionCategoryMappings")]
        public Dictionary<string, string> ExceptionCategoryMappings { get; set; } = [];

        [JsonPropertyName("configurations")]
        public LaunchConfig[] Configurations { get; set; } = [];

        #endregion
    }

    private sealed class LangMapping
    {
        #region Properties

        [JsonPropertyName("languageId")]
        public string LanguageId { get; set; } = "";

        [JsonPropertyName("extensions")]
        public string[] Extensions { get; set; } = [];

        #endregion
    }

    private sealed class LaunchConfig
    {
        #region Properties

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("request")]
        public string Request { get; set; } = "";

        [JsonPropertyName("cwd")]
        public string Cwd { get; set; } = "";

        [JsonPropertyName("program")]
        public string Program { get; set; } = "";

        [JsonPropertyName("args")]
        public string[] Args { get; set; } = [];

        [JsonPropertyName("console")]
        public string Console { get; set; } = "";

        #endregion
    }

    #endregion

    /// <summary>
    ///     Builds the adapter JSON from a resolved SSH profile.
    /// </summary>
    public static string Build(SshRemoteAttachProfile profile)
    {
        if (profile is null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        var doc = new AttachDoc
        {
            Adapter = profile.SshExecutable,
            AdapterArgs = BuildAdapterArgs(profile),
            LanguageMappings = new Dictionary<string, LangMapping>
            {
                ["C#"] = new() {LanguageId = "3F5162F8-07C6-11D3-9053-00C04FA302A1", Extensions = ["*"]},
            },
            ExceptionCategoryMappings = new Dictionary<string, string>
            {
                ["CLR"] = "449EC4CC-30D2-4032-9256-EE18EB41B62B",
                ["MDA"] = "6ECE07A9-0EDE-45C4-8296-818D8FC401D4",
            },
            Configurations =
            [
                new LaunchConfig
                {
                    Name = ".NET Core Launch",
                    Type = "coreclr",
                    Request = "launch",
                    Cwd = profile.RemoteWorkingDirectory,
                    Program = profile.RemoteDotnetPath,
                    Args = BuildAppArgs(profile),
                    Console = "internalTerminal",
                },
            ],
        };
        return JsonSerializer.Serialize(doc, Options);
    }

    /// <summary>
    ///     Builds the SSH adapter argument string: <c>[user@]host [-i key] -T [sudo] vsdbg --interpreter=vscode</c>
    /// </summary>
    internal static string BuildAdapterArgs(SshRemoteAttachProfile profile)
    {
        if (profile is null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        var sb = new StringBuilder();
        sb.Append(FormattableString.Invariant($"{profile.SshUser}@{profile.SshHost}"));

        if (!string.IsNullOrEmpty(profile.SshIdentityFile))
        {
            sb.Append(FormattableString.Invariant($" -i \"{profile.SshIdentityFile}\""));
        }

        sb.Append(" -T ");
        if (profile.UseSudo)
        {
            sb.Append("sudo ");
        }

        sb.Append(FormattableString.Invariant($"{profile.RemoteVsDbgPath} --interpreter=vscode"));
        return sb.ToString();
    }

    /// <summary>
    ///     Builds the application argument array: <c>["workdir/app.dll", …extra]</c>
    /// </summary>
    internal static string[] BuildAppArgs(SshRemoteAttachProfile profile)
    {
        if (profile is null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        var dll = $"{profile.RemoteWorkingDirectory.TrimEnd('/')}/{profile.RemoteAppDll}";

        if (string.IsNullOrWhiteSpace(profile.RemoteAppArgs))
        {
            return [dll];
        }

        if (profile.RemoteAppArgs == null)
        {
            return [dll];
        }

        var extra = profile.RemoteAppArgs.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        var result = new string[1 + extra.Length];
        result[0] = dll;
        extra.CopyTo(result, 1);
        return result;
    }
}