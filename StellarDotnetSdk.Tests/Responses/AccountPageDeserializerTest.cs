using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class AccountPageDeserializerTest
{
    [TestMethod]
    public void TestDeserializeAccountPage()
    {
        var jsonPath = Utils.GetTestDataPath("accountPage.json");
        var json = File.ReadAllText(jsonPath);
        var accountsPage = JsonSerializer.Deserialize<Page<AccountResponse>>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(accountsPage);
        AssertTestData(accountsPage);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountPage()
    {
        var jsonPath = Utils.GetTestDataPath("accountPage.json");
        var json = File.ReadAllText(jsonPath);
        var accountsPage = JsonSerializer.Deserialize<Page<AccountResponse>>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(accountsPage);
        var back = JsonSerializer.Deserialize<Page<AccountResponse>>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    private static void AssertTestData(Page<AccountResponse> accountsPage)
    {
        Assert.IsNotNull(accountsPage.Links);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts?asset=TEST%3AGB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST&cursor=&limit=10&order=asc",
            accountsPage.Links.Self.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts?asset=TEST%3AGB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST&cursor=GA7N5VZ27ISU5QD2OGEVE554YHUON45VCQEZ4AIZTL7VUMPHB7BGXWRE&limit=10&order=desc",
            accountsPage.Links.Prev.Href);
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts?asset=TEST%3AGB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST&cursor=GBLD7NSGWYT2KRFTYMRJGQ42SNTGDXMZYNQUE3H7N2VEMARIQYKBB4JJ&limit=10&order=asc",
            accountsPage.Links.Next.Href);

        // Skip checking the main properties as they have been tested in the AccountDeserializerTest
        Assert.AreEqual(3, accountsPage.Records.Count);
        Assert.AreEqual("GA7N5VZ27ISU5QD2OGEVE554YHUON45VCQEZ4AIZTL7VUMPHB7BGXWRE", accountsPage.Records[0].AccountId);
        Assert.AreEqual("GAG3ZVX4564SWFE5YIEHVHNAOBH7XTPTYC6R7U6GCJ5RYATQIOPGTEST", accountsPage.Records[1].AccountId);
        Assert.AreEqual("GBLD7NSGWYT2KRFTYMRJGQ42SNTGDXMZYNQUE3H7N2VEMARIQYKBB4JJ", accountsPage.Records[2].AccountId);
    }
}