using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.ChangeTrustResultCode.ChangeTrustResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class ChangeTrustResult : OperationResult
{
    public static ChangeTrustResult FromXdr(Xdr.ChangeTrustResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.CHANGE_TRUST_SUCCESS => new ChangeTrustSuccess(),
            ResultCodeEnum.CHANGE_TRUST_MALFORMED => new ChangeTrustMalformed(),
            ResultCodeEnum.CHANGE_TRUST_NO_ISSUER => new ChangeTrustNoIssuer(),
            ResultCodeEnum.CHANGE_TRUST_INVALID_LIMIT => new ChangeTrustInvalidLimit(),
            ResultCodeEnum.CHANGE_TRUST_LOW_RESERVE => new ChangeTrustLowReserve(),
            ResultCodeEnum.CHANGE_TRUST_SELF_NOT_ALLOWED => new ChangeTrustSelfNotAllowed(),
            ResultCodeEnum.CHANGE_TRUST_TRUST_LINE_MISSING => new ChangeTrustTrustlineMissing(),
            ResultCodeEnum.CHANGE_TRUST_CANNOT_DELETE => new ChangeTrustCannotDelete(),
            ResultCodeEnum.CHANGE_TRUST_NOT_AUTH_MAINTAIN_LIABILITIES => new ChangeTrustNotAuthMaintainLiabilities(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown ChangeTrust type.")
        };
    }
}

public class ChangeTrustSuccess : ChangeTrustResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     The input to this operation is invalid.
/// </summary>
public class ChangeTrustMalformed : ChangeTrustResult;

/// <summary>
///     The issuer of the asset cannot be found.
/// </summary>
public class ChangeTrustNoIssuer : ChangeTrustResult;

/// <summary>
///     The limit is not sufficient to hold the current balance of the trustline and still satisfy its buying liabilities.
///     This error occurs when attempting to remove a trustline with a non-zero asset balance.
/// </summary>
public class ChangeTrustInvalidLimit : ChangeTrustResult;

/// <summary>
///     This account does not have enough XLM to satisfy the minimum XLM reserve increase caused by adding a subentry and
///     still satisfy its XLM selling liabilities. For every new trustline added to an account, the minimum reserve of XLM
///     that account must hold increases.
/// </summary>
public class ChangeTrustLowReserve : ChangeTrustResult;

/// <summary>
///     The source account attempted to create a trustline for itself, which is not allowed.
/// </summary>
public class ChangeTrustSelfNotAllowed : ChangeTrustResult;

/// <summary>
///     The asset trustline is missing for the liquidity pool.
/// </summary>
public class ChangeTrustTrustlineMissing : ChangeTrustResult;

/// <summary>
///     The asset trustline is still referenced by a liquidity pool.
/// </summary>
public class ChangeTrustCannotDelete : ChangeTrustResult;

/// <summary>
///     The asset trustline is deauthorized.
/// </summary>
public class ChangeTrustNotAuthMaintainLiabilities : ChangeTrustResult;