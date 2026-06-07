using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using SshRemoteAttach.Core;
using SshRemoteAttach.Deployment;

namespace SshRemoteAttach;

/// <summary>
/// Extension entry point — registers services and marks the extension as in-process.
/// </summary>
/// <remarks>
/// <para>
/// <c>RequiresInProcessHosting = true</c> is necessary because the extension calls DTE
/// (to invoke <c>DebugAdapterHost.Launch</c>), which is only accessible in-process.
/// </para>
/// <para>
/// Pattern: https://github.com/microsoft/VSExtensibility/blob/main/New_Extensibility_Model/Samples/CompositeExtension/CompositeExtension/InProcExtensionEntrypoint.cs
/// </para>
/// </remarks>
[VisualStudioContribution]
internal sealed class SshRemoteAttachExtension : Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        RequiresInProcessHosting = true,
    };

    /// <inheritdoc/>
    protected override void InitializeServices(IServiceCollection services)
    {
        base.InitializeServices(services);

        // Register application services so they can be injected into commands.
        services.AddSingleton<LaunchSettingsReader>();
        services.AddSingleton<IDeploymentService, ShareDeploymentService>();
    }
}
