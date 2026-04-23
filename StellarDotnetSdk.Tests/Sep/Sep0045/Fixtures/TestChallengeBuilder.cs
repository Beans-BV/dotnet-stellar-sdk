using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using StellarDotnetSdk;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
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

namespace StellarDotnetSdk.Tests.Sep.Sep0045;

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

    public sealed class BuildResult
    {
        public SorobanAuthorizationEntry[] Entries = Array.Empty<SorobanAuthorizationEntry>();
        public KeyPair ServerKeyPair = null!;
        public string ClientContractId = "";
        public string? ClientDomain;
        public KeyPair? ClientDomainKeyPair;
        public byte[] Nonce = Array.Empty<byte>();
        public uint SignatureExpirationLedger;
    }

    public static BuildResult Build(
        string? homeDomain = null,
        string? webAuthDomain = null,
        string? webAuthContractId = null,
        string? clientContractId = null,
        string? clientDomain = null,
        KeyPair? clientDomainKeyPair = null,
        uint signatureExpirationLedger = 1000,
        byte[]? nonce = null,
        long? serverNonce = null,
        long? clientNonce = null,
        long? clientDomainNonce = null,
        Network? network = null,
        bool signServer = true)
    {
        network ??= Network.Test();
        homeDomain ??= DefaultHomeDomain;
        webAuthDomain ??= DefaultWebAuthDomain;
        webAuthContractId ??= DefaultWebAuthContractId;

        var serverKp = KeyPair.Random();
        var clientCid = clientContractId ?? GenerateRandomContractId();
        var nonceBytes = nonce ?? RandomNumberGenerator.GetBytes(32);

        if (clientDomain != null && clientDomainKeyPair == null)
            throw new ArgumentException("clientDomainKeyPair required when clientDomain supplied");

        var map = new List<SCMapEntry>
        {
            MapEntry("account", AddressToSCVal(clientCid)),
            MapEntry("home_domain", SCValString(homeDomain)),
            MapEntry("nonce", SCValBytes(nonceBytes)),
            MapEntry("web_auth_domain", SCValString(webAuthDomain)),
            MapEntry("web_auth_domain_account", AddressToSCVal(serverKp.AccountId)),
        };
        if (clientDomain != null)
        {
            map.Add(MapEntry("client_domain", SCValString(clientDomain)));
            map.Add(MapEntry("client_domain_account", AddressToSCVal(clientDomainKeyPair!.AccountId)));
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

        var serverEntry = BuildEntry(serverKp.AccountId, serverNonce ?? 1, signatureExpirationLedger, invocation);
        var clientEntry = BuildEntry(clientCid, clientNonce ?? 2, signatureExpirationLedger, invocation);

        var entries = new List<SorobanAuthorizationEntry> { serverEntry, clientEntry };
        if (clientDomain != null)
        {
            var cdEntry = BuildEntry(clientDomainKeyPair!.AccountId, clientDomainNonce ?? 3, signatureExpirationLedger, invocation);
            entries.Add(cdEntry);
        }

        if (signServer)
            SignEntryInPlace(serverEntry, serverKp, network);

        return new BuildResult
        {
            Entries = entries.ToArray(),
            ServerKeyPair = serverKp,
            ClientContractId = clientCid,
            ClientDomain = clientDomain,
            ClientDomainKeyPair = clientDomainKeyPair,
            Nonce = nonceBytes,
            SignatureExpirationLedger = signatureExpirationLedger,
        };
    }

    public static SorobanAuthorizationEntry BuildMinimalServerSignableEntry()
        => Build().Entries[0];

    /// <summary>Encodes entries as base64 of (int32 length) followed by N XDR-encoded entries.</summary>
    public static string EncodeEntries(SorobanAuthorizationEntry[] entries)
    {
        var stream = new XdrDataOutputStream();
        stream.WriteInt(entries.Length);
        foreach (var e in entries)
            SorobanAuthorizationEntry.Encode(stream, e);
        return Convert.ToBase64String(stream.ToArray());
    }

    public static SorobanAuthorizationEntry[] DecodeEntries(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        var stream = new XdrDataInputStream(bytes);
        var count = stream.ReadInt();
        var result = new SorobanAuthorizationEntry[count];
        for (var i = 0; i < count; i++)
            result[i] = SorobanAuthorizationEntry.Decode(stream);
        return result;
    }

    /// <summary>Sign an entry in place; installs the signature as an SCV_VEC of maps {public_key, signature}.</summary>
    public static void SignEntryInPlace(SorobanAuthorizationEntry entry, KeyPair signer, Network network)
    {
        var hash = InvokeComputeAuthorizationHash(entry, network);
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

        entry.Credentials.Address.Signature = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_VEC },
            Vec = new SCVec(new[] { sigMap }),
        };
    }

    // ---- Private builders ----

    private static SorobanAuthorizationEntry BuildEntry(
        string address, long nonce, uint expirationLedger, SorobanAuthorizedInvocation invocation)
    {
        return new SorobanAuthorizationEntry
        {
            Credentials = new SorobanCredentials
            {
                Discriminant = new SorobanCredentialsType
                {
                    InnerValue = SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS,
                },
                Address = new SorobanAddressCredentials
                {
                    Address = AddressToXdr(address),
                    Nonce = new Int64(nonce),
                    SignatureExpirationLedger = new Uint32(expirationLedger),
                    Signature = new SCVal
                    {
                        Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_VEC },
                        Vec = new SCVec(Array.Empty<SCVal>()),
                    },
                },
            },
            RootInvocation = invocation,
        };
    }

    private static SCVal SCValString(string s) => new()
    {
        Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_STRING },
        Str = new SCString(s),
    };

    private static SCVal SCValBytes(byte[] b) => new()
    {
        Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_BYTES },
        Bytes = new SCBytes(b),
    };

    private static SCVal AddressToSCVal(string addr) => new()
    {
        Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_ADDRESS },
        Address = AddressToXdr(addr),
    };

    private static SCAddress AddressToXdr(string addr)
    {
        if (addr.StartsWith("C", StringComparison.Ordinal))
            return new ScContractId(addr).ToXdr();
        return new ScAccountId(addr).ToXdr();
    }

    private static SCMapEntry MapEntry(string key, SCVal value) => new()
    {
        Key = new SCVal
        {
            Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_SYMBOL },
            Sym = new SCSymbol(key),
        },
        Val = value,
    };

    private static string GenerateRandomContractId()
        => StrKey.EncodeContractId(RandomNumberGenerator.GetBytes(32));

    private static string GenerateContractIdFromSeed(byte seed)
    {
        var raw = new byte[32];
        for (int i = 0; i < raw.Length; i++) raw[i] = seed;
        return StrKey.EncodeContractId(raw);
    }

    private static byte[] InvokeComputeAuthorizationHash(SorobanAuthorizationEntry entry, Network network)
    {
        var mi = typeof(StellarDotnetSdk.Sep.Sep0045.Sep45Challenge)
            .GetMethod("ComputeAuthorizationHash",
                BindingFlags.NonPublic | BindingFlags.Static)!;
        return (byte[])mi.Invoke(null, new object[] { entry, network })!;
    }
}
