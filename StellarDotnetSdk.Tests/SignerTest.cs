using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SignerTest
{
    [TestMethod]
    public void ItCreatesSignedPayloadSigner()
    {
        var accountStrKey = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";

        var payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
        var signedPayloadSigner = new SignedPayloadSigner(StrKey.DecodeStellarAccountId(accountStrKey), payload);
        var signerKey = SignerUtil.SignedPayload(signedPayloadSigner);

        Assert.IsTrue(signerKey.Ed25519SignedPayload.Payload.SequenceEqual(payload));
        Assert.AreEqual(signerKey.Ed25519SignedPayload.Ed25519, signedPayloadSigner.SignerAccountID.InnerValue.Ed25519);
    }
}