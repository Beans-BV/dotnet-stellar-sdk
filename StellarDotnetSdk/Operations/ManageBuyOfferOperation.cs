using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using xdr_Operation = StellarDotnetSdk.Xdr.Operation;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Creates, updates, or deletes an offer to buy a specific amount of an asset for another.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#manage-buy-offer">
///         Manage buy offer
///     </a>
/// </summary>
public class ManageBuyOfferOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ManageBuyOfferOperation</c>.
    /// </summary>
    /// <param name="selling">The asset being sold in this operation.</param>
    /// <param name="buying"> The asset being bought in this operation.</param>
    /// <param name="buyAmount"> Amount being bought.</param>
    /// <param name="price"> Price of 1 unit of buying in terms of selling.</param>
    /// <param name="offerId">
    ///     If 0, will create a new offer. Existing offer id numbers can be found using
    ///     the Offers for Account endpoint.
    /// </param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ManageBuyOfferOperation(
        Asset selling,
        Asset buying,
        string buyAmount,
        string price,
        long offerId,
        IAccountId? sourceAccount = null) : base(sourceAccount)
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

    public override xdr_Operation.OperationBody ToOperationBody()
    {
        var body = new xdr_Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.MANAGE_BUY_OFFER),
            ManageBuyOfferOp = new ManageBuyOfferOp
            {
                Selling = Selling.ToXdr(),
                Buying = Buying.ToXdr(),
                BuyAmount = new Int64(ToXdrAmount(BuyAmount)),
                Price = StellarDotnetSdk.Price.FromString(Price).ToXdr(),
                OfferID = new Int64(OfferId),
            },
        };
        return body;
    }

    public static ManageBuyOfferOperation FromXdr(ManageBuyOfferOp manageBuyOfferOp)
    {
        return new ManageBuyOfferOperation(
            Asset.FromXdr(manageBuyOfferOp.Selling),
            Asset.FromXdr(manageBuyOfferOp.Buying),
            FromXdrAmount(manageBuyOfferOp.BuyAmount.InnerValue),
            Amount.DecimalToString(
                decimal.Divide(
                    new decimal(manageBuyOfferOp.Price.N.InnerValue),
                    new decimal(manageBuyOfferOp.Price.D.InnerValue))),
            manageBuyOfferOp.OfferID.InnerValue
        );
    }
}