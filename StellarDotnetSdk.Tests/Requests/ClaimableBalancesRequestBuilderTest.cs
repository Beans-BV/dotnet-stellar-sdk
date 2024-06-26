using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class ClaimableBalancesRequestBuilderTest
{
    [TestMethod]
    public void TestForNativeAsset()
    {
        var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.ClaimableBalances.ForAsset(new AssetTypeNative()).BuildUri();
        Assert.AreEqual("https://horizon-testnet.stellar.org/claimable_balances?asset=native", uri.ToString());
    }

    [TestMethod]
    public void TestForCreditAsset()
    {
        var asset = Asset.CreateNonNativeAsset("ABC", "GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O");
        var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.ClaimableBalances.ForAsset(asset).BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances?asset=ABC%3aGBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O",
            uri.ToString());
    }

    [TestMethod]
    public void TestForClaimant()
    {
        var claimant = KeyPair.FromAccountId("GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O");
        var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.ClaimableBalances.ForClaimant(claimant).BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances?claimant=GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O",
            uri.ToString());
    }

    [TestMethod]
    public void TestForSponsor()
    {
        var sponsor = KeyPair.FromAccountId("GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O");
        using var server = Utils.CreateTestServerWithContent(null);
        var uri = server.ClaimableBalances.ForSponsor(sponsor).BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/claimable_balances?sponsor=GBM2LMVS2EG3GHJ5DKR7CKZ4TP6DQKCHRMDKCZK6WG2NGQVTLF35YE6O",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestClaimableBalance()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/claimableBalance.json");
        var claimableBalance =
            await server.ClaimableBalances.ClaimableBalance(
                "00000000c582697b67cbec7f9ce64f4dc67bfb2bfd26318bb9f964f4d70e3f41f650b1e6");
        ClaimableBalanceDeserializerTest.AssertTestData(claimableBalance);
    }
}