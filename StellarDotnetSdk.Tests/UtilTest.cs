using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StellarDotnetSdk.Tests;

/// <summary>
/// Unit tests for <see cref="Util"/> class.
/// </summary>
[TestClass]
public class UtilTest
{
    /// <summary>
    /// Verifies that BytesToHex and HexToBytes methods round-trip correctly.
    /// </summary>
    [TestMethod]
    public void BytesToHexAndHexToBytes_WithTestString_RoundTripsCorrectly()
    {
        // Arrange
        const string test = "This is a test of this method, 1234567890:;''<>!@#$%^&*()";
        var byteTest = Encoding.Default.GetBytes(test);

        // Act
        var bytesToHex = Util.BytesToHex(byteTest);
        var hexToBytes = Util.HexToBytes(bytesToHex);

        var bytesToString = Encoding.Default.GetString(hexToBytes);

        // Assert
        Assert.AreEqual(test, bytesToString);
    }

    /// <summary>
    /// Verifies that PaddedByteArray with byte array pads the array correctly with zeros.
    /// </summary>
    [TestMethod]
    public void PaddedByteArray_WithBytes_PadsWithZeros()
    {
        // Arrange
        var testBytes = Encoding.Default.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        // Act
        var result = Util.PaddedByteArray(testBytes, 40);

        // Assert
        for (var i = 26; i < result.Length; i++)
        {
            Assert.AreEqual(result[i], 0);
        }
    }

    /// <summary>
    /// Verifies that PaddedByteArray with string pads the array correctly with zeros.
    /// </summary>
    [TestMethod]
    public void PaddedByteArray_WithString_PadsWithZeros()
    {
        // Arrange
        const string testString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Act
        var result = Util.PaddedByteArray(testString, 40);

        // Assert
        for (var i = 26; i < result.Length; i++)
        {
            Assert.AreEqual(result[i], 0);
        }
    }

    /// <summary>
    /// Verifies that PaddedByteArrayToString correctly converts padded byte array to string without zero padding.
    /// </summary>
    [TestMethod]
    public void PaddedByteArrayToString_WithPaddedByteArray_ReturnsStringWithoutZeros()
    {
        // Arrange
        const string testString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var result = Util.PaddedByteArray(testString, 40);

        // Act
        var stringResult = Util.PaddedByteArrayToString(result);

        // Assert
        Assert.IsTrue(!stringResult.Contains("0"));
    }

    /// <summary>
    /// Verifies that IsIdentical extension method returns true for identical byte arrays.
    /// </summary>
    [TestMethod]
    public void IsIdentical_WithIdenticalByteArrays_ReturnsTrue()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("Something cool");
        var bytes2 = Encoding.UTF8.GetBytes("Something cool");

        // Act & Assert
        Assert.IsTrue(bytes.IsIdentical(bytes2));
    }
}