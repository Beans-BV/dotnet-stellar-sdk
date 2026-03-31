using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;
using static System.String;

namespace StellarDotnetSdk.Assets;

/// <summary>
///     Represents a Stellar credit asset with an alphanumeric code between 1 and 4 characters long.
/// </summary>
public class AssetTypeCreditAlphaNum4 : AssetTypeCreditAlphaNum
{
    /// <summary>
    ///     The Horizon REST API type identifier for alphanumeric 4-character assets.
    /// </summary>
    public const string RestApiType = "credit_alphanum4";

    /// <summary>
    ///     Initializes a new instance of <see cref="AssetTypeCreditAlphaNum4" /> with the specified asset code and issuer.
    /// </summary>
    /// <param name="code">The asset code (1-4 alphanumeric characters).</param>
    /// <param name="issuer">The Stellar account ID of the asset issuer.</param>
    /// <exception cref="StellarDotnetSdk.Exceptions.AssetCodeLengthInvalidException">Thrown when the code length is not between 1 and 4.</exception>
    public AssetTypeCreditAlphaNum4(string code, string issuer) : base(code, issuer)
    {
        if (code.Length is < 1 or > 4)
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
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4),
        };
        var credit = new AlphaNum4
        {
            AssetCode = new AssetCode4(Util.PaddedByteArray(Code, 4)),
        };
        var accountId = new AccountID
        {
            InnerValue = KeyPair.FromAccountId(Issuer).XdrPublicKey,
        };
        credit.Issuer = accountId;
        thisXdr.AlphaNum4 = credit;
        return thisXdr;
    }

    /// <inheritdoc />
    public override int CompareTo(Asset asset)
    {
        switch (asset.Type)
        {
            case AssetTypeCreditAlphaNum12.RestApiType:
                return -1;
            case AssetTypeNative.RestApiType:
                return 1;
        }

        var other = (AssetTypeCreditAlphaNum)asset;

        return Code != other.Code
            ? Compare(Code, other.Code, StringComparison.Ordinal)
            : Compare(Issuer, other.Issuer, StringComparison.Ordinal);
    }
}