using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="ClaimableBalancesRequestBuilder" /> class.
/// </summary>
[TestClass]
public class ClaimableBalancesRequestBuilderTest
{
    /// <summary>
    ///     Verifies that ClaimableBalancesRequestBuilder.ForAsset correctly constructs URI with native asset parameter.
    /// </summary>
    [TestMethod]
    public void ForAsset_WithNativeAsset_BuildsCorrectUri()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.ClaimableBalances.ForAsset(new AssetTypeNative()).BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/claimable_balances?asset=native", uri.ToString());
    }

    /// <summary>
    ///     Verifies that ClaimableBalancesRequestBuilder.ForAsset correctly constructs URI with credit asset parameter.
    /// </summary>
    [TestMethod]
    public void ForAsset_WithCreditAsset_BuildsCorrectUri()
    {
        // Arrange
        var asset = Asset.CreateNonNativeAsset("ABC", "GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O");
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.ClaimableBalances.ForAsset(asset).BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances?asset=ABC%3aGBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that ClaimableBalancesRequestBuilder.ForClaimant correctly constructs URI with claimant parameter.
    /// </summary>
    [TestMethod]
    public void ForClaimant_WithValidClaimant_BuildsCorrectUri()
    {
        // Arrange
        var claimant = KeyPair.FromAccountId("GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O");
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.ClaimableBalances.ForClaimant(claimant).BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances?claimant=GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that ClaimableBalancesRequestBuilder.ForSponsor correctly constructs URI with sponsor parameter.
    /// </summary>
    [TestMethod]
    public void ForSponsor_WithValidSponsor_BuildsCorrectUri()
    {
        // Arrange
        var sponsor = KeyPair.FromAccountId("GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O");
        using var server = Utils.CreateTestServerWithContent(null);

        // Act
        var uri = server.ClaimableBalances.ForSponsor(sponsor).BuildUri();

        // Assert
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances?sponsor=GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O",
            uri.ToString());
    }

    /// <summary>
    ///     Verifies that ClaimableBalancesRequestBuilder.ClaimableBalance correctly retrieves and deserializes claimable
    ///     balance data.
    /// </summary>
    [TestMethod]
    public async Task ClaimableBalance_WithValidBalanceId_ReturnsDeserializedClaimableBalance()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/claimableBalance.json");

        // Act
        var claimableBalance =
            await server.ClaimableBalances.ClaimableBalance(
                "00000000c582697b67cbec7f9ce64f4dc67bfb2bfd26318bb9f964f4d70e3f41f650b1e6");

        // Assert
        ClaimableBalanceDeserializerTest.AssertTestData(claimableBalance);
    }
}