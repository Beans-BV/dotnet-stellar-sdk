using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     An individual command that modifies the ledger. Operations are used to send payments, enter orders into the
///     decentralized exchange,
///     change settings on accounts, and authorize accounts to hold assets.
/// </summary>
public abstract class Operation
{
    protected Operation(IAccountId? sourceAccount)
    {
        SourceAccount = sourceAccount;
    }

    /// <summary>
    ///     The account to execute this operation upon.
    /// </summary>
    public IAccountId? SourceAccount { get; protected set; }

    /// <summary>
    ///     Threshold level for the operation.
    /// </summary>
    public virtual OperationThreshold Threshold => OperationThreshold.MEDIUM;

    public static long ToXdrAmount(string value)
    {
        return Amount.ToXdr(value);
    }

    public static string FromXdrAmount(long value)
    {
        return Amount.FromXdr(value);
    }

    /// <summary>
    ///     Generates Operation XDR object.
    /// </summary>
    public Xdr.Operation ToXdr()
    {
        var thisXdr = new Xdr.Operation();
        if (SourceAccount != null)
        {
            thisXdr.SourceAccount = SourceAccount.MuxedAccount;
        }

        thisXdr.Body = ToOperationBody();
        return thisXdr;
    }

    /// <summary>
    ///     Returns base64-encoded <see cref="Xdr.Operation" /> XDR string.
    /// </summary>
    public string ToXdrBase64()
    {
        var operation = ToXdr();
        var writer = new XdrDataOutputStream();
        Xdr.Operation.Encode(writer, operation);
        return Convert.ToBase64String(writer.ToArray());
    }

    /// <returns>new Operation object from Operation XDR object.</returns>
    /// <param name="thisXdr">XDR object</param>
    public static Operation FromXdr(Xdr.Operation thisXdr)
    {
        var body = thisXdr.Body;
        Operation operation = body.Discriminant.InnerValue switch
        {
            OperationType.OperationTypeEnum.CREATE_ACCOUNT => CreateAccountOperation.FromXdr(
                body.CreateAccountOp),
            OperationType.OperationTypeEnum.PAYMENT => PaymentOperation.FromXdr(body.PaymentOp),
            OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE =>
                PathPaymentStrictReceiveOperation.FromXdr(body.PathPaymentStrictReceiveOp),
            OperationType.OperationTypeEnum.MANAGE_SELL_OFFER => ManageSellOfferOperation.FromXdr(
                body.ManageSellOfferOp),
            OperationType.OperationTypeEnum.CREATE_PASSIVE_SELL_OFFER =>
                CreatePassiveSellOfferOperation.FromXdr(body.CreatePassiveSellOfferOp),
            OperationType.OperationTypeEnum.SET_OPTIONS =>
                SetOptionsOperation.FromXdr(body.SetOptionsOp),
            OperationType.OperationTypeEnum.CHANGE_TRUST => ChangeTrustOperation.FromXdr(body.ChangeTrustOp),
            OperationType.OperationTypeEnum.ALLOW_TRUST =>
                throw new NotSupportedException("AllowTrustOperation is no longer supported."),
            OperationType.OperationTypeEnum.ACCOUNT_MERGE =>
                new AccountMergeOperation(body.Destination),
            OperationType.OperationTypeEnum.INFLATION => throw new NotSupportedException(
                "InflationOperation is no longer supported."),
            OperationType.OperationTypeEnum.MANAGE_DATA =>
                ManageDataOperation.FromXdr(body.ManageDataOp),
            OperationType.OperationTypeEnum.BUMP_SEQUENCE => new BumpSequenceOperation(body.BumpSequenceOp
                .BumpTo.InnerValue.InnerValue),
            OperationType.OperationTypeEnum.MANAGE_BUY_OFFER => ManageBuyOfferOperation.FromXdr(
                body.ManageBuyOfferOp),
            OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_SEND => PathPaymentStrictSendOperation.FromXdr(
                body.PathPaymentStrictSendOp),
            OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE => CreateClaimableBalanceOperation.FromXdr(
                body.CreateClaimableBalanceOp),
            OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE => new ClaimClaimableBalanceOperation(
                ClaimableBalanceUtils.ToHexString(body.ClaimClaimableBalanceOp.BalanceID)),
            OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES =>
                new BeginSponsoringFutureReservesOperation(
                    KeyPair.FromXdrPublicKey(body.BeginSponsoringFutureReservesOp.SponsoredID.InnerValue)),
            OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES =>
                new EndSponsoringFutureReservesOperation(),
            OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP =>
                body.RevokeSponsorshipOp.Discriminant.InnerValue switch
                {
                    RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_LEDGER_ENTRY =>
                        new RevokeLedgerEntrySponsorshipOperation(
                            LedgerKey.FromXdr(body.RevokeSponsorshipOp.LedgerKey)),
                    RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_SIGNER =>
                        RevokeSignerSponsorshipOperation.FromXdr(body.RevokeSponsorshipOp.Signer),
                    _ => throw new ArgumentOutOfRangeException(nameof(body.RevokeSponsorshipOp.Discriminant),
                        "Invalid RevokeSponsorshipTypeEnum."),
                },
            OperationType.OperationTypeEnum.CLAWBACK => ClawbackOperation.FromXdr(body.ClawbackOp),
            OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE =>
                new ClawbackClaimableBalanceOperation(
                    ClaimableBalanceUtils.ToHexString(body.ClawbackClaimableBalanceOp.BalanceID)),
            OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS => SetTrustlineFlagsOperation.FromXdr(
                body.SetTrustLineFlagsOp),
            OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT => LiquidityPoolDepositOperation.FromXdr(
                body.LiquidityPoolDepositOp),
            OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW => LiquidityPoolWithdrawOperation.FromXdr(
                body.LiquidityPoolWithdrawOp),
            OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION => body.InvokeHostFunctionOp.HostFunction
                    .Discriminant.InnerValue switch
                {
                    HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT =>
                        InvokeContractOperation.FromXdr(body.InvokeHostFunctionOp),
                    HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT or
                        HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT_V2 =>
                        CreateContractOperation.FromXdr(body.InvokeHostFunctionOp),
                    HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM =>
                        UploadContractOperation.FromXdr(body.InvokeHostFunctionOp),
                    _ => throw new InvalidOperationException("Unknown HostFunction type"),
                },
            OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL => ExtendFootprintOperation.FromXdr(
                body.ExtendFootprintTTLOp),
            OperationType.OperationTypeEnum.RESTORE_FOOTPRINT => RestoreFootprintOperation.FromXdr(
                body.RestoreFootprintOp),
            _ => throw new InvalidOperationException("Unknown operation body " + body.Discriminant.InnerValue),
        };

        if (thisXdr.SourceAccount != null)
        {
            operation.SourceAccount = MuxedAccount.FromXdrMuxedAccount(thisXdr.SourceAccount);
        }

        return operation;
    }

    /// <summary>
    ///     Generates <c>Xdr.OperationBody</c> object.
    /// </summary>
    /// <returns>OperationBody XDR object.</returns>
    public abstract Xdr.Operation.OperationBody ToOperationBody();
}