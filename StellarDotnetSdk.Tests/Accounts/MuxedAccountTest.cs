using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests.Accounts;

/// <summary>
///     Unit tests for <see cref="MuxedAccountMed25519" /> class.
/// </summary>
[TestClass]
public class MuxedAccountTest
{
    /// <summary>
    ///     Verifies that FromMuxedAccountId correctly parses muxed account ID and extracts ID and key.
    /// </summary>
    [TestMethod]
    public void FromMuxedAccountId_WithValidMuxedAccountId_ReturnsCorrectMuxedAccount()
    {
        // Arrange
        const string muxedAccountId = "MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26";

        // Act
        var muxed = MuxedAccountMed25519.FromMuxedAccountId(muxedAccountId);

        // Assert
        Assert.AreEqual(1234UL, muxed.Id);
        Assert.AreEqual("GAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSTVY", muxed.Key.Address);
        Assert.AreEqual(muxedAccountId, muxed.Address);
    }

    /// <summary>
    ///     Verifies that Address encodes the key followed by the big-endian ID, for the SEP-23 muxed account vectors.
    /// </summary>
    [TestMethod]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUQ", 0UL)]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVAAAAAAAAAAAAAJLK", 9223372036854775808UL)]
    public void Address_WithSep23Vector_ReturnsExpectedMuxedAccountId(string expected, ulong id)
    {
        // Arrange
        var key = KeyPair.FromAccountId("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ");

        // Act
        var muxed = new MuxedAccountMed25519(key, id);

        // Assert
        Assert.AreEqual(expected, muxed.Address);
    }

    /// <summary>
    ///     Verifies that FromMuxedAccountId throws ArgumentException for anything other than a muxed account ID.
    /// </summary>
    [TestMethod]
    [DataRow("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ", DisplayName = "Valid account ID")]
    [DataRow("CA3D5KRYM6CB7OWQ6TWYRR3Z4T7GNZLKERYNZGGA5SOAOPIFY6YQGAXE", DisplayName = "Valid contract ID")]
    [DataRow("ZZZZZZZZ", DisplayName = "Invalid version byte")]
    [DataRow("M", DisplayName = "Too short")]
    [DataRow("", DisplayName = "Empty")]
    [DataRow("MA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJUAAAAAAAAAAAACJUé",
        DisplayName = "Non-ASCII last character")]
    public void FromMuxedAccountId_WithNonMuxedAccountId_ThrowsArgumentException(string id)
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => MuxedAccountMed25519.FromMuxedAccountId(id));
    }
}