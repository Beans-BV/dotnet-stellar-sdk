using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Assets_Asset = StellarDotnetSdk.Assets.Asset;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Represents a <see cref="PaymentOp" /> operation.
///     Use <see cref="Builder" /> to create a new PaymentOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#payment">Payment</a>
/// </summary>
public class PaymentOperation : Operation
{
    private PaymentOperation(IAccountId destination, Assets_Asset asset, string amount)
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
    public Assets_Asset Asset { get; }

    /// <summary>
    ///     Amount of the aforementioned asset to send.
    /// </summary>
    public string Amount { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.PAYMENT),
            PaymentOp = new PaymentOp
            {
                Destination = Destination.MuxedAccount,
                Asset = Asset.ToXdr(),
                Amount = new xdr_Int64 { InnerValue = ToXdrAmount(Amount) }
            }
        };
        return body;
    }

    /// <summary>
    ///     Builds Payment operation.
    /// </summary>
    /// <see cref="PathPaymentStrictSendOperation" />
    /// ///
    /// <see cref="PathPaymentStrictReceiveOperation" />
    public class Builder
    {
        private readonly string _amount;
        private readonly Assets_Asset _asset;
        private readonly IAccountId _destination;

        private IAccountId? _sourceAccount;

        /// <summary>
        ///     Construct a new PaymentOperation builder from a PaymentOp XDR.
        /// </summary>
        /// <param name="op">
        ///     <see cref="PaymentOp" />
        /// </param>
        public Builder(PaymentOp op)
        {
            _destination = MuxedAccount.FromXdrMuxedAccount(op.Destination);
            _asset = Assets_Asset.FromXdr(op.Asset);
            _amount = FromXdrAmount(op.Amount.InnerValue);
        }

        /// <summary>
        ///     Creates a new PaymentOperation builder.
        /// </summary>
        /// <param name="destination">The destination keypair (uses only the public key).</param>
        /// <param name="asset">The asset to send.</param>
        /// <param name="amount">The amount to send in lumens.</param>
        public Builder(IAccountId destination, Assets_Asset asset, string amount)
        {
            _destination = destination;
            _asset = asset;
            _amount = amount;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="account">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(IAccountId account)
        {
            _sourceAccount = account;
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public PaymentOperation Build()
        {
            var operation = new PaymentOperation(_destination, _asset, _amount);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}