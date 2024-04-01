using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class SetOptionsResult : OperationResult
{
    public static SetOptionsResult FromXdr(Xdr.SetOptionsResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_SUCCESS:
                return new SetOptionsSuccess();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_LOW_RESERVE:
                return new SetOptionsLowReserve();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_TOO_MANY_SIGNERS:
                return new SetOptionsTooManySigners();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_BAD_FLAGS:
                return new SetOptionsBadFlags();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_INVALID_INFLATION:
                return new SetOptionsInvalidInflation();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_CANT_CHANGE:
                return new SetOptionsCantChange();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_UNKNOWN_FLAG:
                return new SetOptionsUnknownFlag();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_THRESHOLD_OUT_OF_RANGE:
                return new SetOptionsThresholdOutOfRange();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_BAD_SIGNER:
                return new SetOptionsBadSigner();
            case SetOptionsResultCode.SetOptionsResultCodeEnum.SET_OPTIONS_INVALID_HOME_DOMAIN:
                return new SetOptionsInvalidHomeDomain();
            default:
                throw new SystemException("Unknown SetOptions type");
        }
    }
}