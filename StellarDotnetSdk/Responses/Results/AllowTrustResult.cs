using System;
using ResultCodeEnum = StellarDotnetSdk.Xdr.AllowTrustResultCode.AllowTrustResultCodeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class AllowTrustResult : OperationResult
{
    public static AllowTrustResult FromXdr(Xdr.AllowTrustResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            ResultCodeEnum.ALLOW_TRUST_SUCCESS => new AllowTrustSuccess(),
            ResultCodeEnum.ALLOW_TRUST_MALFORMED => new AllowTrustMalformed(),
            ResultCodeEnum.ALLOW_TRUST_NO_TRUST_LINE => new AllowTrustNoTrustline(),
            ResultCodeEnum.ALLOW_TRUST_TRUST_NOT_REQUIRED => new AllowTrustNotRequired(),
            ResultCodeEnum.ALLOW_TRUST_CANT_REVOKE => new AllowTrustCantRevoke(),
            ResultCodeEnum.ALLOW_TRUST_SELF_NOT_ALLOWED => new AllowTrustSelfNotAllowed(),
            ResultCodeEnum.ALLOW_TRUST_LOW_RESERVE => new AllowTrustLowReserve(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown AllowTrustResult type."),
        };
    }
}

/// <summary>
///     Operation successful.
/// </summary>
public class AllowTrustSuccess : AllowTrustResult
{
    public override bool IsSuccess => true;
}

/// <summary>
///     The asset specified in type is invalid. In addition, this error happens when the native asset is specified.
/// </summary>
public class AllowTrustMalformed : AllowTrustResult;

/// <summary>
///     The trustor does not have a trustline with the issuer performing this operation.
/// </summary>
public class AllowTrustNoTrustline : AllowTrustResult;

/// <summary>
///     The source account (issuer performing this operation) does not require trust. In other words, it does not have the
///     flag AUTH_REQUIRED_FLAG set.
/// </summary>
public class AllowTrustNotRequired : AllowTrustResult;

/// <summary>
///     The source account is trying to revoke the trustline of the trustor, but it cannot do so.
/// </summary>
public class AllowTrustCantRevoke : AllowTrustResult;

/// <summary>
///     The source account attempted to allow a trustline for itself, which is not allowed because an account cannot create
///     a trustline with itself.
/// </summary>
public class AllowTrustSelfNotAllowed : AllowTrustResult;

/// <summary>
///     Claimable balances can't be created on revocation of asset (or pool share) trustlines associated with a liquidity
///     pool due to low reserves.
/// </summary>
public class AllowTrustLowReserve : AllowTrustResult;