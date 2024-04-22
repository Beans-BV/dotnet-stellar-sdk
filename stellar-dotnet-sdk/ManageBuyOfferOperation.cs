using System;
using stellar_dotnet_sdk.xdr;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="ManageBuyOfferOp" />.
///     Use <see cref="Builder" /> to create a new ManageBuyOfferOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#manage-buy-offer">
///         Manage buy offer
///     </a>
/// </summary>
public class ManageBuyOfferOperation : Operation
{
    private ManageBuyOfferOperation(Asset selling, Asset buying, string buyAmount, string price, long offerId)
    {
        Selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
        Buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
        BuyAmount = buyAmount ?? throw new ArgumentNullException(nameof(buyAmount), "buyAmount cannot be null");
        Price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
        OfferId = offerId;
    }

    public Asset Selling { get; }

    public Asset Buying { get; }

    public string BuyAmount { get; }

    public string Price { get; }

    public long OfferId { get; }

    public override sdkxdr.Operation.OperationBody ToOperationBody()
    {
        var body = new sdkxdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.MANAGE_BUY_OFFER),
            ManageBuyOfferOp = new ManageBuyOfferOp
            {
                Selling = Selling.ToXdr(),
                Buying = Buying.ToXdr(),
                BuyAmount = new sdkxdr.Int64 { InnerValue = ToXdrAmount(BuyAmount) },
                Price = stellar_dotnet_sdk.Price.FromString(Price).ToXdr(),
                OfferID = new sdkxdr.Int64 { InnerValue = OfferId }
            }
        };
        return body;
    }

    public class Builder
    {
        private readonly string _buyAmount;
        private readonly Asset _buying;
        private readonly long _offerId;
        private readonly string _price;

        private readonly Asset _selling;

        private KeyPair? _mSourceAccount;

        /// <summary>
        ///     Construct a new ManageBuyOffer builder from a ManageBuyOfferOp XDR.
        /// </summary>
        /// <param name="op">
        ///     <see cref="sdkxdr.ManageBuyOfferOp" />
        /// </param>
        public Builder(ManageBuyOfferOp op)
        {
            _selling = Asset.FromXdr(op.Selling);
            _buying = Asset.FromXdr(op.Buying);
            _buyAmount = FromXdrAmount(op.BuyAmount.InnerValue);
            var n = new decimal(op.Price.N.InnerValue);
            var d = new decimal(op.Price.D.InnerValue);
            _price = Amount.DecimalToString(decimal.Divide(n, d));
            _offerId = op.OfferID.InnerValue;
        }

        /// <summary>
        ///     Creates a new ManageBuyOffer builder.
        /// </summary>
        /// <param name="selling">The asset being sold in this operation</param>
        /// <param name="buying"> The asset being bought in this operation</param>
        /// <param name="buyAmount"> Amount being bought.</param>
        /// <param name="price"> Price of 1 unit of buying in terms of selling.</param>
        /// <param name="offerId">
        ///     Optional, if not provided, will create a new offer. Existing offer id numbers can be found using
        ///     the Offers for Account endpoint.
        /// </param>
        /// <exception cref="ArithmeticException">when amount has more than 7 decimal places.</exception>
        public Builder(Asset selling, Asset buying, string amount, string price, long? offerId = null)
        {
            _selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
            _buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
            _buyAmount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
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
        public ManageBuyOfferOperation Build()
        {
            var operation = new ManageBuyOfferOperation(_selling, _buying, _buyAmount, _price, _offerId);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}