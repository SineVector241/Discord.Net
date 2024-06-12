namespace Discord.Models;

public interface IIntegrationModel : IEntityModel<ulong>
{
    string Name { get; }
    string Type { get; }
    bool IsEnabled { get; }
    bool? IsSyncing { get; }
    ulong? RoleId { get; }
    bool? EnableEmoticons { get; }
    int? ExpireBehavior { get; }
    int? ExpireGracePeriod { get; }
    ulong? UserId { get; }
    string? AccountId { get; }
    string? AccountName { get; }
    DateTimeOffset? SyncedAt { get; }
    int? SubscriberCount { get; }
    bool? IsRevoked { get; }
    IApplicationModel? Application { get; }
    string[]? Scopes { get; }
}