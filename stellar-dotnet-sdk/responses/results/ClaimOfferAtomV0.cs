namespace stellar_dotnet_sdk.responses.results;

/// <summary>
///     This result is used when offers are taken during an operation.
/// </summary>
public class ClaimOfferAtomV0
{
    private ClaimOfferAtomV0(KeyPair seller, long offerID, Asset assetSold, string amountSold, Asset assetBought,
        string amountBought)
    {
        Seller = seller;
        OfferID = offerID;
        AssetSold = assetSold;
        AmountSold = amountSold;
        AssetBought = assetBought;
        AmountBought = amountBought;
    }

    public KeyPair Seller { get; }
    public long OfferID { get; }
    public Asset AssetSold { get; }
    public string AmountSold { get; }
    public Asset AssetBought { get; }
    public string AmountBought { get; }

    public static ClaimOfferAtomV0 FromXdr(xdr.ClaimOfferAtomV0 claimOfferAtomV0Xdr)
    {
        return new ClaimOfferAtomV0(KeyPair.FromPublicKey(claimOfferAtomV0Xdr.SellerEd25519.InnerValue),
            claimOfferAtomV0Xdr.OfferID.InnerValue,
            Asset.FromXdr(claimOfferAtomV0Xdr.AssetSold),
            Amount.FromXdr(claimOfferAtomV0Xdr.AmountSold.InnerValue),
            Asset.FromXdr(claimOfferAtomV0Xdr.AssetBought),
            Amount.FromXdr(claimOfferAtomV0Xdr.AmountBought.InnerValue));
    }
}