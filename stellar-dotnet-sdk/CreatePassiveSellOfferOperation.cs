using System;
using sdkxdr = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <c>CreatePassiveSellOfferOp</c>.
///     Use <see cref="Builder" /> to create a new CreatePassiveSellOfferOperation.
///     See also:
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

    public Asset Selling { get; }

    public Asset Buying { get; }

    public string Amount { get; }

    public string Price { get; }

    public override sdkxdr.Operation.OperationBody ToOperationBody()
    {
        var body = new sdkxdr.Operation.OperationBody
        {
            Discriminant = sdkxdr.OperationType.Create(sdkxdr.OperationType.OperationTypeEnum.CREATE_PASSIVE_SELL_OFFER),
            CreatePassiveSellOfferOp = new sdkxdr.CreatePassiveSellOfferOp
            {
                Selling = Selling.ToXdr(),
                Buying = Buying.ToXdr(),
                Amount = new sdkxdr.Int64
                {
                    InnerValue = ToXdrAmount(Amount)
                },
                Price = stellar_dotnet_sdk.Price.FromString(Price).ToXdr(),
            },
        };
        return body;
    }

    /// <summary>
    ///     Builds CreatePassiveOffer operation.
    /// </summary>
    /// <see cref="CreatePassiveSellOfferOperation" />
    public class Builder
    {
        private readonly string _amount;
        private readonly Asset _buying;
        private readonly string _price;
        private readonly Asset _selling;

        private KeyPair? _mSourceAccount;

        /// <summary>
        ///     Construct a new CreatePassiveOffer builder from a CreatePassiveOfferOp XDR.
        /// </summary>
        public Builder(sdkxdr.CreatePassiveSellOfferOp op)
        {
            _selling = Asset.FromXdr(op.Selling);
            _buying = Asset.FromXdr(op.Buying);
            _amount = FromXdrAmount(op.Amount.InnerValue);
            var n = new decimal(op.Price.N.InnerValue);
            var d = new decimal(op.Price.D.InnerValue);
            _price = stellar_dotnet_sdk.Amount.DecimalToString(decimal.Divide(n, d));
        }

        /// <summary>
        ///     Creates a new CreatePassiveOffer builder.
        /// </summary>
        /// <param name="selling">The asset being sold in this operation</param>
        /// <param name="buying">The asset being bought in this operation</param>
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
        ///     Builds an operation
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