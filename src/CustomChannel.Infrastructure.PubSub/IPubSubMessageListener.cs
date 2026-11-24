namespace CustomChannel.Infrastructure.PubSub;

/// <summary>
/// Reads messages from the configured Pull subscription.
/// </summary>
public interface IPubSubMessageListener
{
    /// <summary>
    /// Listens until <paramref name="cancellationToken"/> is cancelled or an unrecoverable error occurs.
    /// A message is acknowledged only after <paramref name="handler"/> completes successfully;
    /// if the handler throws, the message is not acknowledged and will be redelivered.
    /// </summary>
    Task ListenAsync(
        Func<PubSubIncomingMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken);
}
