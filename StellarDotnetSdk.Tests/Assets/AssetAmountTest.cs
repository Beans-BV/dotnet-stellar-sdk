using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class AssetAmountTest
{
    [TestMethod]
    public void TestCreation()
    {
        var issuer = KeyPair.Random();

        var assetAmount = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");

        Assert.AreEqual(assetAmount.Asset.CanonicalName(), $"TEST:{issuer.AccountId}");
        Assert.AreEqual(assetAmount.Amount, "100");
    }

    [TestMethod]
    public void TestEquality()
    {
        var issuer = KeyPair.Random();

        var assetAmount = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");
        var assetAmount2 = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");

        Assert.AreEqual(assetAmount, assetAmount2);
        Assert.AreEqual(assetAmount.GetHashCode(), assetAmount2.GetHashCode());
    }

    [TestMethod]
    public void TestEquality_NonMatchingType()
    {
        var issuer = KeyPair.Random();
        var assetAmount = new AssetAmount(Asset.CreateNonNativeAsset("TEST", issuer.AccountId), "100");

        Assert.IsFalse(assetAmount.Equals(null));
        Assert.IsFalse(assetAmount.Equals("not an AssetAmount"));
        Assert.IsFalse(assetAmount.Equals(new object()));
    }
}