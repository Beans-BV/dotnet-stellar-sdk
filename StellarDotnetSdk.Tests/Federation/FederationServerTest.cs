using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Federation;

namespace StellarDotnetSdk.Tests.Federation;

[TestClass]
public abstract class FederationServerTest
{
    private const string StellarToml = "FEDERATIONserver = \"https://api.stellar.org/federation\"";

    [TestMethod]
    public async Task TestCreateForDomain()
    {
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(StellarToml)
        });

        using (var server = await FederationServer.CreateForDomain("stellar.org"))
        {
            Assert.AreEqual(server.ServerUri, "https://api.stellar.org/federation");
            Assert.AreEqual(server.Domain, "stellar.org");
        }

        fakeHttpMessageHandler.Verify(a => a.Send(It.IsAny<HttpRequestMessage>()));

        Assert.AreEqual(new Uri("https://stellar.org/.well-known/stellar.toml"),
            fakeHttpMessageHandler.Object.RequestUri);
    }

    [TestMethod]
    public async Task TestNameFederationSuccess()
    {
        var server =
            CreateTestServer(
                "{\"stellar_address\":\"bob*stellar.org\",\"account_id\":\"GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY\"}");
        var response = await server.ResolveAddress("bob*stellar.org");
        Assert.AreEqual(response.StellarAddress, "bob*stellar.org");
        Assert.AreEqual(response.AccountId, "GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY");
        Assert.IsNull(response.MemoType);
        Assert.IsNull(response.Memo);
    }

    [TestMethod]
    public async Task TestNameFederationSuccessWithMemo()
    {
        var server =
            CreateTestServer(
                "{\"stellar_address\":\"bob*stellar.org\",\"account_id\":\"GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY\", \"memo_type\": \"text\", \"memo\": \"test\"}");
        var response = await server.ResolveAddress("bob*stellar.org");
        Assert.AreEqual(response.StellarAddress, "bob*stellar.org");
        Assert.AreEqual(response.AccountId, "GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY");
        Assert.AreEqual(response.MemoType, "text");
        Assert.AreEqual(response.Memo, "test");
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    public async Task TestNameFederationNotFound()
    {
        var server = CreateTestServer("{\"code\":\"not_found\",\"message\":\"Account not found\"}",
            HttpStatusCode.NotFound);
        var unused = await server.ResolveAddress("bob*stellar.org");
    }

    private static FederationServer CreateTestServer(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Network.UseTestNetwork();
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content)
        };

        httpResponseMessage.Headers.Add("X-Ratelimit-Limit", "-1");
        httpResponseMessage.Headers.Add("X-Ratelimit-Remaining", "-1");
        httpResponseMessage.Headers.Add("X-Ratelimit-Reset", "-1");

        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponseMessage);
        var server = new FederationServer("https://api.stellar.org/federation", "stellar.org");

        server.HttpClient = httpClient;
        return server;
    }
}