using Discord.Models;
using Discord.Models.Json;
using Discord.Rest;
using System.Diagnostics.CodeAnalysis;

namespace Discord;

[Loadable(nameof(Routes.GetChannel), typeof(ThreadChannelModelBase))]
[Modifiable<ModifyThreadChannelProperties>(nameof(Routes.ModifyChannel))]
[SuppressMessage("ReSharper", "PossibleInterfaceMemberAmbiguity")]
public partial interface IThreadChannelActor :
    IGuildChannelActor,
    IMessageChannelActor,
    IThreadMemberRelationship,
    IActor<ulong, IThreadChannel>
{
    IThreadMemberActor CurrentThreadMember { get; }

    IEnumerableIndexableActor<IThreadMemberActor, ulong, IThreadMember> ThreadMembers { get; }

    IThreadMemberActor IThreadMemberRelationship.ThreadMember
        => CurrentThreadMember;

    Task JoinAsync(RequestOptions? options = null, CancellationToken token = default)
        => Client.RestApiClient.ExecuteAsync(Routes.JoinThread(Id), options ?? Client.DefaultRequestOptions, token);

    Task AddThreadMemberAsync(EntityOrId<ulong, IUser> user, RequestOptions? options = null,
        CancellationToken token = default)
        => Client.RestApiClient.ExecuteAsync(
            Routes.AddThreadMember(Id, user.Id),
            options ?? Client.DefaultRequestOptions,
            token
        );

    Task LeaveAsync(RequestOptions? options = null, CancellationToken token = default)
        => Client.RestApiClient.ExecuteAsync(Routes.LeaveThread(Id), options ?? Client.DefaultRequestOptions, token);
}
