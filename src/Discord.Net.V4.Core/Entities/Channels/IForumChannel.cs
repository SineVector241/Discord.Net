using Discord.Models.Json;
using Discord.Rest;

namespace Discord;

using Modifiable = IModifiable<ulong, IForumChannel, ModifyForumChannelProperties, ModifyGuildChannelParams>;


/// <summary>
///     Represents a forum channel in a guild that can create posts.
/// </summary>
public interface IForumChannel :
    INestedChannel,
    IIntegrationChannel,
    Modifiable
{
    /// <summary>
    ///     Gets a value that indicates whether the channel is NSFW.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> if the channel has the NSFW flag enabled; otherwise <see langword="false" />.
    /// </returns>
    bool IsNsfw { get; }

    /// <summary>
    ///     Gets the current topic for this text channel.
    /// </summary>
    /// <returns>
    ///     A string representing the topic set in the channel; <see langword="null" /> if none is set.
    /// </returns>
    string? Topic { get; }

    /// <summary>
    ///     Gets the default archive duration for a newly created post.
    /// </summary>
    ThreadArchiveDuration DefaultAutoArchiveDuration { get; }

    /// <summary>
    ///     Gets a collection of tags inside of this forum channel.
    /// </summary>
    IReadOnlyCollection<ForumTag> Tags { get; }

    /// <summary>
    ///     Gets the current rate limit on creating posts in this forum channel.
    /// </summary>
    /// <returns>
    ///     An <see cref="int" /> representing the time in seconds required before the user can send another
    ///     message; <see langword="null" /> if disabled.
    /// </returns>
    int? ThreadCreationInterval { get; }

    /// <summary>
    ///     Gets the current default slow-mode delay for threads in this forum channel.
    /// </summary>
    /// <returns>
    ///     An <see cref="int" /> representing the time in seconds required before the user can send another
    ///     message; <see langword="null" /> if disabled.
    /// </returns>
    int? DefaultSlowModeInterval { get; }

    /// <summary>
    ///     Gets the emoji to show in the add reaction button on a thread in a forum channel
    /// </summary>
    ILoadableEntity<IEmote> DefaultReactionEmoji { get; }

    /// <summary>
    ///     Gets the rule used to order posts in forum channels.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see langword="null" />, which indicates a preferred sort order hasn't been set
    /// </remarks>
    ForumSortOrder? DefaultSortOrder { get; }

    /// <summary>
    ///     Gets the rule used to display posts in a forum channel.
    /// </summary>
    ForumLayout DefaultLayout { get; }

    static ApiBodyRoute<ModifyGuildChannelParams> Modifiable.ModifyRoute(IPathable path, ulong id,
        ModifyGuildChannelParams args)
        => Routes.ModifyChannel(id, args);
}