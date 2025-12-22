using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.Tests.Assets;

[TestClass]
public class AssetTest
{
    /// <summary>
    ///     Tests that native assets can be serialized to XDR and deserialized back correctly.
    ///     Verifies round-trip conversion for the native XLM asset type.
    /// </summary>
    [TestMethod]
    public void ToXdr_WithNativeAsset_RoundTripsCorrectly()
    {
        // Arrange
        var asset = new AssetTypeNative();

        // Act
        var thisXdr = asset.ToXdr();
        var parsedAsset = Asset.FromXdr(thisXdr);

        // Assert
        Assert.IsTrue(parsedAsset is AssetTypeNative);
    }

    /// <summary>
    ///     Tests the Asset.Create factory method with type, code, and issuer parameters.
    ///     Verifies that it correctly creates native assets and non-native credit assets.
    /// </summary>
    [TestMethod]
    public void Create_WithTypeCodeIssuer_CreatesCorrectAssetType()
    {
        // Arrange & Act
        var nativeAsset = Asset.Create("native", null, null);
        var nonNativeAsset = Asset.Create(null, "XLMTEST", "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        // Assert
        Assert.IsTrue(nativeAsset is AssetTypeNative);
        Assert.IsTrue(nonNativeAsset is AssetTypeCreditAlphaNum);
    }

    /// <summary>
    ///     Tests AssetTypeCreditAlphaNum4 creation and XDR round-trip conversion.
    ///     Verifies that asset codes up to 4 characters are correctly serialized and deserialized.
    /// </summary>
    [TestMethod]
    public void ToXdr_WithCreditAlphaNum4_RoundTripsCorrectly()
    {
        // Arrange
        var code = "USDA";
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum4(code, issuer.AccountId);

        // Act
        var thisXdr = asset.ToXdr();
        var parsedAsset = (AssetTypeCreditAlphaNum4)Asset.FromXdr(thisXdr);

        // Assert
        Assert.AreEqual(code, asset.Code);
        Assert.AreEqual(issuer.AccountId, parsedAsset.Issuer);
    }

    /// <summary>
    ///     Tests AssetTypeCreditAlphaNum12 creation and XDR round-trip conversion.
    ///     Verifies that asset codes between 5-12 characters are correctly serialized and deserialized.
    /// </summary>
    [TestMethod]
    public void ToXdr_WithCreditAlphaNum12_RoundTripsCorrectly()
    {
        // Arrange
        var code = "TESTTEST";
        var issuer = KeyPair.Random();
        var asset = new AssetTypeCreditAlphaNum12(code, issuer.AccountId);

        // Act
        var thisXdr = asset.ToXdr();
        var parsedAsset = (AssetTypeCreditAlphaNum12)Asset.FromXdr(thisXdr);

        // Assert
        Assert.AreEqual(code, asset.Code);
        Assert.AreEqual(issuer.AccountId, parsedAsset.Issuer);
        Assert.AreEqual(asset.Type, "credit_alphanum12");
    }

    /// <summary>
    ///     Tests GetHashCode implementation for all asset types.
    ///     Verifies that equal assets produce the same hash code, and different assets produce different hash codes.
    ///     This is critical for proper behavior in hash-based collections like Dictionary and HashSet.
    /// </summary>
    [TestMethod]
    public void GetHashCode_WithVariousAssets_ReturnsConsistentResults()
    {
        // Arrange
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;

        // Act & Assert - Equal cases
        Assert.AreEqual(new AssetTypeNative().GetHashCode(), new AssetTypeNative().GetHashCode());
        Assert.AreEqual(new AssetTypeCreditAlphaNum4("USD", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum4("USD", issuer1).GetHashCode());
        Assert.AreEqual(new AssetTypeCreditAlphaNum12("ABCDE", issuer1).GetHashCode(),
            new AssetTypeCreditAlphaNum12("ABCDE", issuer1).GetHashCode());

        // Act & Assert - Not equal cases
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

    /// <summary>
    ///     Tests the Equals implementation for all asset types.
    ///     Verifies that assets with the same code and issuer are equal, and assets with different codes or issuers are not
    ///     equal.
    ///     Also verifies that different asset types (native vs credit) are never equal.
    /// </summary>
    [TestMethod]
    public void Equals_WithVariousAssets_ReturnsCorrectResults()
    {
        // Arrange
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;

        // Act & Assert - Equal cases
        Assert.AreEqual(new AssetTypeNative(), new AssetTypeNative());
        Assert.AreEqual(new AssetTypeCreditAlphaNum4("USD", issuer1), new AssetTypeCreditAlphaNum4("USD", issuer1));
        Assert.AreEqual(new AssetTypeCreditAlphaNum12("ABCDE", issuer1),
            new AssetTypeCreditAlphaNum12("ABCDE", issuer1));

        // Act & Assert - Not equal cases
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

    /// <summary>
    ///     Tests that AssetTypeCreditAlphaNum.Equals returns false when comparing with incompatible types.
    ///     Verifies type safety and null handling in the Equals implementation.
    /// </summary>
    [TestMethod]
    public void Equals_WithIncompatibleType_ReturnsFalse()
    {
        // Arrange
        var issuer = KeyPair.Random().AccountId;
        var credit4 = new AssetTypeCreditAlphaNum4("USD", issuer);
        var native = new AssetTypeNative();

        // Act & Assert
        Assert.IsFalse(credit4.Equals(native));
        Assert.IsFalse(credit4.Equals(null!));
        Assert.IsFalse(credit4.Equals("not an asset"));
    }

    /// <summary>
    ///     Tests that Asset.Create throws ArgumentException when canonical form is missing the colon separator.
    ///     Validates input validation for malformed canonical asset strings (format: "CODE:ISSUER").
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithCanonicalFormMissingColon_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Asset.Create("USD");
    }

    /// <summary>
    ///     Tests that Asset.Create throws ArgumentException when canonical form has too many parts.
    ///     Validates that only two parts (code and issuer) separated by a single colon are accepted.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithCanonicalFormTooManyParts_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Asset.Create("USD:ISSUER:EXTRA");
    }

    /// <summary>
    ///     Tests that Asset.Create correctly creates a native asset from the canonical form "native".
    ///     Verifies the special case handling for the native XLM asset.
    /// </summary>
    [TestMethod]
    public void Create_WithCanonicalFormNative_ReturnsNativeAsset()
    {
        // Arrange & Act
        var asset = Asset.Create("native");

        // Assert
        Assert.IsTrue(asset is AssetTypeNative);
    }

    /// <summary>
    ///     Tests that Asset.Create throws ArgumentNullException when code is null for non-native assets.
    ///     Validates that non-native assets require both code and issuer to be provided.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Create_WithNullCode_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Asset.Create("non-native", null, "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");
    }

    /// <summary>
    ///     Tests that Asset.Create throws ArgumentNullException when issuer is null for non-native assets.
    ///     Validates that credit assets require an issuer account ID.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Create_WithNullIssuer_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Asset.Create("non-native", "USD", null);
    }

    /// <summary>
    ///     Tests that CreateNonNativeAsset throws AssetCodeLengthInvalidException for empty asset codes.
    ///     Validates that asset codes must be between 1-12 characters in length.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void CreateNonNativeAsset_WithEmptyCode_ThrowsAssetCodeLengthInvalidException()
    {
        // Arrange & Act & Assert
        Asset.CreateNonNativeAsset("", "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");
    }

    /// <summary>
    ///     Tests that CreateNonNativeAsset throws AssetCodeLengthInvalidException for asset codes exceeding 12 characters.
    ///     Validates the maximum length constraint for asset codes.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void CreateNonNativeAsset_WithCodeTooLong_ThrowsAssetCodeLengthInvalidException()
    {
        // Arrange & Act & Assert
        Asset.CreateNonNativeAsset("THISCODEISTOOLONG", "GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");
    }

    /// <summary>
    ///     Tests that AssetTypeCreditAlphaNum4 constructor throws AssetCodeLengthInvalidException for empty codes.
    ///     Validates that CreditAlphaNum4 requires codes between 1-4 characters.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void AssetTypeCreditAlphaNum4_WithCodeTooShort_ThrowsAssetCodeLengthInvalidException()
    {
        // Arrange & Act & Assert
        _ = new AssetTypeCreditAlphaNum4("", KeyPair.Random().AccountId);
    }

    /// <summary>
    ///     Tests that AssetTypeCreditAlphaNum4 constructor throws AssetCodeLengthInvalidException for codes exceeding 4
    ///     characters.
    ///     Validates the maximum length constraint for CreditAlphaNum4 asset codes.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void AssetTypeCreditAlphaNum4_WithCodeTooLong_ThrowsAssetCodeLengthInvalidException()
    {
        // Arrange & Act & Assert
        _ = new AssetTypeCreditAlphaNum4("FIVES", KeyPair.Random().AccountId);
    }

    /// <summary>
    ///     Tests that AssetTypeCreditAlphaNum12 constructor throws AssetCodeLengthInvalidException for codes shorter than 5
    ///     characters.
    ///     Validates that CreditAlphaNum12 requires codes between 5-12 characters (shorter codes should use CreditAlphaNum4).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void AssetTypeCreditAlphaNum12_WithCodeTooShort_ThrowsAssetCodeLengthInvalidException()
    {
        // Arrange & Act & Assert
        _ = new AssetTypeCreditAlphaNum12("FOUR", KeyPair.Random().AccountId);
    }

    /// <summary>
    ///     Tests that AssetTypeCreditAlphaNum12 constructor throws AssetCodeLengthInvalidException for codes exceeding 12
    ///     characters.
    ///     Validates the maximum length constraint for CreditAlphaNum12 asset codes.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(AssetCodeLengthInvalidException))]
    public void AssetTypeCreditAlphaNum12_WithCodeTooLong_ThrowsAssetCodeLengthInvalidException()
    {
        // Arrange & Act & Assert
        _ = new AssetTypeCreditAlphaNum12("THISCODEISTOOLONG", KeyPair.Random().AccountId);
    }

    /// <summary>
    ///     Tests that Asset.FromXdr throws ArgumentException when encountering an unknown asset type in XDR.
    ///     Validates error handling for malformed or future XDR data with unrecognized asset type discriminants.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void FromXdr_WithUnknownAssetType_ThrowsArgumentException()
    {
        // Arrange
        var xdrAsset = new StellarDotnetSdk.Xdr.Asset
        {
            Discriminant = new AssetType { InnerValue = (AssetType.AssetTypeEnum)999 },
        };

        // Act & Assert
        Asset.FromXdr(xdrAsset);
    }

    /// <summary>
    ///     Tests that native assets compare as equal (CompareTo returns 0).
    ///     Verifies that all native assets are considered equivalent for ordering purposes.
    /// </summary>
    [TestMethod]
    public void CompareTo_NativeVsNative_ReturnsZero()
    {
        // Arrange
        var native1 = new AssetTypeNative();
        var native2 = new AssetTypeNative();

        // Act
        var result = native1.CompareTo(native2);

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    ///     Tests comparison ordering between native assets and CreditAlphaNum4 assets.
    ///     Verifies that native assets sort before credit assets (native returns -1, credit returns 1).
    /// </summary>
    [TestMethod]
    public void CompareTo_NativeVsCreditAlphaNum4_ReturnsCorrectOrder()
    {
        // Arrange
        var native = new AssetTypeNative();
        var credit = new AssetTypeCreditAlphaNum4("USD", KeyPair.Random().AccountId);

        // Act & Assert
        Assert.AreEqual(-1, native.CompareTo(credit));
        Assert.AreEqual(1, credit.CompareTo(native));
    }

    /// <summary>
    ///     Tests comparison ordering between native assets and CreditAlphaNum12 assets.
    ///     Verifies that native assets sort before credit assets regardless of code length.
    /// </summary>
    [TestMethod]
    public void CompareTo_NativeVsCreditAlphaNum12_ReturnsCorrectOrder()
    {
        // Arrange
        var native = new AssetTypeNative();
        var credit = new AssetTypeCreditAlphaNum12("TESTTEST", KeyPair.Random().AccountId);

        // Act & Assert
        Assert.AreEqual(-1, native.CompareTo(credit));
        Assert.AreEqual(1, credit.CompareTo(native));
    }

    /// <summary>
    ///     Tests comparison ordering between CreditAlphaNum4 and CreditAlphaNum12 assets.
    ///     Verifies that CreditAlphaNum4 assets sort before CreditAlphaNum12 assets (4-character codes before 5-12 character
    ///     codes).
    /// </summary>
    [TestMethod]
    public void CompareTo_CreditAlphaNum4VsCreditAlphaNum12_ReturnsCorrectOrder()
    {
        // Arrange
        var issuer = KeyPair.Random().AccountId;
        var credit4 = new AssetTypeCreditAlphaNum4("USD", issuer);
        var credit12 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer);

        // Act & Assert
        Assert.AreEqual(-1, credit4.CompareTo(credit12));
        Assert.AreEqual(1, credit12.CompareTo(credit4));
    }

    /// <summary>
    ///     Tests that assets with the same code but different issuers are not equal.
    ///     Verifies that issuer is part of the comparison logic (same code from different issuers are different assets).
    /// </summary>
    [TestMethod]
    public void CompareTo_CreditAlphaNum4WithSameCodeDifferentIssuer_ReturnsNonZero()
    {
        // Arrange
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum4("USD", issuer1);
        var credit2 = new AssetTypeCreditAlphaNum4("USD", issuer2);

        // Act
        var comparison = credit1.CompareTo(credit2);

        // Assert
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    ///     Tests that assets with different codes but the same issuer are not equal.
    ///     Verifies that asset code is part of the comparison logic.
    /// </summary>
    [TestMethod]
    public void CompareTo_CreditAlphaNum4WithDifferentCode_ReturnsNonZero()
    {
        // Arrange
        var issuer = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum4("EUR", issuer);
        var credit2 = new AssetTypeCreditAlphaNum4("USD", issuer);

        // Act
        var comparison = credit1.CompareTo(credit2);

        // Assert
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    ///     Tests that CreditAlphaNum12 assets with the same code but different issuers are not equal.
    ///     Verifies that issuer is part of the comparison logic for longer asset codes.
    /// </summary>
    [TestMethod]
    public void CompareTo_CreditAlphaNum12WithSameCodeDifferentIssuer_ReturnsNonZero()
    {
        // Arrange
        var issuer1 = KeyPair.Random().AccountId;
        var issuer2 = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer1);
        var credit2 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer2);

        // Act
        var comparison = credit1.CompareTo(credit2);

        // Assert
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    ///     Tests that CreditAlphaNum12 assets with different codes but the same issuer are not equal.
    ///     Verifies that asset code is part of the comparison logic for longer codes.
    /// </summary>
    [TestMethod]
    public void CompareTo_CreditAlphaNum12WithDifferentCode_ReturnsNonZero()
    {
        // Arrange
        var issuer = KeyPair.Random().AccountId;
        var credit1 = new AssetTypeCreditAlphaNum12("EUROPEAN", issuer);
        var credit2 = new AssetTypeCreditAlphaNum12("TESTTEST", issuer);

        // Act
        var comparison = credit1.CompareTo(credit2);

        // Assert
        Assert.IsTrue(comparison != 0);
    }

    /// <summary>
    ///     Tests ToQueryParameterEncodedString extension method for native assets.
    ///     Verifies that native assets are encoded as "native" for use in URL query parameters.
    /// </summary>
    [TestMethod]
    public void ToQueryParameterEncodedString_WithNativeAsset_ReturnsNative()
    {
        // Arrange
        var nativeAsset = new AssetTypeNative();

        // Act
        var result = nativeAsset.ToQueryParameterEncodedString();

        // Assert
        Assert.AreEqual("native", result);
    }

    /// <summary>
    ///     Tests ToQueryParameterEncodedString extension method for CreditAlphaNum4 assets.
    ///     Verifies that credit assets are encoded as "CODE:ISSUER" format for use in URL query parameters.
    /// </summary>
    [TestMethod]
    public void ToQueryParameterEncodedString_WithCreditAlphaNum4_ReturnsCodeColonIssuer()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var code = "USD";
        var asset = new AssetTypeCreditAlphaNum4(code, issuer.AccountId);

        // Act
        var result = asset.ToQueryParameterEncodedString();

        // Assert
        Assert.AreEqual($"{code}:{issuer.AccountId}", result);
    }

    /// <summary>
    ///     Tests ToQueryParameterEncodedString extension method for CreditAlphaNum12 assets.
    ///     Verifies that longer credit asset codes are also encoded as "CODE:ISSUER" format.
    /// </summary>
    [TestMethod]
    public void ToQueryParameterEncodedString_WithCreditAlphaNum12_ReturnsCodeColonIssuer()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var code = "TESTTEST";
        var asset = new AssetTypeCreditAlphaNum12(code, issuer.AccountId);

        // Act
        var result = asset.ToQueryParameterEncodedString();

        // Assert
        Assert.AreEqual($"{code}:{issuer.AccountId}", result);
    }
}