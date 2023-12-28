using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;

namespace stellar_dotnet_sdk_test.operations;

[TestClass]
public class FootprintOperationTest
{
    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

    [TestMethod]
    public void TestExtendFootprintOperationWithMissingExtensionPoint()
    {
        var extendTo = 10000U;
        var builder = new ExtendFootprintOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetExtendTo(extendTo);

        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Extension point cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestExtendFootprintOperationWithMissingExtendTo()
    {
        var zeroExt = new ExtensionPointZero();
        var builder = new ExtendFootprintOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);
        builder.SetExtensionPoint(zeroExt);

        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Extend to cannot be null", ex.Message);
    }

    [TestMethod]
    public void TestExtendFootprintOperationWithMissingSourceAccount()
    {
        var zeroExt = new ExtensionPointZero();
        var extendTo = 10000U;
        var builder = new ExtendFootprintOperation.Builder();
        builder.SetExtendTo(extendTo);
        builder.SetExtensionPoint(zeroExt);

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
    public void TestExtendFootprintOperationWithValidConfiguration()
    {
        var zeroExt = new ExtensionPointZero();
        var extendTo = 10000U;
        var builder = new ExtendFootprintOperation.Builder();
        builder.SetExtendTo(extendTo);
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
    public void TestRestoreFootprintOperationWithMissingExtensionPoint()
    {
        var builder = new RestoreFootprintOperation.Builder();
        builder.SetSourceAccount(_sourceAccount);

        var ex = Assert.ThrowsException<InvalidOperationException>(() => builder.Build());
        Assert.AreEqual("Extension point cannot be null", ex.Message);
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