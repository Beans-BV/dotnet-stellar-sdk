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
///     default to <see cref="Preserve" />, keeping the entry's existing variant and matching the JS
///     reference SDK (v16), whose <c>authorizeEntry</c> never changes the credential type. Pass
///     <see cref="V2" /> explicitly to upgrade a legacy entry to the Protocol 27 address-bound
///     credential.
/// </summary>
public enum SorobanCredentialsVersion
{
    /// <summary>
    ///     Keep the entry's existing credential variant (V1 stays V1, V2 stays V2). The default for
    ///     signing helpers, matching the JS reference SDK, which preserves the credential type. Safe
    ///     for networks not yet on Protocol 27 when the input entry is itself V1.
    /// </summary>
    // Explicit values: this enum's members are positional defaults (Preserve = default(T)); pin the
    // numeric values so reordering the members cannot silently change a persisted/serialized value.
    Preserve = 0,

    /// <summary>
    ///     Legacy <c>SOROBAN_CREDENTIALS_ADDRESS</c> (CAP-0046). Accepted on all protocol versions;
    ///     force this when targeting a network not yet on Protocol 27.
    /// </summary>
    V1 = 1,

    /// <summary>
    ///     Protocol 27 address-bound <c>SOROBAN_CREDENTIALS_ADDRESS_V2</c> (CAP-0071-02); rejected by
    ///     pre-Protocol-27 networks.
    /// </summary>
    V2 = 2,
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
    // Bounds in-memory recursion over caller-built delegate trees, mirroring the XDR decoder's depth cap
    // (a decoded tree is already capped at XdrDataInputStream.DefaultMaxDepth). Converts an otherwise
    // uncatchable StackOverflowException on a pathologically deep tree into a clear, catchable exception.
    private const int MaxDelegateNestingDepth = XdrDataInputStream.DefaultMaxDepth;

    /// <summary>The single canonical-XDR-key comparer used to order and de-duplicate delegate addresses.</summary>
    private static readonly IComparer<byte[]> XdrKeyComparer =
        Comparer<byte[]>.Create((left, right) => left.AsSpan().SequenceCompareTo(right));

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
    ///         cref="AuthorizeEntry(SorobanAuthorizationEntry, ISorobanEntrySigner, uint, Network, SorobanCredentialsVersion, ScAddress?)" />
    ///     .
    /// </summary>
    /// <param name="entry">
    ///     The entry whose signing payload to compute. Its credentials must be address (V1/V2) or
    ///     delegated credentials; source-account credentials are rejected.
    /// </param>
    /// <param name="validUntilLedgerSeq">The ledger sequence number on which the signature expires.</param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <returns>A 32-byte SHA-256 hash of the XDR-encoded preimage.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="entry" /> or <paramref name="network" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown for source-account credentials, which have no signing payload (they are authorized via
    ///     the transaction source account); pass an address-credentialed or delegated entry instead.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown for unrecognized credential subtypes.</exception>
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
            SorobanSourceAccountCredentials => throw new ArgumentException(
                "Source-account credentials have no signing payload; they are authorized via the transaction " +
                "source account. Pass an entry with ADDRESS, ADDRESS_V2, or delegated credentials.",
                nameof(entry)),
            _ => throw new InvalidOperationException(
                $"Unknown SorobanCredentials type: {entry.Credentials.GetType()}"),
        };
    }

    /// <summary>
    ///     Signs an authorization entry with a classic Ed25519 <see cref="KeyPair" />. By default it
    ///     preserves the entry's existing credential variant (see <see cref="SorobanCredentialsVersion" />).
    /// </summary>
    /// <param name="entry">
    ///     The (typically simulation-produced) entry to sign. Address or delegated credentials are
    ///     signed; source-account credentials are returned unchanged (a no-op — <paramref name="forAddress" />
    ///     and <paramref name="version" /> are ignored for them), so callers can authorize every entry
    ///     from a simulation result without special-casing the mix.
    /// </param>
    /// <param name="signer">The Ed25519 key pair authorizing the invocation.</param>
    /// <param name="validUntilLedgerSeq">
    ///     The ledger sequence number on which the signature expires. In multi-party flows every
    ///     signer must pass the same value; see the signer-based overload's remarks.
    /// </param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="version">
    ///     Which credential variant to produce. Defaults to <see cref="SorobanCredentialsVersion.Preserve" />,
    ///     keeping the entry's existing variant (matching the JS reference SDK, whose
    ///     <c>authorizeEntry</c> never changes the credential type); pass
    ///     <see cref="SorobanCredentialsVersion.V2" /> to upgrade a legacy entry to the Protocol 27
    ///     address-bound credential, or <see cref="SorobanCredentialsVersion.V1" /> to force the legacy
    ///     credential. Ignored for entries carrying delegated credentials, which keep their credential type.
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
        SorobanCredentialsVersion version = SorobanCredentialsVersion.Preserve,
        ScAddress? forAddress = null)
    {
        ArgumentNullException.ThrowIfNull(signer);

        return AuthorizeEntry(entry, new KeyPairEntrySigner(signer), validUntilLedgerSeq, network, version,
            forAddress);
    }

    /// <summary>
    ///     Signs an authorization entry with a custom <see cref="ISorobanEntrySigner" /> (for
    ///     smart-contract accounts). By default it preserves the entry's existing credential variant
    ///     (see <see cref="SorobanCredentialsVersion" />).
    /// </summary>
    /// <param name="entry">
    ///     The (typically simulation-produced) entry to sign. Address or delegated credentials are
    ///     signed; source-account credentials are returned unchanged (a no-op — <paramref name="forAddress" />
    ///     and <paramref name="version" /> are ignored for them), so callers can authorize every entry
    ///     from a simulation result without special-casing the mix.
    /// </param>
    /// <param name="signer">The signer authorizing the invocation.</param>
    /// <param name="validUntilLedgerSeq">
    ///     The ledger sequence number on which the signature expires. In multi-party flows every
    ///     signer must pass the same value; see remarks.
    /// </param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <param name="version">
    ///     Which credential variant to produce. Defaults to <see cref="SorobanCredentialsVersion.Preserve" />,
    ///     keeping the entry's existing variant (matching the JS reference SDK, whose
    ///     <c>authorizeEntry</c> never changes the credential type); pass
    ///     <see cref="SorobanCredentialsVersion.V2" /> to upgrade a legacy entry to the Protocol 27
    ///     address-bound credential, or <see cref="SorobanCredentialsVersion.V1" /> to force the legacy
    ///     credential. Ignored for entries carrying delegated credentials, which keep their credential type.
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
    ///         <c>authorizeEntry(..., forAddress)</c>. Throws <see cref="ArgumentException" />
    ///         when <paramref name="forAddress" /> matches no credential node.
    ///     </para>
    ///     <para>
    ///         IMPORTANT — multi-party coordination: the signing payload embeds
    ///         <paramref name="validUntilLedgerSeq" />, and this method updates the root credential's
    ///         expiration to that value while leaving the other nodes' signatures untouched. Every party
    ///         signing the same delegated entry must therefore pass the <b>same</b>
    ///         <paramref name="validUntilLedgerSeq" />: re-signing with a different value invalidates the
    ///         signatures already attached, because their payloads embedded the old expiration. As a
    ///         partial safety net this method throws an <see cref="InvalidOperationException" /> when the
    ///         expiration would change out from under an existing real (non-placeholder) signature it must
    ///         preserve — both when signing a delegate while the root already holds a signature over a
    ///         different expiration, and when re-signing the root (whether <paramref name="forAddress" />
    ///         is omitted or names the root address) while a delegate already holds one. Delegate-vs-delegate
    ///         mismatches (re-signing one delegate
    ///         while a sibling is signed over a different expiration) cannot be detected here, because a
    ///         delegate node carries no expiration of its own; fix the expiration once (e.g. via
    ///         <see cref="BuildWithDelegatesEntry" />) before collecting signatures.
    ///     </para>
    /// </remarks>
    public static SorobanAuthorizationEntry AuthorizeEntry(
        SorobanAuthorizationEntry entry,
        ISorobanEntrySigner signer,
        uint validUntilLedgerSeq,
        Network network,
        SorobanCredentialsVersion version = SorobanCredentialsVersion.Preserve,
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
            // Source-account credentials are authorized by the transaction source-account signature, so
            // there is nothing to sign here; return the entry unchanged (forAddress and version have no
            // effect and are ignored), matching the JS and Java SDKs. This lets callers authorize every
            // entry from simulation — passing a uniform version/forAddress across a mixed list — without
            // special-casing the source-account entries.
            SorobanSourceAccountCredentials => entry,
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

        if (forAddress is not null && !KeyEqualsAddress(forAddress.ToXdrByteArray(), address))
        {
            throw new ArgumentException(
                "The authorization entry has no credential node for the requested forAddress.",
                nameof(forAddress));
        }

        // Preserve the entry's existing variant unless the caller forces V1/V2 (matching the JS SDK,
        // whose authorizeEntry never changes the credential type).
        var produceV2 = version switch
        {
            SorobanCredentialsVersion.V1 => false,
            SorobanCredentialsVersion.V2 => true,
            _ => existing is SorobanAddressCredentialsV2,
        };

        var payloadHash = produceV2
            ? BuildAddressAuthPreimageHash(network, address, nonce, validUntilLedgerSeq, entry.RootInvocation)
            : BuildAuthPreimageHash(network, nonce, validUntilLedgerSeq, entry.RootInvocation);

        var signature = signer.Sign(payloadHash);

        SorobanCredentials credentials = produceV2
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

        // Encode the routing target once; node comparisons reuse the cached key.
        var forAddressKey = forAddress?.ToXdrByteArray();
        var signRoot = forAddressKey is null || KeyEqualsAddress(forAddressKey, root.Address);

        // Resolve which node(s) forAddress targets BEFORE invoking the (possibly interactive) signer, so a
        // bogus forAddress is rejected without first prompting a hardware wallet / remote co-signer for a
        // signature that would only be discarded. Mirrors AuthorizeEntryWithDelegates, which likewise
        // validates the delegate set before signing.
        if (forAddressKey is not null && !signRoot &&
            !DelegatesContainAddress(withDelegates.Delegates, forAddressKey, 0))
        {
            throw new ArgumentException(
                "The authorization entry has no credential node for the requested forAddress.",
                nameof(forAddress));
        }

        // When signing only a delegate (not the root), the root's expiration is still rewritten to
        // validUntilLedgerSeq below while its existing signature is preserved. If that signature is a
        // real (non-placeholder) one taken over a different expiration, preserving it would silently
        // produce an entry the network rejects in __check_auth — CAP-71 requires every signature in the
        // entry to commit to the same expiration. The JS SDK rewrites silently; we fail fast instead,
        // since the mismatch is detectable locally.
        if (!signRoot && root.Signature is not SCVoid &&
            root.SignatureExpirationLedger != validUntilLedgerSeq)
        {
            throw new InvalidOperationException(
                "Cannot sign a delegate with a validUntilLedgerSeq that differs from the root credential's " +
                "existing signature expiration; the preserved root signature would be invalid. Re-build the " +
                "entry (for example via BuildWithDelegatesEntry) so every party signs the same expiration.");
        }

        // Symmetric guard for the opposite direction: re-signing the root rewrites the root expiration
        // below while every delegate signature is preserved untouched. If any delegate already holds a
        // real (non-SCVoid) signature, it was taken over the old expiration and would be silently
        // invalidated. Fail fast for the same reason as above. This keys on signRoot, not "forAddress is
        // null", so it also fires when the caller targets the root by passing its address explicitly
        // (forAddress == root.Address) — both paths re-sign the root identically. (Delegate-vs-delegate
        // mismatches — re-signing one delegate while a sibling is signed over a different expiration —
        // still cannot be detected here, as a delegate node carries no expiration of its own.)
        if (signRoot && root.SignatureExpirationLedger != validUntilLedgerSeq &&
            AnyDelegateHasRealSignature(withDelegates.Delegates, 0))
        {
            throw new InvalidOperationException(
                "Cannot re-sign the root with a validUntilLedgerSeq that differs from the entry's current " +
                "expiration while delegates already hold signatures over the old expiration; those preserved " +
                "delegate signatures would be invalid. Re-build the entry (for example via " +
                "BuildWithDelegatesEntry) so every party signs the same expiration.");
        }

        // CAP-0071-01: the top-level account and every delegate sign the same payload, bound to the
        // top-level (root) address. Sign only after every validation above has passed.
        var payloadHash = BuildAuthorizationEntryPreimageHash(entry, validUntilLedgerSeq, network);
        var signature = signer.Sign(payloadHash);

        var delegates = withDelegates.Delegates;
        if (forAddressKey is not null)
        {
            // forAddress was confirmed to match the root or a delegate above, so routing always places
            // the signature on at least one node.
            delegates = RouteDelegateSignature(delegates, forAddressKey, signature, 0);
        }

        var newRoot = new SorobanAddressCredentials(
            root.Address, root.Nonce, validUntilLedgerSeq, signRoot ? signature : root.Signature);

        return new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(newRoot, delegates),
            entry.RootInvocation);
    }

    private static void ThrowIfDelegateNestingTooDeep(int depth)
    {
        if (depth > MaxDelegateNestingDepth)
        {
            throw new InvalidOperationException(
                $"Delegate nesting exceeds the maximum supported depth of {MaxDelegateNestingDepth}.");
        }
    }

    private static SorobanDelegateSignature[] RouteDelegateSignature(
        SorobanDelegateSignature[] delegates, byte[] forAddressKey, SCVal signature, int depth)
    {
        ThrowIfDelegateNestingTooDeep(depth);
        var result = new SorobanDelegateSignature[delegates.Length];
        for (var i = 0; i < delegates.Length; i++)
        {
            var current = delegates[i];
            if (current is null)
            {
                // Reject null entries with a clear error rather than a NullReferenceException, matching
                // the null guard in SorobanDelegateSignature.ToXdr.
                throw new InvalidOperationException("Delegate signatures must not contain null entries.");
            }

            var nested = RouteDelegateSignature(current.NestedDelegates, forAddressKey, signature, depth + 1);
            result[i] = KeyEqualsAddress(forAddressKey, current.Address)
                ? new SorobanDelegateSignature(current.Address, signature, nested)
                : new SorobanDelegateSignature(current.Address, current.Signature, nested);
        }

        return result;
    }

    /// <summary>
    ///     Whether any node in the delegate forest (including nested delegates) matches the precomputed
    ///     <paramref name="forAddressKey" />. Used to resolve a routing target before signing. Rejects
    ///     null nodes with the same clear error as <see cref="RouteDelegateSignature" />, so a malformed
    ///     tree surfaces its structural error instead of being masked by the forAddress check.
    /// </summary>
    private static bool DelegatesContainAddress(
        SorobanDelegateSignature[] delegates, byte[] forAddressKey, int depth)
    {
        ThrowIfDelegateNestingTooDeep(depth);
        foreach (var current in delegates)
        {
            if (current is null)
            {
                throw new InvalidOperationException("Delegate signatures must not contain null entries.");
            }

            if (KeyEqualsAddress(forAddressKey, current.Address) ||
                DelegatesContainAddress(current.NestedDelegates, forAddressKey, depth + 1))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Whether <paramref name="address" /> encodes to the same canonical XDR key as the precomputed
    ///     <paramref name="key" /> (see <see cref="ScAddressExtensions.ToXdrByteArray" />). The single
    ///     address-equality check used by all credential-node routing.
    /// </summary>
    private static bool KeyEqualsAddress(byte[] key, ScAddress address)
    {
        return key.AsSpan().SequenceEqual(address.ToXdrByteArray());
    }

    /// <summary>
    ///     Whether any node in the delegate forest (including nested delegates) carries a real
    ///     (non-<see cref="SCVoid" />) signature. Used to fail fast when re-signing the root would bump
    ///     the expiration out from under an already-signed delegate. Null nodes are ignored here; they
    ///     are rejected later by <see cref="RouteDelegateSignature" /> or serialization.
    /// </summary>
    private static bool AnyDelegateHasRealSignature(SorobanDelegateSignature[] delegates, int depth)
    {
        ThrowIfDelegateNestingTooDeep(depth);
        foreach (var current in delegates)
        {
            if (current is null)
            {
                continue;
            }

            if (current.Signature is not SCVoid || AnyDelegateHasRealSignature(current.NestedDelegates, depth + 1))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Signs an authorization entry as delegated credentials (Protocol 27, CAP-0071-01):
    ///     the root signer authorizes, and each delegate co-signs on its behalf. Produces
    ///     <see cref="SorobanAddressCredentialsWithDelegates" />.
    /// </summary>
    /// <param name="entry">
    ///     The entry to sign. Its credentials must be ADDRESS or ADDRESS_V2; source-account and
    ///     already-delegated entries are rejected.
    /// </param>
    /// <param name="rootSigner">The root account delegating its authorization.</param>
    /// <param name="delegateSigners">
    ///     The delegates co-signing on the root's behalf; input order is irrelevant, as the method
    ///     sorts them into the protocol-required ascending-address order. May be empty, producing
    ///     delegated credentials with no delegates.
    /// </param>
    /// <param name="validUntilLedgerSeq">The ledger sequence number on which the signatures expire.</param>
    /// <param name="network">The network the transaction is submitted to.</param>
    /// <returns>A new signed <see cref="SorobanAuthorizationEntry" /> with delegated credentials.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="entry" />, <paramref name="rootSigner" />,
    ///     <paramref name="delegateSigners" />, or <paramref name="network" /> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown — before any signer is invoked — when <paramref name="entry" /> does not carry ADDRESS
    ///     or ADDRESS_V2 credentials (source-account or already-delegated), when any element of
    ///     <paramref name="delegateSigners" /> is null, or when two delegate signers share the same
    ///     address (CAP-0071-01 forbids duplicate delegate addresses).
    /// </exception>
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
    ///         <see cref="ArgumentException" /> before any signer is invoked, because the protocol
    ///         rejects duplicate delegate addresses at invocation time.
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

        // Mirror BuildWithDelegatesEntry: an already-delegated entry is not a valid input here. Sign
        // such an entry incrementally with AuthorizeEntry(..., forAddress) instead.
        if (entry.Credentials is SorobanAddressCredentialsWithDelegates)
        {
            throw new ArgumentException(
                "AuthorizeEntryWithDelegates expects ADDRESS or ADDRESS_V2 credentials, but the entry already " +
                "carries delegated (WITH_DELEGATES) credentials. Sign an existing delegated entry incrementally " +
                "with AuthorizeEntry(..., forAddress).",
                nameof(entry));
        }

        if (entry.Credentials is not SorobanAddressCredentialsBase existing)
        {
            throw new ArgumentException(
                "AuthorizeEntryWithDelegates requires address credentials (ADDRESS or ADDRESS_V2).",
                nameof(entry));
        }

        var nonce = existing.Nonce;
        // The root credential address comes from the entry, not rootSigner
        var rootAddress = existing.Address;

        var rootHash =
            BuildAddressAuthPreimageHash(network, rootAddress, nonce, validUntilLedgerSeq, entry.RootInvocation);

        // Validate the delegate set (null entries + CAP-71 duplicate addresses) BEFORE invoking any
        // signer: the addresses are known from SignerAddress alone, and an interactive signer (hardware
        // wallet, remote co-signing service) must not be prompted for a request that is guaranteed to be
        // rejected once a duplicate is found. The canonical XDR keys computed here are reused for the
        // post-signing sort below, so each delegate address is encoded only once.
        var addresses = new ScAddress[delegateSigners.Count];
        var keys = new byte[delegateSigners.Count][];
        for (var i = 0; i < delegateSigners.Count; i++)
        {
            if (delegateSigners[i] is null)
            {
                throw new ArgumentException($"Delegate signer at index {i} is null.", nameof(delegateSigners));
            }

            addresses[i] = delegateSigners[i].SignerAddress;
            if (addresses[i] is null)
            {
                throw new ArgumentException(
                    $"Delegate signer at index {i} returned a null SignerAddress.", nameof(delegateSigners));
            }

            keys[i] = addresses[i].ToXdrByteArray();
        }

        ThrowOnDuplicateKeys(keys, nameof(delegateSigners));

        // The WITH_DELEGATES XDR embeds a bare SorobanAddressCredentials struct (no V1/V2 discriminant),
        // so the concrete wrapper variant is irrelevant here; a V1 instance carries the fields even
        // though the signature is computed over the V2 address-bound payload.
        var root = new SorobanAddressCredentials(rootAddress, nonce, validUntilLedgerSeq, rootSigner.Sign(rootHash));

        // CAP-0071-01: every delegate signs the same payload as the top-level account — the
        // address-bound preimage bound to the top-level (root) address, i.e. rootHash above.
        var delegates = new SorobanDelegateSignature[delegateSigners.Count];
        for (var i = 0; i < delegateSigners.Count; i++)
        {
            delegates[i] = new SorobanDelegateSignature(addresses[i], delegateSigners[i].Sign(rootHash), []);
        }

        // CAP-0071-01 requires delegate arrays sorted by increasing address. All delegate signatures
        // are over the same root-bound payload and therefore independent of array order, so sorting
        // after signing is safe. Reuse the keys computed above (no second encode pass); keys[i] is the
        // key of delegates[i].Address, so the in-tandem sort keeps them aligned. (Duplicates were
        // already rejected above.)
        Array.Sort(keys, delegates, XdrKeyComparer);

        return new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(root, delegates),
            entry.RootInvocation);
    }

    /// <summary>
    ///     Builds an unsigned delegated authorization entry (mirroring the JS SDK's
    ///     <c>buildWithDelegatesEntry</c>): the root credential and every delegate node carry an
    ///     <see cref="SCVoid" /> placeholder signature, ready for incremental signing via
    ///     <see
    ///         cref="AuthorizeEntry(SorobanAuthorizationEntry, ISorobanEntrySigner, uint, Network, SorobanCredentialsVersion, ScAddress?)" />
    ///     with <c>forAddress</c>.
    /// </summary>
    /// <param name="entry">
    ///     The (typically simulation-produced) entry to convert. Its ADDRESS or ADDRESS_V2 credentials
    ///     supply the root address and nonce. An entry that already carries delegated (WITH_DELEGATES)
    ///     credentials is rejected with an <see cref="ArgumentException" /> (matching the JS
    ///     SDK), because rebuilding it would discard every signature already collected on the existing
    ///     delegate tree; sign such an entry incrementally with
    ///     <see
    ///         cref="AuthorizeEntry(SorobanAuthorizationEntry, ISorobanEntrySigner, uint, Network, SorobanCredentialsVersion, ScAddress?)" />
    ///     instead.
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
            // Rebuilding an already-delegated entry would reset every collected signature to a
            // placeholder. Reject it (matching the JS SDK) rather than silently discarding signatures.
            SorobanAddressCredentialsWithDelegates => throw new ArgumentException(
                "BuildWithDelegatesEntry expects ADDRESS or ADDRESS_V2 credentials, but the entry already carries " +
                "delegated (WITH_DELEGATES) credentials; rebuilding it would discard all collected signatures. " +
                "Sign the existing entry incrementally with AuthorizeEntry(..., forAddress) instead.",
                nameof(entry)),
            SorobanAddressCredentialsBase address => (address.Address, address.Nonce),
            _ => throw new ArgumentException(
                "BuildWithDelegatesEntry requires address credentials (ADDRESS or ADDRESS_V2).",
                nameof(entry)),
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

        Array.Sort(sortKeys, delegates, XdrKeyComparer);

        for (var i = 1; i < sortKeys.Length; i++)
        {
            if (sortKeys[i - 1].AsSpan().SequenceEqual(sortKeys[i]))
            {
                throw DuplicateDelegateAddressException(paramName);
            }
        }
    }

    /// <summary>
    ///     Throws <see cref="ArgumentException" /> if two of the precomputed canonical XDR
    ///     <paramref name="keys" /> are equal, as CAP-0071-01 forbids within a delegates array. Sorts a
    ///     copy so the caller's key ordering (aligned with its element array) is preserved for a later
    ///     in-tandem sort. Used to reject duplicates before any signer runs.
    /// </summary>
    private static void ThrowOnDuplicateKeys(byte[][] keys, string paramName)
    {
        var sorted = (byte[][])keys.Clone();
        Array.Sort(sorted, XdrKeyComparer);

        for (var i = 1; i < sorted.Length; i++)
        {
            if (sorted[i - 1].AsSpan().SequenceEqual(sorted[i]))
            {
                throw DuplicateDelegateAddressException(paramName);
            }
        }
    }

    private static ArgumentException DuplicateDelegateAddressException(string paramName)
    {
        return new ArgumentException(
            $"{paramName} contains two entries with the same address; CAP-0071-01 forbids duplicate addresses in a delegates array.",
            paramName);
    }

    private static byte[] HashPreimage(HashIDPreimage preimage)
    {
        var writer = new XdrDataOutputStream();
        HashIDPreimage.Encode(writer, preimage);
        return Util.Hash(writer.ToArray());
    }
}