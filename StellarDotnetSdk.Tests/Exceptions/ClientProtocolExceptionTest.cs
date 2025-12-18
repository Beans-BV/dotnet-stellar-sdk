using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;

namespace StellarDotnetSdk.Tests.Exceptions;

/// <summary>
///     Tests for ClientProtocolException class functionality.
/// </summary>
[TestClass]
public class ClientProtocolExceptionTest
{
    /// <summary>
    ///     Verifies that ClientProtocolException constructor creates instance with correct message.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMessage_CreatesInstanceWithCorrectMessage()
    {
        // Arrange & Act
        var clientProtocolException = new ClientProtocolException("Test");

        // Assert
        Assert.AreEqual("Test", clientProtocolException.Message);
    }

    /// <summary>
    ///     Verifies that ClientProtocolException can be thrown and caught correctly.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ClientProtocolException))]
    public void Throw_ThrowsClientProtocolException()
    {
        // Act
        throw new ClientProtocolException("Test");
    }
}