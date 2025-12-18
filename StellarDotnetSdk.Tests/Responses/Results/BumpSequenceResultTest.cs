using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Responses.Results;

namespace StellarDotnetSdk.Tests.Responses.Results;

/// <summary>
/// Unit tests for bump sequence result types.
/// </summary>
[TestClass]
public class BumpSequenceResultTest
{
    /// <summary>
    /// Verifies that BumpSequenceSuccess result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBumpSequenceSuccessXdr_ReturnsBumpSequenceSuccess()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAALAAAAAAAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(BumpSequenceSuccess), true);
    }

    /// <summary>
    /// Verifies that BumpSequenceBadSeq result can be deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithBumpSequenceBadSeqXdr_ReturnsBumpSequenceBadSeq()
    {
        // Arrange
        var xdrBase64 = "AAAAAACYloD/////AAAAAQAAAAAAAAAL/////wAAAAA=";

        // Act & Assert
        Utils.AssertResultOfType(xdrBase64, typeof(BumpSequenceBadSeq), false);
    }
}