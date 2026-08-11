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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Extensibility;
using Mks.SshRemoteAttach.Extension.Core;
using Mks.SshRemoteAttach.Extension.Services;

namespace Mks.SshRemoteAttach.Extension;

/// <summary>
///     Extension entry point — registers services and marks the extension as in-process.
/// </summary>
/// <remarks>
///     <para>
///         <c>RequiresInProcessHosting = true</c> is necessary because the extension calls DTE
///         (to invoke <c>DebugAdapterHost.Launch</c>), which is only accessible in-process.
///     </para>
///     <para>
///         Pattern:
///         https://github.com/microsoft/VSExtensibility/blob/main/New_Extensibility_Model/Samples/CompositeExtension/CompositeExtension/InProcExtensionEntrypoint.cs
///     </para>
/// </remarks>
[VisualStudioContribution]
internal sealed class SshRemoteAttachExtension : Microsoft.VisualStudio.Extensibility.Extension
{
    #region Properties

    /// <inheritdoc />
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        RequiresInProcessHosting = true,
    };

    #endregion

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection services)
    {
        base.InitializeServices(services);

        // Register application services so they can be injected into commands.
        services.AddSingleton<SelectedProfileService>();
        services.AddSingleton<LaunchSettingsReader>();
        services.AddSingleton<IDeploymentService, ShareDeploymentService>();
        
    }

}