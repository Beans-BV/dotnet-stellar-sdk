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
}