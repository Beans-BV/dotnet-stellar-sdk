using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0038.Responses;

namespace StellarDotnetSdk.Tests.Sep.Sep0038;

/// <summary>
///     Reads the SEP-38 responses through a source-generated <see cref="JsonSerializerContext" />, as a trimmed or
///     Native AOT consumer would. Such a context carries metadata only for the types it can see from its roots, so a
///     property converter that delegates for any other type (<c>T[]</c>, <c>IEnumerable&lt;T&gt;</c>) fails with
///     <see cref="System.NotSupportedException" /> here. Converter accessibility is checked separately, by reflection
///     in <see cref="QuoteServiceTest" />, because this assembly sees the SDK's internals.
/// </summary>
[TestClass]
public class SourceGeneratedContextTest
{
    private static string ReadTestData(string fileName)
    {
        return File.ReadAllText(Utils.GetTestDataAbsolutePath(fileName));
    }

    [TestMethod]
    public void QuoteResponse_ReadsThroughASourceGeneratedContext()
    {
        var quote = JsonSerializer.Deserialize(ReadTestData("quote-response.json"),
            Sep38SourceGenContext.Default.QuoteResponse)!;

        Assert.AreEqual("5.00", quote.Price.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.AreEqual(System.TimeSpan.Zero, quote.ExpiresAt.Offset);
        Assert.AreEqual(3, quote.Fee.Details!.Count);
        Assert.AreEqual("PIX fee", quote.Fee.Details[0].Name);
    }

    [TestMethod]
    public void InfoAndPricesResponses_ReadThroughASourceGeneratedContext()
    {
        var info = JsonSerializer.Deserialize(ReadTestData("info-response.json"),
            Sep38SourceGenContext.Default.InfoResponse)!;
        var prices = JsonSerializer.Deserialize(ReadTestData("prices-sell-response.json"),
            Sep38SourceGenContext.Default.PricesResponse)!;

        Assert.IsTrue(info.Assets.Count > 0);
        Assert.IsTrue(info.Assets.Any(a => a.SellDeliveryMethods is { Count: > 0 }));
        Assert.AreEqual(2, prices.BuyAssets!.Count);
        Assert.AreEqual(0.1795000m, prices.BuyAssets[1].Price);
    }

    [TestMethod]
    public void NullListElement_IsRejectedThroughASourceGeneratedContext()
    {
        var json = ReadTestData("info-response.json").Replace("\"assets\": [", "\"assets\": [null, ");

        var ex = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize(json, Sep38SourceGenContext.Default.InfoResponse));

        StringAssert.Contains(ex.Message, "null element at index 0");
    }

    [TestMethod]
    public void QuoteResponse_RoundTripsThroughASourceGeneratedContext()
    {
        var context = Sep38SourceGenContext.Default;
        var quote = JsonSerializer.Deserialize(ReadTestData("quote-response.json"), context.QuoteResponse)!;

        var again = JsonSerializer.Deserialize(JsonSerializer.Serialize(quote, context.QuoteResponse),
            context.QuoteResponse)!;

        Assert.AreEqual(quote.Price, again.Price);
        Assert.AreEqual(quote.ExpiresAt, again.ExpiresAt);
        CollectionAssert.AreEqual(quote.Fee.Details!.Select(d => d.Name).ToList(),
            again.Fee.Details!.Select(d => d.Name).ToList());
    }
}

[JsonSerializable(typeof(QuoteResponse))]
[JsonSerializable(typeof(InfoResponse))]
[JsonSerializable(typeof(PricesResponse))]
[JsonSerializable(typeof(PriceResponse))]
internal partial class Sep38SourceGenContext : JsonSerializerContext
{
}
