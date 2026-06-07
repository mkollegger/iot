using System;

namespace SshRemoteAttach.Core;

/// <summary>
/// Raised when the launch sequence fails with a user-actionable message.
/// </summary>
internal sealed class LaunchException : Exception
{
    public LaunchException(string message) : base(message) { }

    public LaunchException(string message, Exception inner) : base(message, inner) { }
}
