namespace SupportDesk.Application.Abstract.Messaging;

/// <summary>
/// Provides an abstraction for broadcasting real-time push notifications to clients.
/// </summary>
public interface IRealtimePublisher
{
    /// <summary>
    /// Publishes a real-time event payload to a targeted group across the distributed system.
    /// </summary>
    /// <param name="hubType">The target destination.</param>
    /// <param name="targetGroup">The identifier of the target group.</param>
    /// <param name="action">The client-side event name to invoke.</param>
    /// <param name="payload">The data object to serialize and deliver to the client.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishAsync(RealtimeHubType hubType, string targetGroup, string action, object payload, CancellationToken cancellationToken = default);
}