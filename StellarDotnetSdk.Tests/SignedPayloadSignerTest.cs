using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

/// <summary>
///     Unit tests for <see cref="SignedPayloadSigner" /> class.
/// </summary>
[TestClass]
public class SignedPayloadSignerTest
{
    /// <summary>
    ///     Verifies that constructor throws ArgumentNullException when account ID is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAccountId_ThrowsArgumentNullException()
    {
        // Arrange
        xdrSDK.AccountID? accountId = null;

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new SignedPayloadSigner(accountId, []));
    }

    /// <summary>
    ///     Verifies that constructor throws ArgumentException when payload length exceeds maximum allowed size.
    /// </summary>
    [TestMethod]
    public void Constructor_WithPayloadLengthTooBig_ThrowsArgumentException()
    {
        // Arrange
        const string accountStrKey = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";
        var payload = Util.HexToBytes(
            "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f200102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f2001");

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new SignedPayloadSigner(StrKey.DecodeEd25519PublicKey(accountStrKey), payload));
    }
}