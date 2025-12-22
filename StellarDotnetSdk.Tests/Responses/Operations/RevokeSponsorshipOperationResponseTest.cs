using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="RevokeSponsorshipOperationResponse" /> class.
/// </summary>
[TestClass]
public class RevokeSponsorshipOperationResponseTest
{
    /// <summary>
    ///     Verifies that RevokeSponsorshipOperationResponse with account ID can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithRevokeSponsorshipAccountIdOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("revokeSponsorshipAccountID.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertRevokeSponsorshipAccountIdData(back);
    }

    private static void AssertRevokeSponsorshipAccountIdData(OperationResponse instance)
    {
        Assert.IsTrue(instance is RevokeSponsorshipOperationResponse);
        var operation = (RevokeSponsorshipOperationResponse)instance;

        Assert.AreEqual(286156491067394, operation.Id);
        Assert.AreEqual("GCLHBHJAYWFT6JA27KEPUQCCGIHUB33HURYAKNWIY4FB7IY3K24PRXET", operation.AccountId);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipOperationResponse with claimable balance can be serialized and deserialized
    ///     correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithRevokeSponsorshipClaimableBalanceOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("revokeSponsorshipClaimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertRevokeSponsorshipClaimableBalanceData(back);
    }

    private static void AssertRevokeSponsorshipClaimableBalanceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is RevokeSponsorshipOperationResponse);
        var operation = (RevokeSponsorshipOperationResponse)instance;

        Assert.AreEqual(287054139232258, operation.Id);
        Assert.AreEqual("00000000c582697b67cbec7f9ce64f4dc67bfb2bfd26318bb9f964f4d70e3f41f650b1e6",
            operation.ClaimableBalanceId);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipOperationResponse with data can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithRevokeSponsorshipDataOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("revokeSponsorshipData.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertRevokeSponsorshipDataData(back);
    }

    private static void AssertRevokeSponsorshipDataData(OperationResponse instance)
    {
        Assert.IsTrue(instance is RevokeSponsorshipOperationResponse);
        var operation = (RevokeSponsorshipOperationResponse)instance;

        Assert.AreEqual(286800736161794, operation.Id);
        Assert.AreEqual("GDHSYF7V3DZRM7Q2HS5J6FHAHNWETMBFMG7DOSWU3GA7OM4KGOPZM3FB", operation.DataAccountId);
        Assert.AreEqual("hello", operation.DataName);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipOperationResponse with offer can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithRevokeSponsorshipOfferOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("revokeSponsorshipOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertRevokeSponsorshipOfferData(back);
    }

    private static void AssertRevokeSponsorshipOfferData(OperationResponse instance)
    {
        Assert.IsTrue(instance is RevokeSponsorshipOperationResponse);
        var operation = (RevokeSponsorshipOperationResponse)instance;

        Assert.AreEqual(286800736161794, operation.Id);
        Assert.IsNull(operation.OfferId);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipOperationResponse with signer key can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithRevokeSponsorshipSignerKey_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("revokeSponsorshipSignerKey.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertRevokeSponsorshipSignerKeyData(back);
    }

    private static void AssertRevokeSponsorshipSignerKeyData(OperationResponse instance)
    {
        Assert.IsTrue(instance is RevokeSponsorshipOperationResponse);
        var operation = (RevokeSponsorshipOperationResponse)instance;

        Assert.AreEqual(287363376877570, operation.Id);
        Assert.AreEqual("GAXHU2XHSMTZYAKFCVTULAYUL34BFPPLRVJYZMEOHP7IWPZJKSVY67RJ", operation.SignerAccountId);
        Assert.AreEqual("XAMF7DNTEJY74JPVMGTPZE4LFYTEGBXMGBHNUUMAA7IXMSBGHAMWSND6", operation.SignerKey);
    }

    /// <summary>
    ///     Verifies that RevokeSponsorshipOperationResponse with trustline can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithRevokeSponsorshipTrustline_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("revokeSponsorshipTrustline.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertRevokeSponsorshipTrustlineData(back);
    }

    private static void AssertRevokeSponsorshipTrustlineData(OperationResponse instance)
    {
        Assert.IsTrue(instance is RevokeSponsorshipOperationResponse);
        var operation = (RevokeSponsorshipOperationResponse)instance;

        Assert.AreEqual(286500088451074, operation.Id);
        Assert.AreEqual("GDHSYF7V3DZRM7Q2HS5J6FHAHNWETMBFMG7DOSWU3GA7OM4KGOPZM3FB", operation.TrustlineAccountId);
        Assert.AreEqual("XYZ:GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", operation.TrustlineAsset);
    }
}