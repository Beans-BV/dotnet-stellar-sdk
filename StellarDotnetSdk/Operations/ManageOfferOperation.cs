using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Assets_Asset = StellarDotnetSdk.Assets.Asset;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Represents a <see cref="ManageSellOfferOp" />.
///     Use <see cref="Builder" /> to create a new ManageSellOfferOperation.
///     See also:
///     <see href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#manage-sell-offer">
///         Manage
///         Offer
///     </see>
/// </summary>
[Obsolete("This class has been renamed to ManageSellOfferOperation.")]
public class ManageOfferOperation : Operation
{
    private ManageOfferOperation(Assets_Asset selling, Assets_Asset buying, string amount, string price, long offerId)
    {
        Selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
        Buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
        Amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
        Price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
        // offerId can be null
        OfferId = offerId;
    }

    public Assets_Asset Selling { get; }

    public Assets_Asset Buying { get; }

    public string Amount { get; }

    public string Price { get; }

    public long OfferId { get; }

    public override xdr_Operation.OperationBody ToOperationBody()
    {
        var body = new xdr_Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.MANAGE_SELL_OFFER),
            ManageSellOfferOp = new ManageSellOfferOp
            {
                Selling = Selling.ToXdr(),
                Buying = Buying.ToXdr(),
                Amount = new Int64(ToXdrAmount(Amount)),
                Price = StellarDotnetSdk.Price.FromString(Price).ToXdr(),
                OfferID = new Int64(OfferId)
            }
        };
        return body;
    }

    /// <summary>
    ///     Builds ManageOffer operation.
    /// </summary>
    public class Builder
    {
        private readonly string _amount;
        private readonly Assets_Asset _buying;
        private readonly string _price;

        private readonly Assets_Asset _selling;

        private KeyPair? _mSourceAccount;
        private long _offerId;

        /// <summary>
        ///     Construct a new <c>ManageOfferOperation</c> builder from a <c>ManageSellOfferOp</c> object.
        /// </summary>
        /// <param name="op">
        ///     <see cref="Xdr.ManageSellOfferOp" />
        /// </param>
        public Builder(ManageSellOfferOp op)
        {
            _selling = Assets_Asset.FromXdr(op.Selling);
            _buying = Assets_Asset.FromXdr(op.Buying);
            _amount = FromXdrAmount(op.Amount.InnerValue);
            var n = new decimal(op.Price.N.InnerValue);
            var d = new decimal(op.Price.D.InnerValue);
            _price = StellarDotnetSdk.Amount.DecimalToString(decimal.Divide(n, d));
            _offerId = op.OfferID.InnerValue;
        }

        /// <summary>
        ///     Creates a new ManageSellOffer builder.
        /// </summary>
        /// <param name="selling">The asset being sold in this operation</param>
        /// <param name="buying"> The asset being bought in this operation</param>
        /// <param name="amount"> Amount of selling being sold.</param>
        /// <param name="price"> Price of 1 unit of selling in terms of buying.</param>
        /// <exception cref="ArithmeticException">when amount has more than 7 decimal places.</exception>
        public Builder(Assets_Asset selling, Assets_Asset buying, string amount, string price)
        {
            _selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
            _buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
            _amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
            _price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
        }

        /// <summary>
        ///     Sets offer ID. <code>0</code> creates a new offer. Set to existing offer ID to change it.
        /// </summary>
        /// <param name="offerId">
        /// </param>
        public Builder SetOfferId(long offerId)
        {
            _offerId = offerId;
            return this;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="sourceAccount">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _mSourceAccount = sourceAccount ??
                              throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
            return this;
        }

        /// <summary>
        ///     Builds an operation
        /// </summary>
        public ManageOfferOperation Build()
        {
            var operation = new ManageOfferOperation(_selling, _buying, _amount, _price, _offerId);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}