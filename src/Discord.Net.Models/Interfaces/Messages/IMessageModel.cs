using Discord.Models.Json;

namespace Discord.Models;

[ModelEquality]
public partial interface IMessageModel : IEntityModel<ulong>
{
    ulong AuthorId { get; }
    ulong ChannelId { get; }
    ulong? WebhookId { get; }
    string? Content { get; }
    DateTimeOffset Timestamp { get; }
    DateTimeOffset? EditedTimestamp { get; }
    bool IsTTS { get; }
    bool MentionsEveryone { get; }
    ulong[] MentionedUsers { get; }
    ulong[] MentionedRoles { get; }
    IEnumerable<IMentionedChannelModel> MentionedChannels { get; }
    IEnumerable<IAttachmentModel> Attachments { get; }
    IEnumerable<IEmbedModel> Embeds { get; }
    IEnumerable<DiscordEmojiId> Reactions { get; }
    bool IsPinned { get; }
    bool IsWebhook { get; }
    int Type { get; }

    IMessageActivityModel? Activity { get; }
    IMessageApplicationModel? Application { get; }

    IMessageReferenceModel? MessageReference { get; }

    int Flags { get; }

    IMessageInteractionMetadataModel? InteractionMetadata { get; }

    ulong? ThreadId { get; }
    ulong? ThreadGuildId { get; } // used for constructing thread loadables, always present with 'thread' in the API

    IEnumerable<MessageComponent> Components { get; }
    IEnumerable<IStickerItemModel> Stickers { get; }

    int? Position { get; }

    IMessageRoleSubscriptionData? RoleSubscriptionData { get; }
    
    IPollModel? Poll { get; }
}

public interface IMessageRoleSubscriptionData
{
    ulong RoleSubscriptionListingId { get; }
    string TierName { get; }
    int TotalMonthsSubscribed { get; }
    bool IsRenewal { get; }
}

[ModelEquality]
public partial interface IMessageInteractionMetadataModel : IEntityModel<ulong>
{
    int Type { get; }
    ulong UserId { get; }
    IDictionary<int, ulong> AuthorizingIntegrationOwners { get; }
    ulong? OriginalResponseMessageId { get; }
    ulong? InteractedMessageId { get; }
    IMessageInteractionMetadataModel? TriggeringInteractionMetadata { get; }
}

public interface IMessageReferenceModel
{
    ulong? MessageId { get; }
    ulong? ChannelId { get; }
    ulong GuildId { get; }
}

public interface IMessageApplicationModel
{
    ulong Id { get; }
    string CoverImage { get; }
    string Description { get; }
    string Icon { get; }
    string Name { get; }
}

public interface IMessageActivityModel
{
    int Type { get; }
    string? PartyId { get; }
}
