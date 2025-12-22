using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing asset page responses from JSON.
/// </summary>
[TestClass]
public class AssetPageDeserializerTest
{
    /// <summary>
    ///     Verifies that Page&lt;AssetResponse&gt; can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAssetPageJson_ReturnsDeserializedAssetPage()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("assetPage.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var assetsPage = JsonSerializer.Deserialize<Page<AssetResponse>>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(assetsPage);
        AssertTestData(assetsPage);
    }

    /// <summary>
    ///     Verifies that Page&lt;AssetResponse&gt; can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAssetPage_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("assetPage.json");
        var json = File.ReadAllText(jsonPath);
        var assetsPage = JsonSerializer.Deserialize<Page<AssetResponse>>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(assetsPage, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<Page<AssetResponse>>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(Page<AssetResponse> assetsPage)
    {
        Assert.IsNotNull(assetsPage.Links);
        Assert.AreEqual("https://horizon-testnet.stellar.org/assets?cursor=&limit=200&order=desc",
            assetsPage.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/assets?cursor=XYZ_GBHLV3AOWIUIHECHYPO5YAVCFJVJS3EKVH5F744VJBIQQQ6NZCFBJJEL_credit_alphanum4&limit=200&order=desc",
            assetsPage.Links.Next.Href);
        var record = assetsPage.Records[0];
        Assert.AreEqual("credit_alphanum4", record.AssetType);
        Assert.AreEqual("ZZZ", record.AssetCode);
        Assert.AreEqual("GCWMJP3GFA2V3M2GSTJUC7H3NM27XG6GDGWJZVM3S536JWYIS6BIWS35", record.AssetIssuer);
        Assert.AreEqual(Asset.Create("ZZZ:GCWMJP3GFA2V3M2GSTJUC7H3NM27XG6GDGWJZVM3S536JWYIS6BIWS35"), record.Asset);
        Assert.AreEqual("ZZZ_GCWMJP3GFA2V3M2GSTJUC7H3NM27XG6GDGWJZVM3S536JWYIS6BIWS35_credit_alphanum4",
            record.PagingToken);
        Assert.AreEqual(1, record.Accounts.Authorized);
        Assert.AreEqual(0, record.Accounts.AuthorizedToMaintainLiabilities);
        Assert.AreEqual(0, record.Accounts.Unauthorized);
        Assert.AreEqual("1200000000.0000000", record.Balances.Authorized);
        Assert.AreEqual("0.0000000", record.Balances.AuthorizedToMaintainLiabilities);
        Assert.AreEqual("0.0000000", record.Balances.Unauthorized);
        Assert.AreEqual(0, record.NumClaimableBalances);
        Assert.AreEqual(0U, record.ContractsQuantity);
        Assert.AreEqual("0.0000000", record.ContractsTotalAmount);
        Assert.AreEqual("0.0000000", record.ClaimableBalancesAmount);
        Assert.AreEqual("", record.Links.Toml.Href);
        Assert.AreEqual(false, record.Flags.AuthRequired);
        Assert.AreEqual(false, record.Flags.AuthRevocable);
        Assert.AreEqual(true, record.Flags.AuthImmutable);
        Assert.AreEqual(false, record.Flags.AuthRevocable);
        Assert.AreEqual("", record.Links.Toml.Href);
        Assert.AreEqual(1, record.NumLiquidityPools);
        Assert.AreEqual("70000000.0000000", record.LiquidityPoolsAmount);
    }

    /// <summary>
    ///     Verifies that empty Page&lt;AssetResponse&gt; can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithEmptyAssetPageJson_ReturnsEmptyAssetPage()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("assetPageEmpty.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var assetsPage = JsonSerializer.Deserialize<Page<AssetResponse>>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(assetsPage);
        Assert.IsNotNull(assetsPage.Links);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/assets?asset_code=01&asset_issuer=GBGN64KCMODLTB6GCPNGNKPTZAZA4CBGRWMWTINEPBTUVQL2X35HPZSB&cursor=&limit=10&order=asc",
            assetsPage.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/assets?asset_code=01&asset_issuer=GBGN64KCMODLTB6GCPNGNKPTZAZA4CBGRWMWTINEPBTUVQL2X35HPZSB&cursor=&limit=10&order=asc",
            assetsPage.Links.Next.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/assets?asset_code=01&asset_issuer=GBGN64KCMODLTB6GCPNGNKPTZAZA4CBGRWMWTINEPBTUVQL2X35HPZSB&cursor=&limit=10&order=desc",
            assetsPage.Links.Prev.Href);
        Assert.IsNotNull(assetsPage.Records);
        Assert.AreEqual(0, assetsPage.Records.Count);
    }
}