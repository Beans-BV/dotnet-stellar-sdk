using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class AccountsRequestBuilderTest
{
    [TestMethod]
    public void TestAccountsBuildUri()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.Accounts
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        Assert.AreEqual("https://horizon-testnet.stellar.org/accounts?cursor=13537736921089&limit=200&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public void TestAccountsBuildUriPathPrefix()
    {
        using var server = new Server("https://nodeapi.com/xlm/authkey/");
        var uri = server.Accounts
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        Assert.AreEqual("https://nodeapi.com/xlm/authkey/accounts?cursor=13537736921089&limit=200&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestAccountsAccount()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/account.json");
        var account = await server.Accounts.Account("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");

        AccountDeserializerTest.AssertTestData(account);
    }

    [TestMethod]
    public async Task TestAccountsData()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/accountData.json");
        var accountData =
            await server.Accounts.AccountData("GAKLBGHNHFQ3BMUYG5KU4BEWO6EYQHZHAXEWC33W34PH2RBHZDSQBD75",
                "TestValue");

        Assert.AreEqual("VGVzdFZhbHVl", accountData.Value);
        Assert.AreEqual("TestValue", accountData.ValueDecoded);
    }

    [TestMethod]
    public async Task TestAccountsWithSigner()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/accountsWithSigner.json");
        var req = server.Accounts
            .WithSigner("GBPOFUJUHOFTZHMZ63H5GE6NX5KVKQRD6N3I2E5AL3T2UG7HSLPLXN2K");

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts?signer=GBPOFUJUHOFTZHMZ63H5GE6NX5KVKQRD6N3I2E5AL3T2UG7HSLPLXN2K",
            req.BuildUri().ToString());
        var accounts = await req.Execute();

        Assert.AreEqual(1, accounts.Records.Count);
    }


    [TestMethod]
    public async Task TestAccountsWithTrustline()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/accountsWithTrustline.json");
        var asset = Asset.CreateNonNativeAsset("FOO",
            "GAGLYFZJMN5HEULSTH5CIGPOPAVUYPG5YSWIYDJMAPIECYEBPM2TA3QR");
        var req = server.Accounts.WithTrustline(asset);

        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts?asset=FOO%3aGAGLYFZJMN5HEULSTH5CIGPOPAVUYPG5YSWIYDJMAPIECYEBPM2TA3QR",
            req.BuildUri().ToString());
        var accounts = await req.Execute();

        Assert.AreEqual(1, accounts.Records.Count);
    }
}