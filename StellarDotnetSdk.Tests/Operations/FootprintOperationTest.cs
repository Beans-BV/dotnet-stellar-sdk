using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using Operation = StellarDotnetSdk.Operations.Operation;

namespace StellarDotnetSdk.Tests.Operations;

[TestClass]
public class FootprintOperationTest
{
    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

    [TestMethod]
    public void TestExtendFootprintOperationWithMissingExtensionPoint()
    {
        var operation = new ExtendFootprintOperation(10000U, null, _sourceAccount);
        Assert.IsInstanceOfType(operation.ExtensionPoint, typeof(ExtensionPointZero));
    }

    [TestMethod]
    public void TestExtendFootprintOperationWithMissingSourceAccount()
    {
        var operation = new ExtendFootprintOperation(10000U, new ExtensionPointZero());

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ExtendFootprintOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(operation.ExtendTo, decodedOperation.ExtendTo);
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestExtendFootprintOperationWithValidConfiguration()
    {
        var operation = new ExtendFootprintOperation(10000U, new ExtensionPointZero(), _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ExtendFootprintOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(operation.ExtendTo, decodedOperation.ExtendTo);
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestRestoreFootprintOperationWithMissingSourceAccount()
    {
        var operation = new RestoreFootprintOperation();

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (RestoreFootprintOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.IsInstanceOfType(decodedOperation.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    [TestMethod]
    public void TestRestoreFootprintOperationWithValidConfiguration()
    {
        var operation = new RestoreFootprintOperation(null, _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (RestoreFootprintOperation)Operation.FromXdr(xdrOperation);

        // Assert
        // ExtensionPoint has no properties
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }
}