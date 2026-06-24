using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0045;
using StellarDotnetSdk.Sep.Sep0045.Exceptions;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Tests.Sep.Sep0045.Fixtures;
using StellarDotnetSdk.Xdr;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using Uint32 = StellarDotnetSdk.Xdr.Uint32;
using SCMap = StellarDotnetSdk.Xdr.SCMap;
using SCMapEntry = StellarDotnetSdk.Xdr.SCMapEntry;
using SCString = StellarDotnetSdk.Xdr.SCString;
using SCSymbol = StellarDotnetSdk.Xdr.SCSymbol;
using SCVal = StellarDotnetSdk.Xdr.SCVal;
using SCVec = StellarDotnetSdk.Xdr.SCVec;
using SCValType = StellarDotnetSdk.Xdr.SCValType;

namespace StellarDotnetSdk.Tests.Sep.Sep0045;

[TestClass]
public class Sep45ChallengeTest
{
    [TestMethod]
    public void ComputeAuthorizationHash_ProducesDeterministic32ByteHash()
    {
        var entry = BuildMinimalEntry();

        var h1 = Sep45Challenge.ComputeAuthorizationHash(entry, Network.Test());
        var h2 = Sep45Challenge.ComputeAuthorizationHash(entry, Network.Test());

        Assert.AreEqual(32, h1.Length);
        Assert.IsTrue(h1.SequenceEqual(h2));
    }

    [TestMethod]
    public void VerifyServerSignature_Passes_WhenSignatureValid()
    {
        var result = TestChallengeBuilder.Build();
        Sep45Challenge.VerifyServerSignature(
            result.Entries[0], result.ServerKeyPair.AccountId, Network.Test());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidServerSignatureException))]
    public void VerifyServerSignature_Throws_WhenSignerKeyMismatches()
    {
        var result = TestChallengeBuilder.Build();
        var otherSigner = KeyPair.Random();
        Sep45Challenge.VerifyServerSignature(
            result.Entries[0], otherSigner.AccountId, Network.Test());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidServerSignatureException))]
    public void VerifyServerSignature_Throws_WhenEntryUnsigned()
    {
        var result = TestChallengeBuilder.Build(signServer: false);
        Sep45Challenge.VerifyServerSignature(
            result.Entries[0], result.ServerKeyPair.AccountId, Network.Test());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidServerSignatureException))]
    public void VerifyServerSignature_Throws_WhenHashTampered()
    {
        var result = TestChallengeBuilder.Build();
        // Tamper with expiration ledger to invalidate the preimage hash
        result.Entries[0].Credentials.Address.SignatureExpirationLedger = new Uint32(9999);
        Sep45Challenge.VerifyServerSignature(
            result.Entries[0], result.ServerKeyPair.AccountId, Network.Test());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidEntryCountException))]
    public void ReadChallenge_Throws_WhenFewerThanTwoEntries()
    {
        var result = TestChallengeBuilder.Build();
        var onlyServer = new[] { result.Entries[0] };
        var xdr = TestChallengeBuilder.EncodeEntries(onlyServer);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(SubInvocationsNotAllowedException))]
    public void ReadChallenge_Throws_WhenSubInvocationsPresent()
    {
        var result = TestChallengeBuilder.Build();
        var leaf = new SorobanAuthorizedInvocation
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
                    ContractAddress = new ScContractId(
                        TestChallengeBuilder.DefaultWebAuthContractId).ToXdr(),
                    FunctionName = new SCSymbol("other_fn"),
                    Args = Array.Empty<SCVal>(),
                },
            },
            SubInvocations = Array.Empty<SorobanAuthorizedInvocation>(),
        };
        result.Entries[0].RootInvocation.SubInvocations = new[] { leaf };
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidContractIdException))]
    public void ReadChallenge_Throws_WhenContractIdWrong()
    {
        var result = TestChallengeBuilder.Build();
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        var wrongContractId = StrKey.EncodeContractId(new byte[32]);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, wrongContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidFunctionNameException))]
    public void ReadChallenge_Throws_WhenFunctionNameWrong()
    {
        var result = TestChallengeBuilder.Build();
        foreach (var e in result.Entries)
        {
            e.RootInvocation.Function.ContractFn.FunctionName = new SCSymbol("wrong_fn");
        }
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    public void ReadChallenge_ReturnsParsed_WhenValid()
    {
        var result = TestChallengeBuilder.Build();
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        var parsed = Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);

        Assert.AreEqual(result.ClientContractId, parsed.ClientAccountId);
        Assert.AreEqual(TestChallengeBuilder.DefaultHomeDomain, parsed.HomeDomain);
        Assert.AreEqual(TestChallengeBuilder.DefaultWebAuthDomain, parsed.WebAuthDomain);
        Assert.AreEqual(result.ServerKeyPair.AccountId, parsed.WebAuthDomainAccountId);
        Assert.AreEqual(result.Nonce, parsed.Nonce);
        Assert.IsNull(parsed.ClientDomain);
        Assert.IsNull(parsed.ClientDomainAccountId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_WhenRequiredKeyMissing()
    {
        var result = TestChallengeBuilder.Build();
        foreach (var e in result.Entries)
        {
            var map = e.RootInvocation.Function.ContractFn.Args[0].Map!;
            var filtered = map.InnerValue.Where(me => me.Key.Sym.InnerValue != "nonce").ToArray();
            e.RootInvocation.Function.ContractFn.Args[0].Map = new SCMap(filtered);
        }
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidNonceException))]
    public void ReadChallenge_Throws_WhenNonceEmpty()
    {
        var result = TestChallengeBuilder.Build(nonce: "");
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidHomeDomainException))]
    public void ReadChallenge_Throws_WhenHomeDomainNotAllowed()
    {
        var result = TestChallengeBuilder.Build("evil.com");
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidWebAuthDomainException))]
    public void ReadChallenge_Throws_WhenWebAuthDomainMismatch()
    {
        var result = TestChallengeBuilder.Build(webAuthDomain: "bad.com");
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    public void ReadChallenge_ReturnsParsed_WithClientDomain()
    {
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "wallet.example", clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        var parsed = Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
        Assert.AreEqual("wallet.example", parsed.ClientDomain);
        Assert.AreEqual(cdKp.AccountId, parsed.ClientDomainAccountId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidClientDomainException))]
    public void ReadChallenge_Throws_WhenClientDomainAccountInArgsButNoEntry()
    {
        // The args map declares client_domain_account, but the corresponding authorization entry is
        // absent. Both peer SDKs (Flutter, Java) reject this; the validator must too, otherwise the
        // client cannot supply the client-domain signature and the server rejects it opaquely.
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "wallet.example", clientDomainKeyPair: cdKp);
        // Drop the client-domain entry (index 2); keep [server, client]. The shared invocation's args
        // still name client_domain_account, so the args/entries are now inconsistent.
        var withoutClientDomainEntry = new[] { result.Entries[0], result.Entries[1] };
        var xdr = TestChallengeBuilder.EncodeEntries(withoutClientDomainEntry);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidClientDomainException))]
    public void ReadChallenge_Throws_WhenClientDomainAccountEqualsServerAccount()
    {
        // Degenerate/hostile: client_domain_account == the server account. Without an explicit guard the
        // server entry would double as the client-domain entry, defeating the entry-presence check.
        var result = TestChallengeBuilder.Build();
        var serverAccount = result.ServerKeyPair.AccountId;
        var args = result.Entries[0].RootInvocation.Function.ContractFn.Args[0];
        args.Map = new SCMap(args.Map!.InnerValue
            .Concat(new[]
            {
                StringMapEntry("client_domain", "wallet.example"),
                StringMapEntry("client_domain_account", serverAccount),
            }).ToArray());
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, serverAccount, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_OnInvalidBase64()
    {
        Sep45Challenge.ReadChallenge(
            "not valid base64!!", KeyPair.Random().AccountId,
            TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_OnTruncatedXdr()
    {
        // Valid base64, but the XDR is cut in half — decode hits EOF. Must surface as the documented
        // SEP-45 exception type, not a raw IndexOutOfRangeException/InvalidDataException.
        var result = TestChallengeBuilder.Build();
        var bytes = Convert.FromBase64String(TestChallengeBuilder.EncodeEntries(result.Entries));
        var truncated = Convert.ToBase64String(bytes[..(bytes.Length / 2)]);
        Sep45Challenge.ReadChallenge(
            truncated, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_OnImplausibleEntryCount()
    {
        // 4-byte payload claiming 100,000,000 entries — must be rejected before allocating, not OOM.
        var stream = new XdrDataOutputStream();
        stream.WriteInt(100_000_000);
        var b64 = Convert.ToBase64String(stream.ToArray());
        Sep45Challenge.ReadChallenge(
            b64, KeyPair.Random().AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidEntryCountException))]
    public void ReadChallenge_Throws_WhenEntryCountExceedsHardCap()
    {
        // Count (200) is small enough to pass the remaining-bytes bound — 200 filler ints = 800 bytes
        // follow — but exceeds the hard cap on authorization entries, so it is rejected as an entry-count
        // violation (the dedicated InvalidEntryCountException, matching the too-few lower bound).
        var stream = new XdrDataOutputStream();
        stream.WriteInt(200);
        for (var i = 0; i < 200; i++)
        {
            stream.WriteInt(0);
        }
        var b64 = Convert.ToBase64String(stream.ToArray());
        Sep45Challenge.ReadChallenge(
            b64, KeyPair.Random().AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidEntryCountException))]
    public void ReadChallenge_RejectsEntryCount_OneAboveHardCap()
    {
        // Boundary: exactly MaxAuthorizationEntries + 1 (101) with enough backing to pass the byte bound
        // must be rejected by the hard cap. Pins the `>` boundary against a `>=` off-by-one.
        var stream = new XdrDataOutputStream();
        stream.WriteInt(101);
        for (var i = 0; i < 101; i++)
        {
            stream.WriteInt(0);
        }
        var b64 = Convert.ToBase64String(stream.ToArray());
        Sep45Challenge.ReadChallenge(
            b64, KeyPair.Random().AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_AllowsEntryCount_AtHardCap()
    {
        // Boundary: exactly MaxAuthorizationEntries (100) is NOT rejected by the cap — it passes the count
        // checks and only fails later when the (zero-filled) entries fail to decode. The expected type here
        // is InvalidArgumentsException (a decode failure), NOT InvalidEntryCountException, which proves the
        // cap admits 100. Together with the 101 test above this pins the boundary at exactly 100/101.
        var stream = new XdrDataOutputStream();
        stream.WriteInt(100);
        for (var i = 0; i < 100; i++)
        {
            stream.WriteInt(0);
        }
        var b64 = Convert.ToBase64String(stream.ToArray());
        Sep45Challenge.ReadChallenge(
            b64, KeyPair.Random().AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_OnInsufficientBacking_BelowHardCap()
    {
        // A count below the hard cap but with fewer remaining bytes than entries must be rejected by the
        // remaining-bytes bound (not the cap). Isolates the `count > remaining` branch so a future change
        // that drops it cannot pass unnoticed: count = 50, only 10 filler ints (40 bytes) follow.
        var stream = new XdrDataOutputStream();
        stream.WriteInt(50);
        for (var i = 0; i < 10; i++)
        {
            stream.WriteInt(0);
        }
        var b64 = Convert.ToBase64String(stream.ToArray());
        Sep45Challenge.ReadChallenge(
            b64, KeyPair.Random().AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_WhenChallengeExceedsInputSizeCap()
    {
        // A challenge far larger than any legitimate one is rejected before decoding. This input-size cap
        // is what bounds total decode-time allocation (the XDR decoders size every nested array by the
        // bytes remaining), so an oversized hostile payload cannot drive a large allocation.
        var oversized = new string('A', 200_000); // well past the ~64 KiB base64 limit; valid base64 chars
        Sep45Challenge.ReadChallenge(
            oversized, KeyPair.Random().AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_WhenRootFunctionIsNotContractFn()
    {
        // An entry whose root invocation is a create-contract host function (not web_auth_verify's
        // CONTRACT_FN) must be rejected as invalid args. Covers the branch whose exception type is
        // InvalidArgumentsException (it is a wrong function *type*, not a wrong contract id).
        var result = TestChallengeBuilder.Build();
        var notContractFn = new SorobanAuthorizedInvocation
        {
            Function = new SorobanAuthorizedFunction
            {
                Discriminant = new SorobanAuthorizedFunctionType
                {
                    InnerValue = SorobanAuthorizedFunctionType.SorobanAuthorizedFunctionTypeEnum
                        .SOROBAN_AUTHORIZED_FUNCTION_TYPE_CREATE_CONTRACT_HOST_FN,
                },
                CreateContractHostFn = new CreateContractArgs
                {
                    ContractIDPreimage = new ContractIDPreimage
                    {
                        Discriminant = new ContractIDPreimageType
                        {
                            InnerValue = ContractIDPreimageType.ContractIDPreimageTypeEnum
                                .CONTRACT_ID_PREIMAGE_FROM_ADDRESS,
                        },
                        FromAddress = new ContractIDPreimage.ContractIDPreimageFromAddress
                        {
                            Address = new ScContractId(TestChallengeBuilder.DefaultWebAuthContractId).ToXdr(),
                            Salt = new Uint256(new byte[32]),
                        },
                    },
                    Executable = new StellarDotnetSdk.Xdr.ContractExecutable
                    {
                        Discriminant = new ContractExecutableType
                        {
                            InnerValue = ContractExecutableType.ContractExecutableTypeEnum
                                .CONTRACT_EXECUTABLE_STELLAR_ASSET,
                        },
                    },
                },
            },
            SubInvocations = Array.Empty<SorobanAuthorizedInvocation>(),
        };
        foreach (var entry in result.Entries)
        {
            entry.RootInvocation = notContractFn;
        }
        var b64 = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            b64, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_OnTrailingBytes()
    {
        var result = TestChallengeBuilder.Build();
        var bytes = Convert.FromBase64String(TestChallengeBuilder.EncodeEntries(result.Entries));
        var withTrailing = bytes.Concat(new byte[] { 1, 2, 3, 4 }).ToArray();
        Sep45Challenge.ReadChallenge(
            Convert.ToBase64String(withTrailing), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public void ReadChallenge_Throws_WhenEntryHasUnexpectedCredentialsAddress()
    {
        // Replace the client entry's credentials address with a foreign account not named anywhere in
        // the args map. The allowlist check must reject it.
        var result = TestChallengeBuilder.Build();
        var foreign = KeyPair.Random();
        result.Entries[1].Credentials.Address.Address = new ScAccountId(foreign.AccountId).ToXdr();
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    public void ReadChallenge_NeverThrowsRawException_OnMalformedInput()
    {
        // Decode trust boundary: a hostile/buggy server response must always surface as a
        // WebAuthContractException, never a raw FormatException / ArgumentOutOfRangeException /
        // IndexOutOfRangeException, etc. Sweeps a real challenge — every truncation offset, plus a
        // length-prefix corrupted to 0x80000000 (> int.MaxValue) and 0x7FFFFFFF at every 4-byte word.
        var result = TestChallengeBuilder.Build();
        var server = result.ServerKeyPair.AccountId;
        var homeDomains = new[] { TestChallengeBuilder.DefaultHomeDomain };
        var bytes = Convert.FromBase64String(TestChallengeBuilder.EncodeEntries(result.Entries));

        var leaks = new List<string>();

        void Probe(byte[] payload, string label)
        {
            try
            {
                Sep45Challenge.ReadChallenge(
                    Convert.ToBase64String(payload), server,
                    TestChallengeBuilder.DefaultWebAuthContractId, homeDomains,
                    TestChallengeBuilder.DefaultWebAuthDomain);
            }
            catch (WebAuthContractException)
            {
                // documented SEP-45 exception — acceptable
            }
            catch (Exception ex)
            {
                leaks.Add($"{label}: {ex.GetType().FullName}");
            }
        }

        for (var len = 1; len < bytes.Length; len++)
        {
            Probe(bytes[..len], $"truncate@{len}");
        }

        var lengthWords = new[]
        {
            new byte[] { 0x80, 0x00, 0x00, 0x00 }, // > int.MaxValue  (would be ArgumentOutOfRangeException)
            new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, // == int.MaxValue (huge but a valid int length)
        };
        foreach (var word in lengthWords)
        {
            for (var o = 0; o + 4 <= bytes.Length; o++)
            {
                var copy = (byte[])bytes.Clone();
                Array.Copy(word, 0, copy, o, 4);
                Probe(copy, $"corrupt@{o}");
            }
        }

        Assert.AreEqual(0, leaks.Count,
            "ReadChallenge leaked non-WebAuthContractException(s): " +
            string.Join(", ", leaks.Take(8).Distinct()));
    }

    private static SCMapEntry StringMapEntry(string key, string value)
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
                Discriminant = new SCValType { InnerValue = SCValType.SCValTypeEnum.SCV_STRING },
                Str = new SCString(value),
            },
        };
    }

    private static SorobanAuthorizationEntry BuildMinimalEntry()
    {
        var serverKp = KeyPair.Random();
        var addressScVal = new ScAccountId(serverKp.AccountId);

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
                    ContractAddress = new ScContractId(
                        StrKey.EncodeContractId(new byte[32])).ToXdr(),
                    FunctionName = new SCSymbol(Sep45Challenge.WebAuthVerifyFunctionName),
                    Args = Array.Empty<SCVal>(),
                },
            },
            SubInvocations = Array.Empty<SorobanAuthorizedInvocation>(),
        };

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
                    Address = addressScVal.ToXdr(),
                    Nonce = new Int64(42),
                    SignatureExpirationLedger = new Uint32(100),
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
}