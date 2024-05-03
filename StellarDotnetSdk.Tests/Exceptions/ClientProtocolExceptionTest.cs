using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;

namespace StellarDotnetSdk.Tests.Exceptions;

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