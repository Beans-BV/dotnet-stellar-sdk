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
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#create-passive-sell-offer">
///         Create passive sell offer
///     </a>
/// </summary>
public class CreatePassiveSellOfferOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>CreatePassiveSellOfferOperation</c>.
    /// </summary>
    /// <param name="selling">Asset the offer creator is selling.</param>
    /// <param name="buying">Asset the offer creator is buying.</param>
    /// <param name="amount">Amount of <c>Selling</c> being sold.</param>
    /// <param name="price">Price of 1 unit of selling in terms of buying.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public CreatePassiveSellOfferOperation(Asset selling, Asset buying, string amount, string price,
        IAccountId? sourceAccount = null) : base(sourceAccount)
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
                Price = StellarDotnetSdk.Price.FromString(Price).ToXdr(),
            },
        };
    }

    public static CreatePassiveSellOfferOperation FromXdr(CreatePassiveSellOfferOp createPassiveSellOfferOp)
    {
        return new CreatePassiveSellOfferOperation(
            Asset.FromXdr(createPassiveSellOfferOp.Selling),
            Asset.FromXdr(createPassiveSellOfferOp.Buying),
            StellarDotnetSdk.Amount.FromXdr(createPassiveSellOfferOp.Amount.InnerValue),
            StellarDotnetSdk.Amount.DecimalToString(decimal.Divide(
                new decimal(createPassiveSellOfferOp.Price.N.InnerValue),
                new decimal(createPassiveSellOfferOp.Price.D.InnerValue)))
        );
    }
}