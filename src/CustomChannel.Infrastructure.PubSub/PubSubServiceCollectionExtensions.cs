using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomChannel.Infrastructure.PubSub;

public static class PubSubServiceCollectionExtensions
{
    /// <summary>
    /// Registers Google Cloud Pub/Sub settings, the Pull-subscription client factory and
    /// <see cref="IPubSubMessageListener"/>. Settings are validated when the host starts;
    /// the client is created only when <see cref="IPubSubMessageListener.ListenAsync"/> runs.
    /// </summary>
    public static IServiceCollection AddGooglePubSub(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<PubSubOptions>()
            .Bind(configuration.GetSection(PubSubOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<PubSubOptions>, PubSubOptionsValidator>();
        services.AddSingleton<PubSubSubscriberClientFactory>();
        services.AddSingleton<IPubSubMessageListener, PubSubMessageListener>();

        return services;
    }
}
