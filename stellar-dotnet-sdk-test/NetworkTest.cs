using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class NetworkTest
{
    [TestCleanup]
    public void ResetNetwork()
    {
        Network.Use(null);
    }

    [TestMethod]
    public void TestSwitchToTestNetwork()
    {
        Network.UseTestNetwork();
        Assert.IsNotNull(Network.Current);
        Assert.AreEqual("Test SDF Network ; September 2015", Network.Current.NetworkPassphrase);
    }

    [TestMethod]
    public void TestSwitchToPublicNetwork()
    {
        Network.UsePublicNetwork();
        Assert.IsNotNull(Network.Current);
        Assert.AreEqual("Public Global Stellar Network ; September 2015", Network.Current.NetworkPassphrase);
    }
}