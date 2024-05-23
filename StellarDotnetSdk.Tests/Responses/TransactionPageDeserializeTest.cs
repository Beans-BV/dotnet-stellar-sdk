using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class TransactionPageDeserializeTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("transactionPage.json");
        var json = File.ReadAllText(jsonPath);
        var transactionsPage = JsonSerializer.Deserialize<Page<TransactionResponse>>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(transactionsPage);
        AssertTestData(transactionsPage);
    }

    public static void AssertTestData(Page<TransactionResponse> transactionsPage)
    {
        Assert.AreEqual(transactionsPage.Records[0].SourceAccount,
            "GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        Assert.AreEqual(transactionsPage.Records[0].PagingToken, "12884905984");
        Assert.AreEqual(transactionsPage.Records[0].Links.Account.Href,
            "/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        Assert.AreEqual(transactionsPage.Records[9].SourceAccount,
            "GAENIE5LBJIXLMJIAJ7225IUPA6CX7EGHUXRX5FLCZFFAQSG2ZUYSWFK");

        Assert.AreEqual(transactionsPage.Links.Next.Href, "/transactions?order=asc&limit=10&cursor=81058917781504");
        Assert.AreEqual(transactionsPage.Links.Prev.Href, "/transactions?order=desc&limit=10&cursor=12884905984");
        Assert.AreEqual(transactionsPage.Links.Self.Href, "/transactions?order=asc&limit=10&cursor=");
    }
}