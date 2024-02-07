using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using xdrSDK = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk_test.operations;

[TestClass]
public class InvokeHostFunctionOperationTest
{
    private const string WasmHash = "ZBYoEJT3IaPMMk3FoRmnEQHoDxewPZL+Uor+xWI4uII=";
    private const long Nonce = -9223372036854775807;
    private const uint SignatureExpirationLedger = 1319013123;

    private readonly SCAccountId _accountAddress =
        new SCAccountId("GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP");

    private readonly SCVal[] _args = { new SCString("world"), new SCBytes(new byte[] { 0x00, 0x01, 0x02 }) };

    private readonly SCAddress _contractAddress =
        new SCContractId("CDJ4RICANSXXZ275W2OY2U7RO73HYURBGBRHVW2UUXZNGEBIVBNRKEF7");

    private readonly SCSymbol _functionName = new("hello");
    private readonly SCString _signature = new("Signature");

    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

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

    private SorobanAuthorizationEntry InitAuthEntry()
    {
        var authorizedContractFn = new SorobanAuthorizedContractFunction
        {
            HostFunction = new InvokeContractHostFunction(_contractAddress, _functionName, _args)
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

        return new SorobanAuthorizationEntry
        {
            RootInvocation = rootInvocation,
            Credentials = InitSorobanAddressCredentials()
        };
    }

    [TestMethod]
    public void TestCreateContractOperationWithMissingSourceAccount()
    {
        var contractExecutableWasm = new ContractExecutableWasm(WasmHash);
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);
        var contractIdAddressPreimage = new ContractIDAddressPreimage(_accountAddress.InnerValue, salt);
        var builder = new CreateContractOperation.Builder();
        builder.SetContractIDPreimage(contractIdAddressPreimage);
        builder.SetExecutable(contractExecutableWasm);
        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();
        var decodedOperation = CreateContractOperation.FromOperationXdrBase64(operationXdrBase64);

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
    public void TestCreateContractOperationWithMissingPreimage()
    {
        var contractExecutableWasm = new ContractExecutableWasm(WasmHash);
        var builder = new CreateContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetExecutable(contractExecutableWasm);
        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Contract ID preimage cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestCreateContractOperationWithMissingExecutable()
    {
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);
        var contractIdAddressPreimage = new ContractIDAddressPreimage(_accountAddress.InnerValue, salt);
        var builder = new CreateContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetContractIDPreimage(contractIdAddressPreimage);
        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Executable cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestCreateContractOperationFromAddressWithMissingAuthorizationEntry()
    {
        var contractExecutableWasm = new ContractExecutableWasm(WasmHash);
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);
        var contractIdAddressPreimage = new ContractIDAddressPreimage(_accountAddress.InnerValue, salt);
        var builder = new CreateContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetExecutable(contractExecutableWasm);
        builder.SetContractIDPreimage(contractIdAddressPreimage);
        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = CreateContractOperation.FromOperationXdrBase64(operationXdrBase64);

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
        var contractExecutableWasm = new ContractExecutableWasm(WasmHash);
        var salt = new byte[32];
        RandomNumberGenerator.Create().GetBytes(salt);
        var contractIdAddressPreimage = new ContractIDAddressPreimage(_accountAddress.InnerValue, salt);
        var builder = new CreateContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetExecutable(contractExecutableWasm);
        builder.SetContractIDPreimage(contractIdAddressPreimage);
        builder.SetAuth(new[] { InitAuthEntry() });
        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = CreateContractOperation.FromOperationXdrBase64(operationXdrBase64);

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
        for (var i = 0; i < operation.Auth.Length; i++)
            Assert.AreEqual(operation.Auth[i].ToXdrBase64(), decodedOperation.Auth[i].ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestCreateContractOperationWithStellarAssetExecutable()
    {
        var builder = new CreateContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetExecutable(new ContractExecutableStellarAsset());
        builder.SetContractIDPreimage(new ContractIDAssetPreimage(new AssetTypeCreditAlphaNum4("VNDC",
                    "GAEBBKKHGCAD53X244CFGTVEKG7LWUQOAEW4STFHMGYHHFS5WOQZZTMP")));
        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = CreateContractOperation.FromOperationXdrBase64(operationXdrBase64);

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
    public void TestUploadContractOperationWithMissingWasm()
    {
        var builder = new UploadContractOperation.Builder();

        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Wasm cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestUploadContractOperationWithMissingSourceAccount()
    {
        byte[] wasm = { 0x00, 0x01, 0x02, 0x03, 0x34, 0x45, 0x66, 0x46 };

        var builder = new UploadContractOperation.Builder();
        builder.SetWasm(wasm);

        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = UploadContractOperation.FromOperationXdrBase64(operationXdrBase64);

        // Assert
        CollectionAssert.AreEqual(operation.HostFunction.Wasm, decodedOperation.HostFunction.Wasm);
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestUploadContractOperationWithMissingAuthorizationEntry()
    {
        // Arrange
        byte[] wasm = { 0x00, 0x01, 0x02, 0x03, 0x34, 0x45, 0x66, 0x46 };

        var builder = new UploadContractOperation.Builder();
        builder.SetWasm(wasm);
        builder.SetSourceAccount(_sourceAccount);

        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = UploadContractOperation.FromOperationXdrBase64(operationXdrBase64);

        // Assert
        CollectionAssert.AreEqual(operation.HostFunction.Wasm, decodedOperation.HostFunction.Wasm);
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        Assert.AreEqual(_sourceAccount.AccountId, decodedOperation.SourceAccount!.AccountId);
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
        byte[] wasm = { 0x00, 0x01, 0x02, 0x03, 0x34, 0x45, 0x66, 0x46 };

        var builder = new UploadContractOperation.Builder();
        builder.SetWasm(wasm);
        builder.SetSourceAccount(_sourceAccount);
        var authEntry = InitAuthEntry();
        builder.AddAuth(authEntry);
        builder.AddAuth(authEntry);
        builder.RemoveAuth(authEntry);
        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = UploadContractOperation.FromOperationXdrBase64(operationXdrBase64);

        // Assert
        CollectionAssert.AreEqual(operation.HostFunction.Wasm, decodedOperation.HostFunction.Wasm);
        Assert.AreEqual(operation.Auth.Length, decodedOperation.Auth.Length);
        for (var i = 0; i < operation.Auth.Length; i++)
            Assert.AreEqual(operation.Auth[i].ToXdrBase64(), decodedOperation.Auth[i].ToXdrBase64());
        Assert.AreEqual(_sourceAccount.AccountId, decodedOperation.SourceAccount!.AccountId);
    }

    [TestMethod]
    public void TestInvokeContractOperationWithMissingAddress()
    {
        var builder = new InvokeContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetFunctionName(_functionName);
        builder.SetArgs(_args);

        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Contract address cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestInvokeContractOperationWithMissingFunctionName()
    {
        var arg = new SCString("world");
        SCVal[] args = { arg };
        var builder = new InvokeContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetArgs(args);
        builder.SetContractAddress(_contractAddress);

        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Function name cannot be null", ex.Message);
    }

    /// <summary></summary>
    /// <remarks>
    ///     It's not necessary to check each of the hostFunction.Args element for type and properties,
    ///     since there are already other tests in the <see cref="ScValTest" /> class that cover different scenarios for
    ///     <see cref="SCVal" />
    /// </remarks>
    [TestMethod]
    public void TestInvokeContractOperationWithMissingAuthorizationEntry()
    {
        var builder = new InvokeContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetFunctionName(_functionName);
        builder.SetArgs(_args);
        builder.SetContractAddress(_contractAddress);

        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = InvokeContractOperation.FromOperationXdrBase64(operationXdrBase64);

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

        Assert.AreEqual(_sourceAccount.AccountId, decodedOperation.SourceAccount!.AccountId);
    }

    /// <summary></summary>
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
        var builder = new InvokeContractOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetFunctionName(_functionName);
        builder.SetArgs(_args);
        builder.SetContractAddress(_contractAddress);
        builder.AddAuth(InitAuthEntry());

        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = InvokeContractOperation.FromOperationXdrBase64(operationXdrBase64);

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

        for (var i = 0; i < operation.Auth.Length; i++)
            Assert.AreEqual(operation.Auth[i].ToXdrBase64(), decodedOperation.Auth[i].ToXdrBase64());

        Assert.AreEqual(_sourceAccount.AccountId, decodedOperation.SourceAccount!.AccountId);
    }
}