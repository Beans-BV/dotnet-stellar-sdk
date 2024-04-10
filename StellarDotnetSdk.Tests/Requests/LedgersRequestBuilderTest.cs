using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class LedgersRequestBuilderTest
{
    [TestMethod]
    public void TestAccounts()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Ledgers
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/ledgers?limit=200&order=asc", uri.ToString());
    }

    [TestMethod]
    public async Task TestLedgersExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/ledgerPage.json");
        var ledgersPage = await server.Ledgers
            .Execute();

        LedgerPageDeserializerTest.AssertTestData(ledgersPage);
    }

    [TestMethod]
    public async Task TestStream()
    {
        var jsonPath = Utils.GetTestDataPath("Responses/ledger.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        var streamableTest = new StreamableTest<LedgerResponse>(json, LedgerDeserializeTest.AssertTestData);
        await streamableTest.Run();
    }
}