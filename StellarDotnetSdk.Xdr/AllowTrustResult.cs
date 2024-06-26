// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  union AllowTrustResult switch (AllowTrustResultCode code)
//  {
//  case ALLOW_TRUST_SUCCESS:
//      void;
//  case ALLOW_TRUST_MALFORMED:
//  case ALLOW_TRUST_NO_TRUST_LINE:
//  case ALLOW_TRUST_TRUST_NOT_REQUIRED:
//  case ALLOW_TRUST_CANT_REVOKE:
//  case ALLOW_TRUST_SELF_NOT_ALLOWED:
//  case ALLOW_TRUST_LOW_RESERVE:
//      void;
//  };

//  ===========================================================================
public class AllowTrustResult
{
    public AllowTrustResultCode Discriminant { get; set; } = new();

    public static void Encode(XdrDataOutputStream stream, AllowTrustResult encodedAllowTrustResult)
    {
        stream.WriteInt((int)encodedAllowTrustResult.Discriminant.InnerValue);
        switch (encodedAllowTrustResult.Discriminant.InnerValue)
        {
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_SUCCESS:
                break;
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_MALFORMED:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_NO_TRUST_LINE:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_TRUST_NOT_REQUIRED:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_CANT_REVOKE:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_SELF_NOT_ALLOWED:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_LOW_RESERVE:
                break;
        }
    }

    public static AllowTrustResult Decode(XdrDataInputStream stream)
    {
        var decodedAllowTrustResult = new AllowTrustResult();
        var discriminant = AllowTrustResultCode.Decode(stream);
        decodedAllowTrustResult.Discriminant = discriminant;
        switch (decodedAllowTrustResult.Discriminant.InnerValue)
        {
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_SUCCESS:
                break;
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_MALFORMED:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_NO_TRUST_LINE:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_TRUST_NOT_REQUIRED:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_CANT_REVOKE:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_SELF_NOT_ALLOWED:
            case AllowTrustResultCode.AllowTrustResultCodeEnum.ALLOW_TRUST_LOW_RESERVE:
                break;
        }

        return decodedAllowTrustResult;
    }
}