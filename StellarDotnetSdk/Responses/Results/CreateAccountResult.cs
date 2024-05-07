using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.CreateAccountResultCode.CreateAccountResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class CreateAccountResult : OperationResult
{
    public static CreateAccountResult FromXdr(Xdr.CreateAccountResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.CREATE_ACCOUNT_SUCCESS => new CreateAccountSuccess(),
            ResultCodeEnum.CREATE_ACCOUNT_MALFORMED => new CreateAccountMalformed(),
            ResultCodeEnum.CREATE_ACCOUNT_UNDERFUNDED => new CreateAccountUnderfunded(),
            ResultCodeEnum.CREATE_ACCOUNT_LOW_RESERVE => new CreateAccountLowReserve(),
            ResultCodeEnum.CREATE_ACCOUNT_ALREADY_EXIST => new CreateAccountAlreadyExists(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown CreateAccountResult type.")
        };
    }
}

public class CreateAccountSuccess : CreateAccountResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     The destination is invalid.
/// </summary>
public class CreateAccountMalformed : CreateAccountResult;

/// <summary>
///     The source account performing the command does not have enough funds to give destination the starting balance
///     amount of XLM and still maintain its minimum XLM reserve plus satisfy its XLM selling liabilities.
/// </summary>
public class CreateAccountUnderfunded : CreateAccountResult;

/// <summary>
///     This operation would create an account with fewer than the minimum number of XLM an account must hold.
/// </summary>
public class CreateAccountLowReserve : CreateAccountResult;

/// <summary>
///     The destination account already exists.
/// </summary>
public class CreateAccountAlreadyExists : CreateAccountResult;