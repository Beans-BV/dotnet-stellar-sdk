// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

using System;

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  enum SetTrustLineFlagsResultCode
//  {
//      // codes considered as "success" for the operation
//      SET_TRUST_LINE_FLAGS_SUCCESS = 0,
//  
//      // codes considered as "failure" for the operation
//      SET_TRUST_LINE_FLAGS_MALFORMED = -1,
//      SET_TRUST_LINE_FLAGS_NO_TRUST_LINE = -2,
//      SET_TRUST_LINE_FLAGS_CANT_REVOKE = -3,
//      SET_TRUST_LINE_FLAGS_INVALID_STATE = -4,
//      SET_TRUST_LINE_FLAGS_LOW_RESERVE = -5 // claimable balances can't be created
//                                            // on revoke due to low reserves
//  };

//  ===========================================================================
public class SetTrustLineFlagsResultCode
{
    public enum SetTrustLineFlagsResultCodeEnum
    {
        SET_TRUST_LINE_FLAGS_SUCCESS = 0,
        SET_TRUST_LINE_FLAGS_MALFORMED = -1,
        SET_TRUST_LINE_FLAGS_NO_TRUST_LINE = -2,
        SET_TRUST_LINE_FLAGS_CANT_REVOKE = -3,
        SET_TRUST_LINE_FLAGS_INVALID_STATE = -4,
        SET_TRUST_LINE_FLAGS_LOW_RESERVE = -5,
    }

    public SetTrustLineFlagsResultCodeEnum InnerValue { get; set; }

    public static SetTrustLineFlagsResultCode Create(SetTrustLineFlagsResultCodeEnum v)
    {
        return new SetTrustLineFlagsResultCode
        {
            InnerValue = v,
        };
    }

    public static SetTrustLineFlagsResultCode Decode(XdrDataInputStream stream)
    {
        var value = stream.ReadInt();
        switch (value)
        {
            case 0: return Create(SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_SUCCESS);
            case -1: return Create(SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_MALFORMED);
            case -2: return Create(SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_NO_TRUST_LINE);
            case -3: return Create(SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_CANT_REVOKE);
            case -4: return Create(SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_INVALID_STATE);
            case -5: return Create(SetTrustLineFlagsResultCodeEnum.SET_TRUST_LINE_FLAGS_LOW_RESERVE);
            default:
                throw new Exception("Unknown enum value: " + value);
        }
    }

    public static void Encode(XdrDataOutputStream stream, SetTrustLineFlagsResultCode value)
    {
        stream.WriteInt((int)value.InnerValue);
    }
}