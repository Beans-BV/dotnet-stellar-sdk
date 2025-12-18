using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="AccountsRequestBuilder"/> class.
/// </summary>
[TestClass]
public class AccountsRequestBuilderTest
{
    /// <summary>
    /// Verifies that AccountsRequestBuilder.BuildUri correctly constructs URI with cursor, limit, and order parameters.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithCursorLimitAndOrder_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Accounts
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/accounts?cursor=13537736921089&limit=200&order=asc",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that AccountsRequestBuilder.BuildUri correctly constructs URI with path prefix.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithPathPrefix_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://nodeapi.com/xlm/authkey/");

        // Act
        var uri = server.Accounts
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();

        // Assert
        Assert.AreEqual("https://nodeapi.com/xlm/authkey/accounts?cursor=13537736921089&limit=200&order=asc",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that AccountsRequestBuilder.Account correctly retrieves and deserializes account data.
    /// </summary>
    [TestMethod]
    public async Task Account_WithValidAccountId_ReturnsDeserializedAccount()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/account.json");

        // Act
        var account = await server.Accounts.Account("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7");

        // Assert
        AccountDeserializerTest.AssertTestData(account);
    }

    /// <summary>
    /// Verifies that AccountsRequestBuilder.AccountData correctly retrieves and decodes account data value.
    /// </summary>
    [TestMethod]
    public async Task AccountData_WithValidAccountIdAndKey_ReturnsDecodedValue()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/accountData.json");

        // Act
        var accountData =
            await server.Accounts.AccountData("GAKLBGHNHFQ3BMUYG5KU4BEWO6EYQHZHAXEWC33W34PH2RBHZDSQBD75",
                "TestValue");

        // Assert
        Assert.AreEqual("VGVzdFZhbHVl", accountData.Value);
        Assert.AreEqual("TestValue", accountData.ValueDecoded);
    }

    /// <summary>
    /// Verifies that AccountsRequestBuilder.WithSigner correctly filters accounts by signer and returns matching accounts.
    /// </summary>
    [TestMethod]
    public async Task WithSigner_WithValidSignerId_ReturnsMatchingAccounts()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/accountsWithSigner.json");

        // Act
        var req = server.Accounts
            .WithSigner("GBPOFUJUHOFTZHMZ63H5GE6NX5KVKQRD6N3I2E5AL3T2UG7HSLPLXN2K");
        var accounts = await req.Execute();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts?signer=GBPOFUJUHOFTZHMZ63H5GE6NX5KVKQRD6N3I2E5AL3T2UG7HSLPLXN2K",
            req.BuildUri().ToString());
        Assert.AreEqual(1, accounts.Records.Count);
    }

    /// <summary>
    /// Verifies that AccountsRequestBuilder.WithTrustline correctly filters accounts by trustline asset and returns matching accounts.
    /// </summary>
    [TestMethod]
    public async Task WithTrustline_WithValidAsset_ReturnsMatchingAccounts()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/accountsWithTrustline.json");
        var asset = Asset.CreateNonNativeAsset("FOO",
            "GAGLYFZJMN5HEULSTH5CIGPOPAVUYPG5YSWIYDJMAPIECYEBPM2TA3QR");

        // Act
        var req = server.Accounts.WithTrustline(asset);
        var accounts = await req.Execute();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/accounts?asset=FOO%3aGAGLYFZJMN5HEULSTH5CIGPOPAVUYPG5YSWIYDJMAPIECYEBPM2TA3QR",
            req.BuildUri().ToString());
        Assert.AreEqual(1, accounts.Records.Count);
    }
}