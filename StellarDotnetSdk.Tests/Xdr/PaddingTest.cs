using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Xdr;

/// <summary>
/// Unit tests for XDR padding validation.
/// </summary>
[TestClass]
public class PaddingTest
{
    /// <summary>
    /// Verifies that String32.Decode throws IOException when non-zero padding is detected.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(IOException))]
    public void Decode_WithNonZeroPadding_ThrowsIOException()
    {
        // Arrange
        byte[] bytes = [0, 0, 0, 2, (byte)'a', (byte)'b', 1, 0];

        // Act
        try
        {
            String32.Decode(new XdrDataInputStream(bytes));
        }
        catch (IOException expectedException)
        {
            // Assert
            Assert.AreEqual("non-zero padding", expectedException.Message);
            throw;
        }
    }

    /// <summary>
    /// Verifies that DataValue.Decode throws IOException when non-zero padding is detected.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(IOException))]
    public void Decode_WithNonZeroPaddingInVarOpaque_ThrowsIOException()
    {
        // Arrange
        byte[] bytes = [0, 0, 0, 2, (byte)'a', (byte)'b', 1, 0];

        // Act
        try
        {
            DataValue.Decode(new XdrDataInputStream(bytes));
        }
        catch (IOException expectedException)
        {
            // Assert
            Assert.AreEqual("non-zero padding", expectedException.Message);
            throw;
        }
    }
}