using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Responses.Effects;

/// <summary>
/// Unit tests for deserializing effects page responses from JSON.
/// </summary>
[TestClass]
public class EffectsPageDeserializerTest
{
    /// <summary>
    /// Verifies that Page&lt;EffectResponse&gt; can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithEffectPageJson_ReturnsDeserializedEffectsPage()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectPage.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var effectsPage = JsonSerializer.Deserialize<Page<EffectResponse>>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(effectsPage);
        AssertTestData(effectsPage);
    }

    public static void AssertTestData(Page<EffectResponse> effectsPage)
    {
        var signerCreatedEffect = (SignerCreatedEffectResponse)effectsPage.Records[0];
        Assert.AreEqual("GAZHVTAM3NRJ6W643LOVA76T2W3TUKPF34ED5VNE4ZKJ2B5T2EUQHIQI", signerCreatedEffect.PublicKey);
        Assert.AreEqual("3964757325385729-3", signerCreatedEffect.PagingToken);

        var accountCreatedEffect = (AccountCreatedEffectResponse)effectsPage.Records[8];
        Assert.AreEqual("10000.0", accountCreatedEffect.StartingBalance);
        Assert.AreEqual("GDIQJ6G5AWSBRMHIZYWVWCFN64Q4BZ4TYEAQRO5GVR4EWR23RKBJ2A4R", accountCreatedEffect.Account);
        Assert.IsNotNull(effectsPage.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&limit=10&cursor=3962163165138945-3",
            effectsPage.Links.Next.Href);
    }
}