using System;
using FakeItEasy;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Generators;

namespace StellarDotnetSdk.Tests;

[TestFixture]
public class BalanceTests
{
    private Balance? Sut { get; set; }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Constructor_LiquidityPool(bool expectedIsAuthorized, bool expectedIsAuthorizedToMaintainLiabilities)
    {
        const string expectedLiquidityPoolId = "1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469";
        const string expectedAssetType = "liquidity_pool_shares";
        const string expectedLimit = "1.0";
        const string expectedBalance = "2.0";

        Sut = new Balance
        {
            AssetType = expectedAssetType,
            Limit = expectedLimit,
            BalanceString = expectedBalance,
            IsAuthorized = expectedIsAuthorized,
            IsAuthorizedToMaintainLiabilities = expectedIsAuthorizedToMaintainLiabilities,
            LiquidityPoolId = expectedLiquidityPoolId
        };

        // expected
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
            IsAuthorizedToMaintainLiabilities = expectedIsAuthorizedToMaintainLiabilities
        };

        // expected
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

    [FsCheck.NUnit.Property(Arbitrary = [typeof(AlphaNum4Generator)])]
    public Property Asset_AlphaNum4(string assetCode)
    {
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
            IsAuthorizedToMaintainLiabilities = A.Dummy<bool>()
        };

        return (Sut.Asset is AssetTypeCreditAlphaNum4).ToProperty();
    }

    [FsCheck.NUnit.Property(Arbitrary = [typeof(AlphaNum12Generator)])]
    public Property Asset_AlphaNum12(string assetCode)
    {
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
            IsAuthorizedToMaintainLiabilities = A.Dummy<bool>()
        };

        return (Sut.Asset is AssetTypeCreditAlphaNum12).ToProperty();
    }

    [Test]
    public void Asset_Native()
    {
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
            IsAuthorizedToMaintainLiabilities = A.Dummy<bool>()
        };

        // actual 
        var actual = Sut.Asset;

        // expected
        actual.Should().BeOfType<AssetTypeNative>();
    }

    [Test]
    public void Asset_LiquidityPool()
    {
        const string expectedLiquidityPoolId = "1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469";

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
            LiquidityPoolId = expectedLiquidityPoolId
        };

        // actual 
        var actual = Sut.Asset;

        // expected
        actual.Should().BeNull();
    }
}