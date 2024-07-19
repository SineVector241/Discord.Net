using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Discord.Net.Hanz.Tasks;

public interface IGenerationTask<T>
    where T : class, IEquatable<T>
{
    bool IsValid(SyntaxNode node, CancellationToken token = default);

    T? GetTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token = default);

    void Execute(SourceProductionContext context, T? target);
}

public interface IGenerationCombineTask<T> where T : class, IEquatable<T>
{
    bool IsValid(SyntaxNode node, CancellationToken token = default);

    T? GetTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token = default);

    void Execute(SourceProductionContext context, ImmutableArray<T?> targets);
}
