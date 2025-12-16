using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;
using XDR = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class AssetTest
{
    [TestMethod]
    public void TestAssetTypeNative()
    {
        var asset = new AssetTypeNative();
        var thisXdr = asset.ToXdr();
        var parsedAsset = Asset.FromXdr(thisXdr);
        Assert.IsTrue(parsedAsset is AssetTypeNative);
    }

    [TestMethod]
    public void TestAssetCreation()
    {
        var nativeAsset = Asset.Create("native", null, null);
        Assert.IsTrue(nativeAsset is AssetTypeNative);

        var nonNativeAsset = Asset.Create(null, "XLMTEST", "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");
        Assert.IsTrue(nonNativeAsset is AssetTypeCreditAlphaNum);
    }

    [TestMethod]
    public void TestAssetTypeCreditAlphaNum4()
    {
        var code = "USDA";
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4(code, issuer.AccountId);
        var thisXdr = asset.ToXdr();
        var parsedAsset = (AssetTypeCreditAlphaNum4)Asset.FromXdr(thisXdr);
        Assert.AreEqual(code, asset.Code);
        Assert.AreEqual(issuer.AccountId, parsedAsset.Issuer);
    }

    [TestMethod]
    public void TestAssetTypeCreditAlphaNum12()
    {
        var code = "TESTTEST";
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12(code, issuer.AccountId);
        var thisXdr = asset.ToXdr();
        var parsedAsset = (AssetTypeCreditAlphaNum12)Asset.FromXdr(thisXdr);
        Assert.AreEqual(code, asset.Code);
        Assert.AreEqual(issuer.AccountId, parsedAsset.Issuer);
        Assert.AreEqual(asset.Type, "credit_alphanum12");
    }

    [TestMethod]
    public void TestHashCode()
    {
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;

        // Equal
        Assert.AreEqual(new AssetTypeNative().GetHashCode(), new AssetTypeNative().GetHashCode());
        Assert.AreEqual(new AssetTypeCreditAlphaNum4("USD", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum4("USD", issuer1).GetHashCode());
        Assert.AreEqual(new AssetTypeCreditAlphaNum12("ABCDE", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum12("ABCDE", issuer1).GetHashCode());

        // Not equal
        Assert.AreNotEqual(new AssetTypeNative().GetHashCode(),
            new AssetTypeCreditAlphaNum4("USD", issuer1).GetHashCode());
        Assert.AreNotEqual(new AssetTypeNative().GetHashCode(),
            new AssetTypeCreditAlphaNum12("ABCDE", issuer1).GetHashCode());
        Assert.AreNotEqual(new AssetTypeCreditAlphaNum4("EUR", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum4("USD", issuer1).GetHashCode());
        Assert.AreNotEqual(new AssetTypeCreditAlphaNum4("EUR", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum4("EUR", issuer2).GetHashCode());
        Assert.AreNotEqual(new AssetTypeCreditAlphaNum4("EUR", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum12("EUROPE", issuer1).GetHashCode());
        Assert.AreNotEqual(new AssetTypeCreditAlphaNum4("EUR", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum12("EUROPE", issuer2).GetHashCode());
        Assert.AreNotEqual(new AssetTypeCreditAlphaNum12("ABCDE", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum12("EDCBA", issuer1).GetHashCode());
        Assert.AreNotEqual(new AssetTypeCreditAlphaNum12("ABCDE", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum12("ABCDE", issuer2).GetHashCode());
    }

    [TestMethod]
    public void TestAssetEquals()
    {
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;

        Assert.AreEqual(new AssetTypeNative(), new AssetTypeNative());
        Assert.AreEqual(new AssetTypeCreditAlphaNum4("USD", issuer1), new AssetTypeCreditAlphaNum4("USD", issuer1));
        Assert.AreEqual(new AssetTypeCreditAlphaNum12("ABCDE", issuer1),
            new AssetTypeCreditAlphaNum12("ABCDE", issuer1));

        Assert.AreNotEqual(new AssetTypeNative(), new AssetTypeCreditAlphaNum4("USD", issuer1));
        Assert.AreNotEqual(new AssetTypeNative(), new AssetTypeCreditAlphaNum12("ABCDE", issuer1));
        Assert.IsFalse(
            new AssetTypeCreditAlphaNum4("EUR", issuer1).Equals(new AssetTypeCreditAlphaNum4("USD", issuer1)));
        Assert.IsFalse(
            new AssetTypeCreditAlphaNum4("EUR", issuer1).Equals(new AssetTypeCreditAlphaNum4("EUR", issuer2)));
        Assert.IsFalse(
            new AssetTypeCreditAlphaNum12("ABCDE", issuer1).Equals(new AssetTypeCreditAlphaNum12("EDCBA", issuer1)));
        Assert.IsFalse(
            new AssetTypeCreditAlphaNum12("ABCDE", issuer1).Equals(new AssetTypeCreditAlphaNum12("ABCDE", issuer2)));
    }

    [TestMethod]
    public void TestAssetTypeCreditAlphaNum_Equals_NonMatchingType()
    {
        var issuer = KeyPair.Random().AccountId;
        var credit4 = new AssetTypeCreditAlphaNum4("USD", issuer);
        var native = new AssetTypeNative();

        Assert.IsFalse(credit4.Equals(native));
        Assert.IsFalse(credit4.Equals(null));
        Assert.IsFalse(credit4.Equals("not an asset"));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestCreateCanonicalFormInvalidFormat_MissingColon()
    {
        Asset.Create("USD");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestCreateCanonicalFormInvalidFormat_TooManyParts()
    {
        Asset.Create("USD:ISSUER:EXTRA");
    }

    [TestMethod]
    public void TestCreateCanonicalForm_Native()
    {
        var asset = Asset.Create("native");
        Assert.IsTrue(asset is AssetTypeNative);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestCreateWithNullCode()
    {
        Asset.Create("non-native", null, "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestCreateWithNullIssuer()
    {
        Asset.Create("non-native", "USD", null);
    }

    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void TestCreateNonNativeAsset_EmptyCode()
    {
        Asset.CreateNonNativeAsset("", "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");
    }

    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void TestCreateNonNativeAsset_CodeTooLong()
    {
        Asset.CreateNonNativeAsset("THISCODEISTOOLONG", "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");
    }

    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void TestAssetTypeCreditAlphaNum4_CodeTooShort()
    {
        new AssetTypeCreditAlphaNum4("", KeyPair.Random().AccountId);
    }

    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void TestAssetTypeCreditAlphaNum4_CodeTooLong()
    {
        new AssetTypeCreditAlphaNum4("FIVES", KeyPair.Random().AccountId);
    }

    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void TestAssetTypeCreditAlphaNum12_CodeTooShort()
    {
        new AssetTypeCreditAlphaNum12("FOUR", KeyPair.Random().AccountId);
    }

    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void TestAssetTypeCreditAlphaNum12_CodeTooLong()
    {
        new AssetTypeCreditAlphaNum12("THISCODEISTOOLONG", KeyPair.Random().AccountId);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestFromXdr_UnknownAssetType()
    {
        var xdrAsset = new StellarDotnetSdk.Xdr.Asset
        {
            Discriminant = new StellarDotnetSdk.Xdr.AssetType { InnerValue = (StellarDotnetSdk.Xdr.AssetType.AssetTypeEnum)999 }
        };
        Asset.FromXdr(xdrAsset);
    }

    [TestMethod]
    public void TestCompareTo_NativeVsNative()
    {
        var native1 = new AssetTypeNative();
        var native2 = new AssetTypeNative();
        Assert.AreEqual(0, native1.CompareTo(native2));
    }

    [TestMethod]
    public void TestCompareTo_NativeVsCreditAlphaNum4()
    {
        var native = new AssetTypeNative();
        var credit = new AssetTypeCreditAlphaNum4("USD", KeyPair.Random().AccountId);
        Assert.AreEqual(-1, native.CompareTo(credit));
        Assert.AreEqual(1, credit.CompareTo(native));
    }

    [TestMethod]
    public void TestCompareTo_NativeVsCreditAlphaNum12()
    {
        var native = new AssetTypeNative();
        var credit = new AssetTypeCreditAlphaNum12("TESTTEST", KeyPair.Random().AccountId);
        Assert.AreEqual(-1, native.CompareTo(credit));
        Assert.AreEqual(1, credit.CompareTo(native));
    }

    [TestMethod]
    public void TestCompareTo_CreditAlphaNum4VsCreditAlphaNum12()
    {
        var issuer = KeyPair.Random().AccountId;
        var credit4 = new AssetTypeCreditAlphaNum4("USD", issuer);
        var credit12 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer);
        Assert.AreEqual(-1, credit4.CompareTo(credit12));
        Assert.AreEqual(1, credit12.CompareTo(credit4));
    }

    [TestMethod]
    public void TestCompareTo_CreditAlphaNum4_SameCodeDifferentIssuer()
    {
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum4("USD", issuer1);
        var credit2 = new AssetTypeCreditAlphaNum4("USD", issuer2);
        var comparison = credit1.CompareTo(credit2);
        Assert.IsTrue(comparison != 0);
    }

    [TestMethod]
    public void TestCompareTo_CreditAlphaNum4_DifferentCode()
    {
        var issuer = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum4("EUR", issuer);
        var credit2 = new AssetTypeCreditAlphaNum4("USD", issuer);
        var comparison = credit1.CompareTo(credit2);
        Assert.IsTrue(comparison != 0);
    }

    [TestMethod]
    public void TestCompareTo_CreditAlphaNum12_SameCodeDifferentIssuer()
    {
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer1);
        var credit2 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer2);
        var comparison = credit1.CompareTo(credit2);
        Assert.IsTrue(comparison != 0);
    }

    [TestMethod]
    public void TestCompareTo_CreditAlphaNum12_DifferentCode()
    {
        var issuer = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum12("EUROPEAN", issuer);
        var credit2 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer);
        var comparison = credit1.CompareTo(credit2);
        Assert.IsTrue(comparison != 0);
    }

    [TestMethod]
    public void TestToQueryParameterEncodedString_Native()
    {
        var nativeAsset = new AssetTypeNative();
        var result = nativeAsset.ToQueryParameterEncodedString();
        Assert.AreEqual("native", result);
    }

    [TestMethod]
    public void TestToQueryParameterEncodedString_CreditAlphaNum4()
    {
        var issuer = KeyPair.Random();
        var code = "USD";
        var asset = new AssetTypeCreditAlphaNum4(code, issuer.AccountId);
        var result = asset.ToQueryParameterEncodedString();
        Assert.AreEqual($"{code}:{issuer.AccountId}", result);
    }

    [TestMethod]
    public void TestToQueryParameterEncodedString_CreditAlphaNum12()
    {
        var issuer = KeyPair.Random();
        var code = "TESTTEST";
        var asset = new AssetTypeCreditAlphaNum12(code, issuer.AccountId);
        var result = asset.ToQueryParameterEncodedString();
        Assert.AreEqual($"{code}:{issuer.AccountId}", result);
    }
}