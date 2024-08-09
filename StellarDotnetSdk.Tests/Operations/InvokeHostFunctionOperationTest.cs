using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.Tests.Operations;

[TestClass]
public class InvokeHostFunctionOperationTest
{
    private const string WasmHash = "6416281094F721A3CC324DC5A119A71101E80F17B03D92FE528AFEC56238B882";
    private const long Nonce = -9223372036854775807;
    private const uint SignatureExpirationLedger = 1319013123;

    private readonly SCAccountId _accountAddress = new("GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP");

    private readonly SCVal[] _args = [new SCString("world"), new SCBytes([0x00, 0x01, 0x02])];

    private readonly SCAddress _contractAddress =
        new SCContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");

    private readonly SCSymbol _functionName = new("hello");
    private readonly SCString _signature = new("Signature");

    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

    private SorobanAddressCredentials InitSorobanAddressCredentials()
    {
        return new SorobanAddressCredentials(_accountAddress, Nonce, SignatureExpirationLedger, _signature);
    }

    private SorobanAuthorizationEntry InitAuthEntry()
    {
        var authorizedContractFn =
            new SorobanAuthorizedContractFunction(
                new InvokeContractHostFunction(_contractAddress, _functionName, _args));

        var rootInvocation = new SorobanAuthorizedInvocation(
            authorizedContractFn,
            [
                new SorobanAuthorizedInvocation(
                    authorizedContractFn,
                    Array.Empty<SorobanAuthorizedInvocation>())
            ]);

        return new SorobanAuthorizationEntry(InitSorobanAddressCredentials(), rootInvocation);
    }

    [TestMethod]
    public void TestCreateContractOperationWithMissingSourceAccount()
    {
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);
        var operation = CreateContractOperation.FromAddress(WasmHash, _accountAddress.InnerValue, salt);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (CreateContractOperation)Operation.FromXdr(xdrOperation);

        var contractIdPreimage = (ContractIDAddressPreimage)operation.HostFunction.ContractIDPreimage;
        var decodedContractIdPreimage = (ContractIDAddressPreimage)decodedOperation.HostFunction.ContractIDPreimage;

        var address = (SCAccountId)contractIdPreimage.Address;
        var decodedAddress = (SCAccountId)decodedContractIdPreimage.Address;

        var contractExecutable = (ContractExecutableWasm)operation.HostFunction.Executable;
        var decodedContractExecutable = (ContractExecutableWasm)decodedOperation.HostFunction.Executable;

        // Assert
        Assert.AreEqual(address.InnerValue, decodedAddress.InnerValue);
        CollectionAssert.AreEqual(contractIdPreimage.Salt, decodedContractIdPreimage.Salt);
        Assert.AreEqual(contractExecutable.WasmHash, decodedContractExecutable.WasmHash);
        Assert.IsTrue(operation.Auth.Length == 0);
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestCreateContractOperationFromAddressWithMissingAuthorizationEntry()
    {
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);
        var operation = CreateContractOperation.FromAddress(WasmHash, _accountAddress.InnerValue, salt, _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (CreateContractOperation)Operation.FromXdr(xdrOperation);

        var contractIdPreimage = (ContractIDAddressPreimage)operation.HostFunction.ContractIDPreimage;
        var decodedContractIdPreimage = (ContractIDAddressPreimage)decodedOperation.HostFunction.ContractIDPreimage;

        var address = (SCAccountId)contractIdPreimage.Address;
        var decodedAddress = (SCAccountId)decodedContractIdPreimage.Address;

        var contractExecutable = (ContractExecutableWasm)operation.HostFunction.Executable;
        var decodedContractExecutable = (ContractExecutableWasm)decodedOperation.HostFunction.Executable;

        // Assert
        Assert.AreEqual(address.InnerValue, decodedAddress.InnerValue);
        CollectionAssert.AreEqual(contractIdPreimage.Salt, decodedContractIdPreimage.Salt);
        Assert.AreEqual(contractExecutable.WasmHash, decodedContractExecutable.WasmHash);
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check each of the operation.Auth element for the type and properties,
    ///     since there is already the dedicated test class <see cref="SorobanAuthorizationTest" /> for
    ///     <see cref="SorobanAuthorizationEntry" />
    /// </remarks>
    [TestMethod]
    public void TestCreateContractOperationFromAddressWithValidArguments()
    {
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);
        var operation = CreateContractOperation.FromAddress(WasmHash, _accountAddress.InnerValue, salt, _sourceAccount);
        operation.Auth = [InitAuthEntry()];

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (CreateContractOperation)Operation.FromXdr(xdrOperation);

        var contractIdPreimage = (ContractIDAddressPreimage)operation.HostFunction.ContractIDPreimage;
        var decodedContractIdPreimage = (ContractIDAddressPreimage)decodedOperation.HostFunction.ContractIDPreimage;

        var address = (SCAccountId)contractIdPreimage.Address;
        var decodedAddress = (SCAccountId)decodedContractIdPreimage.Address;

        var contractExecutable = (ContractExecutableWasm)operation.HostFunction.Executable;
        var decodedContractExecutable = (ContractExecutableWasm)decodedOperation.HostFunction.Executable;

        // Assert
        Assert.AreEqual(address.InnerValue, decodedAddress.InnerValue);
        CollectionAssert.AreEqual(contractIdPreimage.Salt, decodedContractIdPreimage.Salt);
        Assert.AreEqual(contractExecutable.WasmHash, decodedContractExecutable.WasmHash);
        Assert.AreEqual(1, decodedOperation.Auth.Length);

        var credentials = InitSorobanAddressCredentials();
        var decodedAuth = decodedOperation.Auth[0];
        Assert.IsInstanceOfType(decodedAuth.Credentials, typeof(SorobanAddressCredentials));
        var decodedCredentials = (SorobanAddressCredentials)decodedAuth.Credentials;
        Assert.AreEqual(((SCAccountId)credentials.Address).InnerValue,
            ((SCAccountId)decodedCredentials.Address).InnerValue);
        Assert.AreEqual(credentials.Nonce, decodedCredentials.Nonce);
        Assert.AreEqual(credentials.SignatureExpirationLedger, decodedCredentials.SignatureExpirationLedger);
        Assert.AreEqual(((SCString)credentials.Signature).InnerValue,
            ((SCString)decodedCredentials.Signature).InnerValue);

        Assert.IsInstanceOfType(decodedAuth.RootInvocation.Function, typeof(SorobanAuthorizedContractFunction));
        var decodedRootFunction = ((SorobanAuthorizedContractFunction)decodedAuth.RootInvocation.Function).HostFunction;
        Assert.IsNotNull(decodedRootFunction);
        Assert.AreEqual(_functionName.InnerValue, decodedRootFunction.FunctionName.InnerValue);
        Assert.AreEqual(_args.Length, decodedRootFunction.Args.Length);
        Assert.AreEqual(((SCString)_args[0]).InnerValue, ((SCString)decodedRootFunction.Args[0]).InnerValue);
        CollectionAssert.AreEqual(((SCBytes)_args[1]).InnerValue, ((SCBytes)decodedRootFunction.Args[1]).InnerValue);
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);

        var decodedSubInvocations = decodedAuth.RootInvocation.SubInvocations;
        Assert.AreEqual(1, decodedSubInvocations.Length);
        Assert.AreEqual(0, decodedSubInvocations[0].SubInvocations.Length);

        var decodedSubFunction = ((SorobanAuthorizedContractFunction)decodedSubInvocations[0].Function).HostFunction;
        Assert.IsNotNull(decodedSubFunction);
        Assert.AreEqual(_functionName.InnerValue, decodedSubFunction.FunctionName.InnerValue);
        Assert.AreEqual(_args.Length, decodedSubFunction.Args.Length);
        Assert.AreEqual(((SCString)_args[0]).InnerValue, ((SCString)decodedSubFunction.Args[0]).InnerValue);
        CollectionAssert.AreEqual(((SCBytes)_args[1]).InnerValue, ((SCBytes)decodedSubFunction.Args[1]).InnerValue);

        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestCreateContractOperationWithStellarAssetExecutable()
    {
        var operation = CreateContractOperation.FromAsset(new AssetTypeCreditAlphaNum4("VNDC",
            "GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP"), _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (CreateContractOperation)Operation.FromXdr(xdrOperation);

        var contractIdPreimage = (ContractIDAssetPreimage)operation.HostFunction.ContractIDPreimage;
        var decodedContractIdPreimage = (ContractIDAssetPreimage)decodedOperation.HostFunction.ContractIDPreimage;

        var asset = (AssetTypeCreditAlphaNum4)contractIdPreimage.Asset;
        var decodedAsset = (AssetTypeCreditAlphaNum4)decodedContractIdPreimage.Asset;

        // Assert
        Assert.AreEqual(asset.Code, decodedAsset.Code);
        Assert.AreEqual(asset.Issuer, decodedAsset.Issuer);

        Assert.IsTrue(operation.Auth.Length == 0);
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestUploadContractOperationWithMissingSourceAccount()
    {
        byte[] wasm = [0x00, 0x01, 0x02, 0x03, 0x34, 0x45, 0x66, 0x46];

        var operation = new UploadContractOperation(wasm);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (UploadContractOperation)Operation.FromXdr(xdrOperation);

        // Assert
        CollectionAssert.AreEqual(operation.Wasm, decodedOperation.Wasm);
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestUploadContractOperationWithMissingAuthorizationEntry()
    {
        // Arrange
        byte[] wasm = [0x00, 0x01, 0x02, 0x03, 0x34, 0x45, 0x66, 0x46];

        var operation = new UploadContractOperation(wasm, _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (UploadContractOperation)Operation.FromXdr(xdrOperation);

        // Assert
        CollectionAssert.AreEqual(operation.HostFunction.Wasm, decodedOperation.HostFunction.Wasm);
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        var decodedSourceAccount = decodedOperation.SourceAccount;
        Assert.IsNotNull(decodedSourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, decodedSourceAccount.AccountId);
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check each of the operation.Auth element for the type and properties,
    ///     since there is already the dedicated test class <see cref="SorobanAuthorizationTest" /> for
    ///     <see cref="SorobanAuthorizationEntry" />
    /// </remarks>
    [TestMethod]
    public void TestUploadContractOperationWithValidArguments()
    {
        // Arrange
        byte[] wasm = [0x00, 0x01, 0x02, 0x03, 0x34, 0x45, 0x66, 0x46];

        var operation = new UploadContractOperation(wasm, _sourceAccount)
        {
            Auth = [InitAuthEntry()]
        };

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (UploadContractOperation)Operation.FromXdr(xdrOperation);

        // Assert
        CollectionAssert.AreEqual(operation.Wasm, decodedOperation.Wasm);
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        var decodedSourceAccount = decodedOperation.SourceAccount;
        Assert.IsNotNull(decodedSourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, decodedSourceAccount.AccountId);
    }

    /// <remarks>
    ///     It's not necessary to check each of the hostFunction.Args element for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void TestInvokeContractOperationWithMissingAuthorizationEntry()
    {
        var operation = new InvokeContractOperation(_contractAddress, _functionName, _args, _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();

        var decodedOperation = (InvokeContractOperation)Operation.FromXdr(xdrOperation);

        var address = (SCContractId)operation.HostFunction.ContractAddress;
        var decodedAddress = (SCContractId)decodedOperation.HostFunction.ContractAddress;

        var hostFunction = operation.HostFunction;
        var decodedFunction = decodedOperation.HostFunction;

        // Assert
        Assert.AreEqual(address.InnerValue, decodedAddress.InnerValue);
        Assert.AreEqual(hostFunction.FunctionName.InnerValue, decodedFunction.FunctionName.InnerValue);
        Assert.AreEqual(hostFunction.Args.Length, decodedFunction.Args.Length);

        for (var i = 0; i < hostFunction.Args.Length; i++)
            Assert.AreEqual(hostFunction.Args[i].ToXdrBase64(),
                decodedFunction.Args[i].ToXdrBase64());
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        var decodedSourceAccount = decodedOperation.SourceAccount;
        Assert.IsNotNull(decodedSourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, decodedSourceAccount.AccountId);
    }

    /// <remarks>
    ///     It's not necessary to check each of the hostFunction.Args element for type and properties,
    ///     since there are already other tests in class <see cref="ScValTest" /> that cover different scenarios for
    ///     <see cref="SCVal" />
    ///     It's not necessary to check each of the operation.Auth element for the type and properties,
    ///     since there is already the dedicated test class <see cref="SorobanAuthorizationTest" /> for
    ///     <see cref="SorobanAuthorizationEntry" />
    /// </remarks>
    [TestMethod]
    public void TestInvokeContractOperationWithValidArguments()
    {
        var operation = new InvokeContractOperation(_contractAddress, _functionName, _args, _sourceAccount)
        {
            Auth = [InitAuthEntry()]
        };

        // Act
        var xdrOperation = operation.ToXdr();

        var decodedOperation = (InvokeContractOperation)Operation.FromXdr(xdrOperation);

        var hostFunction = operation.HostFunction;
        var decodedHostFunction = decodedOperation.HostFunction;

        // Assert
        Assert.AreEqual(
            ((SCContractId)_contractAddress).InnerValue,
            ((SCContractId)decodedHostFunction.ContractAddress).InnerValue);
        Assert.AreEqual(hostFunction.FunctionName.InnerValue, decodedHostFunction.FunctionName.InnerValue);
        Assert.AreEqual(hostFunction.Args.Length, decodedHostFunction.Args.Length);

        for (var i = 0; i < hostFunction.Args.Length; i++)
            Assert.AreEqual(hostFunction.Args[i].ToXdrBase64(),
                decodedHostFunction.Args[i].ToXdrBase64());
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);

        var decodedSourceAccount = decodedOperation.SourceAccount;
        Assert.IsNotNull(decodedSourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, decodedSourceAccount.AccountId);
    }
}