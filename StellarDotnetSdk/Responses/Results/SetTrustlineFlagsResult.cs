using System;
using StellarDotnetSdk.Xdr;
using ResultCodeEnum = StellarDotnetSdk.Xdr.SetTrustLineFlagsResultCode.SetTrustLineFlagsResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class SetTrustlineFlagsResult : OperationResult
{
    public static SetTrustlineFlagsResult FromXdr(SetTrustLineFlagsResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.SET_TRUST_LINE_FLAGS_CANT_REVOKE => new SetTrustlineFlagsCantRevoke(),
            ResultCodeEnum.SET_TRUST_LINE_FLAGS_INVALID_STATE => new SetTrustlineFlagsInvalidState(),
            ResultCodeEnum.SET_TRUST_LINE_FLAGS_MALFORMED => new SetTrustlineFlagsMalformed(),
            ResultCodeEnum.SET_TRUST_LINE_FLAGS_NO_TRUST_LINE => new SetTrustlineFlagsNoTrustline(),
            ResultCodeEnum.SET_TRUST_LINE_FLAGS_SUCCESS => new SetTrustlineFlagsSuccess(),
            ResultCodeEnum.SET_TRUST_LINE_FLAGS_LOW_RESERVE => new SetTrustlineFlagsLowReserve(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown SetTrustlineFlagsResult type.")
        };
    }
}

public class SetTrustlineFlagsSuccess : SetTrustlineFlagsResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     This can happen for a number of reasons: the asset specified by AssetCode and AssetIssuer is invalid; the asset
///     issuer isn't the source account; the Trustor is the source account; the native asset is specified; or the flags
///     being set/cleared conflict or are otherwise invalid.
/// </summary>
public class SetTrustlineFlagsMalformed : SetTrustlineFlagsResult;

/// <summary>
///     The Trustor does not have a trustline with the issuer performing this operation.
/// </summary>
public class SetTrustlineFlagsNoTrustline : SetTrustlineFlagsResult;

/// <summary>
///     The issuer is trying to revoke the trustline authorization of Trustor, but it cannot do so because
///     AUTH_REVOCABLE_FLAG is not set on the account.
/// </summary>
public class SetTrustlineFlagsCantRevoke : SetTrustlineFlagsResult;

/// <summary>
///     If the final state of the trustline has both AUTHORIZED_FLAG (1) and AUTHORIZED_TO_MAINTAIN_LIABILITIES_FLAG (2)
///     set, which are mutually exclusive.
/// </summary>
public class SetTrustlineFlagsInvalidState : SetTrustlineFlagsResult;

/// <summary>
///     Claimable balances can't be created on revocation of asset (or pool share) trustlines associated with a liquidity
///     pool due to low reserves.
/// </summary>
public class SetTrustlineFlagsLowReserve : SetTrustlineFlagsResult;