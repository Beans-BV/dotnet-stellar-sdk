using System;
using stellar_dotnet_sdk.xdr;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="ManageSellOfferOp" />.
///     Use <see cref="Builder" /> to create a new ManageSellOfferOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#manage-sell-offer">
///         Manage sell offer
///     </a>
/// </summary>
public class ManageSellOfferOperation : Operation
{
    private ManageSellOfferOperation(Asset selling, Asset buying, string amount, string price, long offerId)
    {
        Selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
        Buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
        Amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
        Price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
        OfferId = offerId;
    }

    public Asset Selling { get; }

    public Asset Buying { get; }

    public string Amount { get; }

    public string Price { get; }

    public long OfferId { get; }

    public override sdkxdr.Operation.OperationBody ToOperationBody()
    {
        var body = new sdkxdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.MANAGE_SELL_OFFER),
            ManageSellOfferOp = new ManageSellOfferOp
            {
                Selling = Selling.ToXdr(),
                Buying = Buying.ToXdr(),
                Amount = new sdkxdr.Int64(ToXdrAmount(Amount)),
                Price = stellar_dotnet_sdk.Price.FromString(Price).ToXdr(),
                OfferID = new sdkxdr.Int64(OfferId)
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
        private readonly Asset _buying;
        private readonly string _price;

        private readonly Asset _selling;

        private KeyPair? _mSourceAccount;
        private readonly long _offerId;

        /// <summary>
        ///     Construct a new <c>ManageOfferOperation</c> builder from a <c>ManageSellOfferOp</c> object.
        /// </summary>
        /// <param name="op">
        ///     <see cref="xdr.ManageSellOfferOp" />
        /// </param>
        public Builder(ManageSellOfferOp op)
        {
            _selling = Asset.FromXdr(op.Selling);
            _buying = Asset.FromXdr(op.Buying);
            _amount = FromXdrAmount(op.Amount.InnerValue);
            var n = new decimal(op.Price.N.InnerValue);
            var d = new decimal(op.Price.D.InnerValue);
            _price = stellar_dotnet_sdk.Amount.DecimalToString(decimal.Divide(n, d));
            _offerId = op.OfferID.InnerValue;
        }

        /// <summary>
        ///     Creates a new ManageSellOffer builder.
        /// </summary>
        /// <param name="selling">The asset being sold in this operation</param>
        /// <param name="buying"> The asset being bought in this operation</param>
        /// <param name="amount"> Amount of selling being sold. An amount of zero will delete the offer.</param>
        /// <param name="price"> Price of 1 unit of selling in terms of buying.</param>
        /// <param name="offerId">
        ///     Optional, if not provided, will create a new offer. Existing offer id numbers can be found using
        ///     the Offers for Account endpoint.
        /// </param>
        /// <exception cref="ArithmeticException">when amount has more than 7 decimal places.</exception>
        public Builder(Asset selling, Asset buying, string amount, string price, long? offerId = null)
        {
            _selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
            _buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
            _amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
            _price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
            _offerId = offerId ?? 0L;
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
        public ManageSellOfferOperation Build()
        {
            var operation = new ManageSellOfferOperation(_selling, _buying, _amount, _price, _offerId);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}