using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;

namespace StellarDotnetSdk.Tests.Exceptions;

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