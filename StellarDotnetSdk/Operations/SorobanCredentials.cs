using System;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using CredentialsType = StellarDotnetSdk.Xdr.SorobanCredentialsType.SorobanCredentialsTypeEnum;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Abstract base class for Soroban authorization credentials.
///     Credentials identify and authenticate the entity authorizing a contract invocation.
/// </summary>
/// <seealso cref="SorobanSourceAccountCredentials" />
/// <seealso cref="SorobanAddressCredentials" />
/// <seealso cref="SorobanAddressCredentialsV2" />
/// <seealso cref="SorobanAddressCredentialsWithDelegates" />
public abstract class SorobanCredentials
{
    /// <summary>
    ///     Converts this <see cref="SorobanCredentials" /> to its XDR <see cref="Xdr.SorobanCredentials" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanCredentials" /> XDR object.</returns>
    public abstract Xdr.SorobanCredentials ToXdr();

    /// <summary>
    ///     Creates a <see cref="SorobanCredentials" /> subclass instance from the given XDR
    ///     <see cref="Xdr.SorobanCredentials" />.
    /// </summary>
    /// <param name="xdrSorobanCredentials">The XDR object to deserialize.</param>
    /// <returns>
    ///     A <see cref="SorobanSourceAccountCredentials" />, <see cref="SorobanAddressCredentials" />,
    ///     <see cref="SorobanAddressCredentialsV2" />, or <see cref="SorobanAddressCredentialsWithDelegates" /> instance.
    /// </returns>
    public static SorobanCredentials FromXdr(Xdr.SorobanCredentials xdrSorobanCredentials)
    {
        return xdrSorobanCredentials.Discriminant.InnerValue switch
        {
            CredentialsType.SOROBAN_CREDENTIALS_SOURCE_ACCOUNT =>
                new SorobanSourceAccountCredentials(),
            CredentialsType.SOROBAN_CREDENTIALS_ADDRESS =>
                SorobanAddressCredentials.FromSorobanCredentialsXdr(xdrSorobanCredentials),
            CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_V2 =>
                SorobanAddressCredentialsV2.FromSorobanCredentialsXdr(xdrSorobanCredentials),
            CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_WITH_DELEGATES =>
                SorobanAddressCredentialsWithDelegates.FromSorobanCredentialsXdr(xdrSorobanCredentials),
            _ => throw new InvalidOperationException(
                $"Unknown SorobanCredentials type: {(int)xdrSorobanCredentials.Discriminant.InnerValue}"),
        };
    }
}

/// <summary>
///     This simply uses the signature of the transaction (or operation, if any) source account and hence doesn't require
///     any additional payload.
/// </summary>
public class SorobanSourceAccountCredentials : SorobanCredentials
{
    /// <summary>
    ///     Converts this <see cref="SorobanSourceAccountCredentials" /> to its XDR
    ///     <see cref="Xdr.SorobanCredentials" /> representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanCredentials" /> XDR object.</returns>
    public override Xdr.SorobanCredentials ToXdr()
    {
        return new Xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
            {
                InnerValue = CredentialsType.SOROBAN_CREDENTIALS_SOURCE_ACCOUNT,
            },
        };
    }
}

/// <summary>
///     Shared base for the address-bearing Soroban credentials. V1
///     (<see cref="SorobanAddressCredentials" />) and V2
///     (<see cref="SorobanAddressCredentialsV2" />) have identical fields; they differ only
///     by XDR discriminant and by the signing payload used to produce <see cref="Signature" />.
/// </summary>
public abstract class SorobanAddressCredentialsBase : SorobanCredentials
{
    /// <summary>
    ///     Initializes a new instance of <see cref="SorobanAddressCredentialsBase" />.
    /// </summary>
    /// <param name="address">The address that authorizes the invocation.</param>
    /// <param name="nonce">
    ///     An arbitrary value that must be unique for all signatures performed by <paramref name="address" />
    ///     until <paramref name="signatureExpirationLedger" />.
    /// </param>
    /// <param name="signatureExpirationLedger">The ledger sequence number on which the signature expires.</param>
    /// <param name="signature">The cryptographic signature authenticating the authorization.</param>
    protected SorobanAddressCredentialsBase(
        ScAddress address,
        long nonce,
        uint signatureExpirationLedger,
        SCVal signature)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address), "Address cannot be null.");
        Nonce = nonce;
        SignatureExpirationLedger = signatureExpirationLedger;
        Signature = signature ?? throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
    }

    /// <summary>The address that authorizes invocation.</summary>
    public ScAddress Address { get; }

    /// <summary>A value unique for all signatures performed by <c>Address</c> until <c>SignatureExpirationLedger</c>.</summary>
    public long Nonce { get; }

    /// <summary>The ledger sequence number on which the signature expires.</summary>
    public uint SignatureExpirationLedger { get; }

    /// <summary>The cryptographic signature authenticating the authorization.</summary>
    public SCVal Signature { get; }

    /// <summary>
    ///     Encodes the shared address/nonce/expiration/signature fields into the bare XDR
    ///     <see cref="Xdr.SorobanAddressCredentials" /> struct used by every address-bearing variant
    ///     (V1, V2, and the delegated wrapper's root). The discriminant and union arm are the caller's
    ///     responsibility.
    /// </summary>
    internal Xdr.SorobanAddressCredentials ToAddressCredentialsXdr()
    {
        return new Xdr.SorobanAddressCredentials
        {
            Address = Address.ToXdr(),
            Nonce = new Int64(Nonce),
            SignatureExpirationLedger = new Uint32(SignatureExpirationLedger),
            Signature = Signature.ToXdr(),
        };
    }

    /// <summary>
    ///     Decodes the shared fields from a bare XDR <see cref="Xdr.SorobanAddressCredentials" /> struct.
    ///     The caller selects which concrete wrapper (V1, V2, or root) to construct from them.
    /// </summary>
    internal static (ScAddress Address, long Nonce, uint SignatureExpirationLedger, SCVal Signature)
        FromAddressCredentialsXdr(Xdr.SorobanAddressCredentials xdr)
    {
        return (
            ScAddress.FromXdr(xdr.Address),
            xdr.Nonce.InnerValue,
            xdr.SignatureExpirationLedger.InnerValue,
            SCVal.FromXdr(xdr.Signature));
    }
}

/// <summary>
///     Represents Soroban credentials that authenticate a specific address for contract invocation authorization.
///     Contains the authorizing address, a unique nonce, a signature expiration ledger, and the cryptographic signature.
/// </summary>
public class SorobanAddressCredentials : SorobanAddressCredentialsBase
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAddressCredentials" />.
    /// </summary>
    /// <param name="address">The address that authorizes the invocation.</param>
    /// <param name="nonce">
    ///     An arbitrary value that must be unique for all signatures performed by <paramref name="address" />
    ///     until <paramref name="signatureExpirationLedger" />.
    /// </param>
    /// <param name="signatureExpirationLedger">The ledger sequence number on which the signature expires.</param>
    /// <param name="signature">The cryptographic signature authenticating the authorization.</param>
    public SorobanAddressCredentials(
        ScAddress address,
        long nonce,
        uint signatureExpirationLedger,
        SCVal signature
    ) : base(address, nonce, signatureExpirationLedger, signature)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="SorobanAddressCredentials" /> from the given XDR
    ///     <see cref="Xdr.SorobanCredentials" />.
    /// </summary>
    /// <param name="xdrSorobanCredentials">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAddressCredentials" /> instance.</returns>
    public static SorobanAddressCredentials FromSorobanCredentialsXdr(Xdr.SorobanCredentials xdrSorobanCredentials)
    {
        if (xdrSorobanCredentials.Discriminant.InnerValue != CredentialsType.SOROBAN_CREDENTIALS_ADDRESS)
        {
            throw new InvalidOperationException("Invalid SorobanCredentials type");
        }

        var (address, nonce, expiration, signature) =
            FromAddressCredentialsXdr(xdrSorobanCredentials.Address);
        return new SorobanAddressCredentials(address, nonce, expiration, signature);
    }

    /// <summary>
    ///     Converts this <see cref="SorobanAddressCredentials" /> to its XDR <see cref="Xdr.SorobanCredentials" />
    ///     representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanCredentials" /> XDR object.</returns>
    public override Xdr.SorobanCredentials ToXdr()
    {
        return new Xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
            {
                InnerValue = CredentialsType.SOROBAN_CREDENTIALS_ADDRESS,
            },
            Address = ToAddressCredentialsXdr(),
        };
    }
}

/// <summary>
///     Protocol 27 (CAP-0071-02) address-bound credentials. Wire layout is identical to
///     <see cref="SorobanAddressCredentials" />, but the signature is computed over the
///     address-bound preimage (<c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c>),
///     preventing signature replay between accounts that share a private key.
/// </summary>
/// <remarks>
///     V1 (<see cref="SorobanAddressCredentials" />) and V2 are both valid on Protocol 27; the
///     signing helpers preserve the entry's existing variant by default (matching the JS reference
///     SDK), and V2 is expected to replace V1 in Protocol 28. Pass
///     <see cref="SorobanCredentialsVersion.V2" /> to upgrade a legacy entry. Pre-Protocol-27 networks
///     reject V2 credentials — keep V1 when targeting them.
/// </remarks>
public class SorobanAddressCredentialsV2 : SorobanAddressCredentialsBase
{
    /// <summary>
    ///     Constructs a new <see cref="SorobanAddressCredentialsV2" />.
    /// </summary>
    /// <param name="address">The address that authorizes the invocation.</param>
    /// <param name="nonce">
    ///     An arbitrary value that must be unique for all signatures performed by <paramref name="address" />
    ///     until <paramref name="signatureExpirationLedger" />.
    /// </param>
    /// <param name="signatureExpirationLedger">The ledger sequence number on which the signature expires.</param>
    /// <param name="signature">The cryptographic signature authenticating the authorization.</param>
    public SorobanAddressCredentialsV2(
        ScAddress address,
        long nonce,
        uint signatureExpirationLedger,
        SCVal signature
    ) : base(address, nonce, signatureExpirationLedger, signature)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="SorobanAddressCredentialsV2" /> from the given XDR
    ///     <see cref="Xdr.SorobanCredentials" />.
    /// </summary>
    /// <param name="xdrSorobanCredentials">The XDR object to deserialize.</param>
    /// <returns>A new <see cref="SorobanAddressCredentialsV2" /> instance.</returns>
    public static SorobanAddressCredentialsV2 FromSorobanCredentialsXdr(Xdr.SorobanCredentials xdrSorobanCredentials)
    {
        if (xdrSorobanCredentials.Discriminant.InnerValue != CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_V2)
        {
            throw new InvalidOperationException("Invalid SorobanCredentials type");
        }

        var (address, nonce, expiration, signature) =
            FromAddressCredentialsXdr(xdrSorobanCredentials.AddressV2);
        return new SorobanAddressCredentialsV2(address, nonce, expiration, signature);
    }

    /// <summary>
    ///     Converts this <see cref="SorobanAddressCredentialsV2" /> to its XDR <see cref="Xdr.SorobanCredentials" />
    ///     representation.
    /// </summary>
    /// <returns>A <see cref="Xdr.SorobanCredentials" /> XDR object.</returns>
    public override Xdr.SorobanCredentials ToXdr()
    {
        return new Xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType { InnerValue = CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_V2 },
            AddressV2 = ToAddressCredentialsXdr(),
        };
    }
}

/// <summary>
///     High-level wrapper for a single delegate signature in a delegated authorization
///     (Protocol 27, CAP-0071-01). Recursive: a delegate may itself carry nested delegates.
/// </summary>
public class SorobanDelegateSignature
{
    /// <summary>Constructs a new <see cref="SorobanDelegateSignature" />.</summary>
    /// <param name="address">The delegate address that produced <paramref name="signature" />.</param>
    /// <param name="signature">The delegate's signature payload.</param>
    /// <param name="nestedDelegates">Further delegates chained beneath this one (empty for a leaf).</param>
    public SorobanDelegateSignature(ScAddress address, SCVal signature, SorobanDelegateSignature[] nestedDelegates)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address), "Address cannot be null.");
        Signature = signature ?? throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        NestedDelegates = nestedDelegates ??
                          throw new ArgumentNullException(nameof(nestedDelegates), "NestedDelegates cannot be null.");
    }

    /// <summary>The delegate address that produced <see cref="Signature" />.</summary>
    public ScAddress Address { get; }

    /// <summary>The delegate's signature payload.</summary>
    public SCVal Signature { get; }

    /// <summary>Further delegates chained beneath this one (empty array for a leaf).</summary>
    public SorobanDelegateSignature[] NestedDelegates { get; }

    /// <summary>Converts this wrapper to its XDR <see cref="Xdr.SorobanDelegateSignature" /> representation.</summary>
    /// <remarks>
    ///     Encoding is order-faithful: it serializes <see cref="NestedDelegates" /> as-is and does NOT
    ///     enforce the CAP-71 ascending-address / no-duplicate rule, so any wire data decoded by
    ///     <see cref="FromXdr" /> can always be re-encoded. The signing helpers
    ///     (<c>AuthorizeEntryWithDelegates</c> / <c>BuildWithDelegatesEntry</c>) emit sorted,
    ///     duplicate-free arrays; when building delegate trees manually, order them yourself or the
    ///     network rejects the entry at invocation time.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when a nested delegate entry is null.</exception>
    public Xdr.SorobanDelegateSignature ToXdr()
    {
        var nested = new Xdr.SorobanDelegateSignature[NestedDelegates.Length];
        for (var i = 0; i < NestedDelegates.Length; i++)
        {
            if (NestedDelegates[i] is null)
            {
                throw new InvalidOperationException("Delegate signatures must not contain null entries.");
            }

            nested[i] = NestedDelegates[i].ToXdr();
        }

        return new Xdr.SorobanDelegateSignature
        {
            Address = Address.ToXdr(),
            Signature = Signature.ToXdr(),
            NestedDelegates = nested,
        };
    }

    /// <summary>Creates a wrapper from the given XDR <see cref="Xdr.SorobanDelegateSignature" />.</summary>
    public static SorobanDelegateSignature FromXdr(Xdr.SorobanDelegateSignature xdr)
    {
        var nested = new SorobanDelegateSignature[xdr.NestedDelegates.Length];
        for (var i = 0; i < xdr.NestedDelegates.Length; i++)
        {
            nested[i] = FromXdr(xdr.NestedDelegates[i]);
        }

        return new SorobanDelegateSignature(
            ScAddress.FromXdr(xdr.Address),
            SCVal.FromXdr(xdr.Signature),
            nested
        );
    }
}

/// <summary>
///     Protocol 27 (CAP-0071-01) delegated credentials: a root address credential plus an
///     ordered set of delegate signatures that authenticate on its behalf.
/// </summary>
public class SorobanAddressCredentialsWithDelegates : SorobanCredentials
{
    /// <summary>Constructs a new <see cref="SorobanAddressCredentialsWithDelegates" />.</summary>
    /// <param name="addressCredentials">
    ///     The root address credential being delegated. The embedded XDR is the bare,
    ///     discriminant-free <c>SorobanAddressCredentials</c> struct, so either a V1
    ///     (<see cref="SorobanAddressCredentials" />) or V2 (<see cref="SorobanAddressCredentialsV2" />)
    ///     wrapper may be supplied; only the shared address/nonce/expiration/signature fields are used.
    /// </param>
    /// <param name="delegates">The ordered delegate signatures authorizing on behalf of the root.</param>
    public SorobanAddressCredentialsWithDelegates(
        SorobanAddressCredentialsBase addressCredentials,
        SorobanDelegateSignature[] delegates)
    {
        AddressCredentials = addressCredentials ??
                             throw new ArgumentNullException(nameof(addressCredentials),
                                 "AddressCredentials cannot be null.");
        Delegates = delegates ?? throw new ArgumentNullException(nameof(delegates), "Delegates cannot be null.");
    }

    /// <summary>
    ///     The root address credential being delegated. Variant-agnostic: the wire form is the bare
    ///     <c>SorobanAddressCredentials</c> struct without a V1/V2 discriminant.
    /// </summary>
    /// <remarks>
    ///     WARNING: this is typed as the serializable <see cref="SorobanAddressCredentialsBase" />, so
    ///     <c>AddressCredentials.ToXdr()</c> compiles — but it emits a <b>standalone</b> V1/V2 credential
    ///     whose signature was computed over the delegated (address-bound) payload, which fails
    ///     on-chain signature verification. Treat this property as the source of the root
    ///     address/nonce/expiration/signature fields only; never serialize it on its own to downgrade a
    ///     delegated entry to a plain one.
    /// </remarks>
    public SorobanAddressCredentialsBase AddressCredentials { get; }

    /// <summary>The ordered delegate signatures authorizing on behalf of the root.</summary>
    public SorobanDelegateSignature[] Delegates { get; }

    /// <summary>
    ///     Creates a wrapper from an XDR <see cref="Xdr.SorobanCredentials" /> with discriminant
    ///     <c>ADDRESS_WITH_DELEGATES</c>.
    /// </summary>
    public static SorobanAddressCredentialsWithDelegates FromSorobanCredentialsXdr(
        Xdr.SorobanCredentials xdrSorobanCredentials)
    {
        if (xdrSorobanCredentials.Discriminant.InnerValue != CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_WITH_DELEGATES)
        {
            throw new InvalidOperationException("Invalid SorobanCredentials type");
        }

        var xdrWithDelegates = xdrSorobanCredentials.AddressWithDelegates;
        var (address, nonce, expiration, signature) =
            SorobanAddressCredentialsBase.FromAddressCredentialsXdr(xdrWithDelegates.AddressCredentials);
        var root = new SorobanAddressCredentials(address, nonce, expiration, signature);

        var delegates = new SorobanDelegateSignature[xdrWithDelegates.Delegates.Length];
        for (var i = 0; i < xdrWithDelegates.Delegates.Length; i++)
        {
            delegates[i] = SorobanDelegateSignature.FromXdr(xdrWithDelegates.Delegates[i]);
        }

        return new SorobanAddressCredentialsWithDelegates(root, delegates);
    }

    /// <summary>Converts this wrapper to its XDR <see cref="Xdr.SorobanCredentials" /> representation.</summary>
    /// <remarks>
    ///     Encoding is order-faithful: it serializes <see cref="Delegates" /> as-is and does NOT enforce
    ///     the CAP-71 ascending-address / no-duplicate rule, so any wire data decoded by
    ///     <see cref="FromSorobanCredentialsXdr" /> can always be re-encoded. The signing helpers emit
    ///     sorted, duplicate-free arrays; when building delegate trees manually, order them yourself or
    ///     the network rejects the entry at invocation time.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when a delegate entry is null.</exception>
    public override Xdr.SorobanCredentials ToXdr()
    {
        var delegatesXdr = new Xdr.SorobanDelegateSignature[Delegates.Length];
        for (var i = 0; i < Delegates.Length; i++)
        {
            if (Delegates[i] is null)
            {
                throw new InvalidOperationException("Delegate signatures must not contain null entries.");
            }

            delegatesXdr[i] = Delegates[i].ToXdr();
        }

        return new Xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
                { InnerValue = CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_WITH_DELEGATES },
            AddressWithDelegates = new Xdr.SorobanAddressCredentialsWithDelegates
            {
                AddressCredentials = AddressCredentials.ToAddressCredentialsXdr(),
                Delegates = delegatesXdr,
            },
        };
    }
}