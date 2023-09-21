namespace Discord;

/// <summary>
///     Represents options that should be used when sending a request.
/// </summary>
public readonly struct RequestOptions
{
    /// <summary>
    ///     Gets or sets the maximum time to wait for this request to complete.
    /// </summary>
    /// <remarks>
    ///     Gets or set the max time, in milliseconds, to wait for this request to complete. If
    ///     <see langword="null" />, a request will not time out. If a rate limit has been triggered for this request's bucket
    ///     and will not be unpaused in time, this request will fail immediately.
    /// </remarks>
    /// <returns>
    ///     A <see cref="int"/> in milliseconds for when the request times out.
    /// </returns>
    public readonly int? Timeout;

    /// <summary>
    ///     Gets or sets the retry behavior when the request fails.
    /// </summary>
    public readonly RetryMode? RetryMode;

    /// <summary>
    ///     Gets or sets the reason for this action in the guild's audit log.
    /// </summary>
    /// <remarks>
    ///     Gets or sets the reason that will be written to the guild's audit log if applicable. This may not apply
    ///     to all actions.
    /// </remarks>
    public readonly string? AuditLogReason;

    /// <summary>
    ///		Gets or sets whether or not this request should use the system
    ///		clock for rate-limiting. Defaults to <see langword="true" />.
    /// </summary>
    /// <remarks>
    ///		This property can also be set in <see cref="DiscordConfig"/>.
    ///		On a per-request basis, the system clock should only be disabled
    ///		when millisecond precision is especially important, and the
    ///		hosting system is known to have a desynced clock.
    /// </remarks>
    public readonly bool? UseSystemClock;
    
    /// <summary>
    ///     Initializes a new <see cref="RequestOptions" /> class with the default request timeout set in
    ///     <see cref="DiscordConfig"/>.
    /// </summary>
    public RequestOptions()
    {
        Timeout = DiscordConfig.DefaultRequestTimeout;
    }

    public RequestOptions Clone() => (RequestOptions)MemberwiseClone();
}
