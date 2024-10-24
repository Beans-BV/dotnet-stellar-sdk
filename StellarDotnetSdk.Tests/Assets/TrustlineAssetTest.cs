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
}