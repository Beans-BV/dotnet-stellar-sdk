using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.Tests.Accounts;

[TestClass]
public class MuxedAccountTest
{
    [TestMethod]
    public void TestFromAccountId()
    {
        var muxed = MuxedAccount.FromMuxedAccountId(
            "MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26");
        Assert.AreEqual(1234UL, muxed.Id);
        Assert.AreEqual("GAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSTVY", muxed.Key.Address);
        Assert.AreEqual("MAQAA5L65LSYH7CQ3VTJ7F3HHLGCL3DSLAR2Y47263D56MNNGHSQSAAAAAAAAAAE2LP26", muxed.Address);
    }
}