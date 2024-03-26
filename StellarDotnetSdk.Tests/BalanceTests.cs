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
    private Balance SUT { get; set; }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Constructor_LiquidityPool(bool expectedIsAuthorized, bool expectedIsAuthorizedToMaintainLiabilities)
    {
        const string expectedLiquidityPoolId = "1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469";
        const string expectedAssetType = "liquidity_pool_shares";
        const string expectedLimit = "1.0";
        const string expectedBalance = "2.0";

        // setup
        SUT = new Balance(
            expectedAssetType,
            null,
            null,
            expectedBalance,
            expectedLimit,
            null,
            null,
            expectedIsAuthorized,
            expectedIsAuthorizedToMaintainLiabilities,
            expectedLiquidityPoolId);

        // expected
        SUT.Asset
            .Should().BeNull();

        SUT.AssetType
            .Should().Be(expectedAssetType);

        SUT.Limit
            .Should().Be(expectedLimit);

        SUT.BalanceString
            .Should().Be(expectedBalance);

        SUT.LiquidityPoolId
            .Should().Be(expectedLiquidityPoolId);

        SUT.AssetCode
            .Should().BeNull();

        SUT.AssetIssuer
            .Should().BeNull();

        SUT.BuyingLiabilities
            .Should().BeNull();

        SUT.SellingLiabilities
            .Should().BeNull();

        SUT.IsAuthorized
            .Should().Be(expectedIsAuthorized);

        SUT.IsAuthorizedToMaintainLiabilities
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

        // setup
        SUT = new Balance(
            expectedAssetType,
            expectedAssetCode,
            expectedAssetIssuer,
            expectedBalance,
            expectedLimit,
            expectedBuyingLiabilities,
            expectedSellingLiabilities,
            expectedIsAuthorized,
            expectedIsAuthorizedToMaintainLiabilities,
            null);

        // expected
        SUT.Asset
            .Should().BeOfType(expectedAssetDataType);

        SUT.AssetCode
            .Should().Be(expectedAssetCode);

        SUT.AssetIssuer
            .Should().Be(expectedAssetIssuer);

        SUT.AssetType
            .Should().Be(expectedAssetType);

        SUT.Limit
            .Should().Be(expectedLimit);

        SUT.BalanceString
            .Should().Be(expectedBalance);

        SUT.LiquidityPoolId
            .Should().BeNull();

        SUT.BuyingLiabilities
            .Should().Be(expectedBuyingLiabilities);

        SUT.SellingLiabilities
            .Should().Be(expectedSellingLiabilities);

        SUT.IsAuthorized
            .Should().Be(expectedIsAuthorized);

        SUT.IsAuthorizedToMaintainLiabilities
            .Should().Be(expectedIsAuthorizedToMaintainLiabilities);
    }

    [FsCheck.NUnit.Property(Arbitrary = new[] { typeof(AlphaNum4Generator) })]
    public Property Asset_AlphaNum4(string assetCode)
    {
        // setup
        SUT = new Balance(
            AssetTypeCreditAlphaNum4.RestApiType,
            assetCode,
            A.Dummy<string>(),
            "0.0",
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<bool>(),
            A.Dummy<bool>(),
            null);

        return (SUT.Asset is AssetTypeCreditAlphaNum4).ToProperty();
    }

    [FsCheck.NUnit.Property(Arbitrary = new[] { typeof(AlphaNum12Generator) })]
    public Property Asset_AlphaNum12(string assetCode)
    {
        // setup
        SUT = new Balance(
            AssetTypeCreditAlphaNum12.RestApiType,
            assetCode,
            A.Dummy<string>(),
            "0.0",
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<bool>(),
            A.Dummy<bool>(),
            null);

        return (SUT.Asset is AssetTypeCreditAlphaNum12).ToProperty();
    }

    [Test]
    public void Asset_Native()
    {
        // setup
        SUT = new Balance(
            AssetTypeNative.RestApiType,
            null,
            A.Dummy<string>(),
            "0.0",
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<bool>(),
            A.Dummy<bool>(),
            null);

        // actual 
        var actual = SUT.Asset;

        // expected
        actual
            .Should().BeOfType<AssetTypeNative>();
    }

    [Test]
    public void Asset_LiquidityPool()
    {
        const string expectedLiquidityPoolId = "1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469";

        // setup
        SUT = new Balance(
            "liquidity_pool_shares",
            A.Dummy<string>(),
            A.Dummy<string>(),
            "0.0",
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<string>(),
            A.Dummy<bool>(),
            A.Dummy<bool>(),
            expectedLiquidityPoolId);

        // actual 
        var actual = SUT.Asset;

        // expected
        actual
            .Should().BeNull();
    }
}