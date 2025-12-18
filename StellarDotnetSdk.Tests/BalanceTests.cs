using System;
using FakeItEasy;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Generators;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for <see cref="Balance" /> class.
/// </summary>
[TestFixture]
public class BalanceTests
{
    private Balance? Sut { get; set; }

    /// <summary>
    ///     Verifies that Balance constructor creates instance with correct properties for liquidity pool shares.
    /// </summary>
    /// <param name="expectedIsAuthorized">Expected value for IsAuthorized property.</param>
    /// <param name="expectedIsAuthorizedToMaintainLiabilities">Expected value for IsAuthorizedToMaintainLiabilities property.</param>
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Constructor_LiquidityPool(bool expectedIsAuthorized, bool expectedIsAuthorizedToMaintainLiabilities)
    {
        // Arrange
        const string expectedLiquidityPoolId = "1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469";
        const string expectedAssetType = "liquidity_pool_shares";
        const string expectedLimit = "1.0";
        const string expectedBalance = "2.0";

        // Act
        Sut = new Balance
        {
            AssetType = expectedAssetType,
            Limit = expectedLimit,
            BalanceString = expectedBalance,
            IsAuthorized = expectedIsAuthorized,
            IsAuthorizedToMaintainLiabilities = expectedIsAuthorizedToMaintainLiabilities,
            LiquidityPoolId = expectedLiquidityPoolId,
            LastModifiedLedger = 100,
        };

        // Assert
        Sut.Asset
            .Should().BeNull();

        Sut.AssetType
            .Should().Be(expectedAssetType);

        Sut.Limit
            .Should().Be(expectedLimit);

        Sut.BalanceString
            .Should().Be(expectedBalance);

        Sut.LiquidityPoolId
            .Should().Be(expectedLiquidityPoolId);

        Sut.AssetCode
            .Should().BeNull();

        Sut.AssetIssuer
            .Should().BeNull();

        Sut.BuyingLiabilities
            .Should().BeNull();

        Sut.SellingLiabilities
            .Should().BeNull();

        Sut.IsAuthorized
            .Should().Be(expectedIsAuthorized);

        Sut.IsAuthorizedToMaintainLiabilities
            .Should().Be(expectedIsAuthorizedToMaintainLiabilities);
    }

    /// <summary>
    ///     Verifies that Balance constructor creates instance with correct properties for credit alphanum assets.
    /// </summary>
    /// <param name="expectedAssetCode">Expected asset code.</param>
    /// <param name="expectedIsAuthorized">Expected value for IsAuthorized property.</param>
    /// <param name="expectedIsAuthorizedToMaintainLiabilities">Expected value for IsAuthorizedToMaintainLiabilities property.</param>
    /// <param name="expectedAssetType">Expected asset type string.</param>
    /// <param name="expectedAssetDataType">Expected asset data type.</param>
    [TestCase("ABC4", true, false, AssetTypeCreditAlphaNum4.RestApiType, typeof(AssetTypeCreditAlphaNum4))]
    [TestCase("ABC12", false, true, AssetTypeCreditAlphaNum12.RestApiType, typeof(AssetTypeCreditAlphaNum12))]
    public void Constructor_CreditAlphaNum(
        string expectedAssetCode,
        bool expectedIsAuthorized,
        bool expectedIsAuthorizedToMaintainLiabilities,
        string expectedAssetType,
        Type expectedAssetDataType)
    {
        const string expectedLimit = "1.0";
        const string expectedBalance = "2.0";
        const string expectedAssetIssuer = "Expected Asset Issuer";
        const string expectedBuyingLiabilities = "3.0";
        const string expectedSellingLiabilities = "4.0";

        // Act
        Sut = new Balance
        {
            AssetType = expectedAssetType,
            AssetCode = expectedAssetCode,
            AssetIssuer = expectedAssetIssuer,
            Limit = expectedLimit,
            BalanceString = expectedBalance,
            BuyingLiabilities = expectedBuyingLiabilities,
            SellingLiabilities = expectedSellingLiabilities,
            IsAuthorized = expectedIsAuthorized,
            IsAuthorizedToMaintainLiabilities = expectedIsAuthorizedToMaintainLiabilities,
            LastModifiedLedger = 1200,
        };

        // Assert
        Sut.Asset.Should().BeOfType(expectedAssetDataType);

        Sut.AssetCode.Should().Be(expectedAssetCode);

        Sut.AssetIssuer.Should().Be(expectedAssetIssuer);

        Sut.AssetType.Should().Be(expectedAssetType);

        Sut.Limit.Should().Be(expectedLimit);

        Sut.BalanceString.Should().Be(expectedBalance);

        Sut.LiquidityPoolId.Should().BeNull();

        Sut.BuyingLiabilities.Should().Be(expectedBuyingLiabilities);

        Sut.SellingLiabilities.Should().Be(expectedSellingLiabilities);

        Sut.IsAuthorized.Should().Be(expectedIsAuthorized);

        Sut.IsAuthorizedToMaintainLiabilities.Should().Be(expectedIsAuthorizedToMaintainLiabilities);
    }

    /// <summary>
    ///     Verifies that Balance.Asset property returns AssetTypeCreditAlphaNum4 when AssetType is set to credit alphanum4.
    /// </summary>
    /// <param name="assetCode">The asset code to test.</param>
    [FsCheck.NUnit.Property(Arbitrary = [typeof(AlphaNum4Generator)])]
    public Property Asset_AlphaNum4(string assetCode)
    {
        // Arrange & Act
        Sut = new Balance
        {
            AssetType = AssetTypeCreditAlphaNum4.RestApiType,
            AssetCode = assetCode,
            AssetIssuer = A.Dummy<string>(),
            Limit = A.Dummy<string>(),
            BalanceString = A.Dummy<string>(),
            BuyingLiabilities = A.Dummy<string>(),
            SellingLiabilities = A.Dummy<string>(),
            IsAuthorized = A.Dummy<bool>(),
            IsAuthorizedToMaintainLiabilities = A.Dummy<bool>(),
            LastModifiedLedger = 500,
        };

        // Assert
        return (Sut.Asset is AssetTypeCreditAlphaNum4).ToProperty();
    }

    /// <summary>
    ///     Verifies that Balance.Asset property returns AssetTypeCreditAlphaNum12 when AssetType is set to credit alphanum12.
    /// </summary>
    /// <param name="assetCode">The asset code to test.</param>
    [FsCheck.NUnit.Property(Arbitrary = [typeof(AlphaNum12Generator)])]
    public Property Asset_AlphaNum12(string assetCode)
    {
        // Arrange & Act
        Sut = new Balance
        {
            AssetType = AssetTypeCreditAlphaNum12.RestApiType,
            AssetCode = assetCode,
            AssetIssuer = A.Dummy<string>(),
            Limit = A.Dummy<string>(),
            BalanceString = "0.0",
            BuyingLiabilities = A.Dummy<string>(),
            SellingLiabilities = A.Dummy<string>(),
            IsAuthorized = A.Dummy<bool>(),
            IsAuthorizedToMaintainLiabilities = A.Dummy<bool>(),
            LastModifiedLedger = 1500,
        };

        // Assert
        return (Sut.Asset is AssetTypeCreditAlphaNum12).ToProperty();
    }

    /// <summary>
    ///     Verifies that Balance.Asset property returns AssetTypeNative when AssetType is set to native.
    /// </summary>
    [Test]
    public void Asset_Native()
    {
        // Arrange & Act
        Sut = new Balance
        {
            AssetType = AssetTypeNative.RestApiType,
            AssetCode = null,
            AssetIssuer = A.Dummy<string>(),
            Limit = A.Dummy<string>(),
            BalanceString = "0.0",
            BuyingLiabilities = A.Dummy<string>(),
            SellingLiabilities = A.Dummy<string>(),
            IsAuthorized = A.Dummy<bool>(),
            IsAuthorizedToMaintainLiabilities = A.Dummy<bool>(),
            LastModifiedLedger = 1500,
        };

        // Act
        var actual = Sut.Asset;

        // Assert
        actual.Should().BeOfType<AssetTypeNative>();
    }

    /// <summary>
    ///     Verifies that Balance.Asset property returns null when AssetType is set to liquidity pool shares.
    /// </summary>
    [Test]
    public void Asset_LiquidityPool()
    {
        // Arrange
        const string expectedLiquidityPoolId = "1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469";

        // Act
        Sut = new Balance
        {
            AssetType = "liquidity_pool_shares",
            AssetCode = A.Dummy<string>(),
            AssetIssuer = A.Dummy<string>(),
            Limit = A.Dummy<string>(),
            BalanceString = "0.0",
            BuyingLiabilities = A.Dummy<string>(),
            SellingLiabilities = A.Dummy<string>(),
            IsAuthorized = A.Dummy<bool>(),
            IsAuthorizedToMaintainLiabilities = A.Dummy<bool>(),
            LiquidityPoolId = expectedLiquidityPoolId,
            LastModifiedLedger = 1500,
        };

        // Act
        var actual = Sut.Asset;

        // Assert
        actual.Should().BeNull();
    }
}