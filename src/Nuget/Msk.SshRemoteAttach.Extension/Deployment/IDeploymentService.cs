using System.Threading;
using System.Threading.Tasks;
using SshRemoteAttach.Core;

namespace SshRemoteAttach.Deployment;

/// <summary>
/// Copies the project build output to a local SMB mount that mirrors the remote working directory.
/// </summary>
internal interface IDeploymentService
{
    /// <summary>
    /// Copies <paramref name="outputDirectory"/> to <see cref="SshRemoteAttachProfile.DeployLocalShare"/>.
    /// No-op if <see cref="SshRemoteAttachProfile.DeployBeforeLaunch"/> is false or
    /// <see cref="SshRemoteAttachProfile.DeployLocalShare"/> is null.
    /// </summary>
    /// <exception cref="LaunchException">Thrown on deployment failure.</exception>
    Task DeployAsync(SshRemoteAttachProfile profile, string outputDirectory, CancellationToken cancellationToken = default);
}
