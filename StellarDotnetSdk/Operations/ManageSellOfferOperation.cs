using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Creates, updates, or deletes an offer to sell a specific amount of an asset for another.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#manage-sell-offer">
///         Manage sell offer
///     </a>
/// </summary>
public class ManageSellOfferOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ManageSellOfferOperation</c>.
    /// </summary>
    /// <param name="selling">Asset the offer creator is selling.</param>
    /// <param name="buying">Asset the offer creator is buying.</param>
    /// <param name="amount">Amount of <c>Selling</c> being sold. Set to 0 if you want to delete an existing offer.</param>
    /// <param name="price">Price of 1 unit of <c>Selling</c> in terms of <c>Buying</c>.</param>
    /// <param name="offerId">The ID of the offer. 0 for new offer. Set to existing offer ID to update or delete.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ManageSellOfferOperation(
        Asset selling,
        Asset buying,
        string amount,
        string price,
        long offerId,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        Selling = selling ?? throw new ArgumentNullException(nameof(selling), "selling cannot be null");
        Buying = buying ?? throw new ArgumentNullException(nameof(buying), "buying cannot be null");
        Amount = amount ?? throw new ArgumentNullException(nameof(amount), "amount cannot be null");
        Price = price ?? throw new ArgumentNullException(nameof(price), "price cannot be null");
        OfferId = offerId;
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
    ///     Amount of <see cref="Selling" /> being sold. Set to 0 if you want to delete an existing offer.
    /// </summary>
    public string Amount { get; }

    /// <summary>
    ///     Price of 1 unit of <see cref="Selling" /> in terms of <see cref="Buying" />. For example, if you wanted to sell 30
    ///     XLM and buy 5 BTC, the price would be {5,30}.
    /// </summary>
    public string Price { get; }

    /// <summary>
    ///     The ID of the offer. 0 for new offer. Set to existing offer ID to update or delete.
    /// </summary>
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

    public static ManageSellOfferOperation FromXdr(ManageSellOfferOp manageSellOfferOp)
    {
        return new ManageSellOfferOperation(
            Asset.FromXdr(manageSellOfferOp.Selling),
            Asset.FromXdr(manageSellOfferOp.Buying),
            FromXdrAmount(manageSellOfferOp.Amount.InnerValue),
            StellarDotnetSdk.Amount.DecimalToString(
                decimal.Divide(
                    new decimal(manageSellOfferOp.Price.N.InnerValue),
                    new decimal(manageSellOfferOp.Price.D.InnerValue))),
            manageSellOfferOp.OfferID.InnerValue
        );
    }
}