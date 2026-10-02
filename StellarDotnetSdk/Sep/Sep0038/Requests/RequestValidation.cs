using System;
using System.Globalization;

namespace StellarDotnetSdk.Sep.Sep0038.Requests;

/// <summary>
///     Client-side checks shared by the SEP-38 request types, run before anything is sent.
/// </summary>
internal static class RequestValidation
{
    internal static string ToWireValue(QuoteContext context)
    {
        return context switch
        {
            QuoteContext.Sep6 => "sep6",
            QuoteContext.Sep24 => "sep24",
            QuoteContext.Sep31 => "sep31",
            // An undefined value cast into the enum would otherwise reach the anchor as its ordinal.
            _ => throw new ArgumentOutOfRangeException(nameof(context), context,
                "Context must be one of Sep6, Sep24 or Sep31."),
        };
    }

    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="amount" /> is zero or negative.
    /// </exception>
    internal static string FormatAmount(decimal amount, string propertyName)
    {
        // SEP-38 amounts are quantities to exchange; zero or a negative value can only be a caller error, so it
        // fails here, before anything is sent, like the other request rules.
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(propertyName, amount, $"{propertyName} must be greater than zero.");
        }

        // Invariant culture: a comma decimal separator from the current culture would change the amount the
        // anchor reads. ToString keeps the scale the caller gave ("100.50" stays "100.50").
        return amount.ToString(CultureInfo.InvariantCulture);
    }

    internal static void RequireNonEmpty(string? value, string propertyName)
    {
        // `required` guarantees the member was initialized, not that it holds a usable value.
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} must not be null, empty or whitespace.", propertyName);
        }
    }

    /// <summary>
    ///     Rejects an optional value that is set but blank. <see langword="null" /> means absent; an empty or
    ///     whitespace-only string is rejected rather than read as absent, so a value the caller set cannot silently
    ///     drop out of the request (and, for <c>GET /prices</c>, turn a two-sided request into a valid one-sided one).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> is empty or whitespace.</exception>
    internal static void RequireNullOrNonBlank(string? value, string propertyName)
    {
        if (value != null && string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} must be null or a non-blank value.", propertyName);
        }
    }

    /// <summary>
    ///     Whether an optional value was given, once <see cref="RequireNullOrNonBlank" /> has rejected a blank one.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> is empty or whitespace.</exception>
    internal static bool IsProvided(string? value, string propertyName)
    {
        RequireNullOrNonBlank(value, propertyName);
        return value != null;
    }

    /// <summary>
    ///     Checks a bearer token: a JWT is base64url segments joined by dots, so anything outside printable ASCII, or
    ///     any whitespace, would otherwise surface from <c>System.Net.Http</c> as a <see cref="FormatException" /> or
    ///     an <see cref="System.Net.Http.HttpRequestException" /> that looks like a transport failure.
    /// </summary>
    /// <param name="jwt">The token; <see langword="null" /> is allowed only when <paramref name="required" /> is false.</param>
    /// <param name="propertyName">The name reported in the exception.</param>
    /// <param name="required">Whether the endpoint requires a token.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when a required token is missing, or a given token is blank or is not printable ASCII without
    ///     whitespace.
    /// </exception>
    internal static void RequireValidJwt(string? jwt, string propertyName, bool required)
    {
        if (required)
        {
            RequireNonEmpty(jwt, propertyName);
        }
        else if (!IsProvided(jwt, propertyName))
        {
            return;
        }

        foreach (var c in jwt!)
        {
            if (c <= ' ' || c > '~')
            {
                throw new ArgumentException(
                    $"{propertyName} must consist of printable ASCII characters, without whitespace.", propertyName);
            }
        }
    }

    internal static void RequireExactlyOneAmount(decimal? sellAmount, decimal? buyAmount)
    {
        if (sellAmount.HasValue == buyAmount.HasValue)
        {
            throw new ArgumentException(
                "Exactly one of SellAmount or BuyAmount must be provided, not both and not neither.",
                sellAmount.HasValue ? "BuyAmount" : "SellAmount");
        }
    }

    /// <summary>
    ///     What a request's <c>ToString</c> prints in place of a bearer token: whether one is set, never its value.
    /// </summary>
    internal static string Redact(string? jwt)
    {
        return jwt == null ? "<null>" : "<redacted>";
    }
}
