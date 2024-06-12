using Discord.EntityRelationships;
using Discord.Models.Json;
using Discord.Rest;

namespace Discord;

using Modifiable = IModifiable<ulong, IGuildUser, ModifyGuildUserProperties, ModifyGuildMemberParams>;

public interface IGuildUser :
    IUser,
    Modifiable,
    IGuildMemberEntitySource
{
    /// <summary>
    ///     Gets when this user joined the guild.
    /// </summary>
    /// <returns>
    ///     A <see cref="DateTimeOffset" /> representing the time of which the user has joined the guild;
    ///     <see langword="null" /> when it cannot be obtained.
    /// </returns>
    DateTimeOffset? JoinedAt { get; }

    /// <summary>
    ///     Gets the displayed name for this user.
    /// </summary>
    /// <returns>
    ///     A string representing the display name of the user; If the nickname is null, this will be the username.
    /// </returns>
    string DisplayName
        => Nickname ?? Username;

    /// <summary>
    ///     Gets the nickname for this user.
    /// </summary>
    /// <returns>
    ///     A string representing the nickname of the user; <see langword="null" /> if none is set.
    /// </returns>
    string? Nickname { get; }

    /// <summary>
    ///     Gets the displayed avatar for this user.
    /// </summary>
    /// <returns>
    ///     The users displayed avatar hash. If the user does not have a guild avatar, this will be the regular avatar.
    ///     If the user also does not have a regular avatar, this will be <see langword="null" />.
    /// </returns>
    string? DisplayAvatarId
        => GuildAvatarId ?? AvatarId;

    /// <summary>
    ///     Gets the guild specific avatar for this user.
    /// </summary>
    /// <returns>
    ///     The users guild avatar hash if they have one; otherwise <see langword="null" />.
    /// </returns>
    string? GuildAvatarId { get; }

    /// <summary>
    ///     Gets the guild-level permissions for this user.
    /// </summary>
    /// <returns>
    ///     A <see cref="Discord.GuildPermission" /> enum for this user, representing what
    ///     permissions this user has in the guild.
    /// </returns>
    GuildPermission GuildPermissions { get; }

    /// <summary>
    ///     Gets the date and time for when this user's guild boost began.
    /// </summary>
    /// <returns>
    ///     A <see cref="DateTimeOffset" /> for when the user began boosting this guild; <see langword="null" /> if they are
    ///     not boosting the guild.
    /// </returns>
    DateTimeOffset? PremiumSince { get; }

    /// <summary>
    ///     Whether the user has passed the guild's Membership Screening requirements.
    /// </summary>
    bool? IsPending { get; }

    /// <summary>
    ///     Gets the users position within the role hierarchy.
    /// </summary>
    int Hierarchy { get; }

    /// <summary>
    ///     Gets the date and time that indicates if and for how long a user has been timed out.
    /// </summary>
    /// <remarks>
    ///     <see langword="null" /> or a timestamp in the past if the user is not timed out.
    /// </remarks>
    /// <returns>
    ///     A <see cref="DateTimeOffset" /> indicating how long the user will be timed out for.
    /// </returns>
    DateTimeOffset? TimedOutUntil { get; }

    /// <summary>
    ///     Gets the public flags for this guild member.
    /// </summary>
    GuildUserFlags Flags { get; }

    static ApiBodyRoute<ModifyGuildMemberParams> Modifiable.ModifyRoute(IPathable path, ulong id,
        ModifyGuildMemberParams args)
        => Routes.ModifyGuildMember(path.Require<IGuild>(), id, args);
}