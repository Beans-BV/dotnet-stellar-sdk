using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Federation;

namespace StellarDotnetSdk.Tests.Federation;

[TestClass]
public class FederationTest
{
    [TestMethod]
    public async Task TestResolveSuccess()
    {
        var mockFakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var httpClient = new HttpClient(mockFakeHttpMessageHandler.Object);
        var server = new FederationServer("https://api.stellar.org/federation", "stellar.org");
        server.HttpClient = httpClient;

        mockFakeHttpMessageHandler.SetupSequence(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("FEDERATION_SERVER = \"https://api.stellar.org/federation\"")
            })
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"stellar_address\":\"bob*stellar.org\",\"account_id\":\"GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY\"}")
            });

        var response = await StellarDotnetSdk.Federation.Federation.Resolve("bob*stellar.org");
        Assert.AreEqual(response.StellarAddress, "bob*stellar.org");
        Assert.AreEqual(response.AccountId, "GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY");
        Assert.IsNull(response.MemoType);
        Assert.IsNull(response.Memo);
    }

    [TestMethod]
    [ExpectedException(typeof(MalformedAddressException))]
    public async Task TestMalformedAddress()
    {
        var unused = await StellarDotnetSdk.Federation.Federation.Resolve("bob*stellar.org*test");
    }
}