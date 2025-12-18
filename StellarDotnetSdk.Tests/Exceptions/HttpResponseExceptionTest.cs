using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;

namespace StellarDotnetSdk.Tests.Exceptions;

/// <summary>
///     Tests for HttpResponseException class functionality.
/// </summary>
[TestClass]
public class HttpResponseExceptionTest
{
    /// <summary>
    ///     Verifies that HttpResponseException constructor creates instance with correct message and status code.
    /// </summary>
    [TestMethod]
    public void Constructor_WithStatusCodeAndMessage_CreatesInstanceWithCorrectProperties()
    {
        // Arrange & Act
        var clientProtocolException = new HttpResponseException(200, "Test");

        // Assert
        Assert.AreEqual("Test", clientProtocolException.Message);
        Assert.AreEqual(200, clientProtocolException.StatusCode);
    }

    /// <summary>
    ///     Verifies that HttpResponseException can be thrown and caught correctly.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(HttpResponseException))]
    public void Throw_ThrowsHttpResponseException()
    {
        // Act
        throw new HttpResponseException(200, "Test");
    }
}