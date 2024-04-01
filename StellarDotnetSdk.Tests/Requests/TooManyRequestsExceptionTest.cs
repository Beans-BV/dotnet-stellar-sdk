using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class TooManyRequestsExceptionTest
{
    [TestMethod]
    public void TestCreation()
    {
        var clientProtocolException = new TooManyRequestsException(4);
        Assert.AreEqual(4, clientProtocolException.RetryAfter);
    }

    [TestMethod]
    [ExpectedException(typeof(TooManyRequestsException))]
    public void TestThrow()
    {
        throw new TooManyRequestsException(4);
    }
}