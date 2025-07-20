using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xdrSDK = StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class SignedPayloadSignerTest
{
    [TestMethod]
    public void ItFailsWhenAccountIdIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new SignedPayloadSigner((xdrSDK.AccountID)null, []));
    }

    [TestMethod]
    public void ItFailsWhenPayloadLengthTooBig()
    {
        const string accountStrKey = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";
        var payload = Util.HexToBytes(
            "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f200102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f2001");
        Assert.ThrowsException<ArgumentException>(() =>
            new SignedPayloadSigner(StrKey.DecodeEd25519PublicKey(accountStrKey), payload));
    }
}