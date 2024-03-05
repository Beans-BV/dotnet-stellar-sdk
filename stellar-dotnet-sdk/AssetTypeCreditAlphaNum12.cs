using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class AssetTypeCreditAlphaNum12 : AssetTypeCreditAlphaNum
{
    public const string RestApiType = "credit_alphanum12";

    public AssetTypeCreditAlphaNum12(string code, string issuer) : base(code, issuer)
    {
        if (code.Length is < 5 or > 12)
            throw new AssetCodeLengthInvalidException();
    }

    public override string Type => RestApiType;

    public override xdr.Asset ToXdr()
    {
        var thisXdr = new xdr.Asset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12)
        };
        var credit = new AlphaNum12
        {
            AssetCode = new AssetCode12(Util.PaddedByteArray(Code, 12))
        };
        var accountID = new AccountID
        {
            InnerValue = KeyPair.FromAccountId(Issuer).XdrPublicKey
        };
        credit.Issuer = accountID;
        thisXdr.AlphaNum12 = credit;
        return thisXdr;
    }

    public override int CompareTo(Asset asset)
    {
        if (asset.Type != RestApiType) return 1;

        var other = (AssetTypeCreditAlphaNum)asset;

        return Code != other.Code
            ? string.Compare(Code, other.Code, StringComparison.Ordinal)
            : string.Compare(Issuer, other.Issuer, StringComparison.Ordinal);
    }
}