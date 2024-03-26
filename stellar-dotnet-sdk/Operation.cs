using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     An individual command that modifies the ledger. Operations are used to send payments, enter orders into the
///     decentralized exchange,
///     change settings on accounts, and authorize accounts to hold assets.
/// </summary>
public abstract class Operation
{
    private IAccountId? _sourceAccount;

    /// <summary>
    ///     The account to execute this operation upon.
    /// </summary>
    public IAccountId? SourceAccount
    {
        get => _sourceAccount;
        set => _sourceAccount =
            value ?? throw new ArgumentNullException(nameof(SourceAccount), "source account cannot be null");
    }

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
    ///     Creates a new Operation object from the given Operation XDR base64-encoded string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>Operation object</returns>
    public static Operation FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = xdr.Operation.Decode(reader);
        return FromXdr(thisXdr);
    }

    /// <summary>
    ///     Generates Operation XDR object.
    /// </summary>
    public xdr.Operation ToXdr()
    {
        var thisXdr = new xdr.Operation();
        if (SourceAccount != null) thisXdr.SourceAccount = SourceAccount.MuxedAccount;

        thisXdr.Body = ToOperationBody();
        return thisXdr;
    }

    /// <summary>
    ///     Returns base64-encoded <see cref="stellar_dotnet_sdk.xdr.Operation" /> XDR string.
    /// </summary>
    public string ToXdrBase64()
    {
        var operation = ToXdr();
        var writer = new XdrDataOutputStream();
        xdr.Operation.Encode(writer, operation);
        return Convert.ToBase64String(writer.ToArray());
    }

    // TODO Consider removing this as there is no practical use.
    /// <summary>
    /// </summary>
    /// <returns>new Operation object from Operation XDR object.</returns>
    /// <param name="thisXdr">XDR object</param>
    public static Operation FromXdr(xdr.Operation thisXdr)
    {
        var body = thisXdr.Body;
        Operation operation = body.Discriminant.InnerValue switch
        {
            OperationType.OperationTypeEnum.CREATE_ACCOUNT => new CreateAccountOperation.Builder(
                body.CreateAccountOp).Build(),
            OperationType.OperationTypeEnum.PAYMENT => new PaymentOperation.Builder(body.PaymentOp).Build(),
            OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE =>
                new PathPaymentStrictReceiveOperation.Builder(body.PathPaymentStrictReceiveOp).Build(),
            OperationType.OperationTypeEnum.MANAGE_SELL_OFFER => new ManageSellOfferOperation.Builder(
                body.ManageSellOfferOp).Build(),
            OperationType.OperationTypeEnum.CREATE_PASSIVE_SELL_OFFER =>
                new CreatePassiveSellOfferOperation.Builder(body.CreatePassiveSellOfferOp).Build(),
            OperationType.OperationTypeEnum.SET_OPTIONS =>
                new SetOptionsOperation.Builder(body.SetOptionsOp).Build(),
            OperationType.OperationTypeEnum.CHANGE_TRUST => new ChangeTrustOperation.Builder(body.ChangeTrustOp)
                .Build(),
            OperationType.OperationTypeEnum.ALLOW_TRUST =>
                new AllowTrustOperation.Builder(body.AllowTrustOp).Build(),
            OperationType.OperationTypeEnum.ACCOUNT_MERGE => new AccountMergeOperation.Builder(body).Build(),
            OperationType.OperationTypeEnum.INFLATION => new InflationOperation.Builder().Build(),
            OperationType.OperationTypeEnum.MANAGE_DATA =>
                new ManageDataOperation.Builder(body.ManageDataOp).Build(),
            OperationType.OperationTypeEnum.BUMP_SEQUENCE => new BumpSequenceOperation.Builder(body.BumpSequenceOp)
                .Build(),
            OperationType.OperationTypeEnum.MANAGE_BUY_OFFER => new ManageBuyOfferOperation.Builder(
                body.ManageBuyOfferOp).Build(),
            OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_SEND => new PathPaymentStrictSendOperation.Builder(
                body.PathPaymentStrictSendOp).Build(),
            OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE => new CreateClaimableBalanceOperation.Builder(
                body.CreateClaimableBalanceOp).Build(),
            OperationType.OperationTypeEnum.CLAIM_CLAIMABLE_BALANCE => new ClaimClaimableBalanceOperation.Builder(
                body.ClaimClaimableBalanceOp).Build(),
            OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES =>
                new BeginSponsoringFutureReservesOperation.Builder(body.BeginSponsoringFutureReservesOp).Build(),
            OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES =>
                new EndSponsoringFutureReservesOperation.Builder().Build(),
            OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP =>
                body.RevokeSponsorshipOp.Discriminant.InnerValue switch
                {
                    RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_LEDGER_ENTRY =>
                        new RevokeLedgerEntrySponsorshipOperation.Builder(body.RevokeSponsorshipOp.LedgerKey).Build(),
                    RevokeSponsorshipType.RevokeSponsorshipTypeEnum.REVOKE_SPONSORSHIP_SIGNER =>
                        new RevokeSignerSponsorshipOperation.Builder(body.RevokeSponsorshipOp.Signer).Build(),
                    _ => throw new ArgumentOutOfRangeException(nameof(body.RevokeSponsorshipOp.Discriminant),
                        "Invalid RevokeSponsorshipTypeEnum.")
                },
            OperationType.OperationTypeEnum.CLAWBACK => new ClawbackOperation.Builder(body.ClawbackOp).Build(),
            OperationType.OperationTypeEnum.CLAWBACK_CLAIMABLE_BALANCE =>
                new ClawbackClaimableBalanceOperation.Builder(body.ClawbackClaimableBalanceOp).Build(),
            OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS => new SetTrustlineFlagsOperation.Builder(
                body.SetTrustLineFlagsOp).Build(),
            OperationType.OperationTypeEnum.LIQUIDITY_POOL_DEPOSIT => new LiquidityPoolDepositOperation.Builder(
                body.LiquidityPoolDepositOp).Build(),
            OperationType.OperationTypeEnum.LIQUIDITY_POOL_WITHDRAW => new LiquidityPoolWithdrawOperation.Builder(
                body.LiquidityPoolWithdrawOp).Build(),
            OperationType.OperationTypeEnum.INVOKE_HOST_FUNCTION => body.InvokeHostFunctionOp.HostFunction
                    .Discriminant.InnerValue switch
                {
                    HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_INVOKE_CONTRACT =>
                        new InvokeContractOperation.Builder(body.InvokeHostFunctionOp).Build(),
                    HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_CREATE_CONTRACT =>
                        new CreateContractOperation.Builder(body.InvokeHostFunctionOp).Build(),
                    HostFunctionType.HostFunctionTypeEnum.HOST_FUNCTION_TYPE_UPLOAD_CONTRACT_WASM =>
                        new UploadContractOperation.Builder(body.InvokeHostFunctionOp).Build(),
                    _ => throw new InvalidOperationException("Unknown HostFunction type")
                },
            OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL => new ExtendFootprintOperation.Builder(
                body.ExtendFootprintTTLOp).Build(),
            OperationType.OperationTypeEnum.RESTORE_FOOTPRINT => new RestoreFootprintOperation.Builder(
                body.RestoreFootprintOp).Build(),
            _ => throw new InvalidOperationException("Unknown operation body " + body.Discriminant.InnerValue)
        };

        if (thisXdr.SourceAccount != null)
            operation.SourceAccount = MuxedAccount.FromXdrMuxedAccount(thisXdr.SourceAccount);

        return operation;
    }

    /// <summary>
    ///     Generates OperationBody XDR object
    /// </summary>
    /// <returns>OperationBody XDR object</returns>
    public abstract xdr.Operation.OperationBody ToOperationBody();
}