using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class ManageDataResult : OperationResult
{
    public static ManageDataResult FromXdr(Xdr.ManageDataResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_SUCCESS:
                return new ManageDataSuccess();
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_NOT_SUPPORTED_YET:
                return new ManageDataNotSupportedYet();
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_NAME_NOT_FOUND:
                return new ManageDataNameNotFound();
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_LOW_RESERVE:
                return new ManageDataLowReserve();
            case ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_INVALID_NAME:
                return new ManageDataInvalidName();
            default:
                throw new SystemException("Unknown ManageData type");
        }
    }
}