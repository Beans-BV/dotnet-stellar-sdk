using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Soroban;
using SCVal = StellarDotnetSdk.Soroban.SCVal;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for Soroban address types (ScAccountId, ScContractId, ScLiquidityPoolId, ScClaimableBalanceId,
///     ScMuxedAccountId).
/// </summary>
[TestClass]
public class AddressTest
{
    /// <summary>
    ///     Verifies that ScAccountId constructor throws ArgumentException when account ID is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("Invalid id")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ",
        DisplayName = "Valid muxed account ID")]
    [DataRow("LAAABKAZRNPCLGKMDSS3AVLPV2ZHGJNMORRJNFCBITQKOQDNKAPIUSGG", DisplayName = "Valid liquidity pool ID")]
    [DataRow("GC5ZDZL56FJT75QOWB4OH7KBPBFBBYIJCDNTUNTB3QRFV4Z2IVUE6ES", DisplayName = "Invalid account ID")]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithInvalidAccountId_ThrowsArgumentException(string id)
    {
        // Act & Assert
        _ = new ScAccountId(id);
    }

    /// <summary>
    ///     Verifies that ScContractId constructor throws ArgumentException when contract ID is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("Invalid id")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ",
        DisplayName = "Valid muxed account ID")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q", DisplayName = "Valid account ID")]
    [DataRow("BAAKEZMZYI3VFSJMHZ2PMC3SBANQWP2LDKBVH42XQSHWTSBOH7TDOPHPFA", DisplayName = "Valid claimable balance ID")]
    [DataRow("LAAABKAZRNPCLGKMDSS3AVLPV2ZHGJNMORRJNFCBITQKOQDNKAPIUSGG", DisplayName = "Valid liquidity pool ID")]
    [DataRow("CBFZDBUL3OC2VRMCFKTV4G6H5R5FNRLDVCIE536CRWWF4RKXWGTSSLD", DisplayName = "Invalid contract ID")]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_WithInvalidContractId_ThrowsArgumentException(string id)
    {
        // Act & Assert
        _ = new ScContractId(id);
    }


    /// <summary>
    ///     Verifies that ScLiquidityPoolId constructor throws ArgumentException when liquidity pool ID is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [DataRow("Invalid id")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ",
        DisplayName = "Valid muxed account ID")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q", DisplayName = "Valid account ID")]
    [DataRow("0000a8198b5e25994c1ca5b0556faeb27325ac746296944144e0a7406d501e8a",
        DisplayName = "Valid liquidity pool ID but in hex format")]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VH", DisplayName = "Invalid liquidity pool ID")]
    public void Constructor_WithInvalidLiquidityPoolId_ThrowsArgumentException(string id)
    {
        // Act & Assert
        _ = new ScLiquidityPoolId(id);
    }

    /// <summary>
    ///     Verifies that ScClaimableBalanceId constructor throws ArgumentException when claimable balance ID is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [DataRow("Invalid id")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ",
        DisplayName = "Valid muxed account ID")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q", DisplayName = "Valid account ID")]
    [DataRow("00f6d640e0264946d35889231ce65dd63699e90dbcedea642b2d98ead157ee8758",
        DisplayName = "Valid claimable balance ID but in hex format")]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4T", DisplayName = "Invalid claimable balance ID")]
    public void Constructor_WithInvalidClaimableBalanceId_ThrowsArgumentException(string id)
    {
        // Act & Assert
        _ = new ScClaimableBalanceId(id);
    }

    /// <summary>
    ///     Verifies that ScMuxedAccountId constructor throws ArgumentException when muxed account ID is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [DataRow("Invalid id")]
    [DataRow("GDWIZBODB4V7Q4V5TS4OMPUMDDTDNXMFQOSLVE6JG7PL4MHPTG6XO43Q", DisplayName = "Valid account ID")]
    [DataRow("LAAABKAZRNPCLGKMDSS3AVLPV2ZHGJNMORRJNFCBITQKOQDNKAPIUSGG", DisplayName = "Valid liquidity pool ID")]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU", DisplayName = "Valid claimable balance ID")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJU",
        DisplayName = "Invalid account ID")]
    public void Constructor_WithInvalidMuxedAccountId_ThrowsArgumentException(string id)
    {
        // Act & Assert
        _ = new ScMuxedAccountId(id);
    }

    /// <summary>
    ///     Verifies that ScAccountId round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void ToXdrBase64_WithValidAccountId_RoundTripsCorrectly()
    {
        // Arrange
        var scAccountId = new ScAccountId("GCZFMH32MF5EAWETZTKF3ZV5SEVJPI53UEMDNSW55WBR75GMZJU4U573");

        // Act
        var xdrBase64 = scAccountId.ToXdrBase64();
        var decodedAccountId = (ScAccountId)SCVal.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual(scAccountId.InnerValue, decodedAccountId.InnerValue);
    }

    /// <summary>
    ///     Verifies that ScContractId round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    public void ToXdrBase64_WithValidContractId_RoundTripsCorrectly()
    {
        // Arrange
        var scContractId = new ScContractId("CAC2UYJQMC4ISUZ5REYB2AMDC44YKBNZWG4JB6N6GBL66CEKQO3RDSAB");

        // Act
        var xdrBase64 = scContractId.ToXdrBase64();
        var decodedContractId = (ScContractId)SCVal.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual(scContractId.InnerValue, decodedContractId.InnerValue);
    }

    /// <summary>
    ///     Verifies that ScClaimableBalanceId round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    [DataRow("BAAD6DBUX6J22DMZOHIEZTEQ64CVCHEDRKWZONFEUL5Q26QD7R76RGR4TU")]
    public void ToXdrBase64_WithValidClaimableBalanceId_RoundTripsCorrectly(string id)
    {
        // Arrange
        var claimableBalanceId = new ScClaimableBalanceId(id);

        // Act
        var xdrBase64 = claimableBalanceId.ToXdrBase64();
        var decodedClaimableBalanceId = (ScClaimableBalanceId)SCVal.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual(claimableBalanceId.InnerValue, decodedClaimableBalanceId.InnerValue);
    }

    /// <summary>
    ///     Verifies that ScLiquidityPoolId round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    [DataRow("LAARAHN7HK7EW57IB6FK5SOZIXXK56GKBOMSVJQTMRHOTABLMA2A5VHB")]
    [DataRow("LAAAMWWZQUAU443Y4OAP22N2JDL2GEE5XVXDP5FSVIIWFLP3JD2M2YCG")]
    public void ToXdrBase64_WithValidLiquidityPoolId_RoundTripsCorrectly(string id)
    {
        // Arrange
        var liquidityPoolId = new ScLiquidityPoolId(id);

        // Act
        var xdrBase64 = liquidityPoolId.ToXdrBase64();
        var decodedLiquidityPoolId = (ScLiquidityPoolId)SCVal.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual(liquidityPoolId.InnerLiquidityPoolId, decodedLiquidityPoolId.InnerLiquidityPoolId);
    }

    /// <summary>
    ///     Verifies that ScMuxedAccountId round-trips correctly through XDR serialization.
    /// </summary>
    [TestMethod]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ")]
    [DataRow("MAAAAAAAAAAAACZPKFNFGLIWAV45NTUVN6G7CWYS3GGYLONUBFC5ICGJXSRT2JFFZA5VA")]
    [DataRow("MAAAAAAAAAAAAC5OILZZBNM3E4JSWOEGLPA3YJ6EIFZLMOC5VZR4CMIKBR62SU4BRCDDK")]
    [DataRow("MAAAAAAAAAAAAC4N3AYUY6MEGEP2IXWKNTHWNOEUDY3VYMRNGTICZE726IBULR76S3PWW")]
    public void ToXdrBase64_WithValidMuxedAccountId_RoundTripsCorrectly(string id)
    {
        // Arrange
        var muxedAccountId = new ScMuxedAccountId(id);

        // Act
        var xdrBase64 = muxedAccountId.ToXdrBase64();
        var decodedMuxedAccountId = (ScMuxedAccountId)SCVal.FromXdrBase64(xdrBase64);

        // Assert
        Assert.AreEqual(muxedAccountId.InnerValue, decodedMuxedAccountId.InnerValue);
    }
}