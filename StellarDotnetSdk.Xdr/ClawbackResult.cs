// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  union ClawbackResult switch (ClawbackResultCode code)
//  {
//  case CLAWBACK_SUCCESS:
//      void;
//  case CLAWBACK_MALFORMED:
//  case CLAWBACK_NOT_CLAWBACK_ENABLED:
//  case CLAWBACK_NO_TRUST:
//  case CLAWBACK_UNDERFUNDED:
//      void;
//  };

//  ===========================================================================
public class ClawbackResult
{
    public ClawbackResultCode Discriminant { get; set; } = new();

    public static void Encode(XdrDataOutputStream stream, ClawbackResult encodedClawbackResult)
    {
        stream.WriteInt((int)encodedClawbackResult.Discriminant.InnerValue);
        switch (encodedClawbackResult.Discriminant.InnerValue)
        {
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_SUCCESS:
                break;
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_MALFORMED:
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NOT_CLAWBACK_ENABLED:
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NO_TRUST:
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_UNDERFUNDED:
                break;
        }
    }

    public static ClawbackResult Decode(XdrDataInputStream stream)
    {
        var decodedClawbackResult = new ClawbackResult();
        var discriminant = ClawbackResultCode.Decode(stream);
        decodedClawbackResult.Discriminant = discriminant;
        switch (decodedClawbackResult.Discriminant.InnerValue)
        {
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_SUCCESS:
                break;
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_MALFORMED:
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NOT_CLAWBACK_ENABLED:
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_NO_TRUST:
            case ClawbackResultCode.ClawbackResultCodeEnum.CLAWBACK_UNDERFUNDED:
                break;
        }

        return decodedClawbackResult;
    }
}