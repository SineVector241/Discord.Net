namespace Discord;

public partial interface IApplicationActor :
    IActor<ulong, IApplication>
{
    IGuildApplicationActor.Indexable Guilds { get; }
    
    IApplicationRoleConnectionMetadataActor.Enumerable.BackLink<IApplicationActor> RoleConnectionMetadata { get; }
    
    IApplicationEmoteActor.Enumerable.Indexable.BackLink<IApplicationActor> Emotes { get; }
}