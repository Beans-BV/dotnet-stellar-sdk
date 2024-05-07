using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class FeeStatsRequestBuilderTest
{
    [TestMethod]
    public void TestBuilder()
    {
        var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.FeeStats.BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/fee_stats", uri.ToString());
    }

    [TestMethod]
    public async Task TestExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/feeStats.json");
        var fees = await server.FeeStats.Execute();
        FeeStatsDeserializerTest.AssertTestData(fees);
    }
}