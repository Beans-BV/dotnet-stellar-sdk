using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0045;
using StellarDotnetSdk.Sep.Sep0045.Exceptions;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using Uint32 = StellarDotnetSdk.Xdr.Uint32;
using SCMap = StellarDotnetSdk.Xdr.SCMap;
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

        var mi = typeof(Sep45Challenge).GetMethod(
            "ComputeAuthorizationHash",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var h1 = (byte[])mi.Invoke(null, new object[] { entry, Network.Test() })!;
        var h2 = (byte[])mi.Invoke(null, new object[] { entry, Network.Test() })!;

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
        result.Entries[0].Credentials.Address.SignatureExpirationLedger = new StellarDotnetSdk.Xdr.Uint32(9999);
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
                    Args = System.Array.Empty<SCVal>(),
                },
            },
            SubInvocations = System.Array.Empty<SorobanAuthorizedInvocation>(),
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
            e.RootInvocation.Function.ContractFn.FunctionName = new SCSymbol("wrong_fn");
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
        Assert.AreEqual(32, parsed.Nonce.Length);
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
    public void ReadChallenge_Throws_WhenNonceWrongLength()
    {
        var result = TestChallengeBuilder.Build(nonce: new byte[16]);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        Sep45Challenge.ReadChallenge(
            xdr, result.ServerKeyPair.AccountId, TestChallengeBuilder.DefaultWebAuthContractId,
            new[] { TestChallengeBuilder.DefaultHomeDomain }, TestChallengeBuilder.DefaultWebAuthDomain);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidHomeDomainException))]
    public void ReadChallenge_Throws_WhenHomeDomainNotAllowed()
    {
        var result = TestChallengeBuilder.Build(homeDomain: "evil.com");
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
                    Args = System.Array.Empty<SCVal>(),
                },
            },
            SubInvocations = System.Array.Empty<SorobanAuthorizedInvocation>(),
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
                        Vec = new SCVec(System.Array.Empty<SCVal>()),
                    },
                },
            },
            RootInvocation = invocation,
        };
    }
}
