using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class Claimant
{
    public Claimant(string recipientId, ClaimPredicate predicate) : this(KeyPair.FromAccountId(recipientId), predicate)
    {
    }

    public Claimant(KeyPair destination, ClaimPredicate predicate)
    {
        Destination = destination;
        Predicate = predicate;
    }

    public KeyPair Destination { get; }
    public ClaimPredicate Predicate { get; }

    public xdr.Claimant ToXdr()
    {
        return new xdr.Claimant
        {
            Discriminant = new ClaimantType { InnerValue = ClaimantType.ClaimantTypeEnum.CLAIMANT_TYPE_V0 },
            V0 = new xdr.Claimant.ClaimantV0
            {
                Destination = new AccountID(Destination.XdrPublicKey),
                Predicate = Predicate.ToXdr()
            }
        };
    }

    public static Claimant FromXdr(xdr.Claimant xdr)
    {
        var destination = KeyPair.FromXdrPublicKey(xdr.V0.Destination.InnerValue);
        var predicate = ClaimPredicate.FromXdr(xdr.V0.Predicate);
        return new Claimant(destination, predicate);
    }
}