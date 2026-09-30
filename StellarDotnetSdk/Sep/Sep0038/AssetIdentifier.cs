using System;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Sep.Sep0038;

/// <summary>
///     An asset in the SEP-38 Asset Identification Format, <c>&lt;scheme&gt;:&lt;identifier&gt;</c>.
/// </summary>
/// <remarks>
///     <para>
///         Two schemes are defined. <c>stellar</c> identifies a Stellar asset in the SEP-11 format — <c>native</c>
///         for lumens, otherwise <c>CODE:ISSUER</c>, for example
///         <c>stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN</c>. <c>iso4217</c> identifies a
///         fiat currency by its three-letter ISO 4217 code, for example <c>iso4217:USD</c>.
///     </para>
///     <para>
///         SEP-38 request types take assets as strings so any value an anchor lists can be passed through. This
///         type builds and validates those strings; it converts implicitly to <see cref="string" />, so it can be
///         assigned to a request property directly:
///         <code>
/// var request = new PriceRequest
/// {
///     Context = QuoteContext.Sep6,
///     SellAsset = AssetIdentifier.Iso4217("BRL"),
///     BuyAsset = AssetIdentifier.FromAsset(usdc),
///     SellAmount = 500m,
/// };
///         </code>
///     </para>
/// </remarks>
public sealed class AssetIdentifier : IEquatable<AssetIdentifier>
{
    /// <summary>The scheme of Stellar assets.</summary>
    public const string StellarScheme = "stellar";

    /// <summary>The scheme of fiat currencies.</summary>
    public const string Iso4217Scheme = "iso4217";

    private const string NativeIdentifier = "native";

    private AssetIdentifier(string scheme, string code, string? issuer)
    {
        Scheme = scheme;
        Code = code;
        Issuer = issuer;
    }

    /// <summary>
    ///     The scheme: <see cref="StellarScheme" /> or <see cref="Iso4217Scheme" />.
    /// </summary>
    public string Scheme { get; }

    /// <summary>
    ///     The Stellar asset code, <c>native</c> for lumens, or the ISO 4217 currency code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     The issuing account of a non-native Stellar asset; <see langword="null" /> for lumens and fiat currencies.
    /// </summary>
    public string? Issuer { get; }

    /// <summary>Whether this identifies a Stellar asset (including lumens).</summary>
    public bool IsStellar => Scheme == StellarScheme;

    /// <summary>Whether this identifies lumens (<c>stellar:native</c>).</summary>
    public bool IsNative => IsStellar && Issuer == null;

    /// <summary>Whether this identifies a fiat currency.</summary>
    public bool IsIso4217 => Scheme == Iso4217Scheme;

    /// <summary>
    ///     Identifies a non-native Stellar asset: <c>stellar:CODE:ISSUER</c>.
    /// </summary>
    /// <param name="code">The asset code: 1 to 12 ASCII letters or digits.</param>
    /// <param name="issuer">The issuer's account ID (a <c>G...</c> address).</param>
    /// <returns>The identifier.</returns>
    /// <exception cref="ArgumentException">Thrown when the code or the issuer is not valid.</exception>
    public static AssetIdentifier Stellar(string code, string issuer)
    {
        if (!IsValidStellarCode(code))
        {
            throw new ArgumentException("The asset code must be 1 to 12 ASCII letters or digits.", nameof(code));
        }

        if (issuer == null || !StrKey.IsValidEd25519PublicKey(issuer))
        {
            throw new ArgumentException("The issuer must be a valid account ID (G...).", nameof(issuer));
        }

        return new AssetIdentifier(StellarScheme, code, issuer);
    }

    /// <summary>
    ///     Identifies lumens: <c>stellar:native</c>.
    /// </summary>
    /// <returns>The identifier.</returns>
    public static AssetIdentifier StellarNative()
    {
        return new AssetIdentifier(StellarScheme, NativeIdentifier, null);
    }

    /// <summary>
    ///     Identifies a fiat currency: <c>iso4217:CODE</c>.
    /// </summary>
    /// <param name="currencyCode">The three-letter ISO 4217 currency code, in upper case (for example <c>USD</c>).</param>
    /// <returns>The identifier.</returns>
    /// <exception cref="ArgumentException">Thrown when the code is not three upper-case ASCII letters.</exception>
    public static AssetIdentifier Iso4217(string currencyCode)
    {
        if (!IsValidIso4217Code(currencyCode))
        {
            throw new ArgumentException("The currency code must be three upper-case ASCII letters.",
                nameof(currencyCode));
        }

        return new AssetIdentifier(Iso4217Scheme, currencyCode, null);
    }

    /// <summary>
    ///     Identifies an SDK <see cref="Asset" />: <c>stellar:native</c> or <c>stellar:CODE:ISSUER</c>.
    /// </summary>
    /// <param name="asset">A native or credit asset.</param>
    /// <returns>The identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="asset" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="asset" /> is neither native nor a credit asset (for example a liquidity pool
    ///     share), which SEP-38 cannot identify.
    /// </exception>
    public static AssetIdentifier FromAsset(Asset asset)
    {
        return asset switch
        {
            null => throw new ArgumentNullException(nameof(asset)),
            AssetTypeNative => StellarNative(),
            AssetTypeCreditAlphaNum credit => Stellar(credit.Code, credit.Issuer),
            _ => throw new ArgumentException(
                $"Only native and credit assets can be identified in SEP-38; got {asset.GetType().Name}.",
                nameof(asset)),
        };
    }

    /// <summary>
    ///     Parses a value in the SEP-38 Asset Identification Format.
    /// </summary>
    /// <param name="value">For example <c>stellar:USDC:G...</c>, <c>stellar:native</c> or <c>iso4217:USD</c>.</param>
    /// <returns>The identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     Thrown when the scheme is neither <c>stellar</c> nor <c>iso4217</c>, or the identifier is not valid for it.
    /// </exception>
    public static AssetIdentifier Parse(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (!TryParse(value, out var result))
        {
            throw new FormatException(
                "The value is not in the SEP-38 Asset Identification Format (stellar:native, stellar:CODE:ISSUER " +
                "or iso4217:CODE).");
        }

        return result!;
    }

    /// <summary>
    ///     Tries to parse a value in the SEP-38 Asset Identification Format.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="result">The identifier, or <see langword="null" /> when parsing failed.</param>
    /// <returns><see langword="true" /> when <paramref name="value" /> was parsed.</returns>
    public static bool TryParse(string? value, out AssetIdentifier? result)
    {
        result = null;
        if (value == null)
        {
            return false;
        }

        var separator = value.IndexOf(':');
        if (separator < 0)
        {
            return false;
        }

        var scheme = value.Substring(0, separator);
        var identifier = value.Substring(separator + 1);
        switch (scheme)
        {
            case StellarScheme:
                if (identifier == NativeIdentifier)
                {
                    result = StellarNative();
                    return true;
                }

                var codeEnd = identifier.IndexOf(':');
                if (codeEnd < 0)
                {
                    return false;
                }

                var code = identifier.Substring(0, codeEnd);
                var issuer = identifier.Substring(codeEnd + 1);
                if (!IsValidStellarCode(code) || !StrKey.IsValidEd25519PublicKey(issuer))
                {
                    return false;
                }

                result = new AssetIdentifier(StellarScheme, code, issuer);
                return true;
            case Iso4217Scheme:
                if (!IsValidIso4217Code(identifier))
                {
                    return false;
                }

                result = new AssetIdentifier(Iso4217Scheme, identifier, null);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    ///     Converts a Stellar identifier to the SDK <see cref="Asset" /> it names.
    /// </summary>
    /// <returns>An <see cref="AssetTypeNative" /> or a credit asset.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this identifies a fiat currency.</exception>
    public Asset ToAsset()
    {
        if (!IsStellar)
        {
            throw new InvalidOperationException($"'{this}' is not a Stellar asset.");
        }

        return IsNative ? new AssetTypeNative() : Asset.CreateNonNativeAsset(Code, Issuer!);
    }

    /// <summary>
    ///     The identifier in the SEP-38 Asset Identification Format.
    /// </summary>
    /// <returns>For example <c>stellar:USDC:G...</c>, <c>stellar:native</c> or <c>iso4217:USD</c>.</returns>
    public override string ToString()
    {
        return Issuer == null ? $"{Scheme}:{Code}" : $"{Scheme}:{Code}:{Issuer}";
    }

    /// <summary>
    ///     Converts the identifier to its SEP-38 string form, so it can be assigned to a request property.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <returns>The value of <see cref="ToString" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="identifier" /> is null.</exception>
    public static implicit operator string(AssetIdentifier identifier)
    {
        if (identifier is null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        return identifier.ToString();
    }

    /// <inheritdoc />
    public bool Equals(AssetIdentifier? other)
    {
        return other is not null && Scheme == other.Scheme && Code == other.Code && Issuer == other.Issuer;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as AssetIdentifier);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return System.HashCode.Combine(Scheme, Code, Issuer);
    }

    private static bool IsValidStellarCode(string? code)
    {
        if (code == null || code.Length < 1 || code.Length > 12)
        {
            return false;
        }

        foreach (var c in code)
        {
            if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9'))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidIso4217Code(string? code)
    {
        if (code == null || code.Length != 3)
        {
            return false;
        }

        foreach (var c in code)
        {
            if (c < 'A' || c > 'Z')
            {
                return false;
            }
        }

        return true;
    }
}
