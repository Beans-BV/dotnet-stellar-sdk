using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Represents a Stellar credit asset with an alphanumeric code between 5 and 12 characters long.
/// </summary>
public class AssetTypeCreditAlphaNum12 : AssetTypeCreditAlphaNum
{
    /// <summary>
    ///     The Horizon REST API type identifier for alphanumeric 12-character assets.
    /// </summary>
    public const string RestApiType = "credit_alphanum12";

    /// <summary>
    ///     Initializes a new instance of <see cref="AssetTypeCreditAlphaNum12" /> with the specified asset code and issuer.
    /// </summary>
    /// <param name="code">The asset code (5-12 alphanumeric characters).</param>
    /// <param name="issuer">The Stellar account ID of the asset issuer.</param>
    /// <exception cref="StellarDotnetSdk.Exceptions.AssetCodeLengthInvalidException">Thrown when the code length is not between 5 and 12.</exception>
    public AssetTypeCreditAlphaNum12(string code, string issuer) : base(code, issuer)
    {
        if (code.Length is < 5 or > 12)
        {
            throw new AssetCodeLengthInvalidException();
        }
    }

    /// <inheritdoc />
    public override string Type => RestApiType;

    /// <inheritdoc />
    public override Xdr.Asset ToXdr()
    {
        var thisXdr = new Xdr.Asset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12),
        };
        var credit = new AlphaNum12
        {
            AssetCode = new AssetCode12(Util.PaddedByteArray(Code, 12)),
        };
        var accountId = new AccountID
        {
            InnerValue = KeyPair.FromAccountId(Issuer).XdrPublicKey,
        };
        credit.Issuer = accountId;
        thisXdr.AlphaNum12 = credit;
        return thisXdr;
    }

    /// <inheritdoc />
    public override int CompareTo(Asset asset)
    {
        if (asset.Type != RestApiType)
        {
            return 1;
        }

        var other = (AssetTypeCreditAlphaNum)asset;

        return Code != other.Code
            ? string.Compare(Code, other.Code, StringComparison.Ordinal)
            : string.Compare(Issuer, other.Issuer, StringComparison.Ordinal);
    }
}