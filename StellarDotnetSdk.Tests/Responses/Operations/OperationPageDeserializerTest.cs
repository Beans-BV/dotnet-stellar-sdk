using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for deserializing operation page responses from JSON.
/// </summary>
[TestClass]
public class OperationPageDeserializerTest
{
    private readonly string _operationPageJsonPath = Utils.GetTestDataPath("Responses/operationPage.json");

    /// <summary>
    ///     Verifies that Page&lt;OperationResponse&gt; can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOperationPageJson_ReturnsDeserializedOperationsPage()
    {
        // Arrange
        var json = File.ReadAllText(_operationPageJsonPath);

        // Act
        var operationsPage = JsonSerializer.Deserialize<Page<OperationResponse>>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(operationsPage);
        AssertTestData(operationsPage);
    }

    /// <summary>
    ///     Verifies that Page&lt;OperationResponse&gt; can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithOperationPage_RoundTripsCorrectly()
    {
        // Arrange
        var json = File.ReadAllText(_operationPageJsonPath);
        var operationsPage = JsonSerializer.Deserialize<Page<OperationResponse>>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(operationsPage);
        var back = JsonSerializer.Deserialize<Page<OperationResponse>>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(Page<OperationResponse> operationsPage)
    {
        var createAccountOperation = (CreateAccountOperationResponse)operationsPage.Records[0];
        Assert.AreEqual(createAccountOperation.StartingBalance, "10000.0");
        Assert.AreEqual(createAccountOperation.PagingToken, "3717508943056897");
        Assert.AreEqual(createAccountOperation.Account, "GDFH4NIYMIIAKRVEJJZOIGWKXGQUF3XHJG6ZM6CEA64AMTVDN44LHOQE");
        Assert.AreEqual(createAccountOperation.Funder, "GBS43BF24ENNS3KPACUZVKK2VYPOZVBQO2CISGZ777RYGOPYC2FT6S3K");

        var paymentOperation = (PaymentOperationResponse)operationsPage.Records[4];
        Assert.AreEqual(paymentOperation.Amount, "10.123");
        Assert.AreEqual(paymentOperation.Asset, new AssetTypeNative());
        Assert.AreEqual(paymentOperation.From, "GCYK67DDGBOANS6UODJ62QWGLEB2A7JQ3XUV25HCMLT7CI23PMMK3W6R");
        Assert.AreEqual(paymentOperation.To, "GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H");
    }
}