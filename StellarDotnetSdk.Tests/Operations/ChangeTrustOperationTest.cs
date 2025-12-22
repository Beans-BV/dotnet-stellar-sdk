using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using Operation = StellarDotnetSdk.Operations.Operation;
using ChangeTrustAsset = StellarDotnetSdk.Assets.ChangeTrustAsset;
using LiquidityPoolConstantProductParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolConstantProductParameters;
using LiquidityPoolParameters = StellarDotnetSdk.LiquidityPool.LiquidityPoolParameters;
using XdrInt64 = StellarDotnetSdk.Xdr.Int64;
using XdrChangeTrustAsset = StellarDotnetSdk.Xdr.ChangeTrustAsset;

namespace StellarDotnetSdk.Tests.Operations;

/// <summary>
///     Tests for ChangeTrustOperation class functionality.
/// </summary>
[TestClass]
public class ChangeTrustOperationTest
{
    private readonly KeyPair _sourceAccount =
        KeyPair.FromSecretSeed("SC4CGETADVYTCR5HEAVZRB3DZQY5Y4J7RFNJTRA6ESMHIPEZUSTE2QDK");

    /// <summary>
    ///     Verifies that ChangeTrustOperation constructor with null asset throws ArgumentNullException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullAsset_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        _ = new ChangeTrustOperation((Asset)null!);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with native asset and null limit uses MaxLimit.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNativeAssetAndNullLimit_UsesMaxLimit()
    {
        // Arrange & Act
        Asset nativeAsset = new AssetTypeNative();
        var operation = new ChangeTrustOperation(nativeAsset, limit: null);

        // Assert
        Assert.AreEqual(ChangeTrustOperation.MaxLimit, operation.Limit);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)operation.Asset).Asset is AssetTypeNative);
        Assert.IsNull(operation.SourceAccount);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with native asset and specific limit uses provided limit.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNativeAssetAndSpecificLimit_UsesProvidedLimit()
    {
        // Arrange
        const string limit = "1000.5";

        // Act
        var operation = new ChangeTrustOperation(new AssetTypeNative(), limit);

        // Assert
        Assert.AreEqual(limit, operation.Limit);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)operation.Asset).Asset is AssetTypeNative);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with native asset and limit "0" sets limit to zero.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNativeAssetAndZeroLimit_SetsLimitToZero()
    {
        // Arrange & Act
        var operation = new ChangeTrustOperation(new AssetTypeNative(), "0");

        // Assert
        Assert.AreEqual("0", operation.Limit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with native asset and source account sets source account correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNativeAssetAndSourceAccount_SetsSourceAccount()
    {
        // Arrange & Act
        var operation = new ChangeTrustOperation(new AssetTypeNative(), null, _sourceAccount);

        // Assert
        Assert.IsNotNull(operation.SourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, operation.SourceAccount.AccountId);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with CreditAlphaNum4 asset round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCreditAlphaNum4Asset_RoundTripsThroughXdr()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);
        const string limit = "5000";

        // Act
        var operation = new ChangeTrustOperation(asset, limit);
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ChangeTrustOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(limit, decodedOperation.Limit);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)decodedOperation.Asset).Asset is AssetTypeCreditAlphaNum4);
        var decodedAsset = (AssetTypeCreditAlphaNum4)((ChangeTrustAsset.Wrapper)decodedOperation.Asset).Asset;
        Assert.AreEqual("USD", decodedAsset.Code);
        Assert.AreEqual(issuer.AccountId, decodedAsset.Issuer);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with CreditAlphaNum12 asset round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCreditAlphaNum12Asset_RoundTripsThroughXdr()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12("TESTTEST", issuer.AccountId);
        const string limit = "10000";

        // Act
        var operation = new ChangeTrustOperation(asset, limit);
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ChangeTrustOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(limit, decodedOperation.Limit);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)decodedOperation.Asset).Asset is AssetTypeCreditAlphaNum12);
        var decodedAsset = (AssetTypeCreditAlphaNum12)((ChangeTrustAsset.Wrapper)decodedOperation.Asset).Asset;
        Assert.AreEqual("TESTTEST", decodedAsset.Code);
        Assert.AreEqual(issuer.AccountId, decodedAsset.Issuer);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with ChangeTrustAsset parameter and null limit uses MaxLimit.
    /// </summary>
    [TestMethod]
    public void Constructor_WithChangeTrustAssetAndNullLimit_UsesMaxLimit()
    {
        // Arrange
        var changeTrustAsset = ChangeTrustAsset.Create(new AssetTypeNative());

        // Act
        var operation = new ChangeTrustOperation(changeTrustAsset, null);

        // Assert
        Assert.AreEqual(ChangeTrustOperation.MaxLimit, operation.Limit);
        Assert.AreEqual(changeTrustAsset, operation.Asset);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with ChangeTrustAsset parameter and specific limit uses provided limit.
    /// </summary>
    [TestMethod]
    public void Constructor_WithChangeTrustAssetAndSpecificLimit_UsesProvidedLimit()
    {
        // Arrange
        var changeTrustAsset = ChangeTrustAsset.Create(new AssetTypeNative());
        const string limit = "2000";

        // Act
        var operation = new ChangeTrustOperation(changeTrustAsset, limit);

        // Assert
        Assert.AreEqual(limit, operation.Limit);
        Assert.AreEqual(changeTrustAsset, operation.Asset);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with ChangeTrustAsset parameter and source account sets source account correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithChangeTrustAssetAndSourceAccount_SetsSourceAccount()
    {
        // Arrange
        var changeTrustAsset = ChangeTrustAsset.Create(new AssetTypeNative());

        // Act
        var operation = new ChangeTrustOperation(changeTrustAsset, null, _sourceAccount);

        // Assert
        Assert.IsNotNull(operation.SourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, operation.SourceAccount.AccountId);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation obsolete constructor with liquidity pool assets and null feeBP uses default fee.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void Constructor_Obsolete_WithLiquidityPoolAssetsAndNullFeeBP_UsesDefaultFee()
    {
        // Arrange
        var assetA = Asset.Create($"EUR:{KeyPair.Random().AccountId}");
        var assetB = Asset.Create($"USD:{KeyPair.Random().AccountId}");

        // Act
        var operation = new ChangeTrustOperation(assetA, assetB, null, null);

        // Assert
        Assert.IsTrue(operation.Asset is LiquidityPoolShareChangeTrustAsset);
        var poolAsset = (LiquidityPoolShareChangeTrustAsset)operation.Asset;
        var parameters = (LiquidityPoolConstantProductParameters)poolAsset.Parameters;
        Assert.AreEqual(LiquidityPoolParameters.Fee, parameters.Fee);
        Assert.AreEqual(ChangeTrustOperation.MaxLimit, operation.Limit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation obsolete constructor with liquidity pool assets and specific feeBP uses provided fee.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void Constructor_Obsolete_WithLiquidityPoolAssetsAndSpecificFeeBP_UsesProvidedFee()
    {
        // Arrange
        var assetA = Asset.Create($"EUR:{KeyPair.Random().AccountId}");
        var assetB = Asset.Create($"USD:{KeyPair.Random().AccountId}");
        const int feeBP = 50;

        // Act
        var operation = new ChangeTrustOperation(assetA, assetB, feeBP, null);

        // Assert
        Assert.IsTrue(operation.Asset is LiquidityPoolShareChangeTrustAsset);
        var poolAsset = (LiquidityPoolShareChangeTrustAsset)operation.Asset;
        var parameters = (LiquidityPoolConstantProductParameters)poolAsset.Parameters;
        Assert.AreEqual(feeBP, parameters.Fee);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation obsolete constructor with liquidity pool assets and specific limit uses provided limit.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void Constructor_Obsolete_WithLiquidityPoolAssetsAndSpecificLimit_UsesProvidedLimit()
    {
        // Arrange
        var assetA = Asset.Create($"EUR:{KeyPair.Random().AccountId}");
        var assetB = Asset.Create($"USD:{KeyPair.Random().AccountId}");
        const string limit = "3000";

        // Act
        var operation = new ChangeTrustOperation(assetA, assetB, null, limit);

        // Assert
        Assert.AreEqual(limit, operation.Limit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation obsolete constructor with liquidity pool assets and source account sets source account correctly.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void Constructor_Obsolete_WithLiquidityPoolAssetsAndSourceAccount_SetsSourceAccount()
    {
        // Arrange
        var assetA = Asset.Create($"EUR:{KeyPair.Random().AccountId}");
        var assetB = Asset.Create($"USD:{KeyPair.Random().AccountId}");

        // Act
        var operation = new ChangeTrustOperation(assetA, assetB, null, null, _sourceAccount);

        // Assert
        Assert.IsNotNull(operation.SourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, operation.SourceAccount.AccountId);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with liquidity pool share asset round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLiquidityPoolShareAsset_RoundTripsThroughXdr()
    {
        // Arrange
        var assetA = Asset.Create($"EUR:{KeyPair.Random().AccountId}");
        var assetB = Asset.Create($"USD:{KeyPair.Random().AccountId}");
        var changeTrustAsset = ChangeTrustAsset.Create(assetA, assetB, 30);
        const string limit = "5000";

        // Act
        var operation = new ChangeTrustOperation(changeTrustAsset, limit);
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ChangeTrustOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.AreEqual(limit, decodedOperation.Limit);
        Assert.IsTrue(decodedOperation.Asset is LiquidityPoolShareChangeTrustAsset);
        var decodedPoolAsset = (LiquidityPoolShareChangeTrustAsset)decodedOperation.Asset;
        var decodedParameters = (LiquidityPoolConstantProductParameters)decodedPoolAsset.Parameters;
        var originalParameters = (LiquidityPoolConstantProductParameters)((LiquidityPoolShareChangeTrustAsset)changeTrustAsset).Parameters;
        Assert.AreEqual(originalParameters.AssetA.CanonicalName(), decodedParameters.AssetA.CanonicalName());
        Assert.AreEqual(originalParameters.AssetB.CanonicalName(), decodedParameters.AssetB.CanonicalName());
        Assert.AreEqual(originalParameters.Fee, decodedParameters.Fee);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.ToOperationBody creates correct XDR operation body.
    /// </summary>
    [TestMethod]
    public void ToOperationBody_WithNativeAsset_CreatesCorrectXdrOperationBody()
    {
        // Arrange
        var operation = new ChangeTrustOperation(new AssetTypeNative(), "1000");

        // Act
        var operationBody = operation.ToOperationBody();

        // Assert
        Assert.AreEqual(OperationType.OperationTypeEnum.CHANGE_TRUST, operationBody.Discriminant.InnerValue);
        Assert.IsNotNull(operationBody.ChangeTrustOp);
        Assert.IsNotNull(operationBody.ChangeTrustOp.Line);
        Assert.AreEqual(AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE, operationBody.ChangeTrustOp.Line.Discriminant.InnerValue);
        Assert.AreEqual(10000000000L, operationBody.ChangeTrustOp.Limit.InnerValue);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.ToOperationBody with MaxLimit creates correct XDR operation body.
    /// </summary>
    [TestMethod]
    public void ToOperationBody_WithMaxLimit_CreatesCorrectXdrOperationBody()
    {
        // Arrange
        var operation = new ChangeTrustOperation(new AssetTypeNative(), ChangeTrustOperation.MaxLimit);

        // Act
        var operationBody = operation.ToOperationBody();

        // Assert
        Assert.AreEqual(long.MaxValue, operationBody.ChangeTrustOp.Limit.InnerValue);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.ToOperationBody with zero limit creates correct XDR operation body.
    /// </summary>
    [TestMethod]
    public void ToOperationBody_WithZeroLimit_CreatesCorrectXdrOperationBody()
    {
        // Arrange
        var operation = new ChangeTrustOperation(new AssetTypeNative(), "0");

        // Act
        var operationBody = operation.ToOperationBody();

        // Assert
        Assert.AreEqual(0L, operationBody.ChangeTrustOp.Limit.InnerValue);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.FromXdr with native asset type deserializes correctly.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithNativeAssetType_DeserializesCorrectly()
    {
        // Arrange
        var changeTrustOp = new ChangeTrustOp
        {
            Line = new XdrChangeTrustAsset
            {
                Discriminant = new AssetType { InnerValue = AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE },
            },
            Limit = new XdrInt64(long.MaxValue),
        };

        // Act
        var operation = ChangeTrustOperation.FromXdr(changeTrustOp);

        // Assert
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)operation.Asset).Asset is AssetTypeNative);
        Assert.AreEqual(ChangeTrustOperation.MaxLimit, operation.Limit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.FromXdr with CreditAlphaNum4 asset type deserializes correctly.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithCreditAlphaNum4AssetType_DeserializesCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);
        var changeTrustOp = new ChangeTrustOp
        {
            Line = ChangeTrustAsset.Create(asset).ToXdr(),
            Limit = new XdrInt64(50000000000L), // 5000
        };

        // Act
        var operation = ChangeTrustOperation.FromXdr(changeTrustOp);

        // Assert
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)operation.Asset).Asset is AssetTypeCreditAlphaNum4);
        var decodedAsset = (AssetTypeCreditAlphaNum4)((ChangeTrustAsset.Wrapper)operation.Asset).Asset;
        Assert.AreEqual("USD", decodedAsset.Code);
        Assert.AreEqual(issuer.AccountId, decodedAsset.Issuer);
        Assert.AreEqual("5000", operation.Limit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.FromXdr with CreditAlphaNum12 asset type deserializes correctly.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithCreditAlphaNum12AssetType_DeserializesCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12("TESTTEST", issuer.AccountId);
        var changeTrustOp = new ChangeTrustOp
        {
            Line = ChangeTrustAsset.Create(asset).ToXdr(),
            Limit = new XdrInt64(100000000000L), // 10000
        };

        // Act
        var operation = ChangeTrustOperation.FromXdr(changeTrustOp);

        // Assert
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)operation.Asset).Asset is AssetTypeCreditAlphaNum12);
        var decodedAsset = (AssetTypeCreditAlphaNum12)((ChangeTrustAsset.Wrapper)operation.Asset).Asset;
        Assert.AreEqual("TESTTEST", decodedAsset.Code);
        Assert.AreEqual(issuer.AccountId, decodedAsset.Issuer);
        Assert.AreEqual("10000", operation.Limit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.FromXdr with pool share asset type deserializes correctly.
    /// </summary>
    [TestMethod]
    public void FromXdr_WithPoolShareAssetType_DeserializesCorrectly()
    {
        // Arrange
        var assetA = Asset.Create($"EUR:{KeyPair.Random().AccountId}");
        var assetB = Asset.Create($"USD:{KeyPair.Random().AccountId}");
        const int feeBP = 30;
        var changeTrustAsset = ChangeTrustAsset.Create(assetA, assetB, feeBP);
        var changeTrustOp = new ChangeTrustOp
        {
            Line = changeTrustAsset.ToXdr(),
            Limit = new XdrInt64(50000000000L), // 5000
        };

        // Act
        var operation = ChangeTrustOperation.FromXdr(changeTrustOp);

        // Assert
        Assert.IsTrue(operation.Asset is LiquidityPoolShareChangeTrustAsset);
        var poolAsset = (LiquidityPoolShareChangeTrustAsset)operation.Asset;
        var parameters = (LiquidityPoolConstantProductParameters)poolAsset.Parameters;
        Assert.AreEqual(assetA.CanonicalName(), parameters.AssetA.CanonicalName());
        Assert.AreEqual(assetB.CanonicalName(), parameters.AssetB.CanonicalName());
        Assert.AreEqual(feeBP, parameters.Fee);
        Assert.AreEqual("5000", operation.Limit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.FromXdr with unknown asset type throws ArgumentOutOfRangeException.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void FromXdr_WithUnknownAssetType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var changeTrustOp = new ChangeTrustOp
        {
            Line = new XdrChangeTrustAsset
            {
                Discriminant = new AssetType { InnerValue = (AssetType.AssetTypeEnum)999 },
            },
            Limit = new XdrInt64(10000000000L),
        };

        // Act & Assert
        _ = ChangeTrustOperation.FromXdr(changeTrustOp);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation.MaxLimit constant has correct value.
    /// </summary>
    [TestMethod]
    public void MaxLimit_Constant_HasCorrectValue()
    {
        // Assert
        Assert.AreEqual("922337203685.4775807", ChangeTrustOperation.MaxLimit);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with native asset and source account round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithNativeAssetAndSourceAccount_RoundTripsCorrectly()
    {
        // Arrange
        var operation = new ChangeTrustOperation(new AssetTypeNative(), "1000", _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ChangeTrustOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        Assert.AreEqual("1000", decodedOperation.Limit);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)decodedOperation.Asset).Asset is AssetTypeNative);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with CreditAlphaNum4 asset and source account round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithCreditAlphaNum4AssetAndSourceAccount_RoundTripsCorrectly()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4("USD", issuer.AccountId);
        var operation = new ChangeTrustOperation(asset, "2000", _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ChangeTrustOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        Assert.AreEqual("2000", decodedOperation.Limit);
        Assert.IsTrue(((ChangeTrustAsset.Wrapper)decodedOperation.Asset).Asset is AssetTypeCreditAlphaNum4);
    }

    /// <summary>
    ///     Verifies that ChangeTrustOperation with liquidity pool share asset and source account round-trips correctly through XDR.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithLiquidityPoolShareAssetAndSourceAccount_RoundTripsCorrectly()
    {
        // Arrange
        var assetA = Asset.Create($"EUR:{KeyPair.Random().AccountId}");
        var assetB = Asset.Create($"USD:{KeyPair.Random().AccountId}");
        var changeTrustAsset = ChangeTrustAsset.Create(assetA, assetB, 30);
        var operation = new ChangeTrustOperation(changeTrustAsset, "3000", _sourceAccount);

        // Act
        var xdrOperation = operation.ToXdr();
        var decodedOperation = (ChangeTrustOperation)Operation.FromXdr(xdrOperation);

        // Assert
        Assert.IsNotNull(decodedOperation.SourceAccount);
        Assert.AreEqual(_sourceAccount.AccountId, decodedOperation.SourceAccount.AccountId);
        Assert.AreEqual("3000", decodedOperation.Limit);
        Assert.IsTrue(decodedOperation.Asset is LiquidityPoolShareChangeTrustAsset);
    }
}

