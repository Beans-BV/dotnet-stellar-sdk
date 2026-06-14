using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Xdr;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using Uint64 = StellarDotnetSdk.Xdr.Uint64;

namespace StellarDotnetSdk.Tests.Xdr;

/// <summary>
///     Unit tests for XDR data stream operations.
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
    ///     Verifies that XDR string streaming round-trips correctly with standard ASCII characters.
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
    ///     Verifies that XDR string streaming round-trips correctly with non-standard ASCII characters.
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
    ///     Verifies that XDR string streaming round-trips correctly with all non-standard ASCII characters.
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
    ///     Verifies that XdrDataInputStream.Read correctly reads fixed-length opaque array and subsequent bytes.
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

    /// <summary>
    ///     Verifies that XDR int values round-trip correctly through the data streams.
    /// </summary>
    [TestMethod]
    public void ReadInt_WithRoundTrip_PreservesValues()
    {
        // Arrange
        int[] values = [int.MinValue, -1, 0, 1, 42, int.MaxValue];
        var outputStream = new XdrDataOutputStream();
        foreach (var value in values)
        {
            outputStream.WriteInt(value);
        }
        var inputStream = new XdrDataInputStream(outputStream.ToArray());

        // Act & Assert
        foreach (var value in values)
        {
            Assert.AreEqual(value, inputStream.ReadInt());
        }
    }

    /// <summary>
    ///     Verifies that XDR uint values round-trip correctly through the data streams.
    /// </summary>
    [TestMethod]
    public void ReadUInt_WithRoundTrip_PreservesValues()
    {
        // Arrange
        uint[] values = [uint.MinValue, 1, 42, 0x80000000, uint.MaxValue];
        var outputStream = new XdrDataOutputStream();
        foreach (var value in values)
        {
            outputStream.WriteUInt(value);
        }
        var inputStream = new XdrDataInputStream(outputStream.ToArray());

        // Act & Assert
        foreach (var value in values)
        {
            Assert.AreEqual(value, inputStream.ReadUInt());
        }
    }

    /// <summary>
    ///     Verifies that XDR long values round-trip correctly through the data streams.
    /// </summary>
    [TestMethod]
    public void Decode_WithInt64RoundTrip_PreservesValues()
    {
        // Arrange
        long[] values = [long.MinValue, -1, 0, 1, 42, long.MaxValue];
        var outputStream = new XdrDataOutputStream();
        foreach (var value in values)
        {
            Int64.Encode(outputStream, new Int64(value));
        }
        var inputStream = new XdrDataInputStream(outputStream.ToArray());

        // Act & Assert
        foreach (var value in values)
        {
            Assert.AreEqual(value, Int64.Decode(inputStream).InnerValue);
        }
    }

    /// <summary>
    ///     Verifies that XDR ulong values round-trip correctly through the data streams.
    /// </summary>
    [TestMethod]
    public void Decode_WithUint64RoundTrip_PreservesValues()
    {
        // Arrange
        ulong[] values = [ulong.MinValue, 1, 42, 0x8000000000000000, ulong.MaxValue];
        var outputStream = new XdrDataOutputStream();
        foreach (var value in values)
        {
            Uint64.Encode(outputStream, new Uint64(value));
        }
        var inputStream = new XdrDataInputStream(outputStream.ToArray());

        // Act & Assert
        foreach (var value in values)
        {
            Assert.AreEqual(value, Uint64.Decode(inputStream).InnerValue);
        }
    }

    /// <summary>
    ///     Verifies that reading a single byte past the end of the input throws EndOfStreamException.
    /// </summary>
    [TestMethod]
    public void Read_PastEndOfInput_ThrowsEndOfStreamException()
    {
        // Arrange
        var inputStream = new XdrDataInputStream([]);

        // Act & Assert
        Assert.ThrowsException<EndOfStreamException>(() => inputStream.Read());
    }

    /// <summary>
    ///     Verifies that reading an int from truncated input throws EndOfStreamException instead of
    ///     returning a partially read value.
    /// </summary>
    [TestMethod]
    public void ReadInt_WithTruncatedInput_ThrowsEndOfStreamException()
    {
        // Arrange
        var inputStream = new XdrDataInputStream([0, 1]);

        // Act & Assert
        Assert.ThrowsException<EndOfStreamException>(() => inputStream.ReadInt());
    }

    /// <summary>
    ///     Verifies that decoding a long from truncated input throws EndOfStreamException instead of
    ///     returning a partially read value.
    /// </summary>
    [TestMethod]
    public void Decode_WithTruncatedInt64_ThrowsEndOfStreamException()
    {
        // Arrange
        var inputStream = new XdrDataInputStream([0, 1, 2, 3]);

        // Act & Assert
        Assert.ThrowsException<EndOfStreamException>(() => Int64.Decode(inputStream));
    }

    /// <summary>
    ///     Verifies that reading a fixed-length opaque array from truncated input throws InvalidDataException.
    /// </summary>
    [TestMethod]
    public void Read_WithTruncatedFixedLengthOpaqueArray_ThrowsInvalidDataException()
    {
        // Arrange
        var inputStream = new XdrDataInputStream([1, 2, 3]);
        var buffer = new byte[5];

        // Act & Assert
        Assert.ThrowsException<InvalidDataException>(() => inputStream.Read(buffer, 0, 5));
    }

    /// <summary>
    ///     Verifies that reading a string with a truncated length prefix throws FormatException.
    /// </summary>
    [TestMethod]
    public void ReadString_WithTruncatedLengthPrefix_ThrowsFormatException()
    {
        // Arrange
        var inputStream = new XdrDataInputStream([0, 0]);

        // Act & Assert
        Assert.ThrowsException<FormatException>(() => inputStream.ReadString());
    }

    /// <summary>
    ///     Verifies that XDR float values round-trip bit-exactly through the data streams,
    ///     including signed zeros, infinities, and NaN.
    /// </summary>
    [TestMethod]
    public void ReadSingle_WithRoundTrip_PreservesBitPatterns()
    {
        // Arrange
        float[] values =
        [
            float.MinValue, -1.5f, -0f, 0f, float.Epsilon, 1.5f, float.MaxValue,
            float.PositiveInfinity, float.NegativeInfinity, float.NaN,
        ];
        var outputStream = new XdrDataOutputStream();
        foreach (var value in values)
        {
            outputStream.WriteSingle(value);
        }
        var inputStream = new XdrDataInputStream(outputStream.ToArray());

        // Act & Assert
        foreach (var value in values)
        {
            Assert.AreEqual(
                BitConverter.SingleToInt32Bits(value),
                BitConverter.SingleToInt32Bits(inputStream.ReadSingle()));
        }
    }

    /// <summary>
    ///     Verifies that XDR double values round-trip bit-exactly through the data streams,
    ///     including signed zeros, infinities, and NaN.
    /// </summary>
    [TestMethod]
    public void ReadDouble_WithRoundTrip_PreservesBitPatterns()
    {
        // Arrange
        double[] values =
        [
            double.MinValue, -1.5, -0d, 0d, double.Epsilon, 1.5, double.MaxValue,
            double.PositiveInfinity, double.NegativeInfinity, double.NaN,
        ];
        var outputStream = new XdrDataOutputStream();
        foreach (var value in values)
        {
            outputStream.WriteDouble(value);
        }
        var inputStream = new XdrDataInputStream(outputStream.ToArray());

        // Act & Assert
        foreach (var value in values)
        {
            Assert.AreEqual(
                BitConverter.DoubleToInt64Bits(value),
                BitConverter.DoubleToInt64Bits(inputStream.ReadDouble()));
        }
    }

    /// <summary>
    ///     Verifies that reading a string whose length prefix is intact but whose body is truncated
    ///     throws InvalidDataException.
    /// </summary>
    [TestMethod]
    public void ReadString_WithTruncatedBody_ThrowsInvalidDataException()
    {
        // Arrange
        var inputStream = new XdrDataInputStream([0, 0, 0, 5, 1, 2]);

        // Act & Assert
        Assert.ThrowsException<InvalidDataException>(() => inputStream.ReadString());
    }

    /// <summary>
    ///     Verifies that reading a variable-length opaque whose length prefix is intact but whose body
    ///     is truncated throws InvalidDataException.
    /// </summary>
    [TestMethod]
    public void ReadVarOpaque_WithTruncatedBody_ThrowsInvalidDataException()
    {
        // Arrange
        var inputStream = new XdrDataInputStream([0, 0, 0, 5, 1, 2]);

        // Act & Assert
        Assert.ThrowsException<InvalidDataException>(() => inputStream.ReadVarOpaque(10));
    }
}
