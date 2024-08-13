using Discord.Models;
using Discord.Rest;
using System.Diagnostics.CodeAnalysis;

namespace Discord;

[Trait]
[Loadable(nameof(Routes.GetChannel))]
[SuppressMessage("ReSharper", "PossibleInterfaceMemberAmbiguity")]
public partial interface IMessageChannelTrait :
    IChannelActor,
    IActorTrait<ulong, IMessageChannel>
{
    [return: TypeHeuristic(nameof(Messages))]
    IMessageActor Message(ulong id) => Messages[id];
    IPagedIndexableActor<IMessageActor, ulong, IMessage, PageChannelMessagesParams> Messages { get; }

    IChannel IEntityProvider<IChannel, IChannelModel>.CreateEntity(IChannelModel model)
        => (this as IEntityProvider<IMessageChannel, IChannelModel>).CreateEntity(model);
    IChannel? IEntityProvider<IChannel, IChannelModel>.CreateNullableEntity(IChannelModel? model)
        => (this as IEntityProvider<IMessageChannel, IChannelModel>).CreateNullableEntity(model);
}