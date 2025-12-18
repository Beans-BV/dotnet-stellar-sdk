using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

/// <summary>
/// Unit tests for <see cref="ClaimableBalanceIdUtils"/> class.
/// </summary>
[TestClass]
public class ClaimableBalanceIdUtilsTest
{
    /// <summary>
    /// Verifies that ClaimableBalanceIdUtils.FromXdr converts XDR claimable balance ID to base32 string.
    /// </summary>
    [TestMethod]
    [DataRow("299a32106238f3b2d84d4142783fe320253bcda775d1bfb7accdb533021ddccf",
        "BAACTGRSCBRDR45S3BGUCQTYH7RSAJJ3ZWTXLUN7W6WM3NJTAIO5ZT2U6I")]
    public void FromXdr_WithClaimableBalanceId_ReturnsBase32String(string hex, string base32)
    {
        // Arrange
        var xdrClaimableBalanceId = new ClaimableBalanceID
        {
            Discriminant =
                ClaimableBalanceIDType.Create(ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum
                    .CLAIMABLE_BALANCE_ID_TYPE_V0),
            V0 = new Hash(Convert.FromHexString(hex)),
        };

        // Act
        var id = ClaimableBalanceIdUtils.FromXdr(xdrClaimableBalanceId);

        // Assert
        Assert.AreEqual(id, base32);
    }

    /// <summary>
    /// Verifies that ClaimableBalanceIdUtils.ToHexString converts base32 string to hex string.
    /// </summary>
    [TestMethod]
    [DataRow("BAACTGRSCBRDR45S3BGUCQTYH7RSAJJ3ZWTXLUN7W6WM3NJTAIO5ZT2U6I",
        "00000000299a32106238f3b2d84d4142783fe320253bcda775d1bfb7accdb533021ddccf")]
    public void ToHexString_WithBase32String_ReturnsHexString(string input, string output)
    {
        // Arrange
        // Base32 string and expected hex string provided via DataRow attribute

        // Act
        var hex = ClaimableBalanceIdUtils.ToHexString(input).ToLower();

        // Assert
        Assert.AreEqual(output, hex);
    }

    /// <summary>
    /// Verifies that ClaimableBalanceIdUtils.ToBase32String converts hex string to base32 string.
    /// </summary>
    [TestMethod]
    [DataRow("00000000299a32106238f3b2d84d4142783fe320253bcda775d1bfb7accdb533021ddccf",
        "BAACTGRSCBRDR45S3BGUCQTYH7RSAJJ3ZWTXLUN7W6WM3NJTAIO5ZT2U6I")]
    public void ToBase32String_WithHexString_ReturnsBase32String(string input, string output)
    {
        // Arrange
        // Hex string and expected base32 string provided via DataRow attribute

        // Act
        var base32 = ClaimableBalanceIdUtils.ToBase32String(input);

        // Assert
        Assert.AreEqual(output, base32);
    }
}