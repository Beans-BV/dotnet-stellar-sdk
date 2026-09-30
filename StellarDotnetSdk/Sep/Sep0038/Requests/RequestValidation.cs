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

    internal static string FormatAmount(decimal amount)
    {
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
