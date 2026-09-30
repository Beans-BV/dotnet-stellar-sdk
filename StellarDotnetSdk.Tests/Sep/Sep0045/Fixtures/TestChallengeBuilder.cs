using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using CredentialsType = StellarDotnetSdk.Xdr.SorobanCredentialsType.SorobanCredentialsTypeEnum;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using Uint32 = StellarDotnetSdk.Xdr.Uint32;
using SCVal = StellarDotnetSdk.Xdr.SCVal;
using SCValType = StellarDotnetSdk.Xdr.SCValType;
using SCSymbol = StellarDotnetSdk.Xdr.SCSymbol;
using SCString = StellarDotnetSdk.Xdr.SCString;
using SCBytes = StellarDotnetSdk.Xdr.SCBytes;
using SCVec = StellarDotnetSdk.Xdr.SCVec;
using SCMap = StellarDotnetSdk.Xdr.SCMap;
using SCMapEntry = StellarDotnetSdk.Xdr.SCMapEntry;
using SdkSorobanAuthorizationEntry = StellarDotnetSdk.Operations.SorobanAuthorizationEntry;
using SorobanAddressCredentialsBase = StellarDotnetSdk.Operations.SorobanAddressCredentialsBase;
using SorobanAuthorization = StellarDotnetSdk.Operations.SorobanAuthorization;

namespace StellarDotnetSdk.Tests.Sep.Sep0045.Fixtures;

/// <summary>
///     Builds valid SEP-45 challenge authorization entries entirely in-memory for tests.
///     Mirrors the shape produced by a real SEP-45 server (web_auth_verify invocation with
///     an SCMap argument) without requiring a live Soroban RPC.
/// </summary>
internal static class TestChallengeBuilder
{
    public const string DefaultHomeDomain = "sep45.example.com";

    public const string DefaultWebAuthDomain = "auth.example.com";

    // Deterministic 32-byte placeholder → a valid C... address (test-only).
    public static readonly string DefaultWebAuthContractId = GenerateContractIdFromSeed(1);

    public static BuildResult Build(
        string? homeDomain = null,
        string? webAuthDomain = null,
        string? webAuthContractId = null,
        string? clientContractId = null,
        string? clientDomain = null,
        KeyPair? clientDomainKeyPair = null,
        uint signatureExpirationLedger = 1000,
        string? nonce = null,
        long? serverNonce = null,
        long? clientNonce = null,
        long? clientDomainNonce = null,
        Network? network = null,
        bool signServer = true,
        CredentialsType credentialsType = CredentialsType.SOROBAN_CREDENTIALS_ADDRESS,
        CredentialsType? serverCredentialsType = null,
        CredentialsType? clientCredentialsType = null)
    {
        network ??= Network.Test();
        homeDomain ??= DefaultHomeDomain;
        webAuthDomain ??= DefaultWebAuthDomain;
        webAuthContractId ??= DefaultWebAuthContractId;

        var serverKp = KeyPair.Random();
        var clientCid = clientContractId ?? GenerateRandomContractId();
        // SEP-45 reference server emits the nonce as an SCV_STRING (base64 of 48 random bytes).
        var nonceStr = nonce ?? Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

        if (clientDomain != null && clientDomainKeyPair == null)
        {
            throw new ArgumentException("clientDomainKeyPair required when clientDomain supplied");
        }

        // Per the reference web_auth_verify contract the args map is Map<Symbol, String>:
        // every value — including the addresses and nonce — is an SCV_STRING (strkey text
        // for the addresses), not SCV_ADDRESS / SCV_BYTES.
        var map = new List<SCMapEntry>
        {
            MapEntry("account", SCValString(clientCid)),
            MapEntry("home_domain", SCValString(homeDomain)),
            MapEntry("nonce", SCValString(nonceStr)),
            MapEntry("web_auth_domain", SCValString(webAuthDomain)),
            MapEntry("web_auth_domain_account", SCValString(serverKp.AccountId)),
        };
        if (clientDomain != null)
        {
            map.Add(MapEntry("client_domain", SCValString(clientDomain)));
            map.Add(MapEntry("client_domain_account", SCValString(clientDomainKeyPair!.AccountId)));
        }

        var args = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_MAP },
            Map = new SCMap(map.ToArray()),
        };

        var invocation = new SorobanAuthorizedInvocation
        {
            Function = new SorobanAuthorizedFunction
            {
                Discriminant = new SorobanAuthorizedFunctionType
                {
                    InnerValue = SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum
                        .SOROBAN_AUTHORIZED_FUNCTION_TYPE_CONTRACT_FN,
                },
                ContractFn = new InvokeContractArgs
                {
                    ContractAddress = new ScContractId(webAuthContractId).ToXdr(),
                    FunctionName = new SCSymbol("web_auth_verify"),
                    Args = new[] { args },
                },
            },
            SubInvocations = Array.Empty<SorobanAuthorizedInvocation>(),
        };

        // credentialsType applies to every entry; the per-entry overrides build mixed V1/V2 challenges.
        var serverEntry = BuildEntry(serverCredentialsType ?? credentialsType, serverKp.AccountId,
            serverNonce ?? 1, signatureExpirationLedger, invocation);
        var clientEntry = BuildEntry(clientCredentialsType ?? credentialsType, clientCid, clientNonce ?? 2,
            signatureExpirationLedger, invocation);

        var entries = new List<SorobanAuthorizationEntry> { serverEntry, clientEntry };
        if (clientDomain != null)
        {
            var cdEntry = BuildEntry(credentialsType, clientDomainKeyPair!.AccountId, clientDomainNonce ?? 3,
                signatureExpirationLedger, invocation);
            entries.Add(cdEntry);
        }

        if (signServer)
        {
            SignEntryInPlace(serverEntry, serverKp, network);
        }

        return new BuildResult
        {
            Entries = entries.ToArray(),
            ServerKeyPair = serverKp,
            ClientContractId = clientCid,
            ClientDomain = clientDomain,
            ClientDomainKeyPair = clientDomainKeyPair,
            Nonce = nonceStr,
            SignatureExpirationLedger = signatureExpirationLedger,
        };
    }

    public static SorobanAuthorizationEntry BuildMinimalServerSignableEntry()
    {
        return Build().Entries[0];
    }

    /// <summary>Encodes entries as base64 of (int32 length) followed by N XDR-encoded entries.</summary>
    public static string EncodeEntries(SorobanAuthorizationEntry[] entries)
    {
        var stream = new XdrDataOutputStream();
        stream.WriteInt(entries.Length);
        foreach (var e in entries)
        {
            SorobanAuthorizationEntry.Encode(stream, e);
        }
        return Convert.ToBase64String(stream.ToArray());
    }

    public static SorobanAuthorizationEntry[] DecodeEntries(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        var stream = new XdrDataInputStream(bytes);
        var count = stream.ReadInt();
        var result = new SorobanAuthorizationEntry[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = SorobanAuthorizationEntry.Decode(stream);
        }
        return result;
    }

    /// <summary>
    ///     The payload a signer of <paramref name="entry" /> must sign, computed by the SDK's public
    ///     <see cref="SorobanAuthorization.BuildAuthorizationEntryPreimageHash" /> (itself pinned to the JS SDK by
    ///     known-answer vectors). Tests use it as an oracle independent of
    ///     <c>Sep45Challenge.ComputeAuthorizationHash</c>, the implementation under test: legacy
    ///     <c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION</c> for ADDRESS entries, the address-bound
    ///     <c>ENVELOPE_TYPE_SOROBAN_AUTHORIZATION_WITH_ADDRESS</c> for ADDRESS_V2 entries.
    /// </summary>
    public static byte[] ReferenceAuthorizationHash(SorobanAuthorizationEntry entry, Network network)
    {
        var sdkEntry = SdkSorobanAuthorizationEntry.FromXdr(entry);
        var expiration = ((SorobanAddressCredentialsBase)sdkEntry.Credentials).SignatureExpirationLedger;
        return SorobanAuthorization.BuildAuthorizationEntryPreimageHash(sdkEntry, expiration, network);
    }

    /// <summary>
    ///     The address credentials of either address arm (the XDR union keeps ADDRESS and ADDRESS_V2 in
    ///     separate fields).
    /// </summary>
    public static SorobanAddressCredentials AddressCredentials(SorobanAuthorizationEntry entry)
    {
        return entry.Credentials.Discriminant.InnerValue switch
        {
            CredentialsType.SOROBAN_CREDENTIALS_ADDRESS => entry.Credentials.Address,
            CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_V2 => entry.Credentials.AddressV2,
            var other => throw new ArgumentException($"Not an address credential arm: {other}", nameof(entry)),
        };
    }

    /// <summary>
    ///     Sign an entry in place over <see cref="ReferenceAuthorizationHash" />; installs the signature as an
    ///     SCV_VEC of maps {public_key, signature}.
    /// </summary>
    public static void SignEntryInPlace(SorobanAuthorizationEntry entry, KeyPair signer, Network network)
    {
        SignEntryInPlace(entry, signer, ReferenceAuthorizationHash(entry, network));
    }

    /// <summary>Sign an entry in place over an explicit payload (e.g. a deliberately wrong preimage).</summary>
    public static void SignEntryInPlace(SorobanAuthorizationEntry entry, KeyPair signer, byte[] hash)
    {
        var sig = signer.Sign(hash);
        var pubKey = signer.PublicKey;

        var sigMap = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_MAP },
            Map = new SCMap(new[]
            {
                MapEntry("public_key", SCValBytes(pubKey)),
                MapEntry("signature", SCValBytes(sig)),
            }),
        };

        AddressCredentials(entry).Signature = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_VEC },
            Vec = new SCVec(new[] { sigMap }),
        };
    }

    // ---- Private builders ----

    private static SorobanAuthorizationEntry BuildEntry(
        CredentialsType credentialsType, string address, long nonce, uint expirationLedger,
        SorobanAuthorizedInvocation invocation)
    {
        var addressCredentials = new SorobanAddressCredentials
        {
            Address = AddressToXdr(address),
            Nonce = new Int64(nonce),
            SignatureExpirationLedger = new Uint32(expirationLedger),
            Signature = new SCVal
            {
                Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_VEC },
                Vec = new SCVec(Array.Empty<SCVal>()),
            },
        };
        var credentials = new SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType { InnerValue = credentialsType },
        };
        switch (credentialsType)
        {
            case CredentialsType.SOROBAN_CREDENTIALS_ADDRESS:
                credentials.Address = addressCredentials;
                break;
            case CredentialsType.SOROBAN_CREDENTIALS_ADDRESS_V2:
                credentials.AddressV2 = addressCredentials;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(credentialsType), credentialsType,
                    "SEP-45 challenge entries use ADDRESS or ADDRESS_V2 credentials.");
        }
        return new SorobanAuthorizationEntry
        {
            Credentials = credentials,
            RootInvocation = invocation,
        };
    }

    private static SCVal SCValString(string s)
    {
        return new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_STRING },
            Str = new SCString(s),
        };
    }

    private static SCVal SCValBytes(byte[] b)
    {
        return new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_BYTES },
            Bytes = new SCBytes(b),
        };
    }

    private static SCAddress AddressToXdr(string addr)
    {
        if (addr.StartsWith("C", StringComparison.Ordinal))
        {
            return new ScContractId(addr).ToXdr();
        }
        return new ScAccountId(addr).ToXdr();
    }

    private static SCMapEntry MapEntry(string key, SCVal value)
    {
        return new SCMapEntry
        {
            Key = new SCVal
            {
                Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_SYMBOL },
                Sym = new SCSymbol(key),
            },
            Val = value,
        };
    }

    private static string GenerateRandomContractId()
    {
        return StrKey.EncodeContractId(RandomNumberGenerator.GetBytes(32));
    }

    private static string GenerateContractIdFromSeed(byte seed)
    {
        var raw = new byte[32];
        for (var i = 0; i < raw.Length; i++)
        {
            raw[i] = seed;
        }
        return StrKey.EncodeContractId(raw);
    }

    public sealed class BuildResult
    {
        public string ClientContractId = "";
        public string? ClientDomain;
        public KeyPair? ClientDomainKeyPair;
        public SorobanAuthorizationEntry[] Entries = Array.Empty<SorobanAuthorizationEntry>();
        public string Nonce = "";
        public KeyPair ServerKeyPair = null!;
        public uint SignatureExpirationLedger;
    }
}