using CustomChannel.Infrastructure.PubSub;

namespace CustomChannel.Worker;

/// <summary>
/// Starts reading the Pub/Sub subscription when the host starts and logs every message.
/// Acknowledgement is handled by the adapter after this handler completes.
/// </summary>
internal sealed class PubSubListenerService(
    IPubSubMessageListener listener,
    ILogger<PubSubListenerService> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        listener.ListenAsync(HandleMessageAsync, stoppingToken);

    private Task HandleMessageAsync(PubSubIncomingMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received Pub/Sub message {MessageId}: {Text}",
            message.MessageId,
            message.Text);

        return Task.CompletedTask;
    }
}
