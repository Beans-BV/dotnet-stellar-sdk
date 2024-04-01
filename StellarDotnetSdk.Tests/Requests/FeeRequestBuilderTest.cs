using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class FeeRequestBuilderTest
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
        var jsonResponse = File.ReadAllText(Path.Combine("testdata", "feeStats.json"));
        var fakeHttpClient = FakeHttpClient.CreateFakeHttpClient(jsonResponse);

        using (var server = new Server("https://horizon-testnet.stellar.org", fakeHttpClient))
        {
            var fees = await server.FeeStats.Execute();
            FeeStatsDeserializerTest.AssertTestData(fees);
        }
    }
}