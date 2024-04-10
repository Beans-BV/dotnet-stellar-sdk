using System;
using System.Linq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using PathPaymentStrictReceiveOp = StellarDotnetSdk.Xdr.PathPaymentStrictReceiveOp;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     A payment where the asset received can be different from the asset sent; allows the user to specify the amount of
///     the asset received.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#path-payment-strict-receive">
///         Path payment strict receive
///     </a>
/// </summary>
public class PathPaymentStrictReceiveOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>PathPaymentStrictReceiveOperation</c>.
    /// </summary>
    /// <param name="sendAsset">The asset deducted from the sender's account.</param>
    /// <param name="sendMax">The asset deducted from the sender's account.</param>
    /// <param name="destination">Payment destination.</param>
    /// <param name="destAsset">The asset the destination account receives.</param>
    /// <param name="destAmount">The amount of destination asset the destination account receives.</param>
    /// <param name="path">
    ///     The assets (other than send asset and destination asset) involved in the offers the path takes.
    ///     For example, if you can only find a path from USD to EUR through XLM and BTC, the path would be USD ->
    ///     XLM -> BTC -> EUR and the path field would contain XLM and BTC.
    /// </param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public PathPaymentStrictReceiveOperation(
        Asset sendAsset,
        string sendMax,
        IAccountId destination,
        Asset destAsset,
        string destAmount,
        Asset[]? path,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        SendAsset = sendAsset ?? throw new ArgumentNullException(nameof(sendAsset), "sendAsset cannot be null");
        SendMax = sendMax ?? throw new ArgumentNullException(nameof(sendMax), "sendMax cannot be null");
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
        DestAsset = destAsset ?? throw new ArgumentNullException(nameof(destAsset), "destAsset cannot be null");
        DestAmount = destAmount ?? throw new ArgumentNullException(nameof(destAmount), "destAmount cannot be null");

        path ??= Array.Empty<Asset>();
        if (path.Length > 5)
            throw new ArgumentException("The maximum number of assets in the path is 5.", nameof(path));
        Path = path;
    }

    /// <summary>
    ///     The asset deducted from the sender's account.
    /// </summary>
    public Asset SendAsset { get; }

    /// <summary>
    ///     The asset deducted from the sender's account.
    /// </summary>
    public string SendMax { get; }

    /// <summary>
    ///     Payment destination.
    /// </summary>
    public IAccountId Destination { get; }

    /// <summary>
    ///     The asset the destination account receives.
    /// </summary>
    public Asset DestAsset { get; }

    /// <summary>
    ///     The amount of destination asset the destination account receives.
    /// </summary>
    public string DestAmount { get; }

    /// <summary>
    ///     The assets (other than send asset and destination asset) involved in the offers the path takes.
    ///     For example, if you can only find a path from USD to EUR through XLM and BTC, the path would be USD ->
    ///     XLM -> BTC -> EUR and the path field would contain XLM and BTC.
    /// </summary>
    public Asset[] Path { get; }

    public override xdr_Operation.OperationBody ToOperationBody()
    {
        return new xdr_Operation.OperationBody
        {
            Discriminant =
                OperationType.Create(OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_RECEIVE),
            PathPaymentStrictReceiveOp = new PathPaymentStrictReceiveOp
            {
                SendAsset = SendAsset.ToXdr(),
                SendMax = new Int64 { InnerValue = ToXdrAmount(SendMax) },
                Destination = Destination.MuxedAccount,
                DestAsset = DestAsset.ToXdr(),
                DestAmount = new Int64 { InnerValue = ToXdrAmount(DestAmount) },
                Path = Path.Select(a => a.ToXdr()).ToArray()
            }
        };
    }

    public static PathPaymentStrictReceiveOperation FromXdr(PathPaymentStrictReceiveOp paymentStrictReceiveOp)
    {
        return new PathPaymentStrictReceiveOperation(
            Asset.FromXdr(paymentStrictReceiveOp.SendAsset),
            Amount.FromXdr(paymentStrictReceiveOp.SendMax.InnerValue),
            MuxedAccount.FromXdrMuxedAccount(paymentStrictReceiveOp.Destination),
            Asset.FromXdr(paymentStrictReceiveOp.DestAsset),
            Amount.FromXdr(paymentStrictReceiveOp.DestAmount.InnerValue),
            paymentStrictReceiveOp.Path.Select(Asset.FromXdr).ToArray()
        );
    }
}