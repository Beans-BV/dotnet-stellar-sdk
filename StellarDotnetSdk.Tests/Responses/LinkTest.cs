using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class LinkTest
{
    [TestMethod]
    public async Task TestLink()
    {
        var link = new Link<AccountResponse>(
            "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");
        Assert.AreEqual("horizon.stellar.org", link.Uri.Host);
        Assert.AreEqual("/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            string.Concat(link.Uri.Segments));

        var jsonPath = Utils.GetTestDataPath("account.json");

        var json = await File.ReadAllTextAsync(jsonPath);
        var fakeHttpClient = Utils.CreateFakeHttpClient(json);

        var response = await link.Follow(fakeHttpClient);

        AccountDeserializerTest.AssertTestData(response);
    }

    [TestMethod]
    public void TestTemplatedLink()
    {
        var link = new TemplatedLink<OperationResponse>(
            "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/operations{?cursor,limit,order}");
        Assert.AreEqual("horizon.stellar.org", link.Uri.Host);
        Assert.AreEqual("/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/operations",
            string.Concat(link.Uri.Segments));
    }

    [TestMethod]
    public void TestTemplatedLinkWithQueryParameters()
    {
        var link = new TemplatedLink<OperationResponse>(
            "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/operations{?cursor,limit,order}");
        var uri = link.Resolve(new
        {
            limit = 10,
            order = OrderDirection.DESC,
            cursor = "now",
        });

        var query = HttpUtility.ParseQueryString(uri.Query);
        Assert.AreEqual("10", query["limit"]);
        Assert.AreEqual("desc", query["order"]);
        Assert.AreEqual("now", query["cursor"]);
    }

    [TestMethod]
    public void TestTemplatedLinkWithVariable()
    {
        var link = new TemplatedLink<AccountDataResponse>(
            "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/data/{key}");

        var uri = link.Resolve(new
        {
            key = "foobar",
        });

        Assert.AreEqual(
            "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/data/foobar",
            uri.ToString());
    }
}