using Discord.Net.Hanz.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Discord.Net.Hanz.Tasks.Traits;

public sealed class EntityTraits : IGenerationCombineTask<EntityTraits.GenerationTarget>
{
    public static readonly Dictionary<string, string> TraitAttributes = new()
    {
        {"DeletableAttribute", "Discord.IDeletable"},
        {"ModifiableAttribute", "Discord.IModifiable"},
        {"RefreshableAttribute", "Discord.IRefreshable"},
        {"FetchableAttribute", "Discord.IFetchable"},
        {"FetchableOfManyAttribute", "Discord.IFetchableOfMany"},
        {"LoadableAttribute", "Discord.ILoadable"}
    };

    public class GenerationTarget(
        SemanticModel semanticModel,
        InterfaceDeclarationSyntax interfaceDeclarationSyntax,
        INamedTypeSymbol interfaceSymbol,
        HashSet<string> requestedTraits)
    {
        public SemanticModel SemanticModel { get; } = semanticModel;
        public InterfaceDeclarationSyntax InterfaceDeclarationSyntax { get; } = interfaceDeclarationSyntax;
        public INamedTypeSymbol InterfaceSymbol { get; } = interfaceSymbol;
        public HashSet<string> RequestedTraits { get; } = requestedTraits;
    }

    public bool IsValid(SyntaxNode node, CancellationToken token = default)
        => node is InterfaceDeclarationSyntax {AttributeLists.Count: > 0};

    public GenerationTarget? GetTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token = default)
    {
        if (context.Node is not InterfaceDeclarationSyntax {AttributeLists.Count: > 0} interfaceDeclarationSyntax)
            return null;

        if (ModelExtensions.GetDeclaredSymbol(context.SemanticModel, interfaceDeclarationSyntax) is not INamedTypeSymbol
            interfaceSymbol)
            return null;

        if (interfaceSymbol.AllInterfaces.All(x =>
                !x.ToDisplayString().StartsWith("Discord.IActor") &&
                !x.ToDisplayString().StartsWith("Discord.IEntity<")
            ))
            return null;

        var traitsRequested = new HashSet<string>();

        foreach (var attribute in interfaceSymbol.GetAttributes())
        {
            if (attribute.AttributeClass is null) continue;

            if (TraitAttributes.TryGetValue(attribute.AttributeClass.Name, out _))
                traitsRequested.Add(attribute.AttributeClass.Name);
        }

        if (traitsRequested.Count == 0)
            return null;

        return new GenerationTarget(
            context.SemanticModel,
            interfaceDeclarationSyntax,
            interfaceSymbol,
            traitsRequested
        );
    }

    public void Execute(SourceProductionContext context, ImmutableArray<GenerationTarget?> targets)
    {
        var entitySyntax = new Dictionary<string, InterfaceDeclarationSyntax>();

        var producedFiles = new HashSet<string>();

        foreach (var target in targets)
        {
            if (target is null)
                continue;

            var fromEntitySyntax = entitySyntax
                .TryGetValue(target.InterfaceSymbol.ToDisplayString(), out var syntax);

            if (!fromEntitySyntax)
                syntax = SyntaxFactory.InterfaceDeclaration(
                    [],
                    target.InterfaceDeclarationSyntax.Modifiers,
                    target.InterfaceDeclarationSyntax.Identifier,
                    target.InterfaceDeclarationSyntax.TypeParameterList,
                    null,
                    target.InterfaceDeclarationSyntax.ConstraintClauses,
                    []
                );


            foreach (var trait in target.RequestedTraits)
            {
                try
                {
                    ProcessTraitRequest(ref syntax, trait, target, entitySyntax);
                }
                catch (Exception x)
                {
                    Hanz.Logger.Log(LogLevel.Error,
                        $"Failed on processing {trait} on {target.InterfaceSymbol.Name}: {x}");
                }
            }

            if (syntax!.Members.Count == 0)
                continue;

            if (fromEntitySyntax)
            {
                entitySyntax[target.InterfaceSymbol.ToDisplayString()] = syntax;
                continue;
            }

            if (!producedFiles.Add($"Traits/{target.InterfaceSymbol.Name}"))
            {
                Hanz.Logger.Warn($"Attempted to add a second file for {target.InterfaceSymbol}");
                continue;
            }

            context.AddSource(
                $"Traits/{target.InterfaceSymbol.Name}",
                $$"""
                  {{target.InterfaceDeclarationSyntax.GetFormattedUsingDirectives()}}

                  namespace {{target.InterfaceSymbol.ContainingNamespace}};

                  {{syntax.NormalizeWhitespace()}}
                  """
            );
        }

        foreach (var syntax in entitySyntax)
        {
            var ns = string.Join(".", syntax.Key.Split('.').Take(syntax.Key.Count(x => x is '.')));

            if (!producedFiles.Add($"Traits/{syntax.Value.Identifier}"))
            {
                Hanz.Logger.Warn($"Attempted to add a second file for {syntax.Value.Identifier}");
                continue;
            }

            context.AddSource(
                $"Traits/{syntax.Value.Identifier}",
                $$"""
                  using Discord.Rest;

                  namespace {{ns}};

                  {{syntax.Value.NormalizeWhitespace()}}
                  """
            );
        }
    }

    private static void ProcessTraitRequest(
        ref InterfaceDeclarationSyntax syntax,
        string trait,
        GenerationTarget target,
        Dictionary<string, InterfaceDeclarationSyntax> entitySyntax)
    {
        if (!TraitAttributes.TryGetValue(trait, out var traitInterface))
            return;

        // if it already implements the trait interface, do nothing
        if (target.InterfaceSymbol.AllInterfaces.Any(x => x.ToDisplayString().StartsWith(traitInterface)))
            return;

        var traitAttribute = target.InterfaceSymbol.GetAttributes()
            .FirstOrDefault(x => x.AttributeClass?.Name == trait);

        if (traitAttribute is null)
            return;

        switch (traitInterface)
        {
            case "Discord.IDeletable":
                DeleteTrait.Process(ref syntax, target, traitAttribute);
                break;
            case "Discord.IModifiable":
                ModifyTrait.Process(
                    ref syntax,
                    target,
                    traitAttribute,
                    entitySyntax
                );
                break;
            case "Discord.IRefreshable":
                RefreshableTrait.Process(ref syntax, target, traitAttribute);
                break;
            case "Discord.IFetchable" or "Discord.IFetchableOfMany":
                FetchableTrait.Process(ref syntax, target, traitAttribute);
                break;
            case "Discord.ILoadable":
                LoadableTrait.Process(ref syntax, target, traitAttribute, entitySyntax);
                break;
        }
    }

    public static ISymbol? GetRouteSymbol(
        MemberAccessExpressionSyntax expression,
        SemanticModel semantic,
        int genericCount = 0)
    {
        var symbol = semantic.GetSymbolInfo(expression);

        if (symbol.Symbol is null && symbol is
                {CandidateReason: CandidateReason.MemberGroup, CandidateSymbols.Length: 1})
            return symbol.CandidateSymbols[0];

        if (semantic.GetSymbolInfo(expression.Expression).Symbol is not INamedTypeSymbol namedTypeSymbol)
            return null;

        return namedTypeSymbol.GetMembers(expression.Name.Identifier.ValueText).FirstOrDefault(
            x => x is not IMethodSymbol method || method.TypeParameters.Length == genericCount);
    }

    public static ExpressionSyntax? GetNameOfArgument(AttributeData data)
    {
        if (data.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
            return null;

        if (attributeSyntax.ArgumentList?.Arguments.Count == 0)
            return null;

        var invocation = attributeSyntax.ArgumentList?.Arguments[0]
            .ChildNodes()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault();

        if (invocation?.Expression is not IdentifierNameSyntax ident || ident.Identifier.ValueText != "nameof")
            return null;

        return invocation.ArgumentList.Arguments[0].Expression;
    }

    public static ArgumentListSyntax ParseRouteArguments(
        IMethodSymbol route,
        GenerationTarget target,
        Func<IParameterSymbol, ArgumentSyntax?>? extra = null,
        ExpressionSyntax? pathHolder = null,
        ExpressionSyntax? idParam = null)
    {
        var hasReturnedId = false;

        return SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList(route.Parameters.Select(x =>
            {
                switch (x.Name)
                {
                    case "id":
                        return ReturnOwnId(ref hasReturnedId, idParam, $"{target.InterfaceSymbol}: {x} -> direct 'id' reference");
                    default:
                        if (x.Type is INamedTypeSymbol paramType &&
                            x.Type.ToDisplayString().StartsWith("Discord.EntityOrId"))
                        {
                            var targetType =
                                paramType.Name == "Nullable"
                                    ? (paramType.TypeArguments[0] as INamedTypeSymbol)?.TypeArguments[1]
                                    : paramType.TypeArguments[1];

                            if (targetType is null)
                                break;

                            return SyntaxFactory.Argument(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        pathHolder ?? SyntaxFactory.IdentifierName("path"),
                                        SyntaxFactory.GenericName(
                                            SyntaxFactory.Identifier(x.IsOptional ? "Optionally" : "Require"),
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList([
                                                    SyntaxFactory.ParseTypeName(targetType.ToDisplayString())
                                                ])
                                            )
                                        )
                                    )
                                )
                            );
                        }

                        var entityType = GetParameterRelationType(target, x);

                        if (entityType is null)
                            break;

                        if (entityType.Equals(target.InterfaceSymbol, SymbolEqualityComparer.Default))
                            return ReturnOwnId(ref hasReturnedId, idParam, $"{target.InterfaceSymbol}: {x} is the relation type");

                        var actorInterface = GetActorInterface(target.InterfaceSymbol);

                        CommonConversion? conversion = actorInterface is not null
                            ? target.SemanticModel.Compilation.ClassifyCommonConversion(
                                actorInterface.TypeArguments[1],
                                entityType
                            )
                            : null;

                        if (actorInterface is not null &&
                            (
                                actorInterface.TypeArguments[1].Equals(entityType, SymbolEqualityComparer.Default) ||
                                (conversion?.IsImplicit ?? false)
                            ))
                            return ReturnOwnId(ref hasReturnedId, idParam, $"{target.InterfaceSymbol}: {x} -> common conversion between {actorInterface.TypeArguments[1]} <> {entityType}");

                        return SyntaxFactory.Argument(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    pathHolder ?? SyntaxFactory.IdentifierName("path"),
                                    SyntaxFactory.GenericName(
                                        SyntaxFactory.Identifier("Require"),
                                        SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SeparatedList([
                                                SyntaxFactory.ParseTypeName(entityType.ToDisplayString())
                                            ])
                                        )
                                    )
                                )
                            )
                        );
                }

                return extra?.Invoke(x) ?? throw new NotImplementedException();
            }))
        );
    }

    private static ArgumentSyntax ReturnOwnId(ref bool hasReturnedThis, ExpressionSyntax? idParam, string source)
    {
        if (hasReturnedThis)
        {
            Hanz.Logger.Warn($"using double id in route parameters: {source}");
            //throw new InvalidOperationException("Cannot return 'id' more than once");
        }

        hasReturnedThis = true;

        return SyntaxFactory.Argument(idParam ?? SyntaxFactory.IdentifierName("id"));
    }

    private static ITypeSymbol? GetParameterRelationType(GenerationTarget target, IParameterSymbol parameterSymbol)
    {
        var heuristic = parameterSymbol.GetAttributes()
            .FirstOrDefault(x =>
                x.AttributeClass?.ToDisplayString().StartsWith("Discord.IdHeuristicAttribute") ?? false
            );

        if (heuristic?.AttributeClass is not null)
            return heuristic.AttributeClass.TypeArguments[0];

        if (!parameterSymbol.Name.EndsWith("Id"))
            return null;

        var type = parameterSymbol.Name.Remove(parameterSymbol.Name.Length - 2, 2);
        type = $"{char.ToUpper(type[0])}{type.Remove(0, 1)}";

        // if it's for the actor type, we can return 'id'
        if (target.InterfaceSymbol.Name == $"I{type}Actor")
            return target.InterfaceSymbol;

        var entityTypeName = $"Discord.I{type}";

        return target.SemanticModel.Compilation.GetTypeByMetadataName(entityTypeName);
    }

    public static ITypeSymbol? GetModelInterface(INamedTypeSymbol symbol)
    {
        var model = GetEntityModelOfInterface(symbol)?.TypeArguments[0];
        if (model is not null)
            return model;

        var actor = GetActorInterface(symbol);

        if (actor is null)
            return null;

        return GetEntityModelOfInterface(actor.TypeArguments[1] as INamedTypeSymbol)?.TypeArguments[0];
    }

    public static INamedTypeSymbol? GetActorInterface(INamedTypeSymbol actor)
    {
        return Hierarchy.GetInterfaceHierarchy(actor)
            .FirstOrDefault(x => x.Interface.ToDisplayString().StartsWith("Discord.IActor<"))
            .Interface;
    }

    public static INamedTypeSymbol? GetEntityInterface(INamedTypeSymbol entity)
    {
        return Hierarchy.GetInterfaceHierarchy(entity)
            .FirstOrDefault(x => x.Interface.ToDisplayString().StartsWith("Discord.IEntity<"))
            .Interface;
    }

    public static INamedTypeSymbol? GetEntityModelOfInterface(ITypeSymbol? entity)
    {
        if (entity is null)
            return null;

        return Hierarchy.GetInterfaceHierarchy(entity)
            .FirstOrDefault(x => x.Interface.ToDisplayString().StartsWith("Discord.IEntityOf<"))
            .Interface;
    }
}
