using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for <see cref="Link{T}" /> and <see cref="TemplatedLink{T}" /> classes.
/// </summary>
[TestClass]
public class LinkTest
{
    /// <summary>
    ///     Verifies that Link Follow method correctly follows the link and deserializes the response.
    /// </summary>
    [TestMethod]
    public async Task Follow_WithValidLink_ReturnsDeserializedResponse()
    {
        // Arrange
        var link = new Link<AccountResponse>
        {
            Href =
                "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
        };
        var jsonPath = Utils.GetTestDataPath("account.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        var fakeHttpClient = Utils.CreateFakeHttpClient(json);

        // Act
        var response = await link.Follow(fakeHttpClient);

        // Assert
        Assert.AreEqual("horizon.stellar.org", link.Uri.Host);
        Assert.AreEqual("/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7",
            string.Concat(link.Uri.Segments));
        AccountDeserializerTest.AssertTestData(response);
    }

    /// <summary>
    ///     Verifies that TemplatedLink correctly parses URI and segments from templated href.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTemplatedLink_ParsesUriAndSegmentsCorrectly()
    {
        // Arrange & Act
        var link = new TemplatedLink<OperationResponse>
        {
            Href =
                "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/operations{?cursor,limit,order}",
        };

        // Assert
        Assert.AreEqual("horizon.stellar.org", link.Uri.Host);
        Assert.AreEqual("/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/operations",
            string.Concat(link.Uri.Segments));
    }

    /// <summary>
    ///     Verifies that TemplatedLink Resolve method correctly resolves query parameters.
    /// </summary>
    [TestMethod]
    public void Resolve_WithQueryParameters_ReturnsUriWithCorrectQueryString()
    {
        // Arrange
        var link = new TemplatedLink<OperationResponse>
        {
            Href =
                "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/operations{?cursor,limit,order}",
        };

        // Act
        var uri = link.Resolve(new
        {
            limit = 10,
            order = OrderDirection.DESC,
            cursor = "now",
        });

        // Assert
        var query = HttpUtility.ParseQueryString(uri.Query);
        Assert.AreEqual("10", query["limit"]);
        Assert.AreEqual("desc", query["order"]);
        Assert.AreEqual("now", query["cursor"]);
    }

    /// <summary>
    ///     Verifies that TemplatedLink Resolve method correctly resolves path variables.
    /// </summary>
    [TestMethod]
    public void Resolve_WithPathVariable_ReturnsUriWithResolvedPath()
    {
        // Arrange
        var link = new TemplatedLink<AccountDataResponse>
        {
            Href =
                "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/data/{key}",
        };

        // Act
        var uri = link.Resolve(new
        {
            key = "foobar",
        });

        // Assert
        Assert.AreEqual(
            "https://horizon.stellar.org/accounts/GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7/data/foobar",
            uri.ToString());
    }
}