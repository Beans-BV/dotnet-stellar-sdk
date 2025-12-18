using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.Tests;

/// <summary>
/// Unit tests for <see cref="SignerUtil"/> class.
/// </summary>
[TestClass]
public class SignerTest
{
    /// <summary>
    /// Verifies that SignedPayload creates a signer key with the correct payload and account ID.
    /// </summary>
    [TestMethod]
    public void SignedPayload_WithValidSignedPayloadSigner_ReturnsSignerKeyWithCorrectPayloadAndAccountId()
    {
        // Arrange
        const string accountStrKey = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";

        var payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
        var signedPayloadSigner = new SignedPayloadSigner(StrKey.DecodeEd25519PublicKey(accountStrKey), payload);

        // Act
        var signerKey = SignerUtil.SignedPayload(signedPayloadSigner);

        // Assert
        Assert.IsTrue(signerKey.Ed25519SignedPayload.Payload.SequenceEqual(payload));
        Assert.AreEqual(signerKey.Ed25519SignedPayload.Ed25519, signedPayloadSigner.SignerAccountId.InnerValue.Ed25519);
    }
}