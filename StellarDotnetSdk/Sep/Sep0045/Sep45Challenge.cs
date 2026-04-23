using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using StellarDotnetSdk.Accounts;
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
    ///     Throws <see cref="Exceptions.InvalidServerSignatureException"/> if the server
    ///     did not sign, or if the signature does not verify against the expected server account.
    /// </summary>
    /// <param name="serverEntry">The authorization entry whose credentials should be signed by the server.</param>
    /// <param name="serverAccountId">The expected server signing account (G... address).</param>
    /// <param name="network">The Stellar network the challenge is bound to.</param>
    public static void VerifyServerSignature(
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
            throw new Exceptions.InvalidServerSignatureException(
                "Server entry credentials are not SOROBAN_CREDENTIALS_ADDRESS.");
        }

        var creds = serverEntry.Credentials.Address;
        var signatureContainer = creds.Signature;

        if (signatureContainer?.Discriminant?.InnerValue != SCValType.SCValTypeEnum.SCV_VEC ||
            signatureContainer.Vec == null ||
            signatureContainer.Vec.InnerValue.Length == 0)
        {
            throw new Exceptions.InvalidServerSignatureException(
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
                if (kv.Key.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_SYMBOL) continue;
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
                System.Linq.Enumerable.SequenceEqual(pub, expectedPubKey) &&
                serverKeyPair.Verify(hash, sig))
            {
                return;
            }
        }

        throw new Exceptions.InvalidServerSignatureException(
            "No valid signature for the server account was found in the server entry.");
    }

    /// <summary>
    ///     Parse a base64-XDR encoded array of SorobanAuthorizationEntries, enforcing all SEP-45
    ///     structural rules (entry count, sub-invocations, contract id, function name, arg shape,
    ///     credentials type, shared invocation) and the web_auth_verify args map invariants
    ///     (required keys, value types, nonce length, home domain allowlist, web_auth_domain,
    ///     web_auth_domain_account, paired client domain keys).
    /// </summary>
    /// <param name="authorizationEntriesXdr">
    ///     Base64 encoding of an <see cref="XdrDataOutputStream"/> stream containing an int32
    ///     entry count followed by that many XDR-encoded <see cref="SorobanAuthorizationEntry"/>.
    /// </param>
    /// <param name="serverAccountId">Expected server signing account (G... strkey).</param>
    /// <param name="webAuthContractId">Expected web auth contract id (C... strkey).</param>
    /// <param name="homeDomains">Allowed home domains.</param>
    /// <param name="webAuthDomain">Expected web auth domain binding the challenge.</param>
    /// <returns>The populated <see cref="ChallengeAuthorizationEntries"/>.</returns>
    public static ChallengeAuthorizationEntries ReadChallenge(
        string authorizationEntriesXdr,
        string serverAccountId,
        string webAuthContractId,
        string[] homeDomains,
        string webAuthDomain)
    {
        if (string.IsNullOrEmpty(authorizationEntriesXdr))
            throw new Exceptions.InvalidArgumentsException("authorizationEntriesXdr must not be empty");
        ArgumentException.ThrowIfNullOrEmpty(serverAccountId);
        ArgumentException.ThrowIfNullOrEmpty(webAuthContractId);
        ArgumentNullException.ThrowIfNull(homeDomains);
        ArgumentException.ThrowIfNullOrEmpty(webAuthDomain);

        var entries = DecodeEntriesFromBase64(authorizationEntriesXdr);
        if (entries.Length < 2)
        {
            throw new Exceptions.InvalidEntryCountException(
                $"At least 2 authorization entries required, got {entries.Length}.");
        }

        SorobanAuthorizedInvocation? firstInvocation = null;
        foreach (var entry in entries)
        {
            // 1) No sub-invocations
            if (entry.RootInvocation.SubInvocations != null && entry.RootInvocation.SubInvocations.Length > 0)
            {
                throw new Exceptions.SubInvocationsNotAllowedException(
                    "Sub-invocations are not allowed in SEP-45 challenges.");
            }

            // 2) Function is InvokeContract
            var fn = entry.RootInvocation.Function;
            if (fn.Discriminant.InnerValue !=
                SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum.SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN)
            {
                throw new Exceptions.InvalidContractIdException("Entry is not a contract function invocation.");
            }

            var contractFn = fn.ContractFn;

            // 3) Contract id matches
            var contractIdStr = AddressToStrKey(contractFn.ContractAddress);
            if (contractIdStr != webAuthContractId)
            {
                throw new Exceptions.InvalidContractIdException(
                    $"Contract id mismatch. Expected {webAuthContractId}, got {contractIdStr}.");
            }

            // 4) Function name matches
            if (contractFn.FunctionName.InnerValue != WebAuthVerifyFunctionName)
            {
                throw new Exceptions.InvalidFunctionNameException(
                    $"Function name must be '{WebAuthVerifyFunctionName}', got '{contractFn.FunctionName.InnerValue}'.");
            }

            // 5) Args shape
            if (contractFn.Args == null || contractFn.Args.Length != 1)
                throw new Exceptions.InvalidArgumentsException("Expected exactly one argument.");

            if (contractFn.Args[0].Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_MAP ||
                contractFn.Args[0].Map == null)
            {
                throw new Exceptions.InvalidArgumentsException("Argument must be an SCMap.");
            }

            // 6) Credentials type
            if (entry.Credentials.Discriminant.InnerValue !=
                SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS)
            {
                throw new Exceptions.InvalidArgumentsException("Credentials must be SOROBAN_CREDENTIALS_ADDRESS.");
            }

            // 7) All entries share the same invocation
            if (firstInvocation == null)
            {
                firstInvocation = entry.RootInvocation;
            }
            else if (!InvocationsEqual(firstInvocation, entry.RootInvocation))
            {
                throw new Exceptions.MismatchedInvocationsException(
                    "All entries must share the same root invocation.");
            }
        }

        // Extract + validate map keys
        var firstArgs = entries[0].RootInvocation.Function.ContractFn.Args[0].Map!;
        var extracted = ExtractAndValidateArgs(firstArgs, homeDomains, webAuthDomain, serverAccountId);

        return new ChallengeAuthorizationEntries(
            entries,
            extracted.ClientAccount,
            extracted.HomeDomain,
            extracted.WebAuthDomain,
            extracted.WebAuthDomainAccount,
            extracted.Nonce,
            extracted.ClientDomain,
            extracted.ClientDomainAccount);
    }

    /// <summary>Decode base64-encoded SorobanAuthorizationEntry array (int32 length + entries).</summary>
    internal static SorobanAuthorizationEntry[] DecodeAuthorizationEntries(string base64) =>
        DecodeEntriesFromBase64(base64);

    /// <summary>Encode SorobanAuthorizationEntry array as base64 (int32 length + XDR-encoded entries).</summary>
    internal static string EncodeAuthorizationEntries(SorobanAuthorizationEntry[] entries)
    {
        var stream = new XdrDataOutputStream();
        stream.WriteInt(entries.Length);
        foreach (var e in entries)
            SorobanAuthorizationEntry.Encode(stream, e);
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <summary>
    ///     Install a signature into an entry's credentials. Uses the SEP-45 convention: the
    ///     <c>Signature</c> SCVal is an SCV_VEC of SCV_MAPs each containing public_key / signature bytes.
    /// </summary>
    internal static void InstallSignatureVector(
        SorobanAuthorizationEntry entry, byte[] publicKey, byte[] signature)
    {
        var sigMap = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_MAP },
            Map = new SCMap(new[]
            {
                MakeSymbolBytesEntry("public_key", publicKey),
                MakeSymbolBytesEntry("signature", signature),
            }),
        };
        entry.Credentials.Address.Signature = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_VEC },
            Vec = new SCVec(new[] { sigMap }),
        };
    }

    private static SCMapEntry MakeSymbolBytesEntry(string key, byte[] value) => new()
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

    private static SorobanAuthorizationEntry[] DecodeEntriesFromBase64(string b64)
    {
        try
        {
            var bytes = Convert.FromBase64String(b64);
            var stream = new XdrDataInputStream(bytes);
            var count = stream.ReadInt();
            if (count < 0)
                throw new Exceptions.InvalidArgumentsException("Negative entry count in XDR.");
            var result = new SorobanAuthorizationEntry[count];
            for (var i = 0; i < count; i++)
                result[i] = SorobanAuthorizationEntry.Decode(stream);
            return result;
        }
        catch (FormatException ex)
        {
            throw new Exceptions.InvalidArgumentsException("Invalid base64 XDR: " + ex.Message);
        }
    }

    private static bool InvocationsEqual(SorobanAuthorizedInvocation a, SorobanAuthorizedInvocation b)
    {
        var sa = new XdrDataOutputStream();
        var sb = new XdrDataOutputStream();
        SorobanAuthorizedInvocation.Encode(sa, a);
        SorobanAuthorizedInvocation.Encode(sb, b);
        return sa.ToArray().SequenceEqual(sb.ToArray());
    }

    private static string AddressToStrKey(SCAddress addr)
    {
        var discriminant = addr.Discriminant.InnerValue;
        if (discriminant == SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_CONTRACT)
            return StrKey.EncodeContractId(addr.ContractId.InnerValue.InnerValue);
        if (discriminant == SCAddressType.SCAddressTypeEnum.SC_ADDRESS_TYPE_ACCOUNT)
            return KeyPair.FromXdrPublicKey(addr.AccountId.InnerValue).AccountId;
        throw new Exceptions.InvalidArgumentsException($"Unsupported SCAddress type {discriminant}");
    }

    private sealed record ExtractedArgs(
        string ClientAccount,
        string HomeDomain,
        byte[] Nonce,
        string WebAuthDomain,
        string WebAuthDomainAccount,
        string? ClientDomain,
        string? ClientDomainAccount);

    private static ExtractedArgs ExtractAndValidateArgs(
        SCMap map, string[] allowedHomeDomains, string webAuthDomain, string serverAccountId)
    {
        string? account = null;
        string? homeDomain = null;
        string? wad = null;
        string? wadAccount = null;
        string? clientDomain = null;
        string? clientDomainAccount = null;
        byte[]? nonce = null;

        foreach (var entry in map.InnerValue)
        {
            if (entry.Key.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_SYMBOL)
                throw new Exceptions.InvalidArgumentsException("Map key must be SCV_SYMBOL.");
            var k = entry.Key.Sym.InnerValue;
            var v = entry.Val;

            switch (k)
            {
                case "account":
                    account = RequireAddress(v, "account");
                    break;
                case "home_domain":
                    homeDomain = RequireString(v, "home_domain");
                    break;
                case "nonce":
                    nonce = RequireBytes(v, "nonce");
                    if (nonce.Length != 32)
                        throw new Exceptions.InvalidNonceException($"nonce must be 32 bytes, got {nonce.Length}");
                    break;
                case "web_auth_domain":
                    wad = RequireString(v, "web_auth_domain");
                    break;
                case "web_auth_domain_account":
                    wadAccount = RequireAddress(v, "web_auth_domain_account");
                    break;
                case "client_domain":
                    clientDomain = RequireString(v, "client_domain");
                    break;
                case "client_domain_account":
                    clientDomainAccount = RequireAddress(v, "client_domain_account");
                    break;
                default:
                    // ignore unknown for forward-compatibility
                    break;
            }
        }

        if (account == null || homeDomain == null || nonce == null || wad == null || wadAccount == null)
        {
            throw new Exceptions.InvalidArgumentsException(
                "Required keys missing from web_auth_verify args (account, home_domain, nonce, web_auth_domain, web_auth_domain_account).");
        }

        if ((clientDomain == null) != (clientDomainAccount == null))
        {
            throw new Exceptions.InvalidClientDomainException(
                "client_domain and client_domain_account must both be present or both absent.");
        }

        if (!Array.Exists(allowedHomeDomains, d => d == homeDomain))
        {
            throw new Exceptions.InvalidHomeDomainException(
                $"home_domain '{homeDomain}' is not in allowed list.");
        }

        if (wad != webAuthDomain)
        {
            throw new Exceptions.InvalidWebAuthDomainException(
                $"web_auth_domain mismatch. Expected '{webAuthDomain}', got '{wad}'.");
        }

        if (wadAccount != serverAccountId)
        {
            throw new Exceptions.InvalidArgumentsException(
                $"web_auth_domain_account mismatch. Expected server account '{serverAccountId}', got '{wadAccount}'.");
        }

        return new ExtractedArgs(account, homeDomain, nonce, wad, wadAccount, clientDomain, clientDomainAccount);
    }

    private static string RequireAddress(SCVal v, string keyName)
    {
        if (v.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_ADDRESS || v.Address == null)
            throw new Exceptions.InvalidArgumentsException($"{keyName} must be SCV_ADDRESS.");
        return AddressToStrKey(v.Address);
    }

    private static string RequireString(SCVal v, string keyName)
    {
        if (v.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_STRING || v.Str == null)
            throw new Exceptions.InvalidArgumentsException($"{keyName} must be SCV_STRING.");
        return v.Str.InnerValue;
    }

    private static byte[] RequireBytes(SCVal v, string keyName)
    {
        if (v.Discriminant.InnerValue != SCValType.SCValTypeEnum.SCV_BYTES || v.Bytes == null)
            throw new Exceptions.InvalidArgumentsException($"{keyName} must be SCV_BYTES.");
        return v.Bytes.InnerValue;
    }
}
