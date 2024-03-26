using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class ClientProtocolExceptionTest
{
    [TestMethod]
    public void TestCreation()
    {
        var clientProtocolException = new ClientProtocolException("Test");
        Assert.AreEqual("Test", clientProtocolException.Message);
    }

    [TestMethod]
    [ExpectedException(typeof(ClientProtocolException))]
    public void TestThrow()
    {
        throw new ClientProtocolException("Test");
    }
}