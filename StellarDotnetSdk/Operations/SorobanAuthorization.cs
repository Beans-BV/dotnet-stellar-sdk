using System;
using System.Collections.Generic;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using EnvelopeTypeEnum = StellarDotnetSdk.Xdr.EnvelopeType.EnvelopeTypeEnum;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using ScAddress = StellarDotnetSdk.Soroban.ScAddress;
using SCBytes = StellarDotnetSdk.Soroban.SCBytes;
using SCMap = StellarDotnetSdk.Soroban.SCMap;
using SCMapEntry = StellarDotnetSdk.Soroban.SCMapEntry;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using SCVec = StellarDotnetSdk.Soroban.SCVec;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Selects which address-credential variant a signing operation produces. Signing helpers
///     default to <see cref="V1" /> because it is accepted on every protocol version; <see cref="V2" />
///     is opt-in until Protocol 27 is live on the target network and is expected to replace V1 in
///     Protocol 28.
/// </summary>
public enum SorobanCredentialsVersion
{
    /// <summary>
    ///     Legacy <c>SOROBAN_CREDENTIALS_ADDRESS</c> (CAP-0046). Accepted on all networks; the default.
    /// </summary>
    V1,

    /// <summary>
    ///     Protocol 27 address-bound <c>SOROBAN_CREDENTIALS_ADDRESS_V2</c> (CAP-0071-02). Rejected by
    ///     pre-Protocol-27 networks. Expected to replace V1 in Protocol 28.
    /// </summary>
    V2,
}

/// <summary>
///     Signs a Soroban authorization payload. Implement this for custom (smart-contract)
///     accounts whose <c>__check_auth</c> expects a non-Ed25519 signature format.
/// </summary>
public interface ISorobanEntrySigner
{
    /// <summary>The address this signer authorizes as.</summary>
    ScAddress SignerAddress { get; }

    /// <summary>
    ///     Signs the SHA-256 payload hash and returns the signature as the
    ///     <see cref="SCVal" /> the account contract expects.
    /// </summary>
    /// <param name="payloadHash">The authorization payload hash to sign; must be exactly 32 bytes (SHA-256).</param>
    SCVal Sign(byte[] payloadHash);
}

/// <summary>
///     <see cref="ISorobanEntrySigner" /> for a classic Ed25519 <see cref="KeyPair" />. Produces
///     the standard account-contract signature: <c>SCVec[ SCMap{ public_key, signature } ]</c>.
/// </summary>
public class KeyPairEntrySigner : ISorobanEntrySigner
{
    private const string PublicKeyMapKey = "public_key";
    private const string SignatureMapKey = "signature";

    private readonly KeyPair _keyPair;

    /// <summary>Constructs a signer backed by <paramref name="keyPair" />.</summary>
    public KeyPairEntrySigner(KeyPair keyPair)
    {
        _keyPair = keyPair ?? throw new ArgumentNullException(nameof(keyPair));
    }

    /// <inheritdoc />
    public ScAddress SignerAddress => new ScAccountId(_keyPair.AccountId);

    /// <inheritdoc />
    public SCVal Sign(byte[] payloadHash)
    {
        if (payloadHash is not { Length: 32 })
        {
            throw new ArgumentException("payloadHash must be a 32-byte SHA-256 hash.", nameof(payloadHash));
        }

        var signature = _keyPair.Sign(payloadHash);
        // Map keys MUST be in ascending order: PublicKeyMapKey ("public_key") < SignatureMapKey ("signature").
        return new SCVec([
            new SCMap([
                new SCMapEntry(new SCSymbol(PublicKeyMapKey), new SCBytes(_keyPair.PublicKey)),
                new SCMapEntry(new SCSymbol(SignatureMapKey), new SCBytes(signature)),
            ]),
        ]);
    }
}

/// <summary>
///     Helpers for constructing and signing Soroban authorization entries (Protocol 27).
/// </summary>
public static class SorobanAuthorization
{
    /// <summary>
    ///     Builds and SHA-256-hashes the legacy V1 signing payload
    ///     (<c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION</c>).
    /// </summary>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="nonce">
    ///     A value unique for all signatures performed by the address until
    ///     <paramref name="signatureExpirationLedger" />.
    /// </param>
    /// <param name="signatureExpirationLedger">The ledger sequence number on which the signature expires.</param>
    /// <param name="invocation">The authorized invocation tree to sign.</param>
    /// <returns>A 32-byte SHA-256 hash of the XDR-encoded preimage.</returns>
    public static byte[] BuildAuthPreimageHash(
        Network network,
        long nonce,
        uint signatureExpirationLedger,
        SorobanAuthorizedInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(network);
        ArgumentNullException.ThrowIfNull(invocation);

        var preimage = new HashIDPreimage
        {
            Discriminant = new EnvelopeType { InnerValue = EnvelopeTypeEnum.ENVELOPE_TYPE_SOROBAN_AUTHORIZATION },
            SorobanAuthorization = new HashIDPreimage.HashIDPreimageSorobanAuthorization
            {
                NetworkID = new Hash(network.NetworkId),
                Nonce = new Int64(nonce),
                SignatureExpirationLedger = new Uint32(signatureExpirationLedger),
                Invocation = invocation.ToXdr(),
            },
        };
        return HashPreimage(preimage);
    }

    /// <summary>
    ///     Builds and SHA-256-hashes the Protocol 27 address-bound signing payload
    ///     (<c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c>), used by V2 and delegated
    ///     credentials.
    /// </summary>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="address">The address being bound to the signing payload, preventing cross-account replay.</param>
    /// <param name="nonce">
    ///     A value unique for all signatures performed by <paramref name="address" /> until
    ///     <paramref name="signatureExpirationLedger" />.
    /// </param>
    /// <param name="signatureExpirationLedger">The ledger sequence number on which the signature expires.</param>
    /// <param name="invocation">The authorized invocation tree to sign.</param>
    /// <returns>A 32-byte SHA-256 hash of the XDR-encoded preimage.</returns>
    public static byte[] BuildAddressAuthPreimageHash(
        Network network,
        ScAddress address,
        long nonce,
        uint signatureExpirationLedger,
        SorobanAuthorizedInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(network);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(invocation);

        var preimage = new HashIDPreimage
        {
            Discriminant = new EnvelopeType
                { InnerValue = EnvelopeTypeEnum.ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS },
            SorobanAuthorizationWithAddress = new HashIDPreimage.HashIDPreimageSorobanAuthorizationWithAddress
            {
                NetworkID = new Hash(network.NetworkId),
                Nonce = new Int64(nonce),
                SignatureExpirationLedger = new Uint32(signatureExpirationLedger),
                Address = address.ToXdr(),
                Invocation = invocation.ToXdr(),
            },
        };
        return HashPreimage(preimage);
    }

    /// <summary>
    ///     Signs an authorization entry with a classic Ed25519 <see cref="KeyPair" />. Produces
    ///     V1 credentials by default (see <see cref="SorobanCredentialsVersion" />).
    /// </summary>
    /// <param name="entry">The (typically simulation-produced) entry to sign. Its credentials must be address credentials.</param>
    /// <param name="signer">The Ed25519 key pair authorizing the invocation.</param>
    /// <param name="validUntilLedgerSeq">The ledger sequence number on which the signature expires.</param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="version">
    ///     Which credential variant to produce. Defaults to V1 (accepted on all networks); pass
    ///     <see cref="SorobanCredentialsVersion.V2" /> for address-bound, replay-safe credentials once
    ///     Protocol 27 is live on the target network.
    /// </param>
    /// <returns>A new signed <see cref="SorobanAuthorizationEntry" />.</returns>
    public static SorobanAuthorizationEntry AuthorizeEntry(
        SorobanAuthorizationEntry entry,
        KeyPair signer,
        uint validUntilLedgerSeq,
        Network network,
        SorobanCredentialsVersion version = SorobanCredentialsVersion.V1)
    {
        return AuthorizeEntry(entry, new KeyPairEntrySigner(signer), validUntilLedgerSeq, network, version);
    }

    /// <summary>
    ///     Signs an authorization entry with a custom <see cref="ISorobanEntrySigner" /> (for
    ///     smart-contract accounts). Produces V1 credentials by default (see
    ///     <see cref="SorobanCredentialsVersion" />).
    /// </summary>
    /// <param name="entry">The (typically simulation-produced) entry to sign. Its credentials must be address credentials.</param>
    /// <param name="signer">The signer authorizing the invocation.</param>
    /// <param name="validUntilLedgerSeq">The ledger sequence number on which the signature expires.</param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="version">
    ///     Which credential variant to produce. Defaults to V1 (accepted on all networks); pass
    ///     <see cref="SorobanCredentialsVersion.V2" /> for address-bound, replay-safe credentials once
    ///     Protocol 27 is live on the target network.
    /// </param>
    /// <returns>A new signed <see cref="SorobanAuthorizationEntry" />.</returns>
    /// <remarks>
    ///     The credential address is taken from <paramref name="entry" /> (matching the Java and JS
    ///     SDKs); <paramref name="signer" /> only supplies the signature. Signing with a key that does
    ///     not correspond to the entry's address yields an entry the on-chain <c>__check_auth</c> rejects.
    /// </remarks>
    public static SorobanAuthorizationEntry AuthorizeEntry(
        SorobanAuthorizationEntry entry,
        ISorobanEntrySigner signer,
        uint validUntilLedgerSeq,
        Network network,
        SorobanCredentialsVersion version = SorobanCredentialsVersion.V1)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(signer);
        ArgumentNullException.ThrowIfNull(network);

        if (entry.Credentials is not SorobanAddressCredentialsBase existing)
        {
            throw new InvalidOperationException(
                "AuthorizeEntry requires address credentials; source-account credentials are signed via the transaction source account.");
        }

        var nonce = existing.Nonce;
        // The credential address comes from the entry, not the signer
        var address = existing.Address;

        var payloadHash = version == SorobanCredentialsVersion.V2
            ? BuildAddressAuthPreimageHash(network, address, nonce, validUntilLedgerSeq, entry.RootInvocation)
            : BuildAuthPreimageHash(network, nonce, validUntilLedgerSeq, entry.RootInvocation);

        var signature = signer.Sign(payloadHash);

        SorobanCredentials credentials = version == SorobanCredentialsVersion.V2
            ? new SorobanAddressCredentialsV2(address, nonce, validUntilLedgerSeq, signature)
            : new SorobanAddressCredentials(address, nonce, validUntilLedgerSeq, signature);

        return new SorobanAuthorizationEntry(credentials, entry.RootInvocation);
    }

    /// <summary>
    ///     Signs an authorization entry as delegated credentials (Protocol 27, CAP-0071-01):
    ///     the root signer authorizes, and each delegate co-signs on its behalf. Produces
    ///     <see cref="SorobanAddressCredentialsWithDelegates" />.
    /// </summary>
    /// <param name="entry">The entry to sign. Its credentials must be address credentials.</param>
    /// <param name="rootSigner">The root account delegating its authorization.</param>
    /// <param name="delegateSigners">
    ///     The delegates co-signing on the root's behalf, in order. May be empty, producing
    ///     delegated credentials with no delegates.
    /// </param>
    /// <param name="validUntilLedgerSeq">The ledger sequence number on which the signatures expire.</param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <returns>A new signed <see cref="SorobanAuthorizationEntry" /> with delegated credentials.</returns>
    /// <remarks>
    ///     <para>
    ///         EXPERIMENTAL. Each delegate signs the address-bound payload
    ///         (<c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c>) bound to its own address, with
    ///         no nested delegates. The XDR encoding is stable, but the exact per-delegate payload
    ///         defined by CAP-0071-01 has not yet been confirmed against a reference implementation;
    ///         verify with a cross-SDK known-answer vector before relying on delegated signing on a
    ///         live network. To build deeper delegation chains, construct
    ///         <see cref="SorobanDelegateSignature" /> values manually.
    ///     </para>
    ///     <para>
    ///         The root credential address is taken from the entry; the
    ///         root signer only supplies the root signature. Delegates are emitted sorted by increasing
    ///         address as required by CAP-71; passing two signers with the same address yields a
    ///         duplicate that is rejected when the entry is serialized.
    ///     </para>
    /// </remarks>
    public static SorobanAuthorizationEntry AuthorizeEntryWithDelegates(
        SorobanAuthorizationEntry entry, ISorobanEntrySigner rootSigner,
        IReadOnlyList<ISorobanEntrySigner> delegateSigners, uint validUntilLedgerSeq, Network network)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(rootSigner);
        ArgumentNullException.ThrowIfNull(delegateSigners);
        ArgumentNullException.ThrowIfNull(network);

        if (entry.Credentials is not SorobanAddressCredentialsBase existing)
        {
            throw new InvalidOperationException("AuthorizeEntryWithDelegates requires address credentials.");
        }

        var nonce = existing.Nonce;
        // The root credential address comes from the entry, not rootSigner
        var rootAddress = existing.Address;

        var rootHash =
            BuildAddressAuthPreimageHash(network, rootAddress, nonce, validUntilLedgerSeq, entry.RootInvocation);
        // The WITH_DELEGATES XDR embeds a bare SorobanAddressCredentials struct (no V1/V2 discriminant),
        // so the V1 wrapper is used here even though the signature is over the V2 address-bound payload.
        var root = new SorobanAddressCredentials(rootAddress, nonce, validUntilLedgerSeq, rootSigner.Sign(rootHash));

        var delegates = new SorobanDelegateSignature[delegateSigners.Count];
        for (var i = 0; i < delegateSigners.Count; i++)
        {
            var delegateSigner = delegateSigners[i];
            if (delegateSigner is null)
            {
                throw new ArgumentException($"Delegate signer at index {i} is null.", nameof(delegateSigners));
            }

            var delegateHash = BuildAddressAuthPreimageHash(
                network, delegateSigner.SignerAddress, nonce, validUntilLedgerSeq, entry.RootInvocation);
            delegates[i] =
                new SorobanDelegateSignature(delegateSigner.SignerAddress, delegateSigner.Sign(delegateHash), []);
        }

        // CAP-71 requires delegate arrays sorted by increasing address. Per-delegate signatures are
        // bound to each delegate's own address and are independent of array order, so sorting after
        // signing is safe and guarantees the produced entry serializes to valid wire data. Encode each
        // address to its canonical XDR key once, then sort the delegates by those cached keys.
        var sortKeys = new byte[delegates.Length][];
        for (var i = 0; i < delegates.Length; i++)
        {
            sortKeys[i] = delegates[i].Address.ToXdrByteArray();
        }

        Array.Sort(
            sortKeys, delegates,
            Comparer<byte[]>.Create((left, right) => left.AsSpan().SequenceCompareTo(right)));

        return new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(root, delegates),
            entry.RootInvocation);
    }

    private static byte[] HashPreimage(HashIDPreimage preimage)
    {
        var writer = new XdrDataOutputStream();
        HashIDPreimage.Encode(writer, preimage);
        return Util.Hash(writer.ToArray());
    }
}