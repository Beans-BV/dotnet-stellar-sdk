using System;
using stellar_dotnet_sdk.xdr;
using static System.String;

namespace stellar_dotnet_sdk;

public class AssetTypeCreditAlphaNum4 : AssetTypeCreditAlphaNum
{
    public const string RestApiType = "credit_alphanum4";

    public AssetTypeCreditAlphaNum4(string code, string issuer) : base(code, issuer)
    {
        if (code.Length is < 1 or > 4)
            throw new AssetCodeLengthInvalidException();
    }

    public override string Type => RestApiType;

    public override xdr.Asset ToXdr()
    {
        var thisXdr = new xdr.Asset
        {
            Discriminant = AssetType.Create(AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4)
        };
        var credit = new AlphaNum4
        {
            AssetCode = new AssetCode4(Util.PaddedByteArray(Code, 4))
        };
        var accountID = new AccountID
        {
            InnerValue = KeyPair.FromAccountId(Issuer).XdrPublicKey
        };
        credit.Issuer = accountID;
        thisXdr.AlphaNum4 = credit;
        return thisXdr;
    }

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