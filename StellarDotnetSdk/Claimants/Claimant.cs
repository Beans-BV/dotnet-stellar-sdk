using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Claimants;

/// <summary>
///     Represents a claimant for building claimable balance operations in Stellar transactions.
/// </summary>
/// <remarks>
///     <para>
///         This class is used when constructing <see cref="StellarDotnetSdk.Operations.CreateClaimableBalanceOperation" />
///         to specify who can claim the balance and under what conditions.
///     </para>
///     <para>
///         <strong>For deserializing Horizon API responses</strong>, use <see cref="Responses.Claimant" /> instead.
///     </para>
/// </remarks>
/// <seealso cref="Responses.Claimant" />
public class Claimant
{
    /// <summary>
    ///     Constructs a <c>Claimant</c> object from a public key and a predicate.
    /// </summary>
    /// <param name="recipientId">An Ed25519 public key.</param>
    /// <param name="predicate">A <see cref="ClaimPredicate" /> defining when this claimant can claim the balance.</param>
    public Claimant(string recipientId, ClaimPredicate predicate) : this(KeyPair.FromAccountId(recipientId), predicate)
    {
    }

    /// <summary>
    ///     Constructs a <c>Claimant</c> object from a key pair and a predicate.
    /// </summary>
    /// <param name="destination">The destination account that can claim the balance.</param>
    /// <param name="predicate">A <see cref="ClaimPredicate" /> defining when this claimant can claim the balance.</param>
    public Claimant(KeyPair destination, ClaimPredicate predicate)
    {
        Destination = destination;
        Predicate = predicate;
    }

    /// <summary>Gets the destination account that can claim the balance.</summary>
    public KeyPair Destination { get; }

    /// <summary>Gets the predicate defining when this claimant can claim the balance.</summary>
    public ClaimPredicate Predicate { get; }

    /// <summary>
    ///     Converts this claimant to its XDR representation.
    /// </summary>
    /// <returns>An XDR <see cref="Xdr.Claimant" /> object.</returns>
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

    /// <summary>
    ///     Creates a <see cref="Claimant" /> from an XDR claimant object.
    /// </summary>
    /// <param name="xdr">The XDR claimant to convert.</param>
    /// <returns>A new <see cref="Claimant" /> instance.</returns>
    public static Claimant FromXdr(Xdr.Claimant xdr)
    {
        var destination = KeyPair.FromXdrPublicKey(xdr.V0.Destination.InnerValue);
        var predicate = ClaimPredicate.FromXdr(xdr.V0.Predicate);
        return new Claimant(destination, predicate);
    }
}