using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class TrustlineAssetTest
{
    [TestMethod]
    public void TestCreateCanonicalName()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = TrustlineAsset.Create($"USD:{keypair.AccountId}");
        Assert.AreEqual(((TrustlineAsset.Wrapper)trustlineAsset).Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestCreate()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = TrustlineAsset.Create("non-native", "USD", keypair.AccountId);
        Assert.AreEqual(((TrustlineAsset.Wrapper)trustlineAsset).Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestCreateParameters()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var trustlineAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));
        var trustlineAsset2 = new LiquidityPoolShareTrustlineAsset(trustlineAsset.Id);
        Assert.AreEqual(trustlineAsset.Id, trustlineAsset2.Id);
    }

    [TestMethod]
    public void TestCreateShareChangeTrust()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var liquidityPoolParameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var trustlineAsset =
            (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
                new LiquidityPoolShareChangeTrustAsset(liquidityPoolParameters));
        var trustlineAsset2 = new LiquidityPoolShareTrustlineAsset(trustlineAsset.Id);
        Assert.AreEqual(trustlineAsset.Id, trustlineAsset2.Id);
    }

    [TestMethod]
    public void TestCreateLiquidityPoolId()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var liquidityPoolParameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);
        var trustlineAsset =
            (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
                new LiquidityPoolShareChangeTrustAsset(liquidityPoolParameters));
        var trustlineAsset2 = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(trustlineAsset.Id);
        Assert.AreEqual(trustlineAsset.Id, trustlineAsset2.Id);
    }

    [TestMethod]
    public void TestFromXdrAlphaNum4()
    {
        var keypair = KeyPair.Random();

        var trustlineAssetNativeXdr = new XDR.TrustLineAsset
        {
            Discriminant = new XDR.AssetType { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4 },
            AlphaNum4 = Asset.Create($"USD:{keypair.AccountId}").ToXdr().AlphaNum4,
        };

        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(trustlineAssetNativeXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestFromXdrAlphaNum12()
    {
        var keypair = KeyPair.Random();

        var trustlineAssetNativeXdr = new XDR.TrustLineAsset
        {
            Discriminant = XDR.AssetType.Create(XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12),
            AlphaNum12 = Asset.Create($"USDUSD:{keypair.AccountId}").ToXdr().AlphaNum12,
        };

        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(trustlineAssetNativeXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), $"USDUSD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestFromXdrNative()
    {
        var trustlineAssetNativeXdr = new XDR.TrustLineAsset
        {
            Discriminant = XDR.AssetType.Create(XDR.AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE),
        };

        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(trustlineAssetNativeXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), "native");
    }

    [TestMethod]
    public void TestLiquidityPoolShareTrustlineAsset()
    {
        var asset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.Create(
            new LiquidityPoolConstantProductParameters(new AssetTypeNative(),
                Asset.CreateNonNativeAsset(
                    "VNDT",
                    "GCFRHRU5YRI3IN3IMRMYGWWEG2PX2B6MYH2RJW7NEDE2PTYPISPT3RU7"),
                1000));
        var xdrAsset = asset.ToXdr();
        var decodedAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.FromXdr(xdrAsset);
        CollectionAssert.AreEqual(asset.Id.Hash, decodedAsset.Id.Hash);
    }

    [TestMethod]
    public void TestCreateWithChangeTrustAssetWrapper()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var changeTrustWrapper = new ChangeTrustAsset.Wrapper(asset);
        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.Create(changeTrustWrapper);
        Assert.AreEqual(asset, trustlineAsset.Asset);
    }

    [TestMethod]
    public void TestCreateNonNativeAsset()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.CreateNonNativeAsset("USD", keypair.AccountId);
        Assert.AreEqual($"USD:{keypair.AccountId}", trustlineAsset.Asset.CanonicalName());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestFromXdr_UnknownAssetType()
    {
        var trustlineAssetXdr = new XDR.TrustLineAsset
        {
            Discriminant = new XDR.AssetType { InnerValue = (XDR.AssetType.AssetTypeEnum)999 }
        };
        TrustlineAsset.FromXdr(trustlineAssetXdr);
    }

    [TestMethod]
    public void TestWrapperEquals_Null()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new TrustlineAsset.Wrapper(asset);
        Assert.IsFalse(wrapper.Equals(null));
    }

    [TestMethod]
    public void TestWrapperEquals_TypeMismatch()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new TrustlineAsset.Wrapper(asset);
        Assert.IsFalse(wrapper.Equals(new object()));
    }

    [TestMethod]
    public void TestWrapperEquals_SameAsset()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new TrustlineAsset.Wrapper(asset);
        var wrapper2 = new TrustlineAsset.Wrapper(asset);
        Assert.IsTrue(wrapper1.Equals(wrapper2));
    }

    [TestMethod]
    public void TestWrapperEquals_DifferentAsset()
    {
        var keypair1 = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var asset1 = Asset.Create($"USD:{keypair1.AccountId}");
        var asset2 = Asset.Create($"EUR:{keypair2.AccountId}");
        var wrapper1 = new TrustlineAsset.Wrapper(asset1);
        var wrapper2 = new TrustlineAsset.Wrapper(asset2);
        Assert.IsFalse(wrapper1.Equals(wrapper2));
    }

    [TestMethod]
    public void TestWrapperCompareTo_WithPoolShare()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new TrustlineAsset.Wrapper(asset);

        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"GBP:{keypair2.AccountId}");
        var poolShare = new LiquidityPoolShareTrustlineAsset(
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30));

        var comparison = wrapper.CompareTo(poolShare);
        Assert.AreEqual(-1, comparison);
    }

    [TestMethod]
    public void TestWrapperCompareTo_WithWrapper()
    {
        var keypair = KeyPair.Random();
        var asset1 = Asset.Create($"EUR:{keypair.AccountId}");
        var asset2 = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new TrustlineAsset.Wrapper(asset1);
        var wrapper2 = new TrustlineAsset.Wrapper(asset2);

        var comparison = wrapper1.CompareTo(wrapper2);
        Assert.IsTrue(comparison != 0);
    }

    [TestMethod]
    public void TestToXdr_Wrapper()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        TrustlineAsset trustlineAsset = new TrustlineAsset.Wrapper(asset);
        var xdr = trustlineAsset.ToXdr();
        var roundTripAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripAsset.Asset is AssetTypeCreditAlphaNum);
        Assert.AreEqual($"USD:{keypair.AccountId}", roundTripAsset.Asset.CanonicalName());
    }

    [TestMethod]
    public void TestToXdr_Wrapper_Native()
    {
        var nativeAsset = new AssetTypeNative();
        TrustlineAsset trustlineAsset = new TrustlineAsset.Wrapper(nativeAsset);
        var xdr = trustlineAsset.ToXdr();
        var roundTripAsset = (TrustlineAsset.Wrapper)TrustlineAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripAsset.Asset is AssetTypeNative);
        Assert.AreEqual("native", roundTripAsset.Asset.CanonicalName());
    }

    [TestMethod]
    public void TestToXdr_LiquidityPoolShareTrustlineAsset()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");
        
        var parameters = LiquidityPoolParameters.Create(
            XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
            assetA, assetB, 30);
        TrustlineAsset trustlineAsset = new LiquidityPoolShareTrustlineAsset(parameters);
        var xdr = trustlineAsset.ToXdr();
        var roundTripAsset = (LiquidityPoolShareTrustlineAsset)TrustlineAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripAsset is LiquidityPoolShareTrustlineAsset);
        Assert.AreEqual(parameters.GetId(), roundTripAsset.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestToXdr_UnknownType_ThrowsException()
    {
        // Create a test-only subclass that doesn't match either pattern in the switch expression
        // This tests the default case (_ => throw ...) which is otherwise unreachable
        var unknownTrustlineAsset = new UnknownTrustlineAssetForTesting();
        unknownTrustlineAsset.ToXdr();
    }

    // Test-only class to exercise the default case in ToXdr() switch expression
    private class UnknownTrustlineAssetForTesting : TrustlineAsset
    {
        public override string Type => "unknown_test_type";

        public override bool Equals(object? obj)
        {
            return obj is UnknownTrustlineAssetForTesting;
        }

        public override int CompareTo(TrustlineAsset asset)
        {
            return 0;
        }
    }
}