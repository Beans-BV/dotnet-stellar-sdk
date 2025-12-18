using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Xdr;

/// <summary>
/// Unit tests for XDR data stream operations.
/// </summary>
[TestClass]
public class XdrDataStreamTest
{
    private static string BackAndForthXdrStreaming(string inputString)
    {
        var xdrOutputStream = new XdrDataOutputStream();
        xdrOutputStream.WriteString(inputString);

        var xdrByteOutput = xdrOutputStream.ToArray();

        //XDR back to string
        var xdrInputStream = new XdrDataInputStream(xdrByteOutput);
        var outputString = xdrInputStream.ReadString();

        return outputString;
    }

    /// <summary>
    /// Verifies that XDR string streaming round-trips correctly with standard ASCII characters.
    /// </summary>
    [TestMethod]
    public void BackAndForthXdrStreaming_WithStandardAscii_RoundTripsCorrectly()
    {
        // Arrange
        const string memo = "Dollar Sign $";

        // Act
        var result = BackAndForthXdrStreaming(memo);

        // Assert
        Assert.AreEqual(memo, result);
    }

    /// <summary>
    /// Verifies that XDR string streaming round-trips correctly with non-standard ASCII characters.
    /// </summary>
    [TestMethod]
    public void BackAndForthXdrStreaming_WithNonStandardAscii_RoundTripsCorrectly()
    {
        // Arrange
        const string memo = "Euro Sign €";

        // Act
        var result = BackAndForthXdrStreaming(memo);

        // Assert
        Assert.AreEqual(memo, result);
    }

    /// <summary>
    /// Verifies that XDR string streaming round-trips correctly with all non-standard ASCII characters.
    /// </summary>
    [TestMethod]
    public void BackAndForthXdrStreaming_WithAllNonStandardAscii_RoundTripsCorrectly()
    {
        // Arrange
        const string memo = "øûý™€♠♣♥†‡µ¢£€";

        // Act
        var result = BackAndForthXdrStreaming(memo);

        // Assert
        Assert.AreEqual(memo, result);
    }

    /// <summary>
    /// Verifies that XdrDataInputStream.Read correctly reads fixed-length opaque array and subsequent bytes.
    /// </summary>
    [TestMethod]
    public void Read_WithFixedLengthOpaqueArray_ReadsCorrectly()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3, 4, 5, 0, 0, 0, 1 };
        var xdrInputStream = new XdrDataInputStream(bytes);
        var result = new byte[5];
        var expected = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        xdrInputStream.Read(result, 0, 5);
        var sentinel = xdrInputStream.Read();

        // Assert
        Assert.IsTrue(expected.SequenceEqual(result));
        Assert.AreEqual(1, sentinel);
    }
}