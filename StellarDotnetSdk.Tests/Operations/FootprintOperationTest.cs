using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Soroban;
using Operation = StellarDotnetSdk.Operations.Operation;

namespace StellarDotnetSdk.Tests.Operations;

/// <summary>
///     Tests for footprint-related operations (ExtendFootprintOperation and RestoreFootprintOperation).
/// </summary>
[TestClass]
public class FootprintOperationTest
{
    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

    /// <summary>
    ///     Verifies that ExtendFootprintOperation with null extension point uses ExtensionPointZero.
    /// </summary>
    [TestMethod]
    public void ExtendFootprintOperation_WithNullExtensionPoint_UsesExtensionPointZero()
    {
        // Arrange & Act
        var operation = new ExtendFootprintOperation(10000U, null, _sourceAccount);

        // Assert
        Assert.IsInstanceOfType(operation.ExtensionPoint, typeof(ExtensionPointZero));
    }

    /// <summary>
    ///     Verifies that ExtendFootprintOperation without source account round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void ExtendFootprintOperation_WithoutSourceAccount_RoundTripsThroughXdr()
    {
        // Arrange
        var operation = new ExtendFootprintOperation(10000U, new ExtensionPointZero());

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ExtendFootprintOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(operation.ExtendTo, decodedOperation.ExtendTo);
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    /// <summary>
    ///     Verifies that ExtendFootprintOperation with valid configuration round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void ExtendFootprintOperation_WithValidConfiguration_RoundTripsThroughXdr()
    {
        // Arrange
        var operation = new ExtendFootprintOperation(10000U, new ExtensionPointZero(), _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ExtendFootprintOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(operation.ExtendTo, decodedOperation.ExtendTo);
        Assert.AreEqual(operation.ExtensionPoint.ToXdrBase64(), decodedOperation.ExtensionPoint.ToXdrBase64());
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    /// <summary>
    ///     Verifies that RestoreFootprintOperation without source account round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void RestoreFootprintOperation_WithoutSourceAccount_RoundTripsThroughXdr()
    {
        // Arrange
        var operation = new RestoreFootprintOperation();

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (RestoreFootprintOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.IsInstanceOfType(decodedOperation.ExtensionPoint, typeof(ExtensionPointZero));
        Assert.AreEqual(operation.SourceAccount?.AccountId, decodedOperation.SourceAccount?.AccountId);
    }

    /// <summary>
    ///     Verifies that RestoreFootprintOperation with valid configuration round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void RestoreFootprintOperation_WithValidConfiguration_RoundTripsThroughXdr()
    {
        // Arrange
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