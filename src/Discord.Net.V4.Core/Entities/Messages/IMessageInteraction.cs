using Discord.Models;

namespace Discord;

/// <summary>
///     Represents a partial interaction within a message.
/// </summary>
public interface IMessageInteractionMetadata :
    ISnowflakeEntity<IMessageInteractionMetadataModel>,
    IUpdatable<IMessageInteractionMetadataModel>
{
    /// <summary>
    ///     Gets the type of the interaction.
    /// </summary>
    InteractionType Type { get; }

    /// <summary>
    ///     Gets the <see cref="IUser" /> who invoked the interaction.
    /// </summary>
    IUserActor User { get; }

    IReadOnlyDictionary<ApplicationIntegrationType, ulong> AuthorizingIntegrationOwners { get; }

    IMessageActor? OriginalResponseMessage { get; }
    IMessageActor? InteractedMessage { get; }
    IMessageInteractionMetadata? TriggeringInteractionMetadata { get; }
}