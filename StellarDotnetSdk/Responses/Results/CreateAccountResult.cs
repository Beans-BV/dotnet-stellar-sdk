using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class CreateAccountResult : OperationResult
{
    public static CreateAccountResult FromXdr(Xdr.CreateAccountResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case CreateAccountResultCode.CreateAccountResultCodeEnum.CREATE_ACCOUNT_SUCCESS:
                return new CreateAccountSuccess();
            case CreateAccountResultCode.CreateAccountResultCodeEnum.CREATE_ACCOUNT_MALFORMED:
                return new CreateAccountMalformed();
            case CreateAccountResultCode.CreateAccountResultCodeEnum.CREATE_ACCOUNT_UNDERFUNDED:
                return new CreateAccountUnderfunded();
            case CreateAccountResultCode.CreateAccountResultCodeEnum.CREATE_ACCOUNT_LOW_RESERVE:
                return new CreateAccountLowReserve();
            case CreateAccountResultCode.CreateAccountResultCodeEnum.CREATE_ACCOUNT_ALREADY_EXIST:
                return new CreateAccountAlreadyExists();
            default:
                throw new SystemException("Unknown CreateAccountResult type");
        }
    }
}