using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0045.Exceptions;
using StellarDotnetSdk.Xdr;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using Uint32 = StellarDotnetSdk.Xdr.Uint32;

namespace StellarDotnetSdk.Sep.Sep0045;

/// <summary>
///     Static helpers for SEP-45 (Web Authentication for Contract Accounts) challenge
///     parsing, validation, verification, and construction.
/// </summary>
public static class Sep45Challenge
{
    /// <summary>Name of the function every challenge entry must invoke.</summary>
    public const string WebAuthVerifyFunctionName = "web_auth_verify";

    /// <summary>
    ///     Hard upper bound on the number of authorization entries decoded from a challenge. A legitimate
    ///     SEP-45 challenge carries only a few (the server, the client contract account, and at most the
    ///     optional client_domain account); this constant ceiling stops a hostile payload from forcing a
    ///     large up-front array allocation before decoding fails (memory-exhaustion DoS).
    /// </summary>
    private const int MaxAuthorizationEntries = 100;

    /// <summary>
    ///     Build the SHA-256 hash that signers of a SorobanAuthorizationEntry sign
    ///     (HashIDPreimage of type ENVELOPE_TYPE_SOROBAN_AUTHORIZATION).
    /// </summary>
    internal static byte[] ComputeAuthorizationHash(
        SorobanAuthorizationEntry entry,
        Network network)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(network);

        if (entry.Credentials.Discriminant.InnerValue !=
            SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS)
        {
            throw new InvalidOperationException(
                "Cannot compute authorization hash for non-address credentials.");
        }

        var addressCreds = entry.Credentials.Address;

        var preimage = new HashIDPreimage
        {
            Discriminant = new EnvelopeType
            {
                InnerValue = EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_SOROBAN_AUTHORIZATION,
            },
            SorobanAuthorization = new HashIDPreimage.HashIDPreimageSorobanAuthorization
            {
                NetworkID = new Hash(SHA256.HashData(Encoding.UTF8.GetBytes(network.NetworkPassphrase))),
                Nonce = new Int64(addressCreds.Nonce.InnerValue),
                SignatureExpirationLedger = new Uint32(addressCreds.SignatureExpirationLedger.InnerValue),
                Invocation = entry.RootInvocation,
            },
        };

        var stream = new XdrDataOutputStream();
        HashIDPreimage.Encode(stream, preimage);
        return SHA256.HashData(stream.ToArray());
    }

    /// <summary>
    ///     Verify the Ed25519 signature on the server's SorobanAuthorizationEntry.
    ///     Throws <see cref="Exceptions.InvalidServerSignatureException" /> if the server
    ///     did not sign, or if the signature does not verify against the expected server account.
    /// </summary>
    /// <param name="serverEntry">The authorization entry whose credentials should be signed by the server.</param>
    /// <param name="serverAccountId">The expected server signing account (G... address).</param>
    /// <param name="network">The Stellar network the challenge is bound to.</param>
    internal static void VerifyServerSignature(
        SorobanAuthorizationEntry serverEntry,
        string serverAccountId,
        Network network)
    {
        ArgumentNullException.ThrowIfNull(serverEntry);
        ArgumentException.ThrowIfNullOrEmpty(serverAccountId);
        ArgumentNullException.ThrowIfNull(network);

        if (serverEntry.Credentials.Discriminant.InnerValue !=
            SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS)
        {
            throw new InvalidServerSignatureException(
                "Server entry credentials are not SOROBAN_CREDENTIALS_ADDRESS.");
        }

        var creds = serverEntry.Credentials.Address;
        var signatureContainer = creds.Signature;

        if (signatureContainer?.Discriminant?.InnerValue != SCValType.SCValTypeEnum.SCV_VEC ||
            signatureContainer.Vec == null ||
            signatureContainer.Vec.InnerValue.Length == 0)
        {
            throw new InvalidServerSignatureException(
                "Server entry has no signatures in credentials.");
        }

        var serverKeyPair = KeyPair.FromAccountId(serverAccountId);
        var expectedPubKey = serverKeyPair.PublicKey;
        var hash = ComputeAuthorizationHash(serverEntry, network);

        foreach (var sigVal in signatureContainer.Vec.InnerValue)
        {
            if (sigVal.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_MAP ||
                sigVal.Map == null)
            {
                continue;
            }

            byte[]? pub = null;
            byte[]? sig = null;
            foreach (var kv in sigVal.Map.InnerValue)
            {
                if (kv.Key.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_SYMBOL)
                {
                    continue;
                }
                var keyName = kv.Key.Sym.InnerValue;
                if (keyName == "public_key" &&
                    kv.Val.Discriminant.InnerValue == SCValType.SCValTypeEnum.SCV_BYTES)
                {
                    pub = kv.Val.Bytes.InnerValue;
                }
                else if (keyName == "signature" &&
                         kv.Val.Discriminant.InnerValue == SCValType.SCValTypeEnum.SCV_BYTES)
                {
                    sig = kv.Val.Bytes.InnerValue;
                }
            }

            if (pub != null && sig != null &&
                pub.Length == expectedPubKey.Length &&
                pub.SequenceEqual(expectedPubKey) &&
                serverKeyPair.Verify(hash, sig))
            {
                return;
            }
        }

        throw new InvalidServerSignatureException(
            "No valid signature for the server account was found in the server entry.");
    }

    /// <summary>
    ///     Parse a base64-XDR encoded array of SorobanAuthorizationEntries, enforcing all SEP-45
    ///     structural rules (entry count, sub-invocations, contract id, function name, arg shape,
    ///     credentials type, shared invocation) and the web_auth_verify args map invariants
    ///     (required keys, value types, non-empty nonce, home domain allowlist, web_auth_domain,
    ///     web_auth_domain_account, paired client domain keys).
    /// </summary>
    /// <param name="authorizationEntriesXdr">
    ///     Base64 encoding of an <see cref="XdrDataOutputStream" /> stream containing an int32
    ///     entry count followed by that many XDR-encoded <see cref="SorobanAuthorizationEntry" />.
    /// </param>
    /// <param name="serverAccountId">Expected server signing account (G... strkey).</param>
    /// <param name="webAuthContractId">Expected web auth contract id (C... strkey).</param>
    /// <param name="homeDomains">Allowed home domains.</param>
    /// <param name="webAuthDomain">Expected web auth domain binding the challenge.</param>
    /// <returns>The populated <see cref="ChallengeAuthorizationEntries" />.</returns>
    internal static ChallengeAuthorizationEntries ReadChallenge(
        string authorizationEntriesXdr,
        string serverAccountId,
        string webAuthContractId,
        string[] homeDomains,
        string webAuthDomain)
    {
        if (string.IsNullOrEmpty(authorizationEntriesXdr))
        {
            throw new InvalidArgumentsException("authorizationEntriesXdr must not be empty");
        }
        ArgumentException.ThrowIfNullOrEmpty(serverAccountId);
        ArgumentException.ThrowIfNullOrEmpty(webAuthContractId);
        ArgumentNullException.ThrowIfNull(homeDomains);
        ArgumentException.ThrowIfNullOrEmpty(webAuthDomain);

        var entries = DecodeEntriesFromBase64(authorizationEntriesXdr);
        if (entries.Length < 2)
        {
            throw new InvalidEntryCountException(
                $"At least 2 authorization entries required, got {entries.Length}.");
        }

        byte[]? firstInvocationBytes = null;
        foreach (var entry in entries)
        {
            // 1) No sub-invocations
            if (entry.RootInvocation.SubInvocations != null && entry.RootInvocation.SubInvocations.Length > 0)
            {
                throw new SubInvocationsNotAllowedException(
                    "Sub-invocations are not allowed in SEP-45 challenges.");
            }

            // 2) Function is InvokeContract
            var fn = entry.RootInvocation.Function;
            if (fn.Discriminant.InnerValue !=
                SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum
                    .SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN)
            {
                throw new InvalidArgumentsException("Entry is not a contract function invocation.");
            }

            var contractFn = fn.ContractFn;

            // 3) Contract id matches
            var contractIdStr = AddressToStrKey(contractFn.ContractAddress);
            if (contractIdStr != webAuthContractId)
            {
                throw new InvalidContractIdException(
                    $"Contract id mismatch. Expected {webAuthContractId}, got {contractIdStr}.");
            }

            // 4) Function name matches
            if (contractFn.FunctionName.InnerValue != WebAuthVerifyFunctionName)
            {
                throw new InvalidFunctionNameException(
                    $"Function name must be '{WebAuthVerifyFunctionName}', got '{contractFn.FunctionName.InnerValue}'.");
            }

            // 5) Args shape
            if (contractFn.Args == null || contractFn.Args.Length != 1)
            {
                throw new InvalidArgumentsException("Expected exactly one argument.");
            }

            if (contractFn.Args[0].Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_MAP ||
                contractFn.Args[0].Map == null)
            {
                throw new InvalidArgumentsException("Argument must be an SCMap.");
            }

            // 6) Credentials type
            if (entry.Credentials.Discriminant.InnerValue !=
                SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS)
            {
                throw new InvalidArgumentsException("Credentials must be SOROBAN_CREDENTIALS_ADDRESS.");
            }

            // 7) All entries share the same invocation (encode each once; compare against the first's bytes)
            var invocationBytes = EncodeInvocation(entry.RootInvocation);
            if (firstInvocationBytes == null)
            {
                firstInvocationBytes = invocationBytes;
            }
            else if (!invocationBytes.SequenceEqual(firstInvocationBytes))
            {
                throw new MismatchedInvocationsException(
                    "All entries must share the same root invocation.");
            }
        }

        // Extract + validate map keys
        var firstArgs = entries[0].RootInvocation.Function.ContractFn.Args[0].Map!;
        var extracted = ExtractAndValidateArgs(firstArgs, homeDomains, webAuthDomain, serverAccountId);

        // The client-domain entry is matched purely by its credentials address, so client_domain_account
        // must name a participant distinct from the client account and the server account. A hostile
        // challenge that aliases it to either would let that participant's own entry double as the
        // client-domain entry, satisfying the presence check below without a genuine, separately-signed
        // client-domain entry — so reject the aliasing up front. Both comparisons are reachable: address
        // args may be G... or C... (see RequireStrKeyAddress), so the client-account comparison is not
        // dead code — keep both branches.
        if (extracted.ClientDomainAccount != null &&
            (extracted.ClientDomainAccount == extracted.ClientAccount ||
             extracted.ClientDomainAccount == serverAccountId))
        {
            throw new InvalidClientDomainException(
                $"client_domain_account '{extracted.ClientDomainAccount}' must differ from the client " +
                "account and the server account.");
        }

        // Every entry's credentials address must belong to the known participant set, and entries for
        // both the server and the client account must be present. This rejects a server that injects an
        // extra entry with an arbitrary credentials address.
        var allowedAddresses = new HashSet<string>
        {
            serverAccountId,
            extracted.ClientAccount,
        };
        if (extracted.ClientDomainAccount != null)
        {
            allowedAddresses.Add(extracted.ClientDomainAccount);
        }

        SorobanAuthorizationEntry? serverEntry = null;
        var sawClientEntry = false;
        var sawServerEntry = false;
        var sawClientDomainEntry = false;
        foreach (var entry in entries)
        {
            var credAddress = AddressToStrKey(entry.Credentials.Address.Address);
            if (!allowedAddresses.Contains(credAddress))
            {
                throw new InvalidArgumentsException(
                    $"Authorization entry has unexpected credentials address '{credAddress}'.");
            }
            if (credAddress == extracted.ClientAccount)
            {
                sawClientEntry = true;
            }
            if (credAddress == serverAccountId)
            {
                sawServerEntry = true;
                serverEntry ??= entry; // first match, matching the previous re-scan behavior
            }
            if (extracted.ClientDomainAccount != null && credAddress == extracted.ClientDomainAccount)
            {
                sawClientDomainEntry = true;
            }
        }

        if (!sawServerEntry)
        {
            throw new InvalidServerSignatureException(
                $"No authorization entry found for the server account '{serverAccountId}'.");
        }
        if (!sawClientEntry)
        {
            throw new InvalidClientAccountException(
                $"No authorization entry found for the client account '{extracted.ClientAccount}'.");
        }
        // When the args declare a client_domain_account, a matching authorization entry must be present
        // (both the Flutter and Java peer SDKs enforce this). Otherwise the client cannot supply the
        // client-domain signature and the server rejects the challenge with an opaque error.
        if (extracted.ClientDomainAccount != null && !sawClientDomainEntry)
        {
            throw new InvalidClientDomainException(
                $"Challenge args declare client_domain_account '{extracted.ClientDomainAccount}' " +
                "but no authorization entry was found for it.");
        }

        return new ChallengeAuthorizationEntries(
            entries,
            serverEntry!, // non-null: sawServerEntry (checked above) is set iff serverEntry was assigned
            extracted.ClientAccount,
            extracted.HomeDomain,
            extracted.WebAuthDomain,
            extracted.WebAuthDomainAccount,
            extracted.Nonce,
            extracted.ClientDomain,
            extracted.ClientDomainAccount);
    }

    /// <summary>Decode base64-encoded SorobanAuthorizationEntry array (int32 length + entries).</summary>
    internal static SorobanAuthorizationEntry[] DecodeAuthorizationEntries(string base64)
    {
        return DecodeEntriesFromBase64(base64);
    }

    /// <summary>Encode SorobanAuthorizationEntry array as base64 (int32 length + XDR-encoded entries).</summary>
    internal static string EncodeAuthorizationEntries(SorobanAuthorizationEntry[] entries)
    {
        var stream = new XdrDataOutputStream();
        stream.WriteInt(entries.Length);
        foreach (var e in entries)
        {
            SorobanAuthorizationEntry.Encode(stream, e);
        }
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <summary>
    ///     Append a signature to an entry's credentials. Uses the SEP-45 convention: the
    ///     <c>Signature</c> SCVal is an SCV_VEC of SCV_MAPs each containing public_key / signature bytes.
    ///     Accumulates one map per signer (for an M-of-N contract account) and keeps the vec sorted
    ///     ascending by <c>public_key</c> — the reference smart-wallet <c>__check_auth</c> (and the
    ///     soroban-examples custom account) reject an out-of-order vec with <c>BadSignatureOrder</c>.
    /// </summary>
    internal static void AppendSignature(
        SorobanAuthorizationEntry entry, byte[] publicKey, byte[] signature)
    {
        var sigMap = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_MAP },
            // "public_key" < "signature" in symbol order, so this 2-key map is already canonical.
            Map = new SCMap(new[]
            {
                MakeSymbolBytesEntry("public_key", publicKey),
                MakeSymbolBytesEntry("signature", signature),
            }),
        };

        var creds = entry.Credentials.Address;
        SCVal[] combined;
        if (creds.Signature?.Discriminant?.InnerValue == SCValType.SCValTypeEnum.SCV_VEC &&
            creds.Signature.Vec != null)
        {
            var existing = creds.Signature.Vec.InnerValue;
            combined = new SCVal[existing.Length + 1];
            Array.Copy(existing, combined, existing.Length);
            combined[existing.Length] = sigMap;
        }
        else
        {
            combined = new[] { sigMap };
        }

        // Strictly ascending by public_key so an M-of-N contract account accepts the signatures
        // regardless of the order the signers were supplied in.
        Array.Sort(combined, (a, b) => CompareBytes(SignatureMapPublicKey(a), SignatureMapPublicKey(b)));

        creds.Signature = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_VEC },
            Vec = new SCVec(combined),
        };
    }

    private static byte[] SignatureMapPublicKey(SCVal sigMap)
    {
        if (sigMap.Discriminant.InnerValue == SCValType.SCValTypeEnum.SCV_MAP && sigMap.Map != null)
        {
            foreach (var kv in sigMap.Map.InnerValue)
            {
                if (kv.Key.Discriminant.InnerValue == SCValType.SCValTypeEnum.SCV_SYMBOL &&
                    kv.Key.Sym?.InnerValue == "public_key" &&
                    kv.Val.Discriminant.InnerValue == SCValType.SCValTypeEnum.SCV_BYTES &&
                    kv.Val.Bytes != null)
                {
                    return kv.Val.Bytes.InnerValue;
                }
            }
        }
        return Array.Empty<byte>();
    }

    private static int CompareBytes(byte[] x, byte[] y)
    {
        var min = Math.Min(x.Length, y.Length);
        for (var i = 0; i < min; i++)
        {
            if (x[i] != y[i])
            {
                return x[i].CompareTo(y[i]);
            }
        }
        return x.Length.CompareTo(y.Length);
    }

    private static SCMapEntry MakeSymbolBytesEntry(string key, byte[] value)
    {
        return new SCMapEntry
        {
            Key = new SCVal
            {
                Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_SYMBOL },
                Sym = new SCSymbol(key),
            },
            Val = new SCVal
            {
                Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_BYTES },
                Bytes = new SCBytes(value),
            },
        };
    }

    private static SorobanAuthorizationEntry[] DecodeEntriesFromBase64(string b64)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(b64);
        }
        catch (FormatException ex)
        {
            throw new InvalidArgumentsException("authorizationEntries is not valid base64.", ex);
        }

        try
        {
            var stream = new XdrDataInputStream(bytes);
            var count = stream.ReadInt();
            // A SEP-45 challenge only ever carries a handful of authorization entries, so cap the count
            // with a constant before allocating. The byte-remaining bound alone is too loose: each entry
            // costs one array reference (8 bytes on 64-bit) but needs only one input byte, so count ==
            // remaining still permits an ~8x up-front allocation. The hard cap closes that DoS; the
            // remaining-bytes check still rejects counts that cannot possibly be backed by the input.
            var remaining = stream.GetRemainingInputLen();
            if (count < 0 || count > MaxAuthorizationEntries || count > remaining)
            {
                throw new InvalidArgumentsException(
                    $"Implausible authorization entry count {count} " +
                    $"(max {MaxAuthorizationEntries}, {remaining} remaining byte(s)).");
            }

            var result = new SorobanAuthorizationEntry[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = SorobanAuthorizationEntry.Decode(stream);
            }

            if (stream.GetRemainingInputLen() != 0)
            {
                throw new InvalidArgumentsException(
                    $"{stream.GetRemainingInputLen()} unexpected trailing byte(s) after {count} authorization entries.");
            }

            return result;
        }
        catch (Exception ex) when (
            ex is InvalidDataException or IOException or
                IndexOutOfRangeException or EndOfStreamException or
                FormatException or ArgumentOutOfRangeException)
        {
            // XdrDataInputStream signals truncated/corrupt input with these, so surface them all as the
            // documented SEP-45 exception type instead of letting raw decode exceptions escape:
            //   - a fixed-width read past the end -> IndexOutOfRange / EndOfStream / InvalidData;
            //   - non-zero opaque padding         -> IOException;
            //   - a var-length or string length prefix that runs off the buffer -> FormatException
            //     ("can't read 'length'"), and a length prefix > int.MaxValue -> ArgumentOutOfRangeException.
            // Unknown enum discriminants, over-large element counts, and excessive nesting are already
            // raised as InvalidDataException by the generated XDR decoders, covered by the cases above.
            throw new InvalidArgumentsException(
                "Malformed authorization entries XDR: " + ex.Message, ex);
        }
    }

    internal static byte[] EncodeInvocation(SorobanAuthorizedInvocation invocation)
    {
        var stream = new XdrDataOutputStream();
        SorobanAuthorizedInvocation.Encode(stream, invocation);
        return stream.ToArray();
    }

    /// <summary>
    ///     Converts an XDR <see cref="SCAddress" /> to its strkey form (C... contract or G... account).
    ///     The single shared helper for both challenge validation and signing, so the validated address
    ///     set and the signed address set never diverge on how a credentials address is normalized.
    /// </summary>
    internal static string AddressToStrKey(SCAddress addr)
    {
        var discriminant = addr.Discriminant.InnerValue;
        if (discriminant == SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT)
        {
            return StrKey.EncodeContractId(addr.ContractId.InnerValue.InnerValue);
        }
        if (discriminant == SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT)
        {
            return KeyPair.FromXdrPublicKey(addr.AccountId.InnerValue).AccountId;
        }
        throw new InvalidArgumentsException($"Unsupported SCAddress type {discriminant}");
    }

    private static ExtractedArgs ExtractAndValidateArgs(
        SCMap map, string[] allowedHomeDomains, string webAuthDomain, string serverAccountId)
    {
        string? account = null;
        string? homeDomain = null;
        string? wad = null;
        string? wadAccount = null;
        string? clientDomain = null;
        string? clientDomainAccount = null;
        string? nonce = null;

        foreach (var entry in map.InnerValue)
        {
            if (entry.Key.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_SYMBOL)
            {
                throw new InvalidArgumentsException("Map key must be SCV_SYMBOL.");
            }
            var k = entry.Key.Sym.InnerValue;
            var v = entry.Val;

            // The reference web_auth_verify contract declares args as Map<Symbol, String>, so
            // every value — including the addresses and nonce — is an SCV_STRING on the wire.
            switch (k)
            {
                case "account":
                    account = RequireContractAddress(v, "account");
                    break;
                case "home_domain":
                    homeDomain = RequireString(v, "home_domain");
                    break;
                case "nonce":
                    nonce = RequireString(v, "nonce");
                    if (nonce.Length == 0)
                    {
                        throw new InvalidNonceException("nonce must not be empty.");
                    }
                    break;
                case "web_auth_domain":
                    wad = RequireString(v, "web_auth_domain");
                    break;
                case "web_auth_domain_account":
                    wadAccount = RequireStrKeyAddress(v, "web_auth_domain_account");
                    break;
                case "client_domain":
                    clientDomain = RequireString(v, "client_domain");
                    break;
                case "client_domain_account":
                    clientDomainAccount = RequireStrKeyAddress(v, "client_domain_account");
                    break;
            }
        }

        if (account == null || homeDomain == null || nonce == null || wad == null || wadAccount == null)
        {
            throw new InvalidArgumentsException(
                "Required keys missing from web_auth_verify args (account, home_domain, nonce, web_auth_domain, web_auth_domain_account).");
        }

        if (clientDomain == null != (clientDomainAccount == null))
        {
            throw new InvalidClientDomainException(
                "client_domain and client_domain_account must both be present or both absent.");
        }

        if (!Array.Exists(allowedHomeDomains, d => d == homeDomain))
        {
            throw new InvalidHomeDomainException(
                $"home_domain '{homeDomain}' is not in allowed list.");
        }

        if (wad != webAuthDomain)
        {
            throw new InvalidWebAuthDomainException(
                $"web_auth_domain mismatch. Expected '{webAuthDomain}', got '{wad}'.");
        }

        if (wadAccount != serverAccountId)
        {
            throw new InvalidArgumentsException(
                $"web_auth_domain_account mismatch. Expected server account '{serverAccountId}', got '{wadAccount}'.");
        }

        return new ExtractedArgs(account, homeDomain, nonce, wad, wadAccount, clientDomain, clientDomainAccount);
    }

    private static string RequireString(SCVal v, string keyName)
    {
        if (v.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_STRING || v.Str == null)
        {
            throw new InvalidArgumentsException($"{keyName} must be SCV_STRING.");
        }
        return v.Str.InnerValue;
    }

    /// <summary>
    ///     Reads an address argument. Per the reference web_auth_verify contract the value is an
    ///     SCV_STRING holding the strkey text (the contract calls <c>Address::from_string</c> on it),
    ///     so we validate it is a well-formed contract (C...) or ed25519 account (G...) strkey.
    /// </summary>
    private static string RequireStrKeyAddress(SCVal v, string keyName)
    {
        var s = RequireString(v, keyName);
        if (!StrKey.IsValidContractId(s) && !StrKey.IsValidEd25519PublicKey(s))
        {
            throw new InvalidArgumentsException(
                $"{keyName} is not a valid account (G...) or contract (C...) address: '{s}'.");
        }
        return s;
    }

    /// <summary>
    ///     Reads the client <c>account</c> argument, which SEP-45 restricts to a contract account
    ///     ("This SEP only supports C (contract) accounts"). A <c>G...</c> ed25519 address is rejected
    ///     here — unlike <see cref="RequireStrKeyAddress" />, which also allows <c>G...</c> for the
    ///     server (<c>web_auth_domain_account</c>) and client-domain (<c>client_domain_account</c>) accounts.
    /// </summary>
    private static string RequireContractAddress(SCVal v, string keyName)
    {
        var s = RequireString(v, keyName);
        if (!StrKey.IsValidContractId(s))
        {
            throw new InvalidArgumentsException(
                $"{keyName} must be a contract (C...) address; SEP-45 only supports contract accounts: '{s}'.");
        }
        return s;
    }

    private sealed record ExtractedArgs(
        string ClientAccount,
        string HomeDomain,
        string Nonce,
        string WebAuthDomain,
        string WebAuthDomainAccount,
        string? ClientDomain,
        string? ClientDomainAccount);
}