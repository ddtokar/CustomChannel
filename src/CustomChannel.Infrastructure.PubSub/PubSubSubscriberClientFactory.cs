using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;

namespace CustomChannel.Infrastructure.PubSub;

/// <summary>
/// Creates the Pull-subscription client on demand. Registering this factory does not
/// contact Google Cloud; the client is built only when <see cref="CreateAsync"/> is awaited.
/// </summary>
internal sealed class PubSubSubscriberClientFactory(IOptions<PubSubOptions> options)
{
    public Task<SubscriberClient> CreateAsync(CancellationToken cancellationToken = default)
    {
        PubSubOptions settings = options.Value;

        var builder = new SubscriberClientBuilder
        {
            SubscriptionName = SubscriptionName.FromProjectSubscription(
                settings.ProjectId,
                settings.SubscriptionId),
            // No explicit credentials: Application Default Credentials are used.
        };

        return builder.BuildAsync(cancellationToken);
    }
}
