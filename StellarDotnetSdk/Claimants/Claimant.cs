using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

public class Claimant
{
    /// <summary>
    ///     Constructs a <c>Claimant</c> object from a public key and a predicate.
    /// </summary>
    /// <param name="recipientId">An Ed25519 public key.</param>
    /// <param name="predicate">A <c>Predicate</c> object.</param>
    public Claimant(string recipientId, ClaimPredicate predicate) : this(KeyPair.FromAccountId(recipientId), predicate)
    {
    }

    /// <summary>
    ///     Constructs a <c>Claimant</c> object from a key pair and a predicate.
    /// </summary>
    /// <param name="recipientId">An Ed25519 public key.</param>
    /// <param name="predicate">A <c>Predicate</c> object.</param>
    public Claimant(KeyPair destination, ClaimPredicate predicate)
    {
        Destination = destination;
        Predicate = predicate;
    }

    public KeyPair Destination { get; }
    public ClaimPredicate Predicate { get; }

    public Xdr.Claimant ToXdr()
    {
        return new Xdr.Claimant
        {
            Discriminant = new ClaimantType { InnerValue = ClaimantType.ClaimantTypeEnum.CLAIMANT_TYPE_V0 },
            V0 = new Xdr.Claimant.ClaimantV0
            {
                Destination = new AccountID(Destination.XdrPublicKey),
                Predicate = Predicate.ToXdr(),
            },
        };
    }

    public static Claimant FromXdr(Xdr.Claimant xdr)
    {
        var destination = KeyPair.FromXdrPublicKey(xdr.V0.Destination.InnerValue);
        var predicate = ClaimPredicate.FromXdr(xdr.V0.Predicate);
        return new Claimant(destination, predicate);
    }
}