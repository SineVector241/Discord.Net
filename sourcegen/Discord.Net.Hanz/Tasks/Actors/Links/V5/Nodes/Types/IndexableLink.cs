using System.Collections.Immutable;
using Discord.Net.Hanz.Tasks.Actors.Links.V5.Nodes.Common;
using Discord.Net.Hanz.Utils.Bakery;
using Microsoft.CodeAnalysis;

namespace Discord.Net.Hanz.Tasks.Actors.Links.V5.Nodes.Types;

public class IndexableLink : 
    Node,
    ILinkImplmenter
{
    private readonly record struct State(
        ActorInfo ActorInfo,
        bool RedefinesLinkMembers,
        ImmutableEquatableArray<(string Actor, string OverrideTarget)> AncestorOverrides
    );
    
    public IndexableLink(NodeProviders providers, Logger logger) : base(providers, logger)
    {
    }
    
    public IncrementalValuesProvider<Branch<ILinkImplmenter.LinkImplementation, LinkNode.State>> Branch(
        IncrementalValuesProvider<Branch<LinkNode.State, LinkNode.State>> provider)
    {
        return provider
            .Where(x => x.Value is {IsTemplate: true, Entry.Type.Name: "Indexable"})
            .Select(CreateState)
            .Select((x, token) => x.Mutate(Build(x.Value, token)));
    }

    private Branch<State, LinkNode.State> CreateState(
        Branch<LinkNode.State, LinkNode.State> link,
        CancellationToken token)
    {
        using var logger = Logger
            .GetSubLogger(link.Value.ActorInfo.Assembly.ToString())
            .GetSubLogger(nameof(CreateState))
            .GetSubLogger(link.Value.ActorInfo.Actor.MetadataName);
        
        logger.Log($"{link.Value.ActorInfo.Actor}");
        logger.Log($" - {link.Value.Entry}");
        
        return link.Mutate(
            new State(
                link.Value.ActorInfo,
                link.Value.Actor.State.EntityAssignableAncestors.Count > 0 || !link.Value.ActorInfo.IsCore,
                new(
                    link.Value.Actor.State.Ancestors
                        .Select(x =>
                            (
                                x.ActorInfo.Actor.FullyQualifiedName,
                                x.Ancestors.Count > 0
                                    ? $"{x.ActorInfo.Actor}.{string.Join(".", link.Value.Parts)}"
                                    : $"{x.ActorInfo.FormattedLinkType}.Indexable"
                            )
                        )
                )
            )
        );
    }

    private ILinkImplmenter.LinkImplementation Build(State state, CancellationToken token)
    {
        using var logger = Logger
            .GetSubLogger(state.ActorInfo.Assembly.ToString())
            .GetSubLogger(nameof(Build))
            .GetSubLogger(state.ActorInfo.Actor.MetadataName);
        
        logger.Log("Building indexable link");
        logger.Log($" - {state.ActorInfo.Actor.FullyQualifiedName}");
        
        return new ILinkImplmenter.LinkImplementation(
            CreateInterfaceSpec(state, token),
            CreateImplementationSpec(state, token)
        );
    }

    private static ILinkImplmenter.LinkSpec CreateInterfaceSpec(State state, CancellationToken token)
    {
        var spec = new ILinkImplmenter.LinkSpec(
            Indexers: new([
                new IndexerSpec(
                    Type: state.ActorInfo.Actor.FullyQualifiedName,
                    Modifiers: new(state.RedefinesLinkMembers ? ["new"] : []),
                    Accessibility: Accessibility.Internal,
                    Parameters: new([
                        (state.ActorInfo.FormattedIdentifiable, "identity")
                    ]),
                    Expression: "identity.Actor ?? GetActor(identity.Id)"
                )
            ])
        );

        if (!state.ActorInfo.IsCore)
        {
            spec = spec with
            {
                Indexers = spec.Indexers.AddRange(
                    new IndexerSpec(
                        Type: state.ActorInfo.CoreActor.FullyQualifiedName,
                        Parameters: new([
                            (state.ActorInfo.FormattedIdentifiable, "identity")
                        ]),
                        Expression: "identity.Actor ?? GetActor(identity.Id)",
                        ExplicitInterfaceImplementation: $"{state.ActorInfo.CoreActor}.Indexable"
                    ),
                    new IndexerSpec(
                        Type: state.ActorInfo.CoreActor.FullyQualifiedName,
                        Parameters: new([
                            (state.ActorInfo.Id.FullyQualifiedName, "id")
                        ]),
                        Expression: "this[id]",
                        ExplicitInterfaceImplementation: $"{state.ActorInfo.FormattedCoreLinkType}.Indexable"
                    )
                ),
                Methods = spec.Methods.AddRange(
                    new MethodSpec(
                        Name: "Specifically",
                        ReturnType: state.ActorInfo.Actor.FullyQualifiedName,
                        ExplicitInterfaceImplementation: $"{state.ActorInfo.FormattedCoreLinkType}.Indexable",
                        Parameters: new([
                            (state.ActorInfo.Id.FullyQualifiedName, "id")
                        ]),
                        Expression: "Specifically(id)"
                    )
                )
            };
        }

        if (!state.RedefinesLinkMembers)
            return spec;

        return spec with
        {
            Indexers = spec.Indexers.AddRange([
                new IndexerSpec(
                    Type: state.ActorInfo.Actor.FullyQualifiedName,
                    Modifiers: new(["new"]),
                    Parameters: new([
                        (state.ActorInfo.Id.FullyQualifiedName, "id")
                    ]),
                    Expression: $"(this as {state.ActorInfo.FormattedActorProvider}).GetActor(id)"
                ),
                ..state.AncestorOverrides.Select(x =>
                    new IndexerSpec(
                        Type: x.Actor,
                        Parameters: new([
                            (state.ActorInfo.Id.FullyQualifiedName, "id")
                        ]),
                        ExplicitInterfaceImplementation: x.OverrideTarget,
                        Expression: "this[id]"
                    )
                )
            ]),
            Methods = spec.Methods.AddRange([
                new MethodSpec(
                    Name: "Specifically",
                    ReturnType: state.ActorInfo.Actor.FullyQualifiedName,
                    Modifiers: new(["new"]),
                    Parameters: new([
                        (state.ActorInfo.Id.FullyQualifiedName, "id")
                    ]),
                    Expression: $"(this as {state.ActorInfo.FormattedActorProvider}).GetActor(id)"
                ),
                ..state.AncestorOverrides.Select(x =>
                    new MethodSpec(
                        Name: "Specifically",
                        ReturnType: x.Actor,
                        Parameters: new([
                            (state.ActorInfo.Id.FullyQualifiedName, "id")
                        ]),
                        ExplicitInterfaceImplementation: x.OverrideTarget,
                        Expression: "Specifically(id)"
                    )
                )
            ])
        };
    }

    private static ILinkImplmenter.LinkSpec CreateImplementationSpec(State state, CancellationToken token)
    {
        return ILinkImplmenter.LinkSpec.Empty;
    }
}