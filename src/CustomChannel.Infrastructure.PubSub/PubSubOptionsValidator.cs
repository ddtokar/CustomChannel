using Microsoft.Extensions.Options;

namespace CustomChannel.Infrastructure.PubSub;

/// <summary>
/// Reports every missing required setting by its full configuration key.
/// </summary>
internal sealed class PubSubOptionsValidator : IValidateOptions<PubSubOptions>
{
    public ValidateOptionsResult Validate(string? name, PubSubOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ProjectId))
        {
            failures.Add(MissingKey(nameof(PubSubOptions.ProjectId)));
        }

        if (string.IsNullOrWhiteSpace(options.SubscriptionId))
        {
            failures.Add(MissingKey(nameof(PubSubOptions.SubscriptionId)));
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static string MissingKey(string property) =>
        $"Required configuration key '{PubSubOptions.SectionName}:{property}' is missing or empty. " +
        $"Set it in appsettings.Development.json (not committed) or the environment variable " +
        $"'{PubSubOptions.SectionName}__{property}'.";
}
