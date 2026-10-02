using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Sep.Sep0038.Exceptions;

/// <summary>
///     The domain's stellar.toml does not declare a usable <c>ANCHOR_QUOTE_SERVER</c>: either the key is missing, so
///     the anchor does not advertise SEP-38 support, or its value is not an address <see cref="QuoteService" />
///     accepts (<see cref="DeclaredAddress" /> holds it, and the message says why).
/// </summary>
public class NoAnchorQuoteServerFoundException : QuoteServerException
{
    /// <summary>
    ///     Initializes a new instance for a stellar.toml without <c>ANCHOR_QUOTE_SERVER</c>.
    /// </summary>
    /// <param name="domain">The domain whose stellar.toml was read.</param>
    public NoAnchorQuoteServerFoundException(string domain)
        : base($"ANCHOR_QUOTE_SERVER not found in the stellar.toml of domain {domain}.")
    {
        Domain = domain;
    }

    /// <summary>
    ///     Initializes a new instance for a stellar.toml whose <c>ANCHOR_QUOTE_SERVER</c> value was rejected.
    /// </summary>
    /// <param name="domain">The domain whose stellar.toml was read.</param>
    /// <param name="declaredAddress">The rejected value, as the stellar.toml declared it.</param>
    /// <param name="reason">Why the value was rejected.</param>
    public NoAnchorQuoteServerFoundException(string domain, string declaredAddress, string reason)
        : base($"The stellar.toml of domain {domain} declares an unusable ANCHOR_QUOTE_SERVER " +
               $"{UntrustedJsonValue.Describe(declaredAddress)}: {reason}")
    {
        Domain = domain;
        DeclaredAddress = declaredAddress;
    }

    /// <summary>
    ///     The domain whose stellar.toml was read.
    /// </summary>
    public string Domain { get; }

    /// <summary>
    ///     The <c>ANCHOR_QUOTE_SERVER</c> value the stellar.toml declared, when it declared one that was rejected;
    ///     <see langword="null" /> when the key was missing or blank. Server-controlled: escape it before logging or
    ///     display.
    /// </summary>
    public string? DeclaredAddress { get; }
}
