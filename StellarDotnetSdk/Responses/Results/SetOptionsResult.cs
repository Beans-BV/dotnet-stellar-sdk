using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.SetOptionsResultCode.SetOptionsResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class SetOptionsResult : OperationResult
{
    public static SetOptionsResult FromXdr(Xdr.SetOptionsResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.SET_OPTIONS_SUCCESS => new SetOptionsSuccess(),
            ResultCodeEnum.SET_OPTIONS_LOW_RESERVE => new SetOptionsLowReserve(),
            ResultCodeEnum.SET_OPTIONS_TOO_MANY_SIGNERS => new SetOptionsTooManySigners(),
            ResultCodeEnum.SET_OPTIONS_BAD_FLAGS => new SetOptionsBadFlags(),
            ResultCodeEnum.SET_OPTIONS_INVALID_INFLATION => new SetOptionsInvalidInflation(),
            ResultCodeEnum.SET_OPTIONS_CANT_CHANGE => new SetOptionsCantChange(),
            ResultCodeEnum.SET_OPTIONS_UNKNOWN_FLAG => new SetOptionsUnknownFlag(),
            ResultCodeEnum.SET_OPTIONS_THRESHOLD_OUT_OF_RANGE => new SetOptionsThresholdOutOfRange(),
            ResultCodeEnum.SET_OPTIONS_BAD_SIGNER => new SetOptionsBadSigner(),
            ResultCodeEnum.SET_OPTIONS_INVALID_HOME_DOMAIN => new SetOptionsInvalidHomeDomain(),
            ResultCodeEnum.SET_OPTIONS_AUTH_REVOCABLE_REQUIRED => new SetOptionsAuthRevocableRequired(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown SetOptionsResult type.")
        };
    }
}

public class SetOptionsSuccess : SetOptionsResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     This account does not have enough XLM to satisfy the minimum XLM reserve increase caused by adding a subentry and
///     still satisfy its XLM selling liabilities. For every new signer added to an account, the minimum reserve of XLM
///     that account must hold increases.
/// </summary>
public class SetOptionsLowReserve : SetOptionsResult;

/// <summary>
///     20 is the maximum number of signers an account can have, and adding another signer would exceed that.
/// </summary>
public class SetOptionsTooManySigners : SetOptionsResult;

/// <summary>
///     The flags set and/or cleared are invalid by themselves or in combination.
/// </summary>
public class SetOptionsBadFlags : SetOptionsResult;

/// <summary>
///     The destination account set in the inflation field does not exist.
/// </summary>
public class SetOptionsInvalidInflation : SetOptionsResult;

/// <summary>
///     This account can no longer change the option it wants to change.
/// </summary>
public class SetOptionsCantChange : SetOptionsResult;

/// <summary>
///     The account is trying to set a flag that is unknown.
/// </summary>
public class SetOptionsUnknownFlag : SetOptionsResult;

/// <summary>
///     The value for a key weight or threshold is invalid.
/// </summary>
public class SetOptionsThresholdOutOfRange : SetOptionsResult;

/// <summary>
///     Any additional signers added to the account cannot be the master key.
/// </summary>
public class SetOptionsBadSigner : SetOptionsResult;

/// <summary>
///     Home domain is malformed.
/// </summary>
public class SetOptionsInvalidHomeDomain : SetOptionsResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class SetOptionsAuthRevocableRequired : SetOptionsResult;