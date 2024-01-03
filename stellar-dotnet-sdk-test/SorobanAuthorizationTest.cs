using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using SCAddress = stellar_dotnet_sdk.SCAddress;
using SCString = stellar_dotnet_sdk.SCString;
using InitSorobanAddressCredentials = stellar_dotnet_sdk.SorobanAddressCredentials;
using SorobanAuthorizationEntry = stellar_dotnet_sdk.SorobanAuthorizationEntry;
using SorobanAuthorizedInvocation = stellar_dotnet_sdk.SorobanAuthorizedInvocation;
using SorobanCredentials = stellar_dotnet_sdk.SorobanCredentials;
using xdrSDK = stellar_dotnet_sdk.xdr;
using ContractExecutable = stellar_dotnet_sdk.ContractExecutable;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class SorobanAuthorizationTest
{
    private const long Nonce = -9223372036854775807;
    private const uint SignatureExpirationLedger = 1319013123;

    private readonly SCAddress _accountAddress =
        new SCAccountId("GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP");

    private readonly ContractExecutable _contractExecutableWasm =
        new ContractExecutableWasm("ZBYoEJT3IaPMMk3FoRmnEQHoDxewPZL+Uor+xWI4uII=");

    private readonly SCString _signature = new("Signature");

    private SorobanAddressCredentials InitSorobanAddressCredentials()
    {
        return new SorobanAddressCredentials
        {
            Address = _accountAddress,
            Nonce = Nonce,
            SignatureExpirationLedger = SignatureExpirationLedger,
            Signature = _signature
        };
    }

    private ContractIDAddressPreimage InitContractIDAddressPreimage()
    {
        var random32Bytes = new byte[32];
        RandomNumberGenerator.Create().GetBytes(random32Bytes);

        return new ContractIDAddressPreimage
        {
            Address = _accountAddress,
            Salt = new xdrSDK.Uint256(random32Bytes)
        };
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithValidCredentials()
    {
        var sorobanCredentials = InitSorobanAddressCredentials();

        // Act
        var credentialsXdrBase64 = sorobanCredentials.ToXdrBase64();
        var decodedCredentials = (InitSorobanAddressCredentials)SorobanCredentials.FromXdrBase64(credentialsXdrBase64);

        // Assert
        Assert.AreEqual(((SCAccountId)sorobanCredentials.Address).InnerValue,
            ((SCAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithMissingAddress()
    {
        var sorobanCredentials = InitSorobanAddressCredentials();
        sorobanCredentials.Address = null;

        var ex = Assert.ThrowsException<InvalidOperationException>(() => sorobanCredentials.ToXdrBase64());
        Assert.AreEqual("Address cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithAddressBeingAContractAddress()
    {
        var contractAddress = new SCContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");
        var sorobanCredentials = InitSorobanAddressCredentials();
        sorobanCredentials.Address = contractAddress;

        // Act
        var credentialsXdrBase64 = sorobanCredentials.ToXdrBase64();
        var decodedCredentials = (InitSorobanAddressCredentials)SorobanCredentials.FromXdrBase64(credentialsXdrBase64);

        // Assert
        Assert.AreEqual(contractAddress.InnerValue, ((SCContractId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithMissingSignature()
    {
        var sorobanCredentials = InitSorobanAddressCredentials();
        sorobanCredentials.Signature = null;

        var ex = Assert.ThrowsException<InvalidOperationException>(() => sorobanCredentials.ToXdrBase64());
        Assert.AreEqual("Signature cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithZeroSignatureExpirationLedger()
    {
        var sorobanCredentials = InitSorobanAddressCredentials();
        sorobanCredentials.SignatureExpirationLedger = 0;

        // Act 
        var credentialsXdrBase64 = sorobanCredentials.ToXdrBase64();
        var decodedCredentials = (InitSorobanAddressCredentials)SorobanCredentials.FromXdrBase64(credentialsXdrBase64);

        // Assert
        Assert.AreEqual(((SCAccountId)sorobanCredentials.Address).InnerValue,
            ((SCAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    [TestMethod]
    public void TestSorobanAddressCredentialsWithZeroNonce()
    {
        var sorobanCredentials = InitSorobanAddressCredentials();
        sorobanCredentials.Nonce = 0;
        // Act 
        var credentialsXdrBase64 = sorobanCredentials.ToXdrBase64();
        var decodedCredentials = (InitSorobanAddressCredentials)SorobanCredentials.FromXdrBase64(credentialsXdrBase64);

        // Assert
        Assert.AreEqual(((SCAccountId)sorobanCredentials.Address).InnerValue,
            ((SCAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check for the authEntry.Credentials type and properties,
    ///     since there are already other tests in the <see cref="SorobanAuthorizationTest" /> class that cover different
    ///     scenarios
    ///     for <see cref="SorobanCredentials" />
    /// </remarks>
    [TestMethod]
    public void TestSorobanAuthorizationEntryWithEmptySubInvocations()
    {
        var rootInvocation = new SorobanAuthorizedInvocation
        {
            Function = new SorobanAuthorizedCreateContractFunction
            {
                HostFunction =
                    new CreateContractHostFunction(InitContractIDAddressPreimage(), _contractExecutableWasm)
            },
            SubInvocations = Array.Empty<SorobanAuthorizedInvocation>()
        };

        var authEntry = new SorobanAuthorizationEntry
        {
            RootInvocation = rootInvocation,
            Credentials = InitSorobanAddressCredentials()
        };

        var authEntryXdrBase64 = authEntry.ToXdrBase64();
        var decodedAuthEntry = SorobanAuthorizationEntry.FromXdrBase64(authEntryXdrBase64);

        TestEqualInvocations(authEntry.RootInvocation, decodedAuthEntry.RootInvocation);

        Assert.AreEqual(authEntry.Credentials.ToXdrBase64(), decodedAuthEntry.Credentials.ToXdrBase64());
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check for the authEntry.Credentials type and properties,
    ///     since there are already other tests in the <see cref="SorobanAuthorizationTest" /> class that cover different
    ///     scenarios
    ///     for <see cref="SorobanCredentials" />
    /// </remarks>
    [TestMethod]
    public void TestSorobanAuthorizationEntryContainingAuthorizedCreateContractFunction()
    {
        var authorizedCreateContractFn = new SorobanAuthorizedCreateContractFunction
        {
            HostFunction = new CreateContractHostFunction(InitContractIDAddressPreimage(), _contractExecutableWasm)
        };

        var rootInvocation = new SorobanAuthorizedInvocation
        {
            Function = authorizedCreateContractFn,
            SubInvocations = new SorobanAuthorizedInvocation[]
            {
                new()
                {
                    Function = authorizedCreateContractFn,
                    SubInvocations = Array.Empty<SorobanAuthorizedInvocation>()
                }
            }
        };

        var authEntry = new SorobanAuthorizationEntry
        {
            RootInvocation = rootInvocation,
            Credentials = InitSorobanAddressCredentials()
        };

        var authEntryXdrBase64 = authEntry.ToXdrBase64();
        var decodedAuthEntry = SorobanAuthorizationEntry.FromXdrBase64(authEntryXdrBase64);

        TestEqualInvocations(authEntry.RootInvocation, decodedAuthEntry.RootInvocation);

        Assert.AreEqual(authEntry.Credentials.ToXdrBase64(), decodedAuthEntry.Credentials.ToXdrBase64());
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check for the authEntry.Credentials type and properties,
    ///     since there are already other tests in the <see cref="SorobanAuthorizationTest" /> class that cover different
    ///     scenarios
    ///     for <see cref="SorobanCredentials" />
    /// </remarks>
    [TestMethod]
    public void TestSorobanAuthorizationEntryContainingAuthorizedContractFunction()
    {
        var contractAddress = new SCContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");
        var authorizedContractFn = new SorobanAuthorizedContractFunction
        {
            HostFunction = new InvokeContractHostFunction(contractAddress, new SCSymbol("hello"),
                new SCVal[] { new SCBool(false), new SCString("world") })
        };

        var rootInvocation = new SorobanAuthorizedInvocation
        {
            Function = authorizedContractFn,
            SubInvocations = new SorobanAuthorizedInvocation[]
            {
                new()
                {
                    Function = authorizedContractFn,
                    SubInvocations = Array.Empty<SorobanAuthorizedInvocation>()
                }
            }
        };

        var authEntry = new SorobanAuthorizationEntry
        {
            RootInvocation = rootInvocation,
            Credentials = InitSorobanAddressCredentials()
        };

        var authEntryXdrBase64 = authEntry.ToXdrBase64();
        var decodedAuthEntry = SorobanAuthorizationEntry.FromXdrBase64(authEntryXdrBase64);

        TestEqualInvocations(authEntry.RootInvocation, decodedAuthEntry.RootInvocation);

        Assert.AreEqual(authEntry.Credentials.ToXdrBase64(), decodedAuthEntry.Credentials.ToXdrBase64());
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check for the HostFunction type and properties,
    ///     since there are already other tests in the <see cref="operations.InvokeHostFunctionOperationTest" /> class that
    ///     cover different scenarios
    ///     for <see cref="HostFunction" />
    /// </remarks>
    private void TestEqualInvocations(SorobanAuthorizedInvocation expected, SorobanAuthorizedInvocation actual)
    {
        Assert.AreEqual(expected.Function.GetType(), actual.Function.GetType());
        switch (expected.Function)
        {
            case SorobanAuthorizedContractFunction expectedContractFn:
                var expectedContractHostFunction = expectedContractFn.HostFunction;
                var actualContractHostFunction = ((SorobanAuthorizedContractFunction)actual.Function).HostFunction;
                Assert.AreEqual(expectedContractHostFunction.ToXdrBase64(),
                    actualContractHostFunction.ToXdrBase64());
                break;
            case SorobanAuthorizedCreateContractFunction expectedCreateContractFn:
                var expectedCreateContractHostFunction = expectedCreateContractFn.HostFunction;
                var actualCreateContractHostFunction =
                    ((SorobanAuthorizedCreateContractFunction)actual.Function).HostFunction;
                Assert.AreEqual(expectedCreateContractHostFunction.ToXdrBase64(),
                    actualCreateContractHostFunction.ToXdrBase64());
                break;
        }

        Assert.AreEqual(expected.SubInvocations.Length, actual.SubInvocations.Length);
        for (var i = 0; i < expected.SubInvocations.Length; i++)
            TestEqualInvocations(expected.SubInvocations[0], actual.SubInvocations[0]);
    }
}