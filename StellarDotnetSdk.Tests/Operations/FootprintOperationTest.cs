using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.Tests.Operations;

[TestClass]
public class FootprintOperationTest
{
    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

    [TestMethod]
    public void TestExtendFootprintOperationWithMissingExtensionPoint()
    {
        var builder = new ExtendFootprintOperation.Builder(10000U);
        builder.SetSourceAccount(_sourceAccount);

        var operation = builder.Build();
        Assert.IsInstanceOfType(operation.ExtensionPoint, typeof(ExtensionPointZero));
    }

    [TestMethod]
    public void TestExtendFootprintOperationWithMissingSourceAccount()
    {
        var zeroExt = new ExtensionPointZero();
        var builder = new ExtendFootprintOperation.Builder(10000U);
        builder.SetExtensionPoint(zeroExt);

        var operation = builder.Build();

        // Act
        var xdrOperation = operation.ToXdr();

        var decodedOperation = ExtendFootprintOperation.FromOperationXdrBase64(xdrOperation);

        // Assert
        Assert.AreEqual(operation.ExtendTo, decodedOperation.ExtendTo);
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestExtendFootprintOperationWithValidConfiguration()
    {
        var zeroExt = new ExtensionPointZero();
        var builder = new ExtendFootprintOperation.Builder(10000U);
        builder.SetExtensionPoint(zeroExt);
        builder.SetSourceAccount(_sourceAccount);

        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = ExtendFootprintOperation.FromOperationXdrBase64(operationXdrBase64);

        // Assert
        Assert.AreEqual(operation.ExtendTo, decodedOperation.ExtendTo);
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestRestoreFootprintOperationWithMissingSourceAccount()
    {
        var zeroExt = new ExtensionPointZero();
        var builder = new RestoreFootprintOperation.Builder();
        builder.SetExtensionPoint(zeroExt);

        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = RestoreFootprintOperation.FromOperationXdrBase64(operationXdrBase64);

        // Assert
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestRestoreFootprintOperationWithValidConfiguration()
    {
        var zeroExt = new ExtensionPointZero();
        var builder = new RestoreFootprintOperation.Builder();
        builder.SetExtensionPoint(zeroExt);
        builder.SetSourceAccount(_sourceAccount);

        var operation = builder.Build();

        // Act
        var operationXdrBase64 = operation.ToXdrBase64();

        var decodedOperation = RestoreFootprintOperation.FromOperationXdrBase64(operationXdrBase64);

        // Assert
        // ExtensionPoint has no properties
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }
}