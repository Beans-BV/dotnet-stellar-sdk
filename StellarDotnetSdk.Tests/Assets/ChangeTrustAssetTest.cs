using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.LiquidityPool;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class ChangeTrustAssetTest
{
    [TestMethod]
    public void TestCreateCanonical()
    {
        var keypair = KeyPair.Random();
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create($"USD:{keypair.AccountId}");
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestCreateLiquidityPoolParameters()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var parameters =
            LiquidityPoolParameters.Create(XDR.LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT,
                assetA, assetB, 30);

        var liquidityPoolShareChangeTrustAsset =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.Create(parameters);
        var liquidityPoolShareChangeTrustAsset2 =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.FromXdr(liquidityPoolShareChangeTrustAsset.ToXdr());

        Assert.AreEqual(liquidityPoolShareChangeTrustAsset.Parameters.GetId(),
            liquidityPoolShareChangeTrustAsset2.Parameters.GetId());
    }

    [TestMethod]
    public void TestCreateTrustlineWrapper()
    {
        var keypair = KeyPair.Random();
        var trustlineAsset = (TrustlineAsset.Wrapper)TrustlineAsset.Create("non-native", "USD", keypair.AccountId);
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create(trustlineAsset);

        Assert.AreEqual(trustlineAsset.Asset, changeTrustAsset.Asset);
    }

    [TestMethod]
    public void TestCreateNonNative()
    {
        var keypair = KeyPair.Random();
        var changeTrustAsset =
            (ChangeTrustAsset.Wrapper)ChangeTrustAsset.CreateNonNativeAsset("USD", keypair.AccountId);
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestFromXdrAlphaNum4()
    {
        var keypair = KeyPair.Random();

        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM4 };
        changeTrustAssetXdr.AlphaNum4 = Asset.Create($"USD:{keypair.AccountId}").ToXdr().AlphaNum4;

        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestFromXdrAlphaNum12()
    {
        var keypair = KeyPair.Random();

        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_CREDIT_ALPHANUM12 };
        changeTrustAssetXdr.AlphaNum12 = Asset.Create($"USDUSD:{keypair.AccountId}").ToXdr().AlphaNum12;

        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
        Assert.AreEqual(changeTrustAsset.Asset.CanonicalName(), $"USDUSD:{keypair.AccountId}");
    }

    [TestMethod]
    public void TestFromXdrNative()
    {
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = XDR.AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE };

        var trustlineAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
        Assert.AreEqual(trustlineAsset.Asset.CanonicalName(), "native");
    }

    [TestMethod]
    public void TestCreateWithAsset()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create(asset);
        Assert.AreEqual(asset, changeTrustAsset.Asset);
    }

    [TestMethod]
    public void TestCreateWithAssetAAndAssetB()
    {
        var keypair = KeyPair.Random();
        var keypair2 = KeyPair.Random();

        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"USD:{keypair2.AccountId}");

        var liquidityPoolShareChangeTrustAsset =
            (LiquidityPoolShareChangeTrustAsset)ChangeTrustAsset.Create(assetA, assetB, 30);
        Assert.IsNotNull(liquidityPoolShareChangeTrustAsset.Parameters);
        Assert.AreEqual("pool_share", liquidityPoolShareChangeTrustAsset.Type);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestFromXdr_UnknownAssetType()
    {
        var changeTrustAssetXdr = new XDR.ChangeTrustAsset();
        changeTrustAssetXdr.Discriminant = new XDR.AssetType
            { InnerValue = (XDR.AssetType.AssetTypeEnum)999 };

        ChangeTrustAsset.FromXdr(changeTrustAssetXdr);
    }

    [TestMethod]
    public void TestWrapperEquals_SameAsset()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset);
        Assert.IsTrue(wrapper1.Equals(wrapper2));
    }

    [TestMethod]
    public void TestWrapperEquals_DifferentAsset()
    {
        var keypair1 = KeyPair.Random();
        var keypair2 = KeyPair.Random();
        var asset1 = Asset.Create($"USD:{keypair1.AccountId}");
        var asset2 = Asset.Create($"EUR:{keypair2.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset1);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset2);
        Assert.IsFalse(wrapper1.Equals(wrapper2));
    }

    [TestMethod]
    public void TestWrapperEquals_Null()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new ChangeTrustAsset.Wrapper(asset);
        Assert.IsFalse(wrapper.Equals(null));
    }

    [TestMethod]
    public void TestWrapperCompareTo_WithPoolShare()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper = new ChangeTrustAsset.Wrapper(asset);

        var keypair2 = KeyPair.Random();
        var assetA = Asset.Create($"EUR:{keypair.AccountId}");
        var assetB = Asset.Create($"GBP:{keypair2.AccountId}");
        var poolShare = new LiquidityPoolShareChangeTrustAsset(assetA, assetB, 30);

        var comparison = wrapper.CompareTo(poolShare);
        Assert.AreEqual(-1, comparison);
    }

    [TestMethod]
    public void TestWrapperCompareTo_WithWrapper()
    {
        var keypair = KeyPair.Random();
        var asset1 = Asset.Create($"EUR:{keypair.AccountId}");
        var asset2 = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset1);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset2);

        var comparison = wrapper1.CompareTo(wrapper2);
        Assert.IsTrue(comparison != 0);
    }

    [TestMethod]
    public void TestWrapperGetHashCode()
    {
        var keypair = KeyPair.Random();
        var asset = Asset.Create($"USD:{keypair.AccountId}");
        var wrapper1 = new ChangeTrustAsset.Wrapper(asset);
        var wrapper2 = new ChangeTrustAsset.Wrapper(asset);
        Assert.AreEqual(wrapper1.GetHashCode(), wrapper2.GetHashCode());
    }

    [TestMethod]
    public void TestCreateWithTypeCodeIssuer_Native()
    {
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create("native", null, null);
        Assert.IsTrue(changeTrustAsset.Asset is AssetTypeNative);
        Assert.AreEqual("native", changeTrustAsset.Asset.CanonicalName());
    }

    [TestMethod]
    public void TestCreateWithTypeCodeIssuer_NonNative()
    {
        var keypair = KeyPair.Random();
        var changeTrustAsset = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.Create("non-native", "USD", keypair.AccountId);
        Assert.IsTrue(changeTrustAsset.Asset is AssetTypeCreditAlphaNum);
        Assert.AreEqual($"USD:{keypair.AccountId}", changeTrustAsset.Asset.CanonicalName());
    }

    [TestMethod]
    public void TestWrapperToXdr_Native()
    {
        var wrapper = new ChangeTrustAsset.Wrapper(new AssetTypeNative());
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeNative);
        Assert.AreEqual("native", roundTripWrapper.Asset.CanonicalName());
    }

    [TestMethod]
    public void TestWrapperToXdr_CreditAlphaNum4()
    {
        var keypair = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4("USD", keypair.AccountId);
        var wrapper = new ChangeTrustAsset.Wrapper(asset);
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeCreditAlphaNum4);
        Assert.AreEqual($"USD:{keypair.AccountId}", roundTripWrapper.Asset.CanonicalName());
    }

    [TestMethod]
    public void TestWrapperToXdr_CreditAlphaNum12()
    {
        var keypair = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12("TESTTEST", keypair.AccountId);
        var wrapper = new ChangeTrustAsset.Wrapper(asset);
        var xdr = wrapper.ToXdr();
        var roundTripWrapper = (ChangeTrustAsset.Wrapper)ChangeTrustAsset.FromXdr(xdr);
        
        Assert.IsTrue(roundTripWrapper.Asset is AssetTypeCreditAlphaNum12);
        Assert.AreEqual($"TESTTEST:{keypair.AccountId}", roundTripWrapper.Asset.CanonicalName());
    }
}