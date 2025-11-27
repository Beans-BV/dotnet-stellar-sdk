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
        Assert.AreEqual(accountsPage.Records[0].AccountId, "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        Assert.AreEqual(accountsPage.Records[9].AccountId, "GACFGMEV7A5H44O3K4EN6GRQ4SA543YJBZTKGNKPEMEQEAJFO4Q7ENG6");
        Assert.AreEqual(accountsPage.Links.Next.Href, "/accounts?order=asc&limit=10&cursor=86861418598401");
        Assert.AreEqual(accountsPage.Links.Prev.Href, "/accounts?order=desc&limit=10&cursor=1");
        Assert.AreEqual(accountsPage.Links.Self.Href, "/accounts?order=asc&limit=10&cursor=");
    }
}