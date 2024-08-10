using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Sends an amount in a specific asset to a destination account
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#payment">Payment</a>
/// </summary>
public class PaymentOperation : Operation
{
    // <summary>
    ///     Constructs a new <c>PaymentOperation</c>.
    // </summary>
    /// <param name="destination">The destination keypair (uses only the public key).</param>
    /// <param name="asset">The asset to send.</param>
    /// <param name="amount">The amount to send in lumens.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public PaymentOperation(IAccountId destination, Asset asset, string amount, IAccountId? sourceAccount = null) :
        base(sourceAccount)
    {
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
        Asset = asset ?? throw new ArgumentNullException(nameof(asset), "asset cannot be null");
        Amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
    }

    /// <summary>
    ///     Account address that receives the payment.
    /// </summary>
    public IAccountId Destination { get; }

    /// <summary>
    ///     Asset to send to the destination account.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    ///     Amount of the aforementioned asset to send.
    /// </summary>
    public string Amount { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.PAYMENT),
            PaymentOp = new PaymentOp
            {
                Destination = Destination.MuxedAccount,
                Asset = Asset.ToXdr(),
                Amount = new Int64 { InnerValue = ToXdrAmount(Amount) },
            },
        };
    }

    public static PaymentOperation FromXdr(PaymentOp paymentOp)
    {
        return new PaymentOperation(
            MuxedAccount.FromXdrMuxedAccount(paymentOp.Destination),
            Asset.FromXdr(paymentOp.Asset),
            FromXdrAmount(paymentOp.Amount.InnerValue));
    }
}