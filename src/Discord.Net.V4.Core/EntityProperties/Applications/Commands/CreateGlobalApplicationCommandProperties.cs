using System.Globalization;
using Discord.Models.Json;

namespace Discord;

public sealed class CreateGlobalApplicationCommandProperties :
    IEntityProperties<CreateGlobalApplicationCommandParams>
{
    public required string Name { get; set; }
    
    public Optional<Dictionary<CultureInfo, string>?> NameLocalizations { get; set; }
    public Optional<string> Description { get; set; }
    public Optional<Dictionary<CultureInfo, string>?> DescriptionLocalizations { get; set; }
    public Optional<IEnumerable<ApplicationCommandOption>> Options { get; set; }
    public Optional<PermissionSet> DefaultMemberPermissions { get; set; }
    public Optional<IEnumerable<ApplicationIntegrationType>> IntegrationTypes { get; set; }
    public Optional<IEnumerable<InteractionContextType>> ContextTypes { get; set; }
    public Optional<ApplicationCommandType> Type { get; set; }
    public Optional<bool> IsNsfw { get; set; }
    
    public CreateGlobalApplicationCommandParams ToApiModel(CreateGlobalApplicationCommandParams? existing = default)
    {
        return new CreateGlobalApplicationCommandParams()
        {
            Name = Name,
            NameLocalizations = NameLocalizations.Map(v => v
                ?.ToDictionary(x => x.Key.Name, x => x.Value)
            ),
            Description = Description,
            DescriptionLocalizations = DescriptionLocalizations.Map(v => v
                ?.ToDictionary(x => x.Key.Name, x => x.Value)
            ),
            Options = Options.Map(v => v
                .Select(x => x.GetModel())
                .ToArray()
            ),
            DefaultMemberPermissions = DefaultMemberPermissions.Map<string?>(v => v.ToString()),
            IntegrationTypes = IntegrationTypes.Map(v => v
                .Select(v => (int) v)
                .ToArray()
            ),
            Contexts = ContextTypes.Map(v => v
                .Select(v => (int) v)
                .ToArray()
            ),
            Type = Type.MapToInt(),
            IsNsfw = IsNsfw
        };
    }
}