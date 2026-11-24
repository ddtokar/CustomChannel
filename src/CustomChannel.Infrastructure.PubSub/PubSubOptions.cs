namespace CustomChannel.Infrastructure.PubSub;

/// <summary>
/// Settings bound from the "PubSub" configuration section.
/// </summary>
internal sealed class PubSubOptions
{
    public const string SectionName = "PubSub";

    public string ProjectId { get; set; } = string.Empty;

    public string SubscriptionId { get; set; } = string.Empty;
}
