using Discord.EntityRelationships;
using Discord.Models.Json;
using Discord.Rest;

namespace Discord;

public interface ILoadableGuildMemberEntitySource<TMember> :
    IGuildMemberEntitySource<TMember>,
    ILoadableEntity<ulong, TMember>
    where TMember : class, IGuildUser;

public interface IGuildMemberEntitySource<TMember> :
    IGuildRelationship,
    IEntitySource<ulong, TMember>
    where TMember : IGuildUser
{
    IRootEntitySource<ILoadableEntity<ulong, IRole>, ulong, IRole> Roles { get; }
    ILoadableEntity<ulong, IRole> Role(ulong id) => Roles[id];

    Task AddRoleAsync(
        EntityOrId<ulong, IRole> role,
        RequestOptions? options = null,
        CancellationToken token = default
    ) => Client.RestApiClient.ExecuteAsync(
        Routes.AddGuildMemberRole(Guild.Id, Id, role.Id),
        options ?? Client.DefaultRequestOptions,
        token
    );

    Task RemoveRoleAsync(
        EntityOrId<ulong, IRole> role,
        RequestOptions? options = null,
        CancellationToken token = default
    ) => Client.RestApiClient.ExecuteAsync(
        Routes.RemoveGuildMemberRole(Guild.Id, Id, role.Id),
        options ?? Client.DefaultRequestOptions,
        token
    );

    Task KickAsync(
        RequestOptions? options = null,
        CancellationToken token = default
    ) => Client.RestApiClient.ExecuteAsync(
        Routes.RemoveGuildMember(Guild.Id, Id),
        options ?? Client.DefaultRequestOptions,
        token
    );

    Task BanAsync(
        TimeSpan? pruneDuration = null,
        RequestOptions? options = null,
        CancellationToken token = default
    ) => Client.RestApiClient.ExecuteAsync(
        Routes.CreateGuildBan(
            Guild.Id,
            Id,
            new CreateGuildBanParams()
            {
                DeleteMessageSeconds = Optional
                    .FromNullable(pruneDuration)
                    .Map(v => (int)Math.Floor(v.TotalSeconds))
            }
        ),
        options ?? Client.DefaultRequestOptions,
        token
    );

    Task UnbanAsync(
        RequestOptions? options = null,
        CancellationToken token = default
    ) => Client.RestApiClient.ExecuteAsync(
        Routes.RemoveGuildBan(Guild.Id, Id),
        options ?? Client.DefaultRequestOptions,
        token
    );
}