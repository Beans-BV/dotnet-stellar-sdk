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
///     default to <see cref="V2" />, matching the JS reference SDK (v16); pass <see cref="V1" />
///     explicitly when targeting a network that has not yet upgraded to Protocol 27. V2 is expected
///     to replace V1 in Protocol 28.
/// </summary>
public enum SorobanCredentialsVersion
{
    /// <summary>
    ///     Legacy <c>SOROBAN_CREDENTIALS_ADDRESS</c> (CAP-0046). Accepted on all protocol versions;
    ///     use for networks not yet on Protocol 27.
    /// </summary>
    V1,

    /// <summary>
    ///     Protocol 27 address-bound <c>SOROBAN_CREDENTIALS_ADDRESS_V2</c> (CAP-0071-02). The default
    ///     for signing helpers; rejected by pre-Protocol-27 networks. Expected to replace V1 in
    ///     Protocol 28.
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
    ///     Builds and SHA-256-hashes the signing payload for <paramref name="entry" />, selecting the
    ///     preimage variant by the entry's credential type (the single source of truth, mirroring the
    ///     JS SDK's <c>buildAuthorizationEntryPreimage</c>): V1 address credentials select the legacy
    ///     <c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION</c> preimage; V2 and delegated credentials select the
    ///     address-bound <c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c> preimage bound to the
    ///     (root) credential address. Use this to obtain the exact payload for external signers (e.g.
    ///     hardware wallets) without going through
    ///     <see
    ///         cref="AuthorizeEntry(SorobanAuthorizationEntry,
    ///     ISorobanEntrySigner, uint, Network, SorobanCredentialsVersion, ScAddress?)" />
    ///     .
    /// </summary>
    /// <param name="entry">The entry whose signing payload to compute. Its credentials must be address credentials.</param>
    /// <param name="validUntilLedgerSeq">The ledger sequence number on which the signature expires.</param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <returns>A 32-byte SHA-256 hash of the XDR-encoded preimage.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown for source-account credentials, which have no signature payload (they are signed via
    ///     the transaction source account), and for unrecognized credential subtypes.
    /// </exception>
    public static byte[] BuildAuthorizationEntryPreimageHash(
        SorobanAuthorizationEntry entry,
        uint validUntilLedgerSeq,
        Network network)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(network);

        return entry.Credentials switch
        {
            SorobanAddressCredentialsWithDelegates withDelegates => BuildAddressAuthPreimageHash(
                network, withDelegates.AddressCredentials.Address, withDelegates.AddressCredentials.Nonce,
                validUntilLedgerSeq, entry.RootInvocation),
            SorobanAddressCredentialsV2 v2 => BuildAddressAuthPreimageHash(
                network, v2.Address, v2.Nonce, validUntilLedgerSeq, entry.RootInvocation),
            SorobanAddressCredentials v1 => BuildAuthPreimageHash(
                network, v1.Nonce, validUntilLedgerSeq, entry.RootInvocation),
            SorobanSourceAccountCredentials => throw new InvalidOperationException(
                "Source-account credentials have no signature payload; they are signed via the transaction source account."),
            _ => throw new InvalidOperationException(
                $"Unknown SorobanCredentials type: {entry.Credentials.GetType()}"),
        };
    }

    /// <summary>
    ///     Signs an authorization entry with a classic Ed25519 <see cref="KeyPair" />. Produces
    ///     V2 (address-bound) credentials by default (see <see cref="SorobanCredentialsVersion" />).
    /// </summary>
    /// <param name="entry">The (typically simulation-produced) entry to sign. Its credentials must be address credentials.</param>
    /// <param name="signer">The Ed25519 key pair authorizing the invocation.</param>
    /// <param name="validUntilLedgerSeq">
    ///     The ledger sequence number on which the signature expires. In multi-party flows every
    ///     signer must pass the same value; see the signer-based overload's remarks.
    /// </param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="version">
    ///     Which credential variant to produce. Defaults to V2 (address-bound, replay-safe), matching
    ///     the JS reference SDK; pass <see cref="SorobanCredentialsVersion.V1" /> when targeting a
    ///     network that has not yet upgraded to Protocol 27, which rejects V2 credentials. Ignored for
    ///     entries carrying delegated credentials, which keep their credential type.
    /// </param>
    /// <param name="forAddress">
    ///     Optional address of the credential node that receives the signature. When omitted, the
    ///     signature is placed on the root (top-level) credential node. For delegated entries, pass a
    ///     delegate's address to route the signature to every matching delegate node (including nested
    ///     ones), enabling incremental multi-party signing.
    /// </param>
    /// <returns>A new signed <see cref="SorobanAuthorizationEntry" />.</returns>
    public static SorobanAuthorizationEntry AuthorizeEntry(
        SorobanAuthorizationEntry entry,
        KeyPair signer,
        uint validUntilLedgerSeq,
        Network network,
        SorobanCredentialsVersion version = SorobanCredentialsVersion.V2,
        ScAddress? forAddress = null)
    {
        ArgumentNullException.ThrowIfNull(signer);

        return AuthorizeEntry(entry, new KeyPairEntrySigner(signer), validUntilLedgerSeq, network, version,
            forAddress);
    }

    /// <summary>
    ///     Signs an authorization entry with a custom <see cref="ISorobanEntrySigner" /> (for
    ///     smart-contract accounts). Produces V2 (address-bound) credentials by default (see
    ///     <see cref="SorobanCredentialsVersion" />).
    /// </summary>
    /// <param name="entry">The (typically simulation-produced) entry to sign. Its credentials must be address credentials.</param>
    /// <param name="signer">The signer authorizing the invocation.</param>
    /// <param name="validUntilLedgerSeq">
    ///     The ledger sequence number on which the signature expires. In multi-party flows every
    ///     signer must pass the same value; see remarks.
    /// </param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="version">
    ///     Which credential variant to produce. Defaults to V2 (address-bound, replay-safe), matching
    ///     the JS reference SDK; pass <see cref="SorobanCredentialsVersion.V1" /> when targeting a
    ///     network that has not yet upgraded to Protocol 27, which rejects V2 credentials. Ignored for
    ///     entries carrying delegated credentials, which keep their credential type.
    /// </param>
    /// <param name="forAddress">
    ///     Optional address of the credential node that receives the signature. When omitted, the
    ///     signature is placed on the root (top-level) credential node. For delegated entries, pass a
    ///     delegate's address to route the signature to every matching delegate node (including nested
    ///     ones), enabling incremental multi-party signing.
    /// </param>
    /// <returns>A new signed <see cref="SorobanAuthorizationEntry" />.</returns>
    /// <remarks>
    ///     <para>
    ///         The credential address is taken from <paramref name="entry" /> (matching the Java and JS
    ///         SDKs); <paramref name="signer" /> only supplies the signature. Signing with a key that does
    ///         not correspond to the entry's address yields an entry the on-chain <c>__check_auth</c> rejects.
    ///     </para>
    ///     <para>
    ///         Entries carrying <see cref="SorobanAddressCredentialsWithDelegates" /> credentials keep
    ///         their credential type (<paramref name="version" /> is ignored): the payload is the
    ///         CAP-0071-01 root-bound preimage shared by the top-level account and every delegate, the
    ///         root expiration is updated to <paramref name="validUntilLedgerSeq" />, and the signatures
    ///         of all non-target nodes are preserved — mirroring the JS SDK's
    ///         <c>authorizeEntry(..., forAddress)</c>. Throws <see cref="InvalidOperationException" />
    ///         when <paramref name="forAddress" /> matches no credential node.
    ///     </para>
    ///     <para>
    ///         IMPORTANT — multi-party coordination: the signing payload embeds
    ///         <paramref name="validUntilLedgerSeq" />, and this method updates the root credential's
    ///         expiration to that value while leaving the other nodes' signatures untouched. Every party
    ///         signing the same delegated entry must therefore pass the <b>same</b>
    ///         <paramref name="validUntilLedgerSeq" />: re-signing with a different value silently
    ///         invalidates all previously attached signatures, because their payloads embedded the old
    ///         expiration. Fix the expiration once (e.g. via
    ///         <see cref="BuildWithDelegatesEntry" />) before collecting signatures.
    ///     </para>
    /// </remarks>
    public static SorobanAuthorizationEntry AuthorizeEntry(
        SorobanAuthorizationEntry entry,
        ISorobanEntrySigner signer,
        uint validUntilLedgerSeq,
        Network network,
        SorobanCredentialsVersion version = SorobanCredentialsVersion.V2,
        ScAddress? forAddress = null)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(signer);
        ArgumentNullException.ThrowIfNull(network);

        return entry.Credentials switch
        {
            SorobanAddressCredentialsWithDelegates withDelegates =>
                AuthorizeDelegatedEntry(entry, withDelegates, signer, validUntilLedgerSeq, network, forAddress),
            SorobanAddressCredentialsBase existing =>
                AuthorizeAddressEntry(entry, existing, signer, validUntilLedgerSeq, network, version, forAddress),
            SorobanSourceAccountCredentials => throw new InvalidOperationException(
                "AuthorizeEntry requires address credentials; source-account credentials are signed via the transaction source account."),
            _ => throw new InvalidOperationException(
                $"Unknown SorobanCredentials type: {entry.Credentials.GetType()}"),
        };
    }

    private static SorobanAuthorizationEntry AuthorizeAddressEntry(
        SorobanAuthorizationEntry entry,
        SorobanAddressCredentialsBase existing,
        ISorobanEntrySigner signer,
        uint validUntilLedgerSeq,
        Network network,
        SorobanCredentialsVersion version,
        ScAddress? forAddress)
    {
        var nonce = existing.Nonce;
        // The credential address comes from the entry, not the signer
        var address = existing.Address;

        if (forAddress is not null && !AddressEquals(forAddress, address))
        {
            throw new InvalidOperationException(
                "The authorization entry has no credential node for the requested forAddress.");
        }

        var payloadHash = version == SorobanCredentialsVersion.V2
            ? BuildAddressAuthPreimageHash(network, address, nonce, validUntilLedgerSeq, entry.RootInvocation)
            : BuildAuthPreimageHash(network, nonce, validUntilLedgerSeq, entry.RootInvocation);

        var signature = signer.Sign(payloadHash);

        SorobanCredentials credentials = version == SorobanCredentialsVersion.V2
            ? new SorobanAddressCredentialsV2(address, nonce, validUntilLedgerSeq, signature)
            : new SorobanAddressCredentials(address, nonce, validUntilLedgerSeq, signature);

        return new SorobanAuthorizationEntry(credentials, entry.RootInvocation);
    }

    private static SorobanAuthorizationEntry AuthorizeDelegatedEntry(
        SorobanAuthorizationEntry entry,
        SorobanAddressCredentialsWithDelegates withDelegates,
        ISorobanEntrySigner signer,
        uint validUntilLedgerSeq,
        Network network,
        ScAddress? forAddress)
    {
        var root = withDelegates.AddressCredentials;
        // CAP-0071-01: the top-level account and every delegate sign the same payload, bound to
        // the top-level (root) address.
        var payloadHash = BuildAuthorizationEntryPreimageHash(entry, validUntilLedgerSeq, network);
        var signature = signer.Sign(payloadHash);

        // Encode the routing target once; node comparisons reuse the cached key.
        var forAddressKey = forAddress?.ToXdrByteArray();
        var signRoot = forAddressKey is null || forAddressKey.AsSpan().SequenceEqual(root.Address.ToXdrByteArray());
        var matched = signRoot;

        var delegates = withDelegates.Delegates;
        if (forAddressKey is not null)
        {
            delegates = RouteDelegateSignature(delegates, forAddressKey, signature, ref matched);
        }

        if (!matched)
        {
            throw new InvalidOperationException(
                "The authorization entry has no credential node for the requested forAddress.");
        }

        var newRoot = new SorobanAddressCredentials(
            root.Address, root.Nonce, validUntilLedgerSeq, signRoot ? signature : root.Signature);

        return new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(newRoot, delegates),
            entry.RootInvocation);
    }

    private static SorobanDelegateSignature[] RouteDelegateSignature(
        SorobanDelegateSignature[] delegates, byte[] forAddressKey, SCVal signature, ref bool matched)
    {
        var result = new SorobanDelegateSignature[delegates.Length];
        for (var i = 0; i < delegates.Length; i++)
        {
            var current = delegates[i];
            if (current is null)
            {
                // Same rule SorobanDelegateSignature.ValidateOrder enforces at serialization time.
                throw new InvalidOperationException("Delegate signatures must not contain null entries.");
            }

            var nested = RouteDelegateSignature(current.NestedDelegates, forAddressKey, signature, ref matched);
            if (forAddressKey.AsSpan().SequenceEqual(current.Address.ToXdrByteArray()))
            {
                matched = true;
                result[i] = new SorobanDelegateSignature(current.Address, signature, nested);
            }
            else
            {
                result[i] = new SorobanDelegateSignature(current.Address, current.Signature, nested);
            }
        }

        return result;
    }

    private static bool AddressEquals(ScAddress left, ScAddress right)
    {
        return left.ToXdrByteArray().AsSpan().SequenceEqual(right.ToXdrByteArray());
    }

    /// <summary>
    ///     Signs an authorization entry as delegated credentials (Protocol 27, CAP-0071-01):
    ///     the root signer authorizes, and each delegate co-signs on its behalf. Produces
    ///     <see cref="SorobanAddressCredentialsWithDelegates" />.
    /// </summary>
    /// <param name="entry">The entry to sign. Its credentials must be address credentials.</param>
    /// <param name="rootSigner">The root account delegating its authorization.</param>
    /// <param name="delegateSigners">
    ///     The delegates co-signing on the root's behalf; input order is irrelevant, as the method
    ///     sorts them into the protocol-required ascending-address order. May be empty, producing
    ///     delegated credentials with no delegates.
    /// </param>
    /// <param name="validUntilLedgerSeq">The ledger sequence number on which the signatures expire.</param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <returns>A new signed <see cref="SorobanAuthorizationEntry" /> with delegated credentials.</returns>
    /// <remarks>
    ///     <para>
    ///         Per CAP-0071-01, the top-level account and every delegate sign the same payload: the
    ///         address-bound preimage (<c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c>)
    ///         bound to the top-level account's address. This method emits leaf delegates only (no
    ///         nested delegates); to build deeper delegation chains, construct
    ///         <see cref="SorobanDelegateSignature" /> values manually — every nested array must
    ///         also be sorted by increasing address with no duplicates. The shared signing payload is
    ///         cross-checked against the JS reference SDK by an in-repo known-answer test; the full
    ///         delegated-entry XDR layout is locked by the vendored Protocol 27 schema.
    ///     </para>
    ///     <para>
    ///         The root credential address is taken from the entry; the
    ///         root signer only supplies the root signature. Delegates are emitted sorted by increasing
    ///         address as required by CAP-71; passing two signers with the same address throws an
    ///         <see cref="ArgumentException" />, because the protocol rejects duplicate delegate
    ///         addresses at invocation time.
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
        // so the concrete wrapper variant is irrelevant here; a V1 instance carries the fields even
        // though the signature is computed over the V2 address-bound payload.
        var root = new SorobanAddressCredentials(rootAddress, nonce, validUntilLedgerSeq, rootSigner.Sign(rootHash));

        // CAP-0071-01: every delegate signs the same payload as the top-level account — the
        // address-bound preimage bound to the top-level (root) address, i.e. rootHash above.
        var delegates = new SorobanDelegateSignature[delegateSigners.Count];
        for (var i = 0; i < delegateSigners.Count; i++)
        {
            var delegateSigner = delegateSigners[i];
            if (delegateSigner is null)
            {
                throw new ArgumentException($"Delegate signer at index {i} is null.", nameof(delegateSigners));
            }

            delegates[i] =
                new SorobanDelegateSignature(delegateSigner.SignerAddress, delegateSigner.Sign(rootHash), []);
        }

        // CAP-0071-01 requires delegate arrays sorted by increasing address with no duplicate
        // addresses. All delegate signatures are over the same root-bound payload and therefore
        // independent of array order, so sorting after signing is safe.
        SortDelegatesByAddress(delegates, nameof(delegateSigners));

        return new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(root, delegates),
            entry.RootInvocation);
    }

    /// <summary>
    ///     Builds an unsigned delegated authorization entry (mirroring the JS SDK's
    ///     <c>buildWithDelegatesEntry</c>): the root credential and every delegate node carry an
    ///     <see cref="SCVoid" /> placeholder signature, ready for incremental signing via
    ///     <see
    ///         cref="AuthorizeEntry(SorobanAuthorizationEntry, ISorobanEntrySigner, uint, Network,
    ///     SorobanCredentialsVersion, ScAddress?)" />
    ///     with <c>forAddress</c>.
    /// </summary>
    /// <param name="entry">
    ///     The (typically simulation-produced) entry to convert. Address credentials supply the root
    ///     address and nonce; if the entry already carries delegated credentials, its root is reused
    ///     and its delegates array is replaced.
    /// </param>
    /// <param name="delegateAddresses">
    ///     The delegate addresses, one leaf node each; input order is irrelevant, as the method sorts
    ///     them into the protocol-required ascending-address order. Duplicate addresses throw an
    ///     <see cref="ArgumentException" />. For deeper delegation chains, construct
    ///     <see cref="SorobanDelegateSignature" /> values manually.
    /// </param>
    /// <param name="validUntilLedgerSeq">
    ///     The ledger sequence number on which the signatures will expire. This value is embedded in
    ///     every party's signing payload — fix it here once, and have every subsequent
    ///     <c>AuthorizeEntry</c> call pass the same value.
    /// </param>
    /// <returns>A new unsigned <see cref="SorobanAuthorizationEntry" /> with delegated credentials.</returns>
    public static SorobanAuthorizationEntry BuildWithDelegatesEntry(
        SorobanAuthorizationEntry entry,
        IReadOnlyList<ScAddress> delegateAddresses,
        uint validUntilLedgerSeq)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(delegateAddresses);

        var (rootAddress, rootNonce) = entry.Credentials switch
        {
            SorobanAddressCredentialsWithDelegates withDelegates =>
                (withDelegates.AddressCredentials.Address, withDelegates.AddressCredentials.Nonce),
            SorobanAddressCredentialsBase address => (address.Address, address.Nonce),
            _ => throw new InvalidOperationException("BuildWithDelegatesEntry requires address credentials."),
        };

        var delegates = new SorobanDelegateSignature[delegateAddresses.Count];
        for (var i = 0; i < delegateAddresses.Count; i++)
        {
            var delegateAddress = delegateAddresses[i];
            if (delegateAddress is null)
            {
                throw new ArgumentException($"Delegate address at index {i} is null.", nameof(delegateAddresses));
            }

            delegates[i] = new SorobanDelegateSignature(delegateAddress, new SCVoid(), []);
        }

        SortDelegatesByAddress(delegates, nameof(delegateAddresses));

        return new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(rootAddress, rootNonce, validUntilLedgerSeq, new SCVoid()),
                delegates),
            entry.RootInvocation);
    }

    /// <summary>
    ///     Sorts a delegate array in place into the CAP-0071-01 ascending-address order and throws
    ///     <see cref="ArgumentException" /> on duplicate addresses. Each address is encoded to its
    ///     canonical XDR key exactly once.
    /// </summary>
    private static void SortDelegatesByAddress(SorobanDelegateSignature[] delegates, string paramName)
    {
        var sortKeys = new byte[delegates.Length][];
        for (var i = 0; i < delegates.Length; i++)
        {
            sortKeys[i] = delegates[i].Address.ToXdrByteArray();
        }

        Array.Sort(
            sortKeys, delegates,
            Comparer<byte[]>.Create((left, right) => left.AsSpan().SequenceCompareTo(right)));

        for (var i = 1; i < sortKeys.Length; i++)
        {
            if (sortKeys[i - 1].AsSpan().SequenceEqual(sortKeys[i]))
            {
                throw new ArgumentException(
                    $"{paramName} contains two entries with the same address; CAP-0071-01 forbids duplicate addresses in a delegates array.",
                    paramName);
            }
        }
    }

    private static byte[] HashPreimage(HashIDPreimage preimage)
    {
        var writer = new XdrDataOutputStream();
        HashIDPreimage.Encode(writer, preimage);
        return Util.Hash(writer.ToArray());
    }
}