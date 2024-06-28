using Discord.Models;
using System.Diagnostics.CodeAnalysis;

namespace Discord;

public interface IEntityProvider<out TEntity, in TModel> : IClientProvider
    where TEntity : IEntity
    where TModel : IEntityModel?
{
    internal TEntity CreateEntity(TModel model);

    [return: NotNullIfNotNull(nameof(model))]
    internal TEntity? CreateNullableEntity(TModel? model) => model is null ? default : CreateEntity(model);
}
