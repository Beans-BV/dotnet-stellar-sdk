using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Exceptions;

namespace StellarDotnetSdk.Tests.Exceptions;

/// <summary>
///     Tests for <see cref="SorobanRpcException" /> class functionality.
/// </summary>
[TestClass]
public class SorobanRpcExceptionTest
{
    /// <summary>
    ///     Verifies that the exception message carries both the JSON-RPC error code and the server's message,
    ///     and that the server's message is also preserved verbatim.
    /// </summary>
    [TestMethod]
    public void Constructor_WithCodeAndMessage_CreatesInstanceWithBothInMessage()
    {
        // Act
        var exception = new SorobanRpcException(-32602, "ledger ttl entries cannot be queried directly");

        // Assert
        Assert.AreEqual(-32602, exception.Code);
        Assert.AreEqual("ledger ttl entries cannot be queried directly", exception.ErrorMessage);
        StringAssert.Contains(exception.Message, "-32602");
        StringAssert.Contains(exception.Message, "ledger ttl entries cannot be queried directly");
        Assert.IsNull(exception.ErrorData);
    }

    /// <summary>
    ///     Verifies that a JSON-RPC error without a message still produces a usable exception message
    ///     naming the error code.
    /// </summary>
    [TestMethod]
    public void Constructor_WithoutMessage_CreatesInstanceWithCodeOnlyMessage()
    {
        // Act
        var exception = new SorobanRpcException(-32603, null);

        // Assert
        Assert.AreEqual(-32603, exception.Code);
        Assert.IsNull(exception.ErrorMessage);
        StringAssert.Contains(exception.Message, "-32603");
    }

    /// <summary>
    ///     Verifies that the optional JSON-RPC <c>data</c> member is preserved as-is on the exception.
    /// </summary>
    [TestMethod]
    public void Constructor_WithData_PreservesData()
    {
        // Arrange
        using var document = JsonDocument.Parse("""{"details":"invalid ledger range"}""");

        // Act
        var exception = new SorobanRpcException(-32602, "invalid parameters", document.RootElement.Clone());

        // Assert
        Assert.IsNotNull(exception.ErrorData);
        Assert.AreEqual("invalid ledger range", exception.ErrorData.Value.GetProperty("details").GetString());
    }
}
