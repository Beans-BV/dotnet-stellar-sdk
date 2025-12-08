using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Responses.Effects;

[TestClass]
public class EffectDeserializerTest
{
    [TestMethod]
    public void TestDeserializeAccountCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountCreatedData(back);
    }

    public static void AssertAccountCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountCreatedEffectResponse);
        var effect = (AccountCreatedEffectResponse)instance;

        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", effect.Account);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24", effect.AccountMuxed);
        Assert.AreEqual(5123456789UL, effect.AccountMuxedId);
        Assert.AreEqual(DateTimeOffset.Parse("2025-12-01T16:05:45Z").UtcDateTime, effect.CreatedAt);
        Assert.AreEqual("30.0", effect.StartingBalance);
        Assert.AreEqual("65571265847297-1", effect.PagingToken);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/65571265847297", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265847297-1",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265847297-1",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeAccountRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountRemovedData(back);
    }

    private static void AssertAccountRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountRemovedEffectResponse);
        var effect = (AccountRemovedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.Account);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/65571265847297", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265847297-1",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265847297-1",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeAccountCreditedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountCredited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountCreditedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountCreditedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountCredited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountCreditedData(back);
    }

    private static void AssertAccountCreditedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountCreditedEffectResponse);
        var effect = (AccountCreditedEffectResponse)instance;

        Assert.AreEqual("GDLGTRIBFH24364GPWPUS45GUFC2GU4ARPGWTXVCPLGTUHX3IOS3ON47", effect.Account);
        Assert.AreEqual(new AssetTypeNative(), effect.Asset);
        Assert.AreEqual("native", effect.AssetType);
        Assert.IsNull(effect.AssetCode);
        Assert.IsNull(effect.AssetIssuer);
        Assert.AreEqual("1000.0", effect.Amount);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/13563506724865", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=13563506724865-1",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=13563506724865-1",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeAccountDebitedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountDebited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountDebitedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountDebitedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountDebited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountDebitedData(back);
    }

    private static void AssertAccountDebitedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountDebitedEffectResponse);
        var effect = (AccountDebitedEffectResponse)instance;

        Assert.AreEqual("GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H", effect.Account);
        Assert.AreEqual("credit_alphanum12", effect.AssetType);
        Assert.AreEqual("TESTTEST", effect.AssetCode);
        Assert.AreEqual("GAHXPUDP3AK6F2QQM4FIRBGPNGKLRDDSTQCVKEXXKKRHJZUUQ23D5BU7", effect.AssetIssuer);
        Assert.AreEqual(
            Asset.CreateNonNativeAsset("TESTTEST", "GAHXPUDP3AK6F2QQM4FIRBGPNGKLRDDSTQCVKEXXKKRHJZUUQ23D5BU7"),
            effect.Asset);
        Assert.AreEqual("30.0", effect.Amount);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/65571265843201", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265843201-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265843201-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeAccountThresholdsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountThresholdsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountThresholdsUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountThresholdsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountThresholdsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountThresholdsUpdatedData(back);
    }

    private static void AssertAccountThresholdsUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountThresholdsUpdatedEffectResponse);
        var effect = (AccountThresholdsUpdatedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual(2, effect.LowThreshold);
        Assert.AreEqual(3, effect.MedThreshold);
        Assert.AreEqual(4, effect.HighThreshold);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/18970870550529", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=18970870550529-1",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=18970870550529-1",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeAccountHomeDomainUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountHomeDomainUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountHomeDomainUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountHomeDomainUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountHomeDomainUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountHomeDomainUpdatedData(back);
    }

    private static void AssertAccountHomeDomainUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountHomeDomainUpdatedEffectResponse);
        var effect = (AccountHomeDomainUpdatedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual("stellar.org", effect.HomeDomain);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/18970870550529", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=18970870550529-1",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=18970870550529-1",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeAccountFlagsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        Assert.IsNotNull(instance);
        AssertAccountFlagsUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountFlagsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountFlagsUpdatedData(back);
    }

    private static void AssertAccountFlagsUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountFlagsUpdatedEffectResponse);
        var effect = (AccountFlagsUpdatedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual(false, effect.AuthRequiredFlag);
        Assert.AreEqual(true, effect.AuthRevocableFlag);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/18970870550529", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=18970870550529-1",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=18970870550529-1",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeSignerCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertSignerCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSignerCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSignerCreatedData(back);
    }

    private static void AssertSignerCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerCreatedEffectResponse);
        var effect = (SignerCreatedEffectResponse)instance;

        Assert.AreEqual("GB24LPGAHYTWRYOXIDKXLI55SBRWW42T3TZKDAAW3BOJX4ADVIATFTLU", effect.Account);
        Assert.AreEqual(1, effect.Weight);
        Assert.AreEqual("GB24LPGAHYTWRYOXIDKXLI55SBRWW42T3TZKDAAW3BOJX4ADVIATFTLU", effect.PublicKey);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/65571265859585", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265859585-3",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265859585-3",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeSignerRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertSignerRemoveData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSignerRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSignerRemoveData(back);
    }

    private static void AssertSignerRemoveData(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerRemovedEffectResponse);
        var effect = (SignerRemovedEffectResponse)instance;

        Assert.AreEqual("GCFKT6BN2FEASCEVDNHEC4LLFT2KLUUPEMKM4OJPEJ65H2AEZ7IH4RV6", effect.Account);
        Assert.AreEqual(0, effect.Weight);
        Assert.AreEqual("GCFKT6BN2FEASCEVDNHEC4LLFT2KLUUPEMKM4OJPEJ65H2AEZ7IH4RV6", effect.PublicKey);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/43658342567940", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=43658342567940-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=43658342567940-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeSignerUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertSignerUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSignerUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSignerUpdatedData(back);
    }

    private static void AssertSignerUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerUpdatedEffectResponse);
        var effect = (SignerUpdatedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual(2, effect.Weight);
        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.PublicKey);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeTrustlineCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTrustlineCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeTrustlineCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineCreatedData(back);
    }

    private static void AssertTrustlineCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineCreatedEffectResponse);
        var effect = (TrustlineCreatedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"),
            effect.Asset);
        Assert.AreEqual("credit_alphanum4", effect.AssetType);
        Assert.AreEqual("EUR", effect.AssetCode);
        Assert.AreEqual("GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA", effect.AssetIssuer);
        
        Assert.AreEqual("1000.0", effect.Limit);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeTrustlineRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTrustlineRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeTrustlineRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineRemovedData(back);
    }

    private static void AssertTrustlineRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineRemovedEffectResponse);
        var effect = (TrustlineRemovedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"),
            effect.Asset);
        Assert.AreEqual("0.0", effect.Limit);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestDeserializeTrustlineUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTrustlineUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeTrustlineUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineUpdatedData(back);
    }

    private static void AssertTrustlineUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineUpdatedEffectResponse);
        var effect = (TrustlineUpdatedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual(
            Asset.CreateNonNativeAsset("TESTTEST", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"),
            effect.Asset);
        Assert.AreEqual("100.0", effect.Limit);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    [Obsolete]
    public void TestDeserializeTrustlineAuthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTrustlineAuthorizedData(instance);
    }

    [TestMethod]
    [Obsolete]
    public void TestDeserializeTrustlineAuthorizedToMaintainLiabilitiesEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorizedToMaintainLiabilities.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTrustlineAuthorizedToMaintainLiabilitiesEffect(instance);
    }

    [TestMethod]
    [Obsolete]
    public void TestSerializeDeserializeTrustlineAuthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineAuthorizedData(back);
    }

    [Obsolete]
    private static void AssertTrustlineAuthorizedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineAuthorizedEffectResponse);
        var effect = (TrustlineAuthorizedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual("credit_alphanum12", effect.AssetType);
        Assert.AreEqual("TESTTEST", effect.AssetCode);
        Assert.AreEqual("GB3E4AB4VWXJDUVN4Z3CPBU5HTMWVEQXONZYVDFMHQD6333KHCOL3UBR", effect.Trustor);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    [Obsolete]
    private static void AssertTrustlineAuthorizedToMaintainLiabilitiesEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineAuthorizedToMaintainLiabilitiesEffectResponse);
        var effect = (TrustlineAuthorizedToMaintainLiabilitiesEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    [Obsolete]
    public void TestDeserializeTrustlineDeauthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineDeAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertTrustlineDeauthorizedData(instance);
    }

    [TestMethod]
    [Obsolete]
    public void TestSerializeDeserializeTrustlineDeauthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineDeAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineDeauthorizedData(back);
    }

    [Obsolete]
    private static void AssertTrustlineDeauthorizedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineDeauthorizedEffectResponse);
        var effect = (TrustlineDeauthorizedEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual("credit_alphanum4", effect.AssetType);
        Assert.AreEqual("EUR", effect.AssetCode);
        Assert.AreEqual("GB3E4AB4VWXJDUVN4Z3CPBU5HTMWVEQXONZYVDFMHQD6333KHCOL3UBR", effect.Trustor);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    [TestMethod]
    public void TestAlphaNum12NativeTradeEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectTradeAlphaNum12Native.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAlphaNum12NativeTradeData(instance);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAlphaNum12NativeTradeData(back);
    }

    [TestMethod]
    public void TestNativeAlphaNum4TradeEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectTradeNativeAphaNum4.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertNativeAphaNum4TradeData(instance);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertNativeAphaNum4TradeData(back);
    }

    private static void AssertOtherTradeData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TradeEffectResponse);
        var effect = (TradeEffectResponse)instance;

        Assert.AreEqual("GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO", effect.Account);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", effect.Seller);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24", effect.SellerMuxed);
        Assert.AreEqual(5123456789UL, effect.SellerMuxedId);
        Assert.AreEqual("1", effect.OfferId);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    private static void AssertAlphaNum12NativeTradeData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TradeEffectResponse);
        var effect = (TradeEffectResponse)instance;
        Assert.AreEqual("1000.0", effect.SoldAmount);
        Assert.AreEqual(
            Asset.CreateNonNativeAsset("TESTTEST", "GAHXPUDP3AK6F2QQM4FIRBGPNGKLRDDSTQCVKEXXKKRHJZUUQ23D5BU7"),
            effect.SoldAsset);
        Assert.AreEqual(DateTimeOffset.Parse("2025-12-01T16:05:45Z").UtcDateTime, effect.CreatedAt);
        Assert.AreEqual("60.0", effect.BoughtAmount);
        Assert.AreEqual("native", effect.BoughtAssetType);
        Assert.IsNull(effect.BoughtAssetCode);
        Assert.IsNull(effect.BoughtAssetIssuer);
        Assert.AreEqual(new AssetTypeNative(), effect.BoughtAsset);

        AssertOtherTradeData(instance);
    }

    private static void AssertNativeAphaNum4TradeData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TradeEffectResponse);
        var effect = (TradeEffectResponse)instance;
        Assert.AreEqual("1000.0", effect.SoldAmount);
        Assert.AreEqual("native", effect.SoldAssetType);
        Assert.IsNull(effect.SoldAssetIssuer);
        Assert.IsNull(effect.SoldAssetCode);
        Assert.AreEqual(new AssetTypeNative(), effect.SoldAsset);

        Assert.AreEqual(Asset.CreateNonNativeAsset("DOGX", "GAOI7QQAXO37FH64LLGRFQIYTYUTMSMZKBBDIGNEDOMSW7SLGA4LDOGX"),
            effect.BoughtAsset);
        Assert.AreEqual("credit_alphanum4", effect.BoughtAssetType);
        Assert.AreEqual("DOGX", effect.BoughtAssetCode);
        Assert.AreEqual("GAOI7QQAXO37FH64LLGRFQIYTYUTMSMZKBBDIGNEDOMSW7SLGA4LDOGX", effect.BoughtAssetIssuer);
        Assert.AreEqual("60.0", effect.BoughtAmount);

        AssertOtherTradeData(instance);
    }

    [TestMethod]
    public void TestDeserializeAccountInflationUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountInflationUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountInflationUpdated(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountInflationUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountInflationUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountInflationUpdated(back);
    }

    private static void AssertAccountInflationUpdated(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountInflationDestinationUpdatedEffectResponse);
        var effect = (AccountInflationDestinationUpdatedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
    }

    [TestMethod]
    public void TestDeserializeDataCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertDataCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeDataCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertDataCreatedData(back);
    }

    private static void AssertDataCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataCreatedEffectResponse);
        var effect = (DataCreatedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
        Assert.AreEqual("my key", effect.Name);
        Assert.AreEqual("dGVzdC5jb20=", effect.Value);
    }

    [TestMethod]
    public void TestDeserializeDataRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertDataRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeDataRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertDataRemovedData(back);
    }

    private static void AssertDataRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataRemovedEffectResponse);
        var effect = (DataRemovedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
        Assert.AreEqual("my key", effect.Name);
    }

    [TestMethod]
    public void TestDeserializeDataUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertDataUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeDataUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertDataUpdatedData(back);
    }

    private static void AssertDataUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataUpdatedEffectResponse);
        var effect = (DataUpdatedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
        Assert.AreEqual("my key", effect.Name);
        Assert.AreEqual("Mg==", effect.Value);
    }

    [TestMethod]
    public void TestUnknownEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectUnknown.json");
        var json = File.ReadAllText(jsonPath);
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions));
    }

    [TestMethod]
    public void TestDeserializeSequenceBumpedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSequenceBumped.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertSequenceBumpedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSequenceBumpedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSequenceBumped.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSequenceBumpedData(back);
    }

    private static void AssertSequenceBumpedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is SequenceBumpedEffectResponse);
        var effect = (SequenceBumpedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
        Assert.AreEqual(79473726952833048L, effect.NewSequence);
    }

    [TestMethod]
    public void TestDeserializeOfferCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertOfferCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeOfferCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertOfferCreatedData(back);
    }

    private static void AssertOfferCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is OfferCreatedEffectResponse);
        var effect = (OfferCreatedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
    }

    [TestMethod]
    public void TestDeserializeOfferRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertOfferRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeOfferRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertOfferRemovedData(back);
    }

    private static void AssertOfferRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is OfferRemovedEffectResponse);
        var effect = (OfferRemovedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
    }

    [TestMethod]
    public void TestDeserializeOfferUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertOfferUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeOfferUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertOfferUpdatedData(back);
    }

    private static void AssertOfferUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is OfferUpdatedEffectResponse);
        var effect = (OfferUpdatedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
    }

    [TestMethod]
    public void TestSerializationAccountSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountSponsorshipCreatedData(back);
    }

    private static void AssertAccountSponsorshipCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipCreatedEffectResponse);
        var effect = (AccountSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.Sponsor);
    }

    [TestMethod]
    public void TestSerializationAccountSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountSponsorshipRemovedData(back);
    }

    private static void AssertAccountSponsorshipRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipRemovedEffectResponse);
        var effect = (AccountSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
    }

    [TestMethod]
    public void TestSerializationAccountSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountSponsorshipUpdatedData(back);
    }

    private static void AssertAccountSponsorshipUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipUpdatedEffectResponse);
        var effect = (AccountSponsorshipUpdatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
        Assert.AreEqual("GBVFLWXYCIGPO3455XVFIKHS66FCT5AI64ZARKS7QJN4NF7K5FOXTJNL", effect.NewSponsor);
    }

    [TestMethod]
    public void TestSerializationClaimableBalanceClaimantCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClaimantCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertClaimableBalanceClaimantCreatedEffect(back);
    }

    private static void AssertClaimableBalanceClaimantCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceClaimantCreatedEffectResponse);
        var effect = (ClaimableBalanceClaimantCreatedEffectResponse)instance;

        Assert.AreEqual("native", effect.Asset);
        Assert.AreEqual("00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0", effect.BalanceId);
        Assert.AreEqual("1.0000000", effect.Amount);
        Assert.IsNotNull(effect.Predicate);
        Assert.IsNotNull(effect.Predicate.ToClaimPredicate());
    }


    [TestMethod]
    public void TestSerializationClaimableBalanceClaimedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClaimed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertClaimableBalanceClaimedEffect(back);
    }

    private static void AssertClaimableBalanceClaimedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceClaimedEffectResponse);
        var effect = (ClaimableBalanceClaimedEffectResponse)instance;

        Assert.AreEqual("native", effect.Asset);
        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7", effect.BalanceId);
        Assert.AreEqual("1.0000000", effect.Amount);
    }

    [TestMethod]
    public void TestSerializationClaimableBalanceCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertClaimableBalanceCreatedEffect(back);
    }

    private static void AssertClaimableBalanceCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceCreatedEffectResponse);
        var effect = (ClaimableBalanceCreatedEffectResponse)instance;

        Assert.AreEqual("native", effect.Asset);
        Assert.AreEqual("00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0", effect.BalanceId);
        Assert.AreEqual("1.0000000", effect.Amount);
    }

    [TestMethod]
    public void TestSerializationClaimableBalanceSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertClaimableBalanceSponsorshipCreatedEffect(back);
    }

    private static void AssertClaimableBalanceSponsorshipCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceSponsorshipCreatedEffectResponse);
        var effect = (ClaimableBalanceSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0", effect.BalanceId);
        Assert.AreEqual("GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.Sponsor);
    }

    [TestMethod]
    public void TestSerializationClaimableBalanceSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertClaimableBalanceSponsorshipRemovedEffect(back);
    }

    private static void AssertClaimableBalanceSponsorshipRemovedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceSponsorshipRemovedEffectResponse);
        var effect = (ClaimableBalanceSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0", effect.BalanceId);
        Assert.AreEqual("GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.FormerSponsor);
    }

    [TestMethod]
    public void TestSerializationClaimableBalanceSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertClaimableBalanceSponsorshipUpdatedEffect(back);
    }

    private static void AssertClaimableBalanceSponsorshipUpdatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceSponsorshipUpdatedEffectResponse);
        var effect = (ClaimableBalanceSponsorshipUpdatedEffectResponse)instance;

        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7", effect.BalanceId);
        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
        Assert.AreEqual("GBVFLWXYCIGPO3455XVFIKHS66FCT5AI64ZARKS7QJN4NF7K5FOXTJNL", effect.NewSponsor);
    }

    [TestMethod]
    public void TestSerializationSignerSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSignerSponsorshipCreatedEffect(back);
    }

    private static void AssertSignerSponsorshipCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerSponsorshipCreatedEffectResponse);
        var effect = (SignerSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("XAMF7DNTEJY74JPVMGTPZE4LFYTEGBXMGBHNUUMAA7IXMSBGHAMWSND6", effect.Signer);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.Sponsor);
    }

    [TestMethod]
    public void TestSerializationSignerSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSignerSponsorshipRemovedEffect(back);
    }

    private static void AssertSignerSponsorshipRemovedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerSponsorshipRemovedEffectResponse);
        var effect = (SignerSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("XAMF7DNTEJY74JPVMGTPZE4LFYTEGBXMGBHNUUMAA7IXMSBGHAMWSND6", effect.Signer);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.FormerSponsor);
    }

    [TestMethod]
    public void TestSerializationSignerSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertSignerSponsorshipUpdatedEffect(back);
    }

    private static void AssertSignerSponsorshipUpdatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerSponsorshipUpdatedEffectResponse);
        var effect = (SignerSponsorshipUpdatedEffectResponse)instance;

        Assert.AreEqual("XAMF7DNTEJY74JPVMGTPZE4LFYTEGBXMGBHNUUMAA7IXMSBGHAMWSND6", effect.Signer);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.FormerSponsor);
        Assert.AreEqual("GB5N4275ETC6A77K4DTDL3EFAQMN66PC7UITDUZUBM7Y6LDJP7EYSGOB", effect.NewSponsor);
    }

    [TestMethod]
    public void TestSerializationTrustlineSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineSponsorshipCreatedEffect(back);
    }

    private static void AssertTrustlineSponsorshipCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineSponsorshipCreatedEffectResponse);
        var effect = (TrustlineSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("ABC:GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.Asset);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.Sponsor);
    }

    [TestMethod]
    public void TestSerializationTrustlineSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineSponsorshipRemovedEffect(back);
    }

    private static void AssertTrustlineSponsorshipRemovedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineSponsorshipRemovedEffectResponse);
        var effect = (TrustlineSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("ABC:GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.Asset);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.FormerSponsor);
    }

    [TestMethod]
    public void TestSerializationTrustlineSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineSponsorshipUpdatedEffect(back);
    }

    private static void AssertTrustlineSponsorshipUpdatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineSponsorshipUpdatedEffectResponse);
        var effect = (TrustlineSponsorshipUpdatedEffectResponse)instance;

        Assert.AreEqual("XYZ:GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.Asset);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.FormerSponsor);
        Assert.AreEqual("GB5N4275ETC6A77K4DTDL3EFAQMN66PC7UITDUZUBM7Y6LDJP7EYSGOB", effect.NewSponsor);
    }

    [TestMethod]
    public void TestSerializationDataSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertDataSponsorshipCreatedData(back);
    }

    private static void AssertDataSponsorshipCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataSponsorshipCreatedEffectResponse);
        var effect = (DataSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.Sponsor);
        Assert.AreEqual("welcome-friend", effect.DataName);
    }

    [TestMethod]
    public void TestSerializationDataSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertDataSponsorshipRemovedData(back);
    }

    private static void AssertDataSponsorshipRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataSponsorshipRemovedEffectResponse);
        var effect = (DataSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
        Assert.AreEqual("welcome-friend", effect.DataName);
    }

    [TestMethod]
    public void TestSerializationDataSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertDataSponsorshipUpdatedData(back);
    }

    private static void AssertDataSponsorshipUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataSponsorshipUpdatedEffectResponse);
        var effect = (DataSponsorshipUpdatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
        Assert.AreEqual("GBVFLWXYCIGPO3455XVFIKHS66FCT5AI64ZARKS7QJN4NF7K5FOXTJNL", effect.NewSponsor);
        Assert.AreEqual("welcome-friend", effect.DataName);
    }

    [TestMethod]
    public void TestSerializationTrustlineFlagsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertTrustlineFlagsUpdatedEffect(back);
    }

    private static void AssertTrustlineFlagsUpdatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineFlagsUpdatedEffectResponse);
        var effect = (TrustlineFlagsUpdatedEffectResponse)instance;

        Assert.AreEqual("credit_alphanum4", effect.AssetType);
        Assert.AreEqual("EUR", effect.AssetCode);
        Assert.AreEqual("GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM", effect.AssetIssuer);
        Assert.AreEqual("GDZ55LVXECRTW4G36EZPTHI4XIYS5JUC33TUS22UOETVFVOQ77JXWY4F", effect.Trustor);
        Assert.IsTrue(effect.AuthorizedFlag);
        Assert.IsTrue(effect.AuthorizedToMaintainLiabilities);
        Assert.IsTrue(effect.ClawbackEnabledFlag);
    }

    [TestMethod]
    public void TestSerializationClaimableBalanceClawedBackEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClawedBack.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertClaimableBalanceClawedBackEffect(back);
    }

    private static void AssertClaimableBalanceClawedBackEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceClawedBackEffectResponse);
        var effect = (ClaimableBalanceClawedBackEffectResponse)instance;

        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7", effect.BalanceId);
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolCreatedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);

        Assert.IsTrue(back is LiquidityPoolCreatedEffectResponse);
        var effect = (LiquidityPoolCreatedEffectResponse)back;

        Assert.AreEqual(1278881UL, effect.AccountMuxedId);
        Assert.IsNotNull(effect.LiquidityPool);
        Assert.AreEqual("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385",
            effect.LiquidityPool.Id.ToString());

        Assert.AreEqual(30, effect.LiquidityPool.FeeBp);
        Assert.AreEqual(1, effect.LiquidityPool.TotalTrustlines);
        Assert.AreEqual("0.0000000", effect.LiquidityPool.TotalShares);

        Assert.AreEqual("native", effect.LiquidityPool.Reserves[0].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", effect.LiquidityPool.Reserves[0].Amount);

        Assert.AreEqual("TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25",
            effect.LiquidityPool.Reserves[1].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", effect.LiquidityPool.Reserves[1].Amount);
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolDepositedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolDepositedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolDepositedEffectResponse);
        var effect = (LiquidityPoolDepositedEffectResponse)back;

        Assert.AreEqual(1278881UL, effect.AccountMuxedId);
        Assert.IsNotNull(effect.LiquidityPool);
        Assert.AreEqual(effect.LiquidityPool.Id.ToString(),
            "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385");

        Assert.AreEqual(30, effect.LiquidityPool.FeeBp);
        Assert.AreEqual(1, effect.LiquidityPool.TotalTrustlines);
        Assert.AreEqual("1500.0000000", effect.LiquidityPool.TotalShares);
        Assert.IsNotNull(effect.ReservesDeposited);
        Assert.AreEqual("native", effect.ReservesDeposited[0].Asset.CanonicalName());
        Assert.AreEqual("123.456789", effect.ReservesDeposited[0].Amount);

        Assert.AreEqual("TEST:GD5Y3PMKI46MPILDG4OQP4SGFMRNKYEPJVDAPR3P3I2BMZ3O7IX6DB2Y",
            effect.ReservesDeposited[1].Asset.CanonicalName());
        Assert.AreEqual("478.7867966", effect.ReservesDeposited[1].Amount);

        Assert.AreEqual("250.0000000", effect.SharesReceived);
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolRemovedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolRemovedEffectResponse);
        var effect = (LiquidityPoolRemovedEffectResponse)back;

        Assert.AreEqual(1278881UL, effect.AccountMuxedId);
        Assert.IsNotNull(effect.LiquidityPoolId);
        Assert.AreEqual("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385",
            effect.LiquidityPoolId.ToString());
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolRevokedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolRevokedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolRevokedEffectResponse);
        var effect = (LiquidityPoolRevokedEffectResponse)back;

        Assert.AreEqual(1278881UL, effect.AccountMuxedId);
        Assert.IsNotNull(effect.LiquidityPool);
        Assert.AreEqual("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385",
            effect.LiquidityPool.Id.ToString());

        Assert.AreEqual(30, effect.LiquidityPool.FeeBp);
        Assert.AreEqual(1, effect.LiquidityPool.TotalTrustlines);
        Assert.AreEqual("0.0000000", effect.LiquidityPool.TotalShares);

        Assert.AreEqual("native", effect.LiquidityPool.Reserves[0].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", effect.LiquidityPool.Reserves[0].Amount);

        Assert.AreEqual("TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25",
            effect.LiquidityPool.Reserves[1].Asset.CanonicalName());
        Assert.AreEqual("0.0000000", effect.LiquidityPool.Reserves[1].Amount);
        Assert.IsNotNull(effect.ReservesRevoked);
        var asset = effect.ReservesRevoked[0].Asset;
        Assert.IsNotNull(asset);
        Assert.AreEqual("TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25", asset.CanonicalName());
        Assert.AreEqual("1500.0000000", effect.ReservesRevoked[0].Amount);
        Assert.AreEqual("00000000836f572dd43b76853df6c88ca1b89394b547d74de0c87334ce7f9270cb342203",
            effect.ReservesRevoked[0].ClaimableBalanceId);

        Assert.AreEqual("100.0000000", effect.SharesRevoked);
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolTradeEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolTradeEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolTradeEffectResponse);
        var effect = (LiquidityPoolTradeEffectResponse)back;
        Assert.IsNotNull(effect.LiquidityPool);
        Assert.AreEqual("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385",
            effect.LiquidityPool.Id.ToString());
        Assert.IsNotNull(effect.Sold);
        Assert.AreEqual("TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25",
            effect.Sold.Asset.CanonicalName());
        Assert.AreEqual("93.1375850", effect.Sold.Amount);
        Assert.IsNotNull(effect.Bought);
        Assert.AreEqual("TEST2:GDQ4273UBKSHIE73RJB5KLBBM7W3ESHWA74YG7ZBXKZLKT5KZGPKKB7E",
            effect.Bought.Asset.CanonicalName());
        Assert.AreEqual("100.0000000", effect.Bought.Amount);
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolWithdrewEffectEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolWithdrewEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolWithdrewEffectResponse);
        var effect = (LiquidityPoolWithdrewEffectResponse)back;
        Assert.IsNotNull(effect.LiquidityPool);

        Assert.AreEqual("MDB2ATEJPYNF2KNSK7YZ5C5J5IF2OLYMWCFQ2AAERYKV7Y5BE3XT2AAAAAAAAE4DUGUA4", effect.AccountMuxed);
        Assert.AreEqual(1278881UL, effect.AccountMuxedId);
        Assert.IsNotNull(effect.LiquidityPool);
        Assert.AreEqual("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385",
            effect.LiquidityPool.Id.ToString());

        Assert.AreEqual(30, effect.LiquidityPool.FeeBp);
        Assert.AreEqual(1, effect.LiquidityPool.TotalTrustlines);
        Assert.AreEqual("1500.0000000", effect.LiquidityPool.TotalShares);
        Assert.IsNotNull(effect.ReservesReceived);
        Assert.AreEqual("native", effect.ReservesReceived[0].Asset.CanonicalName());
        Assert.AreEqual("123.456789", effect.ReservesReceived[0].Amount);

        Assert.AreEqual("TEST:GD5Y3PMKI46MPILDG4OQP4SGFMRNKYEPJVDAPR3P3I2BMZ3O7IX6DB2Y",
            effect.ReservesReceived[1].Asset.CanonicalName());
        Assert.AreEqual("478.7867966", effect.ReservesReceived[1].Amount);

        Assert.AreEqual("250.0000000", effect.SharesRedeemed);
    }
}