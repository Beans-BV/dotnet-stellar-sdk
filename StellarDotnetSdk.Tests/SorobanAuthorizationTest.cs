using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCString = StellarDotnetSdk.Soroban.SCString;
using SCSymbol = StellarDotnetSdk.Soroban.SCSymbol;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using SorobanAddressCredentials = StellarDotnetSdk.Operations.SorobanAddressCredentials;
using SorobanAddressCredentialsV2 = StellarDotnetSdk.Operations.SorobanAddressCredentialsV2;
using SorobanAddressCredentialsWithDelegates = StellarDotnetSdk.Operations.SorobanAddressCredentialsWithDelegates;
using SorobanAuthorizationEntry = StellarDotnetSdk.Operations.SorobanAuthorizationEntry;
using SorobanAuthorizedInvocation = StellarDotnetSdk.Operations.SorobanAuthorizedInvocation;
using SorobanCredentials = StellarDotnetSdk.Operations.SorobanCredentials;
using SorobanDelegateSignature = StellarDotnetSdk.Operations.SorobanDelegateSignature;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for Soroban authorization-related classes and functionality.
/// </summary>
[TestClass]
public class SorobanAuthorizationTest
{
    private const long Nonce = -9223372036854775807;
    private const uint SignatureExpirationLedger = 1319013123;

    private const string WasmHash = "6416281094F721A3CC324DC5A119A71101E80F17B03D92FE528AFEC56238B882";

    private readonly ScAccountId _accountAddress = new("GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP");

    private readonly ContractExecutableWasm _contractExecutableWasm = new(WasmHash);

    private readonly SCString _signature = new("Signature");

    private SorobanAddressCredentials InitSorobanAddressCredentials()
    {
        return new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentials round-trips correctly through XDR serialization with valid credentials.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAddressCredentialsWithValidCredentials_RoundTripsCorrectly()
    {
        // Arrange
        var sorobanCredentials = InitSorobanAddressCredentials();

        // Act
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(((ScAccountId)sorobanCredentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentialsV2 round-trips correctly through XDR serialization,
    ///     and that the decoded instance has the V2 discriminant and concrete type.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAddressCredentialsV2_RoundTripsCorrectly()
    {
        // Arrange
        var credentials =
            new SorobanAddressCredentialsV2(_accountAddress, Nonce, SignatureExpirationLedger, _signature);

        // Act
        var xdr = credentials.ToXdr();
        var decoded = SorobanCredentials.FromXdr(xdr);

        // Assert: discriminant is V2 and the concrete wrapper type is V2 (not V1)
        Assert.AreEqual(
            SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS_V2,
            xdr.Discriminant.InnerValue);
        Assert.IsInstanceOfType(decoded, typeof(SorobanAddressCredentialsV2));
        var d = (SorobanAddressCredentialsV2)decoded;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue, ((ScAccountId)d.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, d.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, d.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue, ((SCString)d.Signature).InnerValue);
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentialsV2 constructor throws ArgumentNullException when address is null
    ///     (the null guard lives in the shared base, so V2 is covered too).
    /// </summary>
    [TestMethod]
    public void Constructor_SorobanAddressCredentialsV2WithMissingAddress_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            new SorobanAddressCredentialsV2(null, Nonce, SignatureExpirationLedger, _signature));
        Assert.IsTrue(ex.Message.Contains("Address cannot be null."));
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentialsV2 constructor throws ArgumentNullException when signature is null.
    /// </summary>
    [TestMethod]
    public void Constructor_SorobanAddressCredentialsV2WithMissingSignature_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            new SorobanAddressCredentialsV2(_accountAddress, Nonce, SignatureExpirationLedger, null));
        Assert.IsTrue(ex.Message.Contains("Signature cannot be null."));
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentials constructor throws ArgumentNullException when address is null.
    /// </summary>
    [TestMethod]
    public void Constructor_SorobanAddressCredentialsWithMissingAddress_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            new SorobanAddressCredentials(null, Nonce, SignatureExpirationLedger, _signature));
        Assert.IsTrue(ex.Message.Contains("Address cannot be null."));
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentials round-trips correctly through XDR serialization when address is a contract
    ///     address.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAddressCredentialsWithContractAddress_RoundTripsCorrectly()
    {
        // Arrange
        var contractAddress = new ScContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");
        var sorobanCredentials =
            new SorobanAddressCredentials(contractAddress, Nonce, SignatureExpirationLedger, _signature);

        // Act
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(contractAddress.InnerValue, ((ScContractId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentials constructor throws ArgumentNullException when signature is null.
    /// </summary>
    [TestMethod]
    public void Constructor_SorobanAddressCredentialsWithMissingSignature_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsException<ArgumentNullException>(()
            => new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, null));
        Assert.IsTrue(ex.Message.Contains("Signature cannot be null."));
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentials round-trips correctly through XDR serialization with zero signature
    ///     expiration ledger.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAddressCredentialsWithZeroSignatureExpirationLedger_RoundTripsCorrectly()
    {
        // Arrange
        var sorobanCredentials = new SorobanAddressCredentials(_accountAddress, Nonce, 0, _signature);

        // Act
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(((ScAccountId)sorobanCredentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentials round-trips correctly through XDR serialization with zero nonce.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAddressCredentialsWithZeroNonce_RoundTripsCorrectly()
    {
        // Arrange
        var sorobanCredentials =
            new SorobanAddressCredentials(_accountAddress, 0, SignatureExpirationLedger, _signature);

        // Act
        var xdrCredentials = sorobanCredentials.ToXdr();
        var decodedCredentials = (SorobanAddressCredentials)SorobanCredentials.FromXdr(xdrCredentials);

        // Assert
        Assert.AreEqual(((ScAccountId)sorobanCredentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(sorobanCredentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(((SCString)sorobanCredentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);
        Assert.AreEqual(sorobanCredentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
    }

    /// <summary>
    ///     Verifies that SorobanAuthorizationEntry round-trips correctly through XDR serialization with empty sub-invocations.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAuthorizationEntryWithEmptySubInvocations_RoundTripsCorrectly()
    {
        // Arrange
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);

        var preimage = new ContractIdAddressPreimage(_accountAddress.InnerValue, salt);
        var rootInvocation = new SorobanAuthorizedInvocation(
            new SorobanAuthorizedCreateContractFunction(
                new CreateContractHostFunction(preimage, _contractExecutableWasm)),
            []);

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        // Act
        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        // Assert
        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedCreateContractFunction));
        var decodedRootFunction =
            ((SorobanAuthorizedCreateContractFunction)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.IsInstanceOfType(decodedRootFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        var decodedPreimage = (ContractIdAddressPreimage)decodedRootFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedRootFunction.Executable, typeof(ContractExecutableWasm));
        var decodedExecutable = (ContractExecutableWasm)decodedRootFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);
        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(0, decodedSubInvocations.Length);
    }

    /// <summary>
    ///     Verifies that SorobanAuthorizationEntry round-trips correctly through XDR serialization containing
    ///     AuthorizedCreateContractFunction.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAuthorizationEntryWithAuthorizedCreateContractFunction_RoundTripsCorrectly()
    {
        // Arrange
        var hostFunction = new CreateContractHostFunction(WasmHash, _accountAddress.InnerValue);
        var authorizedCreateContractFn = new SorobanAuthorizedCreateContractFunction(hostFunction);

        var rootInvocation = new SorobanAuthorizedInvocation(
            authorizedCreateContractFn,
            [
                new SorobanAuthorizedInvocation(authorizedCreateContractFn, Array.Empty<SorobanAuthorizedInvocation>()),
            ]);

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        // Act
        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        // Assert
        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedCreateContractFunction));
        var decodedRootFunction =
            ((SorobanAuthorizedCreateContractFunction)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.IsInstanceOfType(decodedRootFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        var decodedPreimage = (ContractIdAddressPreimage)decodedRootFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        var salt = ((ContractIdAddressPreimage)hostFunction.ContractIdPreimage).Salt;
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedRootFunction.Executable, typeof(ContractExecutableWasm));
        var decodedExecutable = (ContractExecutableWasm)decodedRootFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);

        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(1, decodedSubInvocations.Length);
        Assert.AreEqual(0, decodedSubInvocations[0].SubInvocations.Length);

        var decodedSubFunction =
            ((SorobanAuthorizedCreateContractFunction)decodedSubInvocations[0].Function).HostFunction;
        Assert.IsNotNull(decodedSubFunction);
        Assert.IsInstanceOfType(decodedSubFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        decodedPreimage = (ContractIdAddressPreimage)decodedSubFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedSubFunction.Executable, typeof(ContractExecutableWasm));
        decodedExecutable = (ContractExecutableWasm)decodedSubFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);
    }

    /// <summary>
    ///     Verifies that SorobanAuthorizationEntry round-trips correctly through XDR serialization containing
    ///     AuthorizedCreateContractV2Function.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAuthorizationEntryWithAuthorizedCreateContractV2Function_RoundTripsCorrectly()
    {
        // Arrange
        var arguments = new SCVal[] { new SCString("Test") };
        var hostFunction =
            new CreateContractV2HostFunction(WasmHash, _accountAddress.InnerValue, arguments);
        var authorizedCreateContractFn = new SorobanAuthorizedCreateContractV2Function(hostFunction);

        var rootInvocation = new SorobanAuthorizedInvocation(
            authorizedCreateContractFn,
            [
                new SorobanAuthorizedInvocation(authorizedCreateContractFn, []),
            ]
        );

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        // Act
        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        // Assert
        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedCreateContractV2Function));
        var decodedRootFunction =
            ((SorobanAuthorizedCreateContractV2Function)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.IsInstanceOfType(decodedRootFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        var decodedPreimage = (ContractIdAddressPreimage)decodedRootFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        var salt = ((ContractIdAddressPreimage)hostFunction.ContractIdPreimage).Salt;
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedRootFunction.Executable, typeof(ContractExecutableWasm));
        var decodedExecutable = (ContractExecutableWasm)decodedRootFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);

        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(1, decodedSubInvocations.Length);
        Assert.AreEqual(0, decodedSubInvocations[0].SubInvocations.Length);

        var decodedSubFunction =
            ((SorobanAuthorizedCreateContractV2Function)decodedSubInvocations[0].Function).HostFunction;
        Assert.IsNotNull(decodedSubFunction);
        Assert.IsInstanceOfType(decodedSubFunction.ContractIdPreimage, typeof(ContractIdAddressPreimage));
        decodedPreimage = (ContractIdAddressPreimage)decodedSubFunction.ContractIdPreimage;
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)decodedPreimage.Address).InnerValue);
        CollectionAssert.AreEqual(salt, decodedPreimage.Salt);
        Assert.IsInstanceOfType(decodedSubFunction.Executable, typeof(ContractExecutableWasm));
        decodedExecutable = (ContractExecutableWasm)decodedSubFunction.Executable;
        Assert.AreEqual(_contractExecutableWasm.WasmHash, decodedExecutable.WasmHash);
        var decodedArguments = decodedRootFunction.Arguments;
        Assert.AreEqual(arguments.Length, decodedArguments.Length);
        Assert.AreEqual(((SCString)arguments[0]).InnerValue, ((SCString)decodedArguments[0]).InnerValue);
    }

    /// <summary>
    ///     Verifies that SorobanAuthorizationEntry round-trips correctly through XDR serialization containing
    ///     AuthorizedContractFunction.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAuthorizationEntryWithAuthorizedContractFunction_RoundTripsCorrectly()
    {
        // Arrange
        var contractAddress = new ScContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");
        var functionName = "hello";
        var argName = "world";
        var args = new SCVal[] { new SCBool(false), new SCString(argName) };
        var authorizedContractFn = new SorobanAuthorizedContractFunction(
            new InvokeContractHostFunction(contractAddress, new SCSymbol(functionName), args));

        var rootInvocation = new SorobanAuthorizedInvocation(
            authorizedContractFn,
            [
                new SorobanAuthorizedInvocation(authorizedContractFn, []),
            ]);

        var credentials = InitSorobanAddressCredentials();
        var authEntry = new SorobanAuthorizationEntry(credentials, rootInvocation);

        // Act
        var xdrAuth = authEntry.ToXdr();
        var decodedAuth = SorobanAuthorizationEntry.FromXdr(xdrAuth);

        // Assert
        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((ScAccountId)credentials.Address).InnerValue,
            ((ScAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedContractFunction));
        var decodedRootFunction = ((SorobanAuthorizedContractFunction)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.AreEqual(functionName, decodedRootFunction.FunctionName.InnerValue);
        Assert.AreEqual(args.Length, decodedRootFunction.Args.Length);
        Assert.AreEqual(((SCBool)args[0]).InnerValue, ((SCBool)decodedRootFunction.Args[0]).InnerValue);
        Assert.AreEqual(((SCString)args[1]).InnerValue, ((SCString)decodedRootFunction.Args[1]).InnerValue);

        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(1, decodedSubInvocations.Length);
        Assert.AreEqual(0, decodedSubInvocations[0].SubInvocations.Length);

        var decodedSubFunction = ((SorobanAuthorizedContractFunction)decodedSubInvocations[0].Function).HostFunction;
        Assert.IsNotNull(decodedSubFunction);
        Assert.AreEqual(functionName, decodedSubFunction.FunctionName.InnerValue);
        Assert.AreEqual(args.Length, decodedSubFunction.Args.Length);
        Assert.AreEqual(((SCBool)args[0]).InnerValue, ((SCBool)decodedSubFunction.Args[0]).InnerValue);
        Assert.AreEqual(((SCString)args[1]).InnerValue, ((SCString)decodedSubFunction.Args[1]).InnerValue);
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentialsWithDelegates round-trips correctly through XDR serialization
    ///     including nested delegates.
    /// </summary>
    [TestMethod]
    public void FromXdr_SorobanAddressCredentialsWithDelegates_RoundTripsWithNestedDelegates()
    {
        // Arrange
        var contractAddress = new ScContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");
        var root = new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var nested = new SorobanDelegateSignature(contractAddress, new SCString("nested-sig"), []);
        var top = new SorobanDelegateSignature(_accountAddress, new SCString("top-sig"), [nested]);
        var credentials = new SorobanAddressCredentialsWithDelegates(root, [top]);

        // Act
        var xdr = credentials.ToXdr();
        var decoded = SorobanCredentials.FromXdr(xdr);

        // Assert
        Assert.AreEqual(
            SorobanCredentialsType.SorobanCredentialsTypeEnum.SOROBAN_CREDENTIALS_ADDRESS_WITH_DELEGATES,
            xdr.Discriminant.InnerValue);
        Assert.IsInstanceOfType(decoded, typeof(SorobanAddressCredentialsWithDelegates));
        var d = (SorobanAddressCredentialsWithDelegates)decoded;
        Assert.AreEqual(((ScAccountId)root.Address).InnerValue, ((ScAccountId)d.AddressCredentials.Address).InnerValue);
        Assert.AreEqual(1, d.Delegates.Length);
        Assert.AreEqual("top-sig", ((SCString)d.Delegates[0].Signature).InnerValue);
        Assert.AreEqual(_accountAddress.InnerValue, ((ScAccountId)d.Delegates[0].Address).InnerValue);
        Assert.AreEqual(1, d.Delegates[0].NestedDelegates.Length);
        Assert.AreEqual("nested-sig", ((SCString)d.Delegates[0].NestedDelegates[0].Signature).InnerValue);
        Assert.AreEqual(contractAddress.InnerValue,
            ((ScContractId)d.Delegates[0].NestedDelegates[0].Address).InnerValue);
    }

    /// <summary>
    ///     Verifies that SorobanCredentials.FromXdr throws InvalidOperationException for an unknown discriminant value.
    /// </summary>
    [TestMethod]
    public void FromXdr_UnknownCredentialsDiscriminant_Throws()
    {
        var xdr = new StellarDotnetSdk.Xdr.SorobanCredentials
        {
            Discriminant = new SorobanCredentialsType
            {
                InnerValue = (SorobanCredentialsType.SorobanCredentialsTypeEnum)4,
            },
        };

        var ex = Assert.ThrowsException<InvalidOperationException>(() => SorobanCredentials.FromXdr(xdr));
        Assert.IsTrue(ex.Message.Contains("Unknown SorobanCredentials type"));
        // The unexpected discriminant value must be included to aid forward-compat diagnostics.
        Assert.IsTrue(ex.Message.Contains("4"));
    }

    /// <summary>
    ///     Verifies that SorobanAuthorizationEntry correctly decodes both V2 and delegated credentials via FromXdr.
    /// </summary>
    [TestMethod]
    public void FromXdr_V2AndDelegatedAuthEntries_DecodeWithoutThrowing()
    {
        var v2Entry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsV2(_accountAddress, Nonce, SignatureExpirationLedger, _signature),
            new SorobanAuthorizedInvocation(
                new SorobanAuthorizedContractFunction(new InvokeContractHostFunction(
                    new ScContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7"),
                    new SCSymbol("hello"), [])),
                []));
        var delegatedEntry = new SorobanAuthorizationEntry(
            new SorobanAddressCredentialsWithDelegates(
                new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature),
                [new SorobanDelegateSignature(_accountAddress, _signature, [])]),
            v2Entry.RootInvocation);

        Assert.IsInstanceOfType(
            SorobanAuthorizationEntry.FromXdr(v2Entry.ToXdr()).Credentials, typeof(SorobanAddressCredentialsV2));
        Assert.IsInstanceOfType(
            SorobanAuthorizationEntry.FromXdr(delegatedEntry.ToXdr()).Credentials,
            typeof(SorobanAddressCredentialsWithDelegates));
    }

    /// <summary>
    ///     Verifies that SorobanAddressCredentialsWithDelegates round-trips correctly when the delegates array is empty.
    /// </summary>
    [TestMethod]
    public void FromXdr_DelegatedCredentialsWithEmptyDelegates_RoundTrips()
    {
        var credentials = new SorobanAddressCredentialsWithDelegates(
            new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature),
            []);

        var decoded = SorobanCredentials.FromXdr(credentials.ToXdr());

        Assert.IsInstanceOfType(decoded, typeof(SorobanAddressCredentialsWithDelegates));
        Assert.AreEqual(0, ((SorobanAddressCredentialsWithDelegates)decoded).Delegates.Length);
    }

    /// <summary>
    ///     The delegated wrapper's root is a discriminant-free bare struct, so either a V1 or a V2
    ///     address-credential wrapper may serve as the root.
    /// </summary>
    [TestMethod]
    public void SorobanAddressCredentialsWithDelegates_AcceptsV2Root_RoundTrips()
    {
        var v2Root = new SorobanAddressCredentialsV2(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var credentials = new SorobanAddressCredentialsWithDelegates(v2Root, []);

        var decoded = (SorobanAddressCredentialsWithDelegates)SorobanCredentials.FromXdr(credentials.ToXdr());

        Assert.AreEqual(
            ((ScAccountId)v2Root.Address).InnerValue,
            ((ScAccountId)decoded.AddressCredentials.Address).InnerValue);
        Assert.AreEqual(0, decoded.Delegates.Length);
    }

    /// <summary>
    ///     Encoding is order-faithful: CAP-71 ordering is enforced by the signing helpers, not the
    ///     codec, so an out-of-order delegate array round-trips rather than throwing. This keeps decode
    ///     and encode symmetric — any wire data that decodes can be re-encoded.
    /// </summary>
    [TestMethod]
    public void ToXdr_DelegatesNotSortedByAddress_RoundTripsLeniently()
    {
        var root = new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var (first, second) = DescendingAddressPair();
        var credentials = new SorobanAddressCredentialsWithDelegates(root, new[]
        {
            new SorobanDelegateSignature(first, _signature, []),
            new SorobanDelegateSignature(second, _signature, []),
        });

        var decoded = (SorobanAddressCredentialsWithDelegates)SorobanCredentials.FromXdr(credentials.ToXdr());

        // The supplied (descending) order is preserved verbatim — no reordering, no throw.
        Assert.AreEqual(2, decoded.Delegates.Length);
        Assert.AreEqual(0, SorobanAuthTestHelpers.CompareAddressXdr(first, decoded.Delegates[0].Address));
        Assert.AreEqual(0, SorobanAuthTestHelpers.CompareAddressXdr(second, decoded.Delegates[1].Address));
    }

    /// <summary>
    ///     Encoding does not enforce the CAP-71 no-duplicate rule (the signing helpers do): a duplicated
    ///     delegate array round-trips rather than throwing, keeping decode and encode symmetric.
    /// </summary>
    [TestMethod]
    public void ToXdr_DuplicateDelegateAddresses_RoundTripsLeniently()
    {
        var root = new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var credentials = new SorobanAddressCredentialsWithDelegates(root, new[]
        {
            new SorobanDelegateSignature(_accountAddress, _signature, []),
            new SorobanDelegateSignature(_accountAddress, _signature, []),
        });

        var decoded = (SorobanAddressCredentialsWithDelegates)SorobanCredentials.FromXdr(credentials.ToXdr());
        Assert.AreEqual(2, decoded.Delegates.Length);
    }

    /// <summary>
    ///     Order-faithful encoding applies recursively: an out-of-order nested delegate array also
    ///     round-trips rather than throwing.
    /// </summary>
    [TestMethod]
    public void ToXdr_NestedDelegatesNotSortedByAddress_RoundTripsLeniently()
    {
        var root = new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var (first, second) = DescendingAddressPair();
        var top = new SorobanDelegateSignature(_accountAddress, _signature, new[]
        {
            new SorobanDelegateSignature(first, _signature, []),
            new SorobanDelegateSignature(second, _signature, []),
        });
        var credentials = new SorobanAddressCredentialsWithDelegates(root, new[] { top });

        var decoded = (SorobanAddressCredentialsWithDelegates)SorobanCredentials.FromXdr(credentials.ToXdr());
        Assert.AreEqual(2, decoded.Delegates[0].NestedDelegates.Length);
    }

    /// <summary>
    ///     A correctly sorted, duplicate-free delegate array must encode without throwing.
    /// </summary>
    [TestMethod]
    public void ToXdr_DelegatesSortedByAddress_DoesNotThrow()
    {
        var root = new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var (first, second) = DescendingAddressPair();
        var credentials = new SorobanAddressCredentialsWithDelegates(root, new[]
        {
            new SorobanDelegateSignature(second, _signature, []),
            new SorobanDelegateSignature(first, _signature, []),
        });

        // second < first by construction, so this array is ascending and must round-trip.
        var decoded = SorobanCredentials.FromXdr(credentials.ToXdr());
        Assert.AreEqual(2, ((SorobanAddressCredentialsWithDelegates)decoded).Delegates.Length);
    }

    /// <summary>A null delegate entry must surface a clear validation error, not a NullReferenceException.</summary>
    [TestMethod]
    public void ToXdr_NullDelegateElement_Throws()
    {
        var root = new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var credentials =
            new SorobanAddressCredentialsWithDelegates(root, new SorobanDelegateSignature[] { null! });

        Assert.ThrowsException<InvalidOperationException>(() => credentials.ToXdr());
    }

    /// <summary>
    ///     Regression for the decode/encode asymmetry: wire data carrying descending (CAP-71-invalid)
    ///     delegates must decode AND re-encode without throwing, so parse-then-reserialize pipelines
    ///     (relayers, transaction inspectors) do not crash on data the codec already accepted.
    /// </summary>
    [TestMethod]
    public void FromXdrThenToXdr_UnsortedDelegates_RoundTripsWithoutThrowing()
    {
        var root = new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
        var (first, second) = DescendingAddressPair();
        var unsorted = new SorobanAddressCredentialsWithDelegates(root, new[]
        {
            new SorobanDelegateSignature(first, _signature, []),
            new SorobanDelegateSignature(second, _signature, []),
        });

        // Serialize to real wire bytes and decode them back, proving this is reachable from network data.
        var outputStream = new XdrDataOutputStream();
        StellarDotnetSdk.Xdr.SorobanCredentials.Encode(outputStream, unsorted.ToXdr());
        var decodedXdr =
            StellarDotnetSdk.Xdr.SorobanCredentials.Decode(new XdrDataInputStream(outputStream.ToArray()));

        var decoded = SorobanCredentials.FromXdr(decodedXdr);

        // Re-encoding the decoded value must not throw (previously ToXdr validated order, FromXdr did not).
        Assert.IsNotNull(decoded.ToXdr());
    }

    /// <summary>Returns two distinct addresses ordered so that <c>first</c> &gt; <c>second</c> by XDR bytes.</summary>
    private static (ScAddress first, ScAddress second) DescendingAddressPair()
    {
        ScAddress a = new ScContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");
        ScAddress b = new ScContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");
        return SorobanAuthTestHelpers.CompareAddressXdr(a, b) < 0 ? (b, a) : (a, b);
    }
}