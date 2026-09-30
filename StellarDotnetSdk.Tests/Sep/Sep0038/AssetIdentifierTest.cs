using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Sep.Sep0038;
using StellarDotnetSdk.Sep.Sep0038.Requests;

namespace StellarDotnetSdk.Tests.Sep.Sep0038;

/// <summary>
///     Unit tests for <see cref="AssetIdentifier" />, the SEP-38 Asset Identification Format.
/// </summary>
[TestClass]
public class AssetIdentifierTest
{
    private const string Issuer = "GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN";

    [TestMethod]
    public void Stellar_BuildsTheSep11Form()
    {
        var identifier = AssetIdentifier.Stellar("USDC", Issuer);

        Assert.AreEqual($"stellar:USDC:{Issuer}", identifier.ToString());
        Assert.AreEqual(AssetIdentifier.StellarScheme, identifier.Scheme);
        Assert.AreEqual("USDC", identifier.Code);
        Assert.AreEqual(Issuer, identifier.Issuer);
        Assert.IsTrue(identifier.IsStellar);
        Assert.IsFalse(identifier.IsNative);
        Assert.IsFalse(identifier.IsIso4217);
    }

    [TestMethod]
    public void StellarNative_BuildsStellarNative()
    {
        var identifier = AssetIdentifier.StellarNative();

        Assert.AreEqual("stellar:native", identifier.ToString());
        Assert.IsTrue(identifier.IsStellar);
        Assert.IsTrue(identifier.IsNative);
        Assert.IsNull(identifier.Issuer);
    }

    [TestMethod]
    public void Iso4217_BuildsTheFiatForm()
    {
        var identifier = AssetIdentifier.Iso4217("USD");

        Assert.AreEqual("iso4217:USD", identifier.ToString());
        Assert.AreEqual(AssetIdentifier.Iso4217Scheme, identifier.Scheme);
        Assert.AreEqual("USD", identifier.Code);
        Assert.IsNull(identifier.Issuer);
        Assert.IsTrue(identifier.IsIso4217);
        Assert.IsFalse(identifier.IsStellar);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("TOOLONGCODE12X")]
    [DataRow("US-D")]
    [DataRow("USDÇ")]
    [DataRow(null)]
    public void Stellar_WithInvalidCode_Throws(string? code)
    {
        var ex = Assert.ThrowsException<ArgumentException>(() => AssetIdentifier.Stellar(code!, Issuer));
        Assert.AreEqual("code", ex.ParamName);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("GBAD")]
    [DataRow("SBGWSG6BTNCKCOB3DIFBGCVMUPQFYPA2G4O34RMTB343OYPXU5DJDVMN")]
    [DataRow(null)]
    public void Stellar_WithInvalidIssuer_Throws(string? issuer)
    {
        var ex = Assert.ThrowsException<ArgumentException>(() => AssetIdentifier.Stellar("USDC", issuer!));
        Assert.AreEqual("issuer", ex.ParamName);
    }

    [TestMethod]
    [DataRow("usd")]
    [DataRow("US")]
    [DataRow("USDC")]
    [DataRow("U5D")]
    [DataRow(null)]
    public void Iso4217_WithInvalidCode_Throws(string? code)
    {
        Assert.ThrowsException<ArgumentException>(() => AssetIdentifier.Iso4217(code!));
    }

    [TestMethod]
    public void FromAsset_ConvertsNativeAndCreditAssets()
    {
        Assert.AreEqual("stellar:native", AssetIdentifier.FromAsset(new AssetTypeNative()).ToString());
        Assert.AreEqual($"stellar:USDC:{Issuer}",
            AssetIdentifier.FromAsset(Asset.CreateNonNativeAsset("USDC", Issuer)).ToString());
        Assert.AreEqual($"stellar:LONGASSET12:{Issuer}",
            AssetIdentifier.FromAsset(Asset.CreateNonNativeAsset("LONGASSET12", Issuer)).ToString());
    }

    [TestMethod]
    public void FromAsset_WithNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => AssetIdentifier.FromAsset(null!));
    }

    [TestMethod]
    [DataRow("stellar:native")]
    [DataRow("stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN")]
    [DataRow("stellar:LONGASSET12:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN")]
    [DataRow("stellar:native:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN")]
    [DataRow("iso4217:USD")]
    [DataRow("iso4217:BRL")]
    public void Parse_RoundTrips(string value)
    {
        var identifier = AssetIdentifier.Parse(value);

        Assert.AreEqual(value, identifier.ToString());
        Assert.IsTrue(AssetIdentifier.TryParse(value, out var parsed));
        Assert.AreEqual(identifier, parsed);
    }

    [TestMethod]
    public void Parse_CreditAssetCodedNative_IsNotLumens()
    {
        var identifier = AssetIdentifier.Parse($"stellar:native:{Issuer}");

        Assert.IsFalse(identifier.IsNative);
        Assert.AreEqual("native", identifier.Code);
        Assert.AreEqual(Issuer, identifier.Issuer);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("USD")]
    [DataRow("iso4217:")]
    [DataRow("iso4217:usd")]
    [DataRow("iso4217:USDX")]
    [DataRow("ISO4217:USD")]
    [DataRow("stellar:")]
    [DataRow("stellar:USDC")]
    [DataRow("stellar:USDC:")]
    [DataRow("stellar:USDC:GBAD")]
    [DataRow("stellar::GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN")]
    [DataRow("stellar:TOOLONGCODE12X:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN")]
    [DataRow("stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN:extra")]
    [DataRow("stellar:NATIVE")]
    [DataRow("Stellar:native")]
    [DataRow("stellar:native ")]
    [DataRow("crypto:BTC")]
    public void Parse_WithInvalidValue_ThrowsFormatException(string value)
    {
        Assert.ThrowsException<FormatException>(() => AssetIdentifier.Parse(value));
        Assert.IsFalse(AssetIdentifier.TryParse(value, out var result));
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => AssetIdentifier.Parse(null!));
        Assert.IsFalse(AssetIdentifier.TryParse(null, out _));
    }

    [TestMethod]
    public void ToAsset_ReturnsTheSdkAsset()
    {
        Assert.IsInstanceOfType(AssetIdentifier.StellarNative().ToAsset(), typeof(AssetTypeNative));
        var credit = (AssetTypeCreditAlphaNum)AssetIdentifier.Stellar("USDC", Issuer).ToAsset();
        Assert.AreEqual("USDC", credit.Code);
        Assert.AreEqual(Issuer, credit.Issuer);
    }

    [TestMethod]
    public void ToAsset_OnAFiatCurrency_ThrowsInvalidOperationException()
    {
        Assert.ThrowsException<InvalidOperationException>(() => AssetIdentifier.Iso4217("USD").ToAsset());
    }

    [TestMethod]
    public void ImplicitConversion_AssignsTheWireFormToARequest()
    {
        var request = new PriceRequest
        {
            Context = QuoteContext.Sep6,
            SellAsset = AssetIdentifier.Iso4217("BRL"),
            BuyAsset = AssetIdentifier.Stellar("USDC", Issuer),
            SellAmount = 1m,
        };

        Assert.AreEqual("iso4217:BRL", request.SellAsset);
        Assert.AreEqual($"stellar:USDC:{Issuer}", request.BuyAsset);
    }

    [TestMethod]
    public void ImplicitConversion_OfNull_ThrowsArgumentNullException()
    {
        AssetIdentifier? identifier = null;
        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            string value = identifier!;
            return value;
        });
    }

    [TestMethod]
    public void Equality_IsByValue()
    {
        Assert.AreEqual(AssetIdentifier.Iso4217("USD"), AssetIdentifier.Parse("iso4217:USD"));
        Assert.AreEqual(AssetIdentifier.Iso4217("USD").GetHashCode(), AssetIdentifier.Parse("iso4217:USD").GetHashCode());
        Assert.AreNotEqual(AssetIdentifier.Iso4217("USD"), AssetIdentifier.Iso4217("EUR"));
        Assert.AreNotEqual(AssetIdentifier.StellarNative(), AssetIdentifier.Stellar("native", Issuer));
        Assert.IsFalse(AssetIdentifier.Iso4217("USD").Equals(null));
    }

    [TestMethod]
    public void EqualityOperators_AgreeWithEquals()
    {
        var usd = AssetIdentifier.Iso4217("USD");
        var sameUsd = AssetIdentifier.Parse("iso4217:USD");
        var eur = AssetIdentifier.Iso4217("EUR");
        AssetIdentifier? none = null;

        Assert.IsTrue(usd == sameUsd);
        Assert.IsFalse(usd != sameUsd);
        Assert.IsFalse(usd == eur);
        Assert.IsTrue(usd != eur);
        Assert.IsFalse(usd == none);
        Assert.IsFalse(none == usd);
        Assert.IsTrue(none == null);
        // Mixed comparisons resolve through the implicit string conversion, by value; pinned so that adding a
        // string-to-identifier conversion later cannot silently flip them.
        Assert.IsTrue(usd == "iso4217:USD");
        Assert.IsFalse(usd == "iso4217:EUR");
    }
}
