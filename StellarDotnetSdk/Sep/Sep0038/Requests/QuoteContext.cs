namespace StellarDotnetSdk.Sep.Sep0038.Requests;

/// <summary>
///     The SEP a price or quote will be used with — the SEP-38 <c>context</c> parameter.
/// </summary>
/// <remarks>
///     <c>POST /quote</c> accepts all three values. <c>GET /price</c> accepts only <see cref="Sep6" /> and
///     <see cref="Sep31" />: SEP-24 flows take firm quotes only.
/// </remarks>
public enum QuoteContext
{
    /// <summary>SEP-6 programmatic deposit and withdrawal (<c>sep6</c>).</summary>
    Sep6,

    /// <summary>SEP-24 interactive deposit and withdrawal (<c>sep24</c>). Valid for <c>POST /quote</c> only.</summary>
    Sep24,

    /// <summary>SEP-31 cross-border payments (<c>sep31</c>).</summary>
    Sep31,
}
