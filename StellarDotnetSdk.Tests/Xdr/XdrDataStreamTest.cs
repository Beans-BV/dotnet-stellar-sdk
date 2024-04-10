using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Xdr;

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

    [TestMethod]
    public void BackAndForthXdrStreamingWithStandardAscii()
    {
        const string memo = "Dollar Sign $";
        Assert.AreEqual(memo, BackAndForthXdrStreaming(memo));
    }

    [TestMethod]
    public void BackAndForthXdrStreamingWithNonStandardAscii()
    {
        const string memo = "Euro Sign €";
        Assert.AreEqual(memo, BackAndForthXdrStreaming(memo));
    }

    [TestMethod]
    public void BackAndForthXdrStreamingWithAllNonStandardAscii()
    {
        const string memo = "øûý™€♠♣♥†‡µ¢£€";
        Assert.AreEqual(memo, BackAndForthXdrStreaming(memo));
    }

    [TestMethod]
    public void ReadFixedLengthOpaqueArray()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5, 0, 0, 0, 1 };
        var xdrInputStream = new XdrDataInputStream(bytes);
        var result = new byte[5];
        xdrInputStream.Read(result, 0, 5);
        var expected = new byte[] { 1, 2, 3, 4, 5 };
        Assert.IsTrue(expected.SequenceEqual(result));

        var sentinel = xdrInputStream.Read();
        Assert.AreEqual(1, sentinel);
    }
}