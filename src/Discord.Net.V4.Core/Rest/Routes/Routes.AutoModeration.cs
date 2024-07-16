using Discord.Models.Json;

namespace Discord.Rest;

public partial class Routes
{
    public static IApiOutRoute<AutoModerationRule[]> GetAutoModerationRules(
        [IdHeuristic<IGuild>] ulong guildId
    )
        => new ApiOutRoute<AutoModerationRule[]>(
            nameof(GetAutoModerationRules),
            RequestMethod.Get,
            $"guilds/{guildId}/auto-moderation/rules",
            (ScopeType.Guild, guildId)
        );

    public static IApiOutRoute<AutoModerationRule> GetAutoModerationRule(
        [IdHeuristic<IGuild>] ulong guildId,
        ulong ruleId
    ) => new ApiOutRoute<AutoModerationRule>(
        nameof(GetAutoModerationRule),
        RequestMethod.Get,
        $"guilds/{guildId}/auto-moderation/rule/{ruleId}",
        (ScopeType.Guild, guildId)
    );

    public static IApiInOutRoute<CreateAutoModRuleParams, AutoModerationRule> CreateAutoModerationRule(
        [IdHeuristic<IGuild>] ulong guildId,
        CreateAutoModRuleParams body
    ) => new ApiInOutRoute<CreateAutoModRuleParams, AutoModerationRule>(nameof(CreateAutoModerationRule),
        RequestMethod.Post,
        $"/guilds/{guildId}/auto-moderation/rules",
        body,
        ContentType.JsonBody,
        (ScopeType.Guild, guildId)
    );

    public static IApiInOutRoute<ModifyAutoModRuleParams, AutoModerationRule> ModifyAutoModerationRule([IdHeuristic<IGuild>] ulong guildId,
        ulong ruleId, ModifyAutoModRuleParams body)
        => new ApiInOutRoute<ModifyAutoModRuleParams, AutoModerationRule>(nameof(ModifyAutoModerationRule),
            RequestMethod.Patch,
            $"/guilds/{guildId}/auto-moderation/rules/{ruleId}",
            body,
            ContentType.JsonBody,
            (ScopeType.Guild, guildId));

    public static IApiRoute DeleteAutoModerationRule([IdHeuristic<IGuild>] ulong guildId, ulong ruleId) =>
        new ApiRoute(nameof(DeleteAutoModerationRule), RequestMethod.Delete,
            $"/guilds/{guildId}/auto-moderation/rules/{ruleId}", (ScopeType.Guild, guildId));
}
