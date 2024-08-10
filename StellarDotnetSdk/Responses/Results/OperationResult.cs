using System;
using OperationResultCodeEnum = StellarDotnetSdk.Xdr.OperationResultCode.OperationResultCodeEnum;
using OperationTypeEnum = StellarDotnetSdk.Xdr.OperationType.OperationTypeEnum;

namespace StellarDotnetSdk.Responses.Results;

public class OperationResult
{
    /// <summary>
    ///     Whether the operation was successful.
    /// </summary>
    public virtual bool IsSuccess => false;

    public static OperationResult FromXdr(Xdr.OperationResult result)
    {
        return result.Discriminant.InnerValue switch
        {
            OperationResultCodeEnum.opINNER => InnerOperationResultFromXdr(result.Tr),
            OperationResultCodeEnum.opBAD_AUTH => new OperationResultBadAuth(),
            OperationResultCodeEnum.opNO_ACCOUNT => new OperationResultNoAccount(),
            OperationResultCodeEnum.opNOT_SUPPORTED => new OperationResultNotSupported(),
            OperationResultCodeEnum.opTOO_MANY_SUBENTRIES => new OperationResultTooManySubEntries(),
            OperationResultCodeEnum.opEXCEEDED_WORK_LIMIT => new OperationResultExceededWorkLimit(),
            OperationResultCodeEnum.opTOO_MANY_SPONSORING => new OperationResultTooManySponsoring(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown OperationResult type."),
        };
    }

    private static OperationResult InnerOperationResultFromXdr(Xdr.OperationResult.OperationResultTr result)
    {
        return result.Discriminant.InnerValue switch
        {
            OperationTypeEnum.CREATE_ACCOUNT
                => CreateAccountResult.FromXdr(result.CreateAccountResult),
            OperationTypeEnum.PAYMENT
                => PaymentResult.FromXdr(result.PaymentResult),
            OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE
                => PathPaymentStrictReceiveResult.FromXdr(result.PathPaymentStrictReceiveResult),
            OperationTypeEnum.MANAGE_BUY_OFFER
                => ManageBuyOfferResult.FromXdr(result.ManageBuyOfferResult),
            OperationTypeEnum.MANAGE_SELL_OFFER
                => ManageSellOfferResult.FromXdr(result.ManageSellOfferResult),
            OperationTypeEnum.CREATE_PASSIVE_SELL_OFFER
                => ManageSellOfferResult.FromXdr(result.CreatePassiveSellOfferResult),
            OperationTypeEnum.SET_OPTIONS
                => SetOptionsResult.FromXdr(result.SetOptionsResult),
            OperationTypeEnum.CHANGE_TRUST
                => ChangeTrustResult.FromXdr(result.ChangeTrustResult),
            OperationTypeEnum.ALLOW_TRUST
                => AllowTrustResult.FromXdr(result.AllowTrustResult),
            OperationTypeEnum.ACCOUNT_MERGE
                => AccountMergeResult.FromXdr(result.AccountMergeResult),
            OperationTypeEnum.INFLATION
                => InflationResult.FromXdr(result.InflationResult),
            OperationTypeEnum.MANAGE_DATA
                => ManageDataResult.FromXdr(result.ManageDataResult),
            OperationTypeEnum.BUMP_SEQUENCE
                => BumpSequenceResult.FromXdr(result.BumpSeqResult),
            OperationTypeEnum.PATH_PAYMENT_STRICT_SEND
                => PathPaymentStrictSendResult.FromXdr(result.PathPaymentStrictSendResult),
            OperationTypeEnum.CREATE_CLAIMABLE_BALANCE
                => CreateClaimableBalanceResult.FromXdr(result.CreateClaimableBalanceResult),
            OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE
                => ClaimClaimableBalanceResult.FromXdr(result.ClaimClaimableBalanceResult),
            OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES
                => BeginSponsoringFutureReservesResult.FromXdr(result.BeginSponsoringFutureReservesResult),
            OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES
                => EndSponsoringFutureReservesResult.FromXdr(result.EndSponsoringFutureReservesResult),
            OperationTypeEnum.REVOKE_SPONSORSHIP
                => RevokeSponsorshipResult.FromXdr(result.RevokeSponsorshipResult),
            OperationTypeEnum.CLAWBACK
                => ClawbackResult.FromXdr(result.ClawbackResult),
            OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE
                => ClawbackClaimableBalanceResult.FromXdr(result.ClawbackClaimableBalanceResult),
            OperationTypeEnum.SET_TRUST_LINE_FLAGS
                => SetTrustlineFlagsResult.FromXdr(result.SetTrustLineFlagsResult),
            OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT
                => LiquidityPoolDepositResult.FromXdr(result.LiquidityPoolDepositResult),
            OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW
                => LiquidityPoolWithdrawResult.FromXdr(result.LiquidityPoolWithdrawResult),
            OperationTypeEnum.INVOKE_HOST_FUNCTION
                => InvokeHostFunctionResult.FromXdr(result.InvokeHostFunctionResult),
            OperationTypeEnum.EXTEND_FOOTPRINT_TTL
                => ExtendFootprintTtlResult.FromXdr(result.ExtendFootprintTTLResult),
            OperationTypeEnum.RESTORE_FOOTPRINT
                => RestoreFootprintResult.FromXdr(result.RestoreFootprintResult),
            _ => throw new ArgumentOutOfRangeException(nameof(result), "Unknown OperationType."),
        };
    }
}

/// <summary>
///     There are too few valid signatures, or the transaction was submitted to the wrong network.
/// </summary>
public class OperationResultBadAuth : OperationResult;

/// <summary>
///     The source account was not found.
/// </summary>
public class OperationResultNoAccount : OperationResult;

/// <summary>
///     The operation is not supported at this time.
/// </summary>
public class OperationResultNotSupported : OperationResult;

/// <summary>
///     Max number of subentries (1000) already reached
/// </summary>
public class OperationResultTooManySubEntries : OperationResult;

/// <summary>
///     Operation did too much work
/// </summary>
public class OperationResultExceededWorkLimit : OperationResult;

/// <summary>
///     TODO Missing on official docs
/// </summary>
public class OperationResultTooManySponsoring : OperationResult;