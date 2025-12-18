using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Federation;

namespace StellarDotnetSdk.Tests.Federation;

/// <summary>
///     Tests for Federation class functionality.
/// </summary>
[TestClass]
public class FederationTest
{
    /// <summary>
    ///     Verifies that Resolve resolves a stellar address to account ID successfully.
    /// </summary>
    [TestMethod]
    public async Task Resolve_WithValidAddress_ResolvesToAccountId()
    {
        // Arrange
        var mockFakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var httpClient = new HttpClient(mockFakeHttpMessageHandler.Object);
        var server = new FederationServer("https://api.stellar.org/federation", "stellar.org");
        server.HttpClient = httpClient;

        mockFakeHttpMessageHandler.SetupSequence(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("FEDERATION_SERVER = \"https://api.stellar.org/federation\""),
            })
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"stellar_address\":\"bob*stellar.org\",\"account_id\":\"GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY\"}"),
            });

        // Act
        var response = await StellarDotnetSdk.Federation.Federation.Resolve("bob*stellar.org");

        // Assert
        Assert.AreEqual(response.StellarAddress, "bob*stellar.org");
        Assert.AreEqual(response.AccountId, "GCW667JUHCOP5Y7KY6KGDHNPHFM4CS3FCBQ7QWDUALXTX3PGXLSOEALY");
        Assert.IsNull(response.MemoType);
        Assert.IsNull(response.Memo);
    }

    /// <summary>
    ///     Verifies that Resolve throws MalformedAddressException when address format is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(MalformedAddressException))]
    public async Task Resolve_WithMalformedAddress_ThrowsMalformedAddressException()
    {
        // Act
        var unused = await StellarDotnetSdk.Federation.Federation.Resolve("bob*stellar.org*test");
    }
}