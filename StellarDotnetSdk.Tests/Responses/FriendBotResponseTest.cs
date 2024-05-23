using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class FriendBotResponseTest
{
    [TestMethod]
    public void TestDeserializeFriendBotResponseFailureResponse()
    {
        var jsonPath = Utils.GetTestDataPath("friendBotFail.json");
        var json = File.ReadAllText(jsonPath);
        var friendBotResponse = JsonSerializer.Deserialize<FriendBotResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(friendBotResponse);
        AssertFriendBotResponseFailureData(friendBotResponse);
    }

    [TestMethod]
    public void TestSerializeDeserializeFriendBotResponseFailureResponse()
    {
        var jsonPath = Utils.GetTestDataPath("friendBotFail.json");
        var json = File.ReadAllText(jsonPath);
        var friendBotResponse = JsonSerializer.Deserialize<FriendBotResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(friendBotResponse);
        var back = JsonSerializer.Deserialize<FriendBotResponse>(serialized);
        Assert.IsNotNull(back);
        AssertFriendBotResponseFailureData(back);
    }

    private static void AssertFriendBotResponseFailureData(FriendBotResponse friendBotResponse)
    {
        Assert.AreEqual(friendBotResponse.Type,
            "https://stellar.org/horizon-errors/https://stellar.org/horizon-errors/transaction_failed");
        Assert.AreEqual(friendBotResponse.Title, "Transaction Failed");
        Assert.AreEqual(friendBotResponse.Status, 400);
        Assert.AreEqual(friendBotResponse.Detail,
            "The transaction failed when submitted to the stellar network. The `extras.result_codes` field on this response contains further details.  Descriptions of each code can be found at: https://www.stellar.org/developers/learn/concepts/list-of-operations.html");
        Assert.AreEqual(friendBotResponse.Extras.EnvelopeXdr,
            "AAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAAAAZABiwhcAAokKAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAFOyCeoYrOjHy/ApBkkDM609BCbP1mWcPvlAF4QWUMFAAAAAXSHboAAAAAAAAAAABhlbgnAAAAEBnJNv91brx37aZVf/fW62x8Lhbqn2HZ2uckrmMTmXErhAoakrCi+qBpk2SjlE0jjHvNOXrqtHJyv7gCWui7IwL");
        Assert.AreEqual(friendBotResponse.Extras.ExtrasResultCodes.TransactionResultCode, "tx_failed");
        Assert.AreEqual(friendBotResponse.Extras.ExtrasResultCodes.OperationsResultCodes[0], "op_already_exists");
        Assert.AreEqual(friendBotResponse.Extras.ResultXdr, "AAAAAAAAAGT/////AAAAAQAAAAAAAAAA/////AAAAAA=");
    }

    [TestMethod]
    public void TestDeserializeFriendBotResponseSuccessResponse()
    {
        var jsonPath = Utils.GetTestDataPath("friendBotSuccess.json");
        var json = File.ReadAllText(jsonPath);
        var friendBotResponse = JsonSerializer.Deserialize<FriendBotResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(friendBotResponse);
        AssertSuccessTestData(friendBotResponse);
    }

    [TestMethod]
    public void TestSerializeDeserializeFriendBotResponseSuccessResponse()
    {
        var jsonPath = Utils.GetTestDataPath("friendBotSuccess.json");
        var json = File.ReadAllText(jsonPath);
        var friendBotResponse = JsonSerializer.Deserialize<FriendBotResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(friendBotResponse);
        var back = JsonSerializer.Deserialize<FriendBotResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSuccessTestData(back);
    }

    public static void AssertSuccessTestData(FriendBotResponse friendBotResponse)
    {
        Assert.AreEqual(friendBotResponse.Links.Transaction.Href,
            "https://horizon-testnet.stellar.org/transactions/fc9a175cc7b21b6c6817b587be61bf0b67a83b800973920855bd9eeb9e77f803");
        Assert.AreEqual(friendBotResponse.Hash, "fc9a175cc7b21b6c6817b587be61bf0b67a83b800973920855bd9eeb9e77f803");
        Assert.AreEqual(friendBotResponse.Ledger, 9866631L);
        Assert.AreEqual(friendBotResponse.EnvelopeXdr,
            "AAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAAAAZABiwhcAAokJAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAFOyCeoYrOjHy/ApBkkDM609BCbP1mWcPvlAF4QWUMFAAAAAXSHboAAAAAAAAAAABhlbgnAAAAEDw1ZD/c9PnaZTfXeSzx1DfDnytwyEbHBPhmM8fwjfdGZsQzyzX/3//foVdF5L+10uo+7DFBychqsabZSoyULUE");
        Assert.AreEqual(friendBotResponse.ResultXdr, "AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAA=");
        Assert.AreEqual(friendBotResponse.ResultMetaXdr,
            "AAAAAAAAAAEAAAADAAAAAACWjYcAAAAAAAAAABTsgnqGKzox8vwKQZJAzOtPQQmz9ZlnD75QBeEFlDBQAAAAF0h26AAAlo2HAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAwCWjYcAAAAAAAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAACvJ1PHMhcAYsIXAAKJCQAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAQCWjYcAAAAAAAAAABB90WssODNIgi6BHveqzxTRmIpvAFRyVNM+Hm2GVuCcAACvEAtQShcAYsIXAAKJCQAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAA");
    }
}