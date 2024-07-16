using Discord.Models.Json;
using Discord.Utils;

namespace Discord.Rest;

public partial class Routes
{
    public static IApiOutRoute<Invite> GetInvite(
        [IdHeuristic<IInvite>] string code,
        bool? withCounts = default,
        bool? withExpiration = default,
        EntityOrId<ulong, IGuildScheduledEvent>? eventId = default
    ) => new ApiOutRoute<Invite>(nameof(GetInvite), RequestMethod.Get,
        $"invites/{code}{RouteUtils.GetUrlEncodedQueryParams(("with_counts", withCounts), ("with_expiration", withExpiration), ("guild_scheduled_event_id", eventId))}");

    public static IApiOutRoute<Invite> DeleteInvite([IdHeuristic<IInvite>] string code) =>
        new ApiOutRoute<Invite>(nameof(DeleteInvite), RequestMethod.Delete, $"invites/{code}");
}
