using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="BeginSponsoringFutureReservesOperationResponse" /> class.
/// </summary>
[TestClass]
public class BeginSponsoringFutureReservesOperationTest
{
    /// <summary>
    ///     Verifies that BeginSponsoringFutureReservesOperationResponse can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithBeginSponsoringFutureReservesOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("beginSponsoringFutureReserves.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertBeginSponsoringFutureReservesData(back);
    }

    private static void AssertBeginSponsoringFutureReservesData(OperationResponse instance)
    {
        Assert.IsTrue(instance is BeginSponsoringFutureReservesOperationResponse);
        var operation = (BeginSponsoringFutureReservesOperationResponse)instance;

        Assert.AreEqual(215542933753857, operation.Id);
        Assert.AreEqual("GAXHU2XHSMTZYAKFCVTULAYUL34BFPPLRVJYZMEOHP7IWPZJKSVY67RJ", operation.SponsoredId);
    }
}