namespace CustomChannel.Infrastructure.PubSub;

/// <summary>
/// A received Pub/Sub message expressed with plain .NET types only.
/// </summary>
/// <param name="MessageId">Server-assigned Pub/Sub message ID.</param>
/// <param name="Text">Message payload decoded as UTF-8.</param>
public sealed record PubSubIncomingMessage(string MessageId, string Text);
