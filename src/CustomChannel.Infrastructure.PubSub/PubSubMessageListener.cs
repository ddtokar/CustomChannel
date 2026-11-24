using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomChannel.Infrastructure.PubSub;

/// <summary>
/// Owns the single <see cref="SubscriberClient"/>: creates it, starts listening,
/// sends Ack/Nack and stops and disposes it on shutdown.
/// </summary>
internal sealed class PubSubMessageListener(
    PubSubSubscriberClientFactory clientFactory,
    IOptions<PubSubOptions> options,
    ILogger<PubSubMessageListener> logger) : IPubSubMessageListener
{
    // How long a graceful stop may wait for in-flight handlers and pending Acks.
    private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(10);

    public async Task ListenAsync(
        Func<PubSubIncomingMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(handler);

        PubSubOptions settings = options.Value;
        string subscription = $"projects/{settings.ProjectId}/subscriptions/{settings.SubscriptionId}";

        SubscriberClient client;
        try
        {
            client = await clientFactory.CreateAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex,
                "Failed to create Pub/Sub subscriber for {Subscription}. " +
                "Check Application Default Credentials (gcloud auth application-default login).",
                subscription);
            throw;
        }

        try
        {
            var listening = client.StartAsync(
                (message, messageToken) => HandleAsync(message, handler, messageToken));

            logger.LogInformation("Listening to Pub/Sub subscription {Subscription}", subscription);

            // Wait until the host is stopping or the subscriber fails on its own.
            await Task.WhenAny(listening, Task.Delay(Timeout.Infinite, cancellationToken));

            if (!listening.IsCompleted)
            {
                logger.LogInformation("Stopping Pub/Sub subscriber for {Subscription}", subscription);
                // WaitForProcessing (default mode): finish in-flight handlers and send their Acks,
                // then hard-stop after StopTimeout.
                await client.StopAsync(
                    new SubscriberClient.ShutdownOptions { Timeout = StopTimeout },
                    CancellationToken.None);
            }

            // Surfaces connection/permission errors (e.g. NotFound, PermissionDenied).
            await listening;

            logger.LogInformation("Pub/Sub subscriber for {Subscription} stopped", subscription);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Pub/Sub subscriber for {Subscription} did not stop within {Timeout}; " +
                "unacknowledged messages will be redelivered.",
                subscription, StopTimeout);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pub/Sub subscriber for {Subscription} failed", subscription);
            throw;
        }
        finally
        {
            await DisposeClientAsync(client, subscription);
        }
    }

    private async Task<SubscriberClient.Reply> HandleAsync(
        PubsubMessage message,
        Func<PubSubIncomingMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var incoming = new PubSubIncomingMessage(message.MessageId, message.Data.ToStringUtf8());
            await handler(incoming, cancellationToken);
            return SubscriberClient.Reply.Ack;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Handling Pub/Sub message {MessageId} failed; it is not acknowledged and will be redelivered",
                message.MessageId);
            return SubscriberClient.Reply.Nack;
        }
    }

    private async Task DisposeClientAsync(SubscriberClient client, string subscription)
    {
        try
        {
            await client.DisposeAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error while disposing Pub/Sub subscriber for {Subscription}", subscription);
        }
    }
}
