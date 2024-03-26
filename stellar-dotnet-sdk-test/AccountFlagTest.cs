using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class AccountFlagTest
{
    [TestMethod]
    public void TestValues()
    {
        Assert.AreEqual(1, (int)AccountFlag.AUTH_REQUIRED_FLAG);
        Assert.AreEqual(2, (int)AccountFlag.AUTH_REVOCABLE_FLAG);
        Assert.AreEqual(4, (int)AccountFlag.AUTH_IMMUTABLE_FLAG);
        Assert.AreEqual(8, (int)AccountFlag.AUTH_CLAWBACK_FLAG);
    }
}