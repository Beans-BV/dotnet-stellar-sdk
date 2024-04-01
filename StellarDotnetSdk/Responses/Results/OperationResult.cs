using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Responses.Results;

public class OperationResult
{
    /// <summary>
    ///     Whether the operation was successful.
    /// </summary>
    public virtual bool IsSuccess => false;

    public static OperationResult FromXdr(Xdr.OperationResult result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case OperationResultCode.OperationResultCodeEnum.opBAD_AUTH:
                return new OperationResultBadAuth();
            case OperationResultCode.OperationResultCodeEnum.opNO_ACCOUNT:
                return new OperationResultNoAccount();
            case OperationResultCode.OperationResultCodeEnum.opNOT_SUPPORTED:
                return new OperationResultNotSupported();
            case OperationResultCode.OperationResultCodeEnum.opINNER:
                return InnerOperationResultFromXdr(result.Tr);
            default:
                throw new SystemException("Unknown OperationResult type");
        }
    }

    private static OperationResult InnerOperationResultFromXdr(Xdr.OperationResult.OperationResultTr result)
    {
        switch (result.Discriminant.InnerValue)
        {
            case OperationType.OperationTypeEnum.CREATE_ACCOUNT:
                return CreateAccountResult.FromXdr(result.CreateAccountResult);
            case OperationType.OperationTypeEnum.PAYMENT:
                return PaymentResult.FromXdr(result.PaymentResult);
            case OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE:
                return PathPaymentStrictReceiveResult.FromXdr(result.PathPaymentStrictReceiveResult);
            case OperationType.OperationTypeEnum.MANAGE_BUY_OFFER:
                return ManageBuyOfferResult.FromXdr(result.ManageBuyOfferResult);
            case OperationType.OperationTypeEnum.MANAGE_SELL_OFFER:
                return ManageSellOfferResult.FromXdr(result.ManageSellOfferResult);
            case OperationType.OperationTypeEnum.CREATE_PASSIVE_SELL_OFFER:
                return ManageSellOfferResult.FromXdr(result.CreatePassiveSellOfferResult);
            case OperationType.OperationTypeEnum.SET_OPTIONS:
                return SetOptionsResult.FromXdr(result.SetOptionsResult);
            case OperationType.OperationTypeEnum.CHANGE_TRUST:
                return ChangeTrustResult.FromXdr(result.ChangeTrustResult);
            case OperationType.OperationTypeEnum.ALLOW_TRUST:
                return AllowTrustResult.FromXdr(result.AllowTrustResult);
            case OperationType.OperationTypeEnum.ACCOUNT_MERGE:
                return AccountMergeResult.FromXdr(result.AccountMergeResult);
            case OperationType.OperationTypeEnum.INFLATION:
                return InflationResult.FromXdr(result.InflationResult);
            case OperationType.OperationTypeEnum.MANAGE_DATA:
                return ManageDataResult.FromXdr(result.ManageDataResult);
            case OperationType.OperationTypeEnum.BUMP_SEQUENCE:
                return BumpSequenceResult.FromXdr(result.BumpSeqResult);
            case OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_SEND:
                return PathPaymentStrictSendResult.FromXdr(result.PathPaymentStrictSendResult);
            case OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE:
                return CreateClaimableBalanceResult.FromXdr(result.CreateClaimableBalanceResult);
            case OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE:
                return ClaimClaimableBalanceResult.FromXdr(result.ClaimClaimableBalanceResult);
            case OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES:
                return BeginSponsoringFutureReservesResult.FromXdr(result.BeginSponsoringFutureReservesResult);
            case OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES:
                return EndSponsoringFutureReservesResult.FromXdr(result.EndSponsoringFutureReservesResult);
            case OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP:
                return RevokeSponsorshipResult.FromXdr(result.RevokeSponsorshipResult);
            case OperationType.OperationTypeEnum.CLAWBACK:
                return ClawbackResult.FromXdr(result.ClawbackResult);
            case OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE:
                return ClawbackClaimableBalanceResult.FromXdr(result.ClawbackClaimableBalanceResult);
            case OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS:
                return SetTrustlineFlagsResult.FromXdr(result.SetTrustLineFlagsResult);
            default:
                throw new SystemException("Unknown OperationType");
        }
    }
}