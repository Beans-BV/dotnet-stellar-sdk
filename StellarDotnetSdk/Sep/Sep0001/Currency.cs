using System.Collections.Generic;

namespace StellarDotnetSdk.Sep.Sep0001;

/// <summary>
///     Currency Documentation. From the stellar.toml CURRENCIES list, one set of fields for each currency supported.
///     Applicable fields should be completed and any that don't apply should be excluded.
///     See <a href="https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0001.md">SEP-0001</a>
/// </summary>
public sealed record Currency
{
    /// <summary>
    ///     Token code.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    ///     A pattern with ? as a single character wildcard. Allows a CURRENCIES entry to apply to multiple assets that share
    ///     the same info.
    ///     An example is futures, where the only difference between issues is the date of the contract. E.g. CORN???????? to
    ///     match codes such as CORN20180604.
    /// </summary>
    public string? CodeTemplate { get; init; }

    /// <summary>
    ///     Token issuer Stellar public key.
    /// </summary>
    public string? Issuer { get; init; }

    /// <summary>
    ///     Contract ID of the token contract. The token must be compatible with the SEP-41 Token Interface to be defined here.
    ///     Required for tokens that are not Stellar Assets. Omitted if the token is a Stellar Asset.
    /// </summary>
    public string? Contract { get; init; }

    /// <summary>
    ///     Status of token. One of live, dead, test, or private. Allows issuer to mark whether token is dead/for testing/for
    ///     private use
    ///     or is live and should be listed in live exchanges.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    ///     Preference for number of decimals to show when a client displays currency balance.
    /// </summary>
    public int? DisplayDecimals { get; init; }

    /// <summary>
    ///     A short name for the token.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Description of token and what it represents.
    /// </summary>
    public string? Desc { get; init; }

    /// <summary>
    ///     Conditions on token.
    /// </summary>
    public string? Conditions { get; init; }

    /// <summary>
    ///     URL to a PNG image on a transparent background representing token.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    ///     Fixed number of tokens, if the number of tokens issued will never change.
    /// </summary>
    public int? FixedNumber { get; init; }

    /// <summary>
    ///     Max number of tokens, if there will never be more than maxNumber tokens.
    /// </summary>
    public int? MaxNumber { get; init; }

    /// <summary>
    ///     The number of tokens is dilutable at the issuer's discretion.
    /// </summary>
    public bool? IsUnlimited { get; init; }

    /// <summary>
    ///     true if token can be redeemed for underlying asset, otherwise false.
    /// </summary>
    public bool? IsAssetAnchored { get; init; }

    /// <summary>
    ///     Type of asset anchored. Can be fiat, crypto, nft, stock, bond, commodity, realestate, or other.
    /// </summary>
    public string? AnchorAssetType { get; init; }

    /// <summary>
    ///     If anchored token, code / symbol for asset that token is anchored to. E.g. USD, BTC, SBUX, Address of real-estate
    ///     investment property.
    /// </summary>
    public string? AnchorAsset { get; init; }

    /// <summary>
    ///     URL to attestation or other proof, evidence, or verification of reserves, such as third-party audits.
    /// </summary>
    public string? AttestationOfReserve { get; init; }

    /// <summary>
    ///     If anchored token, these are instructions to redeem the underlying asset from tokens.
    /// </summary>
    public string? RedemptionInstructions { get; init; }

    /// <summary>
    ///     If this is an anchored crypto token, list of one or more public addresses that hold the assets for which you are
    ///     issuing tokens.
    /// </summary>
    public IReadOnlyCollection<string>? CollateralAddresses { get; init; }

    /// <summary>
    ///     Messages stating that funds in the collateralAddresses list are reserved to back the issued asset.
    /// </summary>
    public IReadOnlyCollection<string>? CollateralAddressMessages { get; init; }

    /// <summary>
    ///     These prove you control the collateralAddresses. For each address you list, sign the entry in
    ///     collateralAddressMessages
    ///     with the address's private key and add the resulting string to this list as a base64-encoded raw signature.
    /// </summary>
    public IReadOnlyCollection<string>? CollateralAddressSignatures { get; init; }

    /// <summary>
    ///     Indicates whether or not this is a sep0008 regulated asset. If missing, false is assumed.
    /// </summary>
    public bool? Regulated { get; init; }

    /// <summary>
    ///     URL of a sep0008 compliant approval service that signs validated transactions.
    /// </summary>
    public string? ApprovalServer { get; init; }

    /// <summary>
    ///     A human readable string that explains the issuer's requirements for approving transactions.
    /// </summary>
    public string? ApprovalCriteria { get; init; }

    /// <summary>
    ///     Alternately, stellar.toml can link out to a separate TOML file for each currency by specifying
    ///     toml="https://DOMAIN/.well-known/CURRENCY.toml" as the currency's only field.
    ///     In this case only this field is filled. To load the currency data, you can use
    ///     StellarToml.CurrencyFromUrlAsync(string toml).
    /// </summary>
    public string? Toml { get; init; }
}