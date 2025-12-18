using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="FriendBotRequestBuilder"/> class.
/// </summary>
[TestClass]
public class FriendBotRequestBuilderTest
{
    [TestCleanup]
    public void Cleanup()
    {
        Network.Use(null);
    }

    /// <summary>
    /// Verifies that accessing TestNetFriendBot throws NotSupportedException when network is not testnet.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void TestNetFriendBot_WhenNotTestnetNetwork_ThrowsNotSupportedException()
    {
        // Arrange
        Network.UsePublicNetwork();

        // Act & Assert
        using var server = new Server("https://horizon.stellar.org");
        var unused = server.TestNetFriendBot;
    }

    /// <summary>
    /// Verifies that accessing TestNetFriendBot does not throw exception when network is testnet.
    /// </summary>
    [TestMethod]
    public void TestNetFriendBot_WhenTestnetNetwork_DoesNotThrowException()
    {
        // Arrange
        Network.UseTestNetwork();

        // Act & Assert
        using var server = new Server("https://horizon-testnet.stellar.org");
        var unused = server.TestNetFriendBot;
    }

    /// <summary>
    /// Verifies that FriendBotRequestBuilder.FundAccount correctly constructs URI for funding account.
    /// </summary>
    [TestMethod]
    public void FundAccount_WithValidAccountId_BuildsCorrectUri()
    {
        // Arrange
        Network.UseTestNetwork();
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.TestNetFriendBot
            .FundAccount("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H")
            .BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/friendbot?addr=GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H",
            uri.ToString());
    }

    /// <summary>
    /// Verifies that FriendBotRequestBuilder.Execute correctly retrieves and deserializes friend bot response.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithValidAccountId_ReturnsDeserializedFriendBotResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/friendBotSuccess.json");

        // Act
        var friendBotResponse = await server.TestNetFriendBot
            .FundAccount("GAAZI4TCR3TY5OJHCTJC2A4QSY6CJWJH5IAJTGKIN2ER7LBNVKOCCWN7")
            .Execute();

        // Assert
        FriendBotResponseTest.AssertSuccessTestData(friendBotResponse);
    }
}