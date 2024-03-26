using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;
using xdrSDK = stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class SignedPayloadSignerTest
{
    [TestMethod]
    public void ItFailsWhenAccountIDIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new SignedPayloadSigner((xdrSDK.AccountID)null, new byte[] { }));
    }

    [TestMethod]
    public void ItFailsWhenPayloadLengthTooBig()
    {
        const string accountStrKey = "GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ";
        var payload =
            Util.HexToBytes(
                "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f200102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f2001");
        Assert.ThrowsException<ArgumentException>(() =>
            new SignedPayloadSigner(StrKey.DecodeStellarAccountId(accountStrKey), payload));
    }
}