using Discord.Models;
using Discord.Models.Json;

namespace Discord.Rest;

using MessageChannelTrait = RestMessageChannelTrait<RestGroupChannelActor, GroupChannelIdentity>;

[ExtendInterfaceDefaults]
public partial class RestGroupChannelActor :
    RestChannelActor,
    IGroupChannelActor,
    IRestActor<ulong, RestGroupChannel, GroupChannelIdentity>
{
    [ProxyInterface(typeof(IMessageChannelTrait))]
    internal MessageChannelTrait MessageChannelTrait { get; }

    [SourceOfTruth] internal sealed override GroupChannelIdentity Identity { get; }

    [TypeFactory]
    public RestGroupChannelActor(
        DiscordRestClient client,
        GroupChannelIdentity channel
    ) : base(client, channel)
    {
        Identity = channel | this;
        MessageChannelTrait = new(client, this, channel);
    }

    [SourceOfTruth]
    internal RestGroupChannel CreateEntity(IGroupDMChannelModel model)
        => RestGroupChannel.Construct(Client, model);

    [SourceOfTruth]
    internal virtual RestInvite CreateEntity(IInviteModel model)
        => RestInvite.Construct(Client, model);
}

public partial class RestGroupChannel :
    RestChannel,
    IGroupChannel,
    IConstructable<RestGroupChannel, IGroupDMChannelModel, DiscordRestClient>
{
    [SourceOfTruth]
    public IDefinedLoadableEntityEnumerable<ulong, IUser> Recipients => throw new NotImplementedException();

    [ProxyInterface(
        typeof(IGroupChannelActor),
        typeof(IMessageChannelTrait),
        typeof(IEntityProvider<IGroupChannel, IGroupDMChannelModel>)
    )]
    internal override RestGroupChannelActor Actor { get; }

    internal override IGroupDMChannelModel Model => _model;

    private IGroupDMChannelModel _model;

    internal RestGroupChannel(
        DiscordRestClient client,
        IGroupDMChannelModel model,
        RestGroupChannelActor? actor = null
    ) : base(client, model)
    {
        _model = model;
        Actor = actor ?? new(client, GroupChannelIdentity.Of(this));
    }

    public static RestGroupChannel Construct(DiscordRestClient client, IGroupDMChannelModel model)
        => new(client, model);

    [CovariantOverride]
    public ValueTask UpdateAsync(IGroupDMChannelModel model, CancellationToken token = default)
    {
        _model = model;

        return base.UpdateAsync(model, token);
    }

    public override IGroupDMChannelModel GetModel() => Model;
}