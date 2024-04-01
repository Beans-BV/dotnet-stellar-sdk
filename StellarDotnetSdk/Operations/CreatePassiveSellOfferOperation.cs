using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using CreatePassiveSellOfferOp = StellarDotnetSdk.Xdr.CreatePassiveSellOfferOp;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Creates an offer to sell one asset for another without taking a reverse offer of equal price.
///     <p>Use <see cref="Builder" /> to create a new <c>CreatePassiveSellOfferOperation</c>.</p>
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#create-passive-sell-offer">
///         Create passive sell offer
///     </a>
/// </summary>
public class CreatePassiveSellOfferOperation : Operation
{
    private CreatePassiveSellOfferOperation(Asset selling, Asset buying, string amount, string price)
    {
        Selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
        Buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
        Amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
        Price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
    }

    /// <summary>
    ///     Asset the offer creator is selling.
    /// </summary>
    public Asset Selling { get; }

    /// <summary>
    ///     Asset the offer creator is buying.
    /// </summary>
    public Asset Buying { get; }

    /// <summary>
    ///     Amount of <c>Selling</c> being sold.
    /// </summary>
    public string Amount { get; }

    /// <summary>
    ///     Price of 1 unit of selling in terms of buying. For example, if you wanted to sell 30 XLM and buy 5 BTC, the price
    ///     would be {5,30}.
    /// </summary>
    public string Price { get; }

    public override xdr_Operation.OperationBody ToOperationBody()
    {
        return new xdr_Operation.OperationBody
        {
            Discriminant =
                OperationType.Create(OperationType.OperationTypeEnum.CREATE_PASSIVE_SELL_OFFER),
            CreatePassiveSellOfferOp = new CreatePassiveSellOfferOp
            {
                Selling = Selling.ToXdr(),
                Buying = Buying.ToXdr(),
                Amount = new Int64(ToXdrAmount(Amount)),
                Price = StellarDotnetSdk.Price.FromString(Price).ToXdr()
            }
        };
    }

    /// <summary>
    ///     Builder for <c>CreatePassiveSellOfferOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly string _amount;
        private readonly Asset _buying;
        private readonly string _price;
        private readonly Asset _selling;
        private KeyPair? _mSourceAccount;

        /// <summary>
        ///     Constructs a new <c>CreatePassiveSellOfferOperation</c> builder.
        /// </summary>
        public Builder(CreatePassiveSellOfferOp op)
        {
            _selling = Asset.FromXdr(op.Selling);
            _buying = Asset.FromXdr(op.Buying);
            _amount = FromXdrAmount(op.Amount.InnerValue);
            var n = new decimal(op.Price.N.InnerValue);
            var d = new decimal(op.Price.D.InnerValue);
            _price = StellarDotnetSdk.Amount.DecimalToString(decimal.Divide(n, d));
        }

        /// <summary>
        ///     Constructs a new <c>CreatePassiveSellOfferOperation</c> builder.
        /// </summary>
        /// <param name="selling">The asset being sold in this operation.</param>
        /// <param name="buying">The asset being bought in this operation.</param>
        /// <param name="amount">Amount of selling being sold.</param>
        /// <param name="price">Price of 1 unit of selling in terms of buying.</param>
        /// <exception cref="ArithmeticException">when amount has more than 7 decimal places.</exception>
        public Builder(Asset selling, Asset buying, string amount, string price)
        {
            _selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
            _buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
            _amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
            _price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
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
        ///     Builds an operation.
        /// </summary>
        public CreatePassiveSellOfferOperation Build()
        {
            var operation = new CreatePassiveSellOfferOperation(_selling, _buying, _amount, _price);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}