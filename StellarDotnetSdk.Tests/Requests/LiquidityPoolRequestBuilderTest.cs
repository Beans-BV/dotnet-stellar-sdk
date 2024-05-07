using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Tests.Requests;

[TestClass]
public class LiquidityPoolRequestBuilderTest
{
    [TestMethod]
    public void TestLiquidityPools()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.LiquidityPools
            .Cursor("13537736921089")
            .Limit(200)
            .Order(OrderDirection.ASC)
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools?cursor=13537736921089&limit=200&order=asc",
            uri.ToString());
    }

    [TestMethod]
    public void TestForReserves()
    {
        using var server = new Server("https://horizon-testnet.stellar.org");
        var uri = server.LiquidityPools
            .ForReserves("EURT:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
                "PHP:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S")
            .BuildUri();
        Assert.AreEqual(
            "https://horizon-testnet.stellar.org/liquidity_pools?reserves=EURT%3aGAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S%2cPHP%3aGAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S",
            uri.ToString());
    }

    [TestMethod]
    public async Task TestLiquidityPoolExecute()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/liquidityPoolPage.json");
        var pages = await server.LiquidityPools.Execute();

        AssertTestData(pages);
    }

    [TestMethod]
    public async Task TestStream()
    {
        var jsonPath = Utils.GetTestDataPath("Responses/liquidityPool.json");
        var json = await File.ReadAllTextAsync(jsonPath);

        var streamableTest =
            new StreamableTest<LiquidityPoolResponse>(json, AssertTestData);
        await streamableTest.Run();
    }

    private static void AssertTestData(LiquidityPoolResponse pool)
    {
        Assert.AreEqual(pool.FeeBp, 30);
        Assert.AreEqual(pool.Id, "67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9");
        Assert.AreEqual(pool.PagingToken, "113725249324879873");

        Assert.AreEqual(pool.Reserves[0].Amount, "1000.0000005");
        Assert.AreEqual(pool.Reserves[0].Asset, "EURT:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S");

        Assert.AreEqual(pool.Reserves[1].Amount, "2000.0000000");
        Assert.AreEqual(pool.Reserves[1].Asset, "PHP:GAP5LETOV6YIE62YAM56STDANPRDO7ZFDBGSNHJQIYGGKSMOZAHOOS2S");

        Assert.AreEqual(pool.TotalShares, "5000.0000000");
        Assert.AreEqual(pool.TotalTrustlines, 300);
        Assert.AreEqual(pool.Type, "constant_product");

        Assert.AreEqual(pool.Links.Operations.Href,
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/operations{?cursor,limit,order}");
        Assert.AreEqual(pool.Links.Self.Href,
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9");
        Assert.AreEqual(pool.Links.Transactions.Href,
            "/liquidity_pools/67260c4c1807b262ff851b0a3fe141194936bb0215b2f77447f1df11998eabb9/transactions{?cursor,limit,order}");
    }

    private static void AssertTestData(Page<LiquidityPoolResponse> poolsPage)
    {
        Assert.AreEqual(poolsPage.Records[1].FeeBp, 30);
        Assert.AreEqual(poolsPage.Records[1].Id.ToString(),
            "b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69");
        Assert.AreEqual(poolsPage.Records[1].PagingToken,
            "b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69");

        Assert.AreEqual(poolsPage.Records[1].Reserves[0].Asset.CanonicalName(), "native");
        Assert.AreEqual(poolsPage.Records[1].Reserves[0].Amount, "0.0000000");

        Assert.AreEqual(poolsPage.Records[1].Reserves[1].Asset.CanonicalName(),
            "USDC:GAKMOAANQHJKF5735OYVSQZL6KC3VMFL4LP4ZYY2LWK256TSUG45IEFB");
        Assert.AreEqual(poolsPage.Records[1].Reserves[1].Amount, "0.0000000");

        Assert.AreEqual(poolsPage.Records[1].TotalShares, "0.0000000");
        Assert.AreEqual(poolsPage.Records[1].TotalTrustlines, "2");
        Assert.AreEqual(poolsPage.Records[1].Type,
            LiquidityPoolType.LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT);

        Assert.AreEqual(poolsPage.Records[1].Links.Operations.Href,
            "https://horizon-testnet.stellar.org/liquidity_pools/b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69/operations{?cursor,limit,order}");
        Assert.AreEqual(poolsPage.Records[1].Links.Self.Href,
            "https://horizon-testnet.stellar.org/liquidity_pools/b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69");
        Assert.AreEqual(poolsPage.Records[1].Links.Transactions.Href,
            "https://horizon-testnet.stellar.org/liquidity_pools/b26c0d6545349ad7f44ba758b7c705459537201583f2e524635be04aff84bc69/transactions{?cursor,limit,order}");
    }
}