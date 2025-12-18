using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;

namespace StellarDotnetSdk.Tests.Exceptions;

/// <summary>
/// Tests for TooManyRequestsException class functionality.
/// </summary>
[TestClass]
public class TooManyRequestsExceptionTest
{
    /// <summary>
    /// Verifies that TooManyRequestsException constructor creates instance with correct retry after value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithRetryAfter_CreatesInstanceWithCorrectRetryAfter()
    {
        // Arrange & Act
        var clientProtocolException = new TooManyRequestsException(4);

        // Assert
        Assert.AreEqual(4, clientProtocolException.RetryAfter);
    }

    /// <summary>
    /// Verifies that TooManyRequestsException can be thrown and caught correctly.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(TooManyRequestsException))]
    public void Throw_ThrowsTooManyRequestsException()
    {
        // Act
        throw new TooManyRequestsException(4);
    }
}