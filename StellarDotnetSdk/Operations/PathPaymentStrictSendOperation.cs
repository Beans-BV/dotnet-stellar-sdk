using System;
using System.Linq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using PathPaymentStrictSendOp = StellarDotnetSdk.Xdr.PathPaymentStrictSendOp;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     A payment where the asset sent can be different than the asset received; allows the user to specify the amount of
///     the asset to send.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#path-payment-strict-send">
///         Path payment strict send
///     </a>
/// </summary>
public class PathPaymentStrictSendOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>PathPaymentStrictSendOperation</c>.
    /// </summary>
    /// <param name="sendAsset">The asset deducted from the sender's account.</param>
    /// <param name="sendAmount">The amount of <c>sendAsset</c> to deduct (excluding fees).</param>
    /// <param name="destination">Account ID of the recipient.</param>
    /// <param name="destAsset">The asset the destination account receives.</param>
    /// <param name="destMin">The minimum amount of <c>destination asset</c> the destination account can receive.</param>
    /// <param name="path">The assets (other than send asset and destination asset) involved in the offers the path takes.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public PathPaymentStrictSendOperation(
        Asset sendAsset,
        string sendAmount,
        IAccountId destination,
        Asset destAsset,
        string destMin,
        Asset[]? path,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        SendAsset = sendAsset ?? throw new ArgumentNullException(nameof(sendAsset), "sendAsset cannot be null");
        SendAmount = sendAmount ?? throw new ArgumentNullException(nameof(sendAmount), "sendAmount cannot be null");
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
        DestAsset = destAsset ?? throw new ArgumentNullException(nameof(destAsset), "destAsset cannot be null");
        DestMin = destMin ?? throw new ArgumentNullException(nameof(destMin), "destMin cannot be null");

        path ??= Array.Empty<Asset>();
        if (path.Length > 5)
            throw new ArgumentException("The maximum number of assets in the path is 5", nameof(path));
        Path = path;
    }

    /// <summary>
    ///     The asset deducted from the sender's account.
    /// </summary>
    public Asset SendAsset { get; }

    /// <summary>
    ///     The amount of <c>SendAsset</c> to deduct (excluding fees).
    /// </summary>
    public string SendAmount { get; }

    /// <summary>
    ///     Account ID of the recipient.
    /// </summary>
    public IAccountId Destination { get; }

    /// <summary>
    ///     The asset the destination account receives.
    /// </summary>
    public Asset DestAsset { get; }

    /// <summary>
    ///     The minimum amount of <c>destination asset</c> the destination account can receive.
    /// </summary>
    public string DestMin { get; }

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
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.PATH_PAYMENT_STRICT_SEND),
            PathPaymentStrictSendOp = new PathPaymentStrictSendOp
            {
                SendAsset = SendAsset.ToXdr(),
                SendAmount = new Int64 { InnerValue = ToXdrAmount(SendAmount) },
                Destination = Destination.MuxedAccount,
                DestAsset = DestAsset.ToXdr(),
                DestMin = new Int64 { InnerValue = ToXdrAmount(DestMin) },
                Path = Path.Select(a => a.ToXdr()).ToArray()
            }
        };
    }

    public static PathPaymentStrictSendOperation FromXdr(PathPaymentStrictSendOp paymentStrictSendOp)
    {
        return new PathPaymentStrictSendOperation(
            Asset.FromXdr(paymentStrictSendOp.SendAsset),
            Amount.FromXdr(paymentStrictSendOp.SendAmount.InnerValue),
            MuxedAccount.FromXdrMuxedAccount(paymentStrictSendOp.Destination),
            Asset.FromXdr(paymentStrictSendOp.DestAsset),
            Amount.FromXdr(paymentStrictSendOp.DestMin.InnerValue),
            paymentStrictSendOp.Path.Select(Asset.FromXdr).ToArray()
        );
    }
}