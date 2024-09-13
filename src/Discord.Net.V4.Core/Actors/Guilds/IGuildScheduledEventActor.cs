using Discord.Models;
using Discord.Models.Json;
using Discord.Rest;

namespace Discord;

[
    Loadable(nameof(Routes.GetGuildScheduledEvent)),
    Deletable(nameof(Routes.DeleteGuildScheduledEvent)),
    Creatable<CreateGuildScheduledEventProperties>(
        nameof(Routes.CreateGuildScheduledEvent),
        nameof(IGuildActor.ScheduledEvents)
    ),
    Modifiable<ModifyGuildScheduledEventProperties>(nameof(Routes.ModifyGuildScheduledEvent))
]
public partial interface IGuildScheduledEventActor :
    IGuildRelationship,
    IActor<ulong, IGuildScheduledEvent>
{
    IGuildScheduledEventUserActor.PagedGuildScheduledEventUsers RSVPs { get; }
}