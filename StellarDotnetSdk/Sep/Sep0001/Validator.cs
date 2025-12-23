namespace StellarDotnetSdk.Sep.Sep0001;

/// <summary>
///     Validator Information. From the stellar.toml VALIDATORS list, one set of fields for each node your organization runs.
///     Combined with the steps outlined in SEP-20, this section allows to declare the node(s), and to let others know
///     the location of any public archives they maintain.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md">SEP-0001</a>
/// </summary>
public sealed record Validator
{
    /// <summary>
    ///     A name for display in stellar-core configs that conforms to `^[a-z0-9-]{2,16}$`.
    /// </summary>
    public string? Alias { get; init; }

    /// <summary>
    ///     A human-readable name for display in quorum explorers and other interfaces.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    ///     The Stellar account associated with the node.
    /// </summary>
    public string? PublicKey { get; init; }

    /// <summary>
    ///     The IP:port or domain:port peers can use to connect to the node.
    /// </summary>
    public string? Host { get; init; }

    /// <summary>
    ///     The location of the history archive published by this validator.
    /// </summary>
    public string? History { get; init; }
}

