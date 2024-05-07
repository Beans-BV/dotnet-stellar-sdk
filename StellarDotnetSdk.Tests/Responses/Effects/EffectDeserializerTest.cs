using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
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
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountCreatedData(back);
    }

    public static void AssertAccountCreatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is AccountCreatedEffectResponse);
        var effect = (AccountCreatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(effect.AccountMuxed, "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(effect.AccountMuxedId, 5123456789UL);
        Assert.AreEqual(effect.StartingBalance, "30.0");
        Assert.AreEqual(effect.PagingToken, "65571265847297-1");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/65571265847297");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265847297-1");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265847297-1");

        var back = new AccountCreatedEffectResponse
        {
            StartingBalance = effect.StartingBalance
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeAccountRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountRemovedData(back);
    }

    private static void AssertAccountRemovedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is AccountRemovedEffectResponse);
        var effect = (AccountRemovedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/65571265847297");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265847297-1");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265847297-1");
    }

    [TestMethod]
    public void TestDeserializeAccountCreditedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountCredited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountCreditedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountCreditedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountCredited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountCreditedData(back);
    }

    private static void AssertAccountCreditedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is AccountCreditedEffectResponse);
        var effect = (AccountCreditedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GDLGTRIBFH24364GPWPUS45GUFC2GU4ARPGWTXVCPLGTUHX3IOS3ON47");
        Assert.AreEqual(effect.Asset, new AssetTypeNative());
        Assert.AreEqual(effect.Amount, "1000.0");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/13563506724865");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=13563506724865-1");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=13563506724865-1");

        var back = new AccountCreditedEffectResponse
        {
            Amount = effect.Amount,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode,
            AssetIssuer = effect.AssetIssuer
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeAccountDebitedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountDebited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountDebitedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountDebitedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountDebited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountDebitedData(back);
    }

    private static void AssertAccountDebitedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is AccountDebitedEffectResponse);
        var effect = (AccountDebitedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GBRPYHIL2CI3FNQ4BXLFMNDLFJUNPU2HY3ZMFSHONUCEOASW7QC7OX2H");
        Assert.AreEqual(effect.Asset, new AssetTypeNative());
        Assert.AreEqual(effect.Amount, "30.0");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/65571265843201");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265843201-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265843201-2");

        var back = new AccountDebitedEffectResponse
        {
            Amount = effect.Amount,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode,
            AssetIssuer = effect.AssetIssuer
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeAccountThresholdsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountThresholdsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountThresholdsUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountThresholdsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountThresholdsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountThresholdsUpdatedData(back);
    }

    private static void AssertAccountThresholdsUpdatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is AccountThresholdsUpdatedEffectResponse);
        var effect = (AccountThresholdsUpdatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.LowThreshold, 2);
        Assert.AreEqual(effect.MedThreshold, 3);
        Assert.AreEqual(effect.HighThreshold, 4);

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/18970870550529");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=18970870550529-1");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=18970870550529-1");

        var back = new AccountThresholdsUpdatedEffectResponse
        {
            LowThreshold = effect.LowThreshold,
            MedThreshold = effect.MedThreshold,
            HighThreshold = effect.HighThreshold
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeAccountHomeDomainUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountHomeDomainUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountHomeDomainUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountHomeDomainUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountHomeDomainUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountHomeDomainUpdatedData(back);
    }

    private static void AssertAccountHomeDomainUpdatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is AccountHomeDomainUpdatedEffectResponse);
        var effect = (AccountHomeDomainUpdatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.HomeDomain, "stellar.org");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/18970870550529");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=18970870550529-1");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=18970870550529-1");

        var back = new AccountHomeDomainUpdatedEffectResponse { HomeDomain = effect.HomeDomain };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeAccountFlagsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);

        Assert.IsNotNull(instance);
        AssertAccountFlagsUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountFlagsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountFlagsUpdatedData(back);
    }

    private static void AssertAccountFlagsUpdatedData(EffectResponse instance)
    {
        // There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is AccountFlagsUpdatedEffectResponse);
        var effect = (AccountFlagsUpdatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.AuthRequiredFlag, false);
        Assert.AreEqual(effect.AuthRevocableFlag, true);

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/18970870550529");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=18970870550529-1");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=18970870550529-1");

        var back = new AccountFlagsUpdatedEffectResponse
        {
            AuthRequiredFlag = effect.AuthRequiredFlag,
            AuthRevocableFlag = effect.AuthRevocableFlag
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeSignerCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertSignerCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSignerCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSignerCreatedData(back);
    }

    private static void AssertSignerCreatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is SignerCreatedEffectResponse);
        var effect = (SignerCreatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GB24LPGAHYTWRYOXIDKXLI55SBRWW42T3TZKDAAW3BOJX4ADVIATFTLU");
        Assert.AreEqual(effect.Weight, 1);
        Assert.AreEqual(effect.PublicKey, "GB24LPGAHYTWRYOXIDKXLI55SBRWW42T3TZKDAAW3BOJX4ADVIATFTLU");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/65571265859585");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=65571265859585-3");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=65571265859585-3");

        var back = new SignerCreatedEffectResponse
        {
            Weight = effect.Weight,
            PublicKey = effect.PublicKey
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeSignerRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertSignerRemoveData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSignerRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSignerRemoveData(back);
    }

    private static void AssertSignerRemoveData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is SignerRemovedEffectResponse);
        var effect = (SignerRemovedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GCFKT6BN2FEASCEVDNHEC4LLFT2KLUUPEMKM4OJPEJ65H2AEZ7IH4RV6");
        Assert.AreEqual(effect.Weight, 0);
        Assert.AreEqual(effect.PublicKey, "GCFKT6BN2FEASCEVDNHEC4LLFT2KLUUPEMKM4OJPEJ65H2AEZ7IH4RV6");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/43658342567940");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=43658342567940-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=43658342567940-2");

        var back = new SignerRemovedEffectResponse
        {
            Weight = effect.Weight,
            PublicKey = effect.PublicKey
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeSignerUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertSignerUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSignerUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSignerUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSignerUpdatedData(back);
    }

    private static void AssertSignerUpdatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is SignerUpdatedEffectResponse);
        var effect = (SignerUpdatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.Weight, 2);
        Assert.AreEqual(effect.PublicKey, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new SignerUpdatedEffectResponse
        {
            Weight = effect.Weight,
            PublicKey = effect.PublicKey
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeTrustlineCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTrustlineCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeTrustlineCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTrustlineCreatedData(back);
    }

    private static void AssertTrustlineCreatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TrustlineCreatedEffectResponse);
        var effect = (TrustlineCreatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.Asset,
            Asset.CreateNonNativeAsset("EUR", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"));
        Assert.AreEqual(effect.Limit, "1000.0");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TrustlineCreatedEffectResponse
        {
            Limit = effect.Limit,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode,
            AssetIssuer = effect.AssetIssuer
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeTrustlineRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTrustlineRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeTrustlineRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTrustlineRemovedData(back);
    }

    private static void AssertTrustlineRemovedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TrustlineRemovedEffectResponse);
        var effect = (TrustlineRemovedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.Asset,
            Asset.CreateNonNativeAsset("EUR", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"));
        Assert.AreEqual(effect.Limit, "0.0");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TrustlineRemovedEffectResponse
        {
            Limit = effect.Limit,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode,
            AssetIssuer = effect.AssetIssuer
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeTrustlineUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTrustlineUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeTrustlineUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTrustlineUpdatedData(back);
    }

    private static void AssertTrustlineUpdatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TrustlineUpdatedEffectResponse);
        var effect = (TrustlineUpdatedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.Asset,
            Asset.CreateNonNativeAsset("TESTTEST", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"));
        Assert.AreEqual(effect.Limit, "100.0");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TrustlineUpdatedEffectResponse
        {
            Limit = effect.Limit,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode,
            AssetIssuer = effect.AssetIssuer
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    [Obsolete]
    public void TestDeserializeTrustlineAuthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTrustlineAuthorizedData(instance);
    }

    [TestMethod]
    [Obsolete]
    public void TestDeserializeTrustlineAuthorizedToMaintainLiabilitiesEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorizedToMaintainLiabilities.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTrustlineAuthorizedToMaintainLiabilitiesEffect(instance);
    }

    [TestMethod]
    [Obsolete]
    public void TestSerializeDeserializeTrustlineAuthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTrustlineAuthorizedData(back);
    }

    [Obsolete]
    private static void AssertTrustlineAuthorizedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TrustlineAuthorizedEffectResponse);
        var effect = (TrustlineAuthorizedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.AssetType, "credit_alphanum12");
        Assert.AreEqual(effect.AssetCode, "TESTTEST");
        Assert.AreEqual(effect.Trustor, "GB3E4AB4VWXJDUVN4Z3CPBU5HTMWVEQXONZYVDFMHQD6333KHCOL3UBR");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TrustlineAuthorizedEffectResponse
        {
            Trustor = effect.Trustor,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode
        };
        Assert.IsNotNull(back);
    }

    [Obsolete]
    private static void AssertTrustlineAuthorizedToMaintainLiabilitiesEffect(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TrustlineAuthorizedToMaintainLiabilitiesEffectResponse);
        var effect = (TrustlineAuthorizedToMaintainLiabilitiesEffectResponse)instance;

        var trustline = new TrustlineAuthorizationResponse
        {
            Trustor = "GB3E4AB4VWXJDUVN4Z3CPBU5HTMWVEQXONZYVDFMHQD6333KHCOL3UBR",
            AssetType = "credit_alphanum12",
            AssetCode = "TESTTEST"
        };

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.AssetType, trustline.AssetType);
        Assert.AreEqual(effect.AssetCode, trustline.AssetCode);
        Assert.AreEqual(effect.Trustor, trustline.Trustor);

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TrustlineAuthorizedToMaintainLiabilitiesEffectResponse
        {
            Trustor = effect.Trustor,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    [Obsolete]
    public void TestDeserializeTrustlineDeauthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineDeAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTrustlineDeauthorizedData(instance);
    }

    [TestMethod]
    [Obsolete]
    public void TestSerializeDeserializeTrustlineDeauthorizedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineDeAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTrustlineDeauthorizedData(back);
    }

    [Obsolete]
    private static void AssertTrustlineDeauthorizedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TrustlineDeauthorizedEffectResponse);
        var effect = (TrustlineDeauthorizedEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.AssetType, "credit_alphanum4");
        Assert.AreEqual(effect.AssetCode, "EUR");
        Assert.AreEqual(effect.Trustor, "GB3E4AB4VWXJDUVN4Z3CPBU5HTMWVEQXONZYVDFMHQD6333KHCOL3UBR");

        Assert.AreEqual(effect.Links.Operation.Href,
            "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TrustlineDeauthorizedEffectResponse
        {
            Trustor = effect.Trustor,
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeTradeEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectTrade.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTradeData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeTradeEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectTrade.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTradeData(back);
    }

    //Before Horizon 1.0.0 the OfferID in the json was a long.
    [TestMethod]
    public void TestDeserializeTradeEffectPre100()
    {
        var jsonPath = Utils.GetTestDataPath("effectTradePre100.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertTradeDataPre100(instance);
    }

    //Before Horizon 1.0.0 the OfferID in the json was a long.
    [TestMethod]
    public void TestSerializeDeserializeTradeEffectPre100()
    {
        var jsonPath = Utils.GetTestDataPath("effectTradePre100.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTradeDataPre100(back);
    }

    private static void AssertTradeDataPre100(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TradeEffectResponse);
        var effect = (TradeEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.Seller, "GCVHDLN6EHZBYW2M3BQIY32C23E4GPIRZZDBNF2Q73DAZ5VJDRGSMYRB");
        Assert.AreEqual(effect.OfferId, "1");
        Assert.AreEqual(effect.SoldAmount, "1000.0");
        Assert.AreEqual(effect.SoldAsset,
            Asset.CreateNonNativeAsset("EUR", "GCWVFBJ24754I5GXG4JOEB72GJCL3MKWC7VAEYWKGQHPVH3ENPNBSKWS"));
        Assert.AreEqual(effect.BoughtAmount, "60.0");
        Assert.AreEqual(effect.BoughtAsset,
            Asset.CreateNonNativeAsset("TESTTEST", "GAHXPUDP3AK6F2QQM4FIRBGPNGKLRDDSTQCVKEXXKKRHJZUUQ23D5BU7"));

        Assert.AreEqual(effect.Links.Operation.Href, "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TradeEffectResponse
        {
            Seller = effect.Seller,
            OfferId = effect.OfferId,
            SoldAmount = effect.SoldAmount,
            SoldAssetType = effect.SoldAssetType,
            SoldAssetCode = effect.SoldAssetCode,
            SoldAssetIssuer = effect.SoldAssetIssuer,
            BoughtAmount = effect.BoughtAmount,
            BoughtAssetType = effect.BoughtAssetType,
            BoughtAssetCode = effect.BoughtAssetCode,
            BoughtAssetIssuer = effect.BoughtAssetType
        };
        Assert.IsNotNull(back);
    }

    private static void AssertTradeData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is TradeEffectResponse);
        var effect = (TradeEffectResponse)instance;

        Assert.AreEqual(effect.Account, "GA6U5X6WOPNKKDKQULBR7IDHDBAQKOWPHYEC7WSXHZBFEYFD3XVZAKOO");
        Assert.AreEqual(effect.Seller, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(effect.SellerMuxed, "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(effect.SellerMuxedId, 5123456789);
        Assert.AreEqual(effect.OfferId, "1");
        Assert.AreEqual(effect.SoldAmount, "1000.0");
        Assert.AreEqual(effect.SoldAsset,
            Asset.CreateNonNativeAsset("EUR", "GCWVFBJ24754I5GXG4JOEB72GJCL3MKWC7VAEYWKGQHPVH3ENPNBSKWS"));
        Assert.AreEqual(effect.BoughtAmount, "60.0");
        Assert.AreEqual(effect.BoughtAsset,
            Asset.CreateNonNativeAsset("TESTTEST", "GAHXPUDP3AK6F2QQM4FIRBGPNGKLRDDSTQCVKEXXKKRHJZUUQ23D5BU7"));

        Assert.AreEqual(effect.Links.Operation.Href, "http://horizon-testnet.stellar.org/operations/33788507721730");
        Assert.AreEqual(effect.Links.Succeeds.Href,
            "http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2");
        Assert.AreEqual(effect.Links.Precedes.Href,
            "http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2");

        var back = new TradeEffectResponse
        {
            Seller = effect.Seller,
            OfferId = effect.OfferId,
            SoldAmount = effect.SoldAmount,
            SoldAssetType = effect.SoldAssetType,
            SoldAssetCode = effect.SoldAssetCode,
            SoldAssetIssuer = effect.SoldAssetIssuer,
            BoughtAmount = effect.BoughtAmount,
            BoughtAssetType = effect.BoughtAssetType,
            BoughtAssetCode = effect.BoughtAssetCode,
            BoughtAssetIssuer = effect.BoughtAssetType
        };
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestDeserializeAccountInflationUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountInflationUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountInflationUpdated(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountInflationUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectAccountInflationUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountInflationUpdated(back);
    }

    private static void AssertAccountInflationUpdated(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
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
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertDataCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeDataCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertDataCreatedData(back);
    }

    private static void AssertDataCreatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is DataCreatedEffectResponse);
        var effect = (DataCreatedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
    }

    [TestMethod]
    public void TestDeserializeDataRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertDataRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeDataRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertDataRemovedData(back);
    }

    private static void AssertDataRemovedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is DataRemovedEffectResponse);
        var effect = (DataRemovedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
    }

    [TestMethod]
    public void TestDeserializeDataUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertDataUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeDataUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectDataUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertDataUpdatedData(back);
    }

    private static void AssertDataUpdatedData(EffectResponse instance)
    {
        //There is a JsonConverter called OperationDeserializer that instantiates the type based on the json type_i element...
        Assert.IsTrue(instance is DataUpdatedEffectResponse);
        var effect = (DataUpdatedEffectResponse)instance;

        Assert.AreEqual("GDPFGP4IPE5DXG6XRXC4ZBUI43PAGRQ5VVNJ3LJTBXDBZ4ITO6HBHNSF", effect.Account);
        Assert.AreEqual(DateTimeOffset.Parse("2018-06-06T10:23:57Z").UtcDateTime, effect.CreatedAt);
    }

    [TestMethod]
    public void TestUnknownEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectUnknown.json");
        var json = File.ReadAllText(jsonPath);
        Assert.ThrowsException<JsonSerializationException>(() => JsonSingleton.GetInstance<EffectResponse>(json));
    }

    [TestMethod]
    public void TestDeserializeSequenceBumpedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSequenceBumped.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertSequenceBumpedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeSequenceBumpedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectSequenceBumped.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertOfferCreatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeOfferCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertOfferRemovedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeOfferRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        Assert.IsNotNull(instance);
        AssertOfferUpdatedData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeOfferUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("effectOfferUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

    //Account Sponsorship Created
    [TestMethod]
    public void TestSerializationAccountSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountSponsorshipCreatedData(back);
    }

    private static void AssertAccountSponsorshipCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipCreatedEffectResponse);
        var effect = (AccountSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.Sponsor);

        var back = new AccountSponsorshipCreatedEffectResponse { Sponsor = effect.Sponsor };
        Assert.IsNotNull(back);
    }

    //Account Sponsorship Removed
    [TestMethod]
    public void TestSerializationAccountSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountSponsorshipRemovedData(back);
    }

    private static void AssertAccountSponsorshipRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipRemovedEffectResponse);
        var effect = (AccountSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);

        var back = new AccountSponsorshipRemovedEffectResponse { FormerSponsor = effect.FormerSponsor };
        Assert.IsNotNull(back);
    }


    //Account Sponsorship Updated
    [TestMethod]
    public void TestSerializationAccountSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountSponsorshipUpdatedData(back);
    }

    private static void AssertAccountSponsorshipUpdatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipUpdatedEffectResponse);
        var effect = (AccountSponsorshipUpdatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
        Assert.AreEqual("GBVFLWXYCIGPO3455XVFIKHS66FCT5AI64ZARKS7QJN4NF7K5FOXTJNL", effect.NewSponsor);

        var back = new AccountSponsorshipUpdatedEffectResponse
            { FormerSponsor = effect.FormerSponsor, NewSponsor = effect.NewSponsor };
        Assert.IsNotNull(back);
    }

    //Claimable Balance Claimant Created
    [TestMethod]
    public void TestSerializationClaimableBalanceClaimantCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClaimantCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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
        Assert.IsNotNull(effect.Predicate.ToClaimPredicate());

        var back = new ClaimableBalanceClaimantCreatedEffectResponse
        {
            Asset = effect.Asset,
            BalanceId = effect.BalanceId,
            Amount = effect.Amount,
            Predicate = effect.Predicate
        };
        Assert.IsNotNull(back);
    }


    // Claimable Balance Claimed
    [TestMethod]
    public void TestSerializationClaimableBalanceClaimedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClaimed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

        var back = new ClaimableBalanceClaimedEffectResponse
        {
            Asset = effect.Asset,
            BalanceId = effect.BalanceId,
            Amount = effect.Amount
        };
        Assert.IsNotNull(back);
    }

    // Claimable Balance Created
    [TestMethod]
    public void TestSerializationClaimableBalanceCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

        var back = new ClaimableBalanceCreatedEffectResponse
        {
            Asset = effect.Asset,
            BalanceId = effect.BalanceId,
            Amount = effect.Amount
        };
        Assert.IsNotNull(back);
    }

    // Claimable Balance Sponsorship Created
    [TestMethod]
    public void TestSerializationClaimableBalanceSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClaimableBalanceSponsorshipCreatedEffect(back);
    }

    private static void AssertClaimableBalanceSponsorshipCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceSponsorshipCreatedEffectResponse);
        var effect = (ClaimableBalanceSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0", effect.BalanceId);
        Assert.AreEqual("GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.Sponsor);

        var back = new ClaimableBalanceSponsorshipCreatedEffectResponse
        {
            BalanceId = effect.BalanceId,
            Sponsor = effect.Sponsor
        };
        Assert.IsNotNull(back);
    }

    // Claimable Balance Sponsorship Removed
    [TestMethod]
    public void TestSerializationClaimableBalanceSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClaimableBalanceSponsorshipRemovedEffect(back);
    }

    private static void AssertClaimableBalanceSponsorshipRemovedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceSponsorshipRemovedEffectResponse);
        var effect = (ClaimableBalanceSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0", effect.BalanceId);
        Assert.AreEqual("GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.FormerSponsor);

        var back = new ClaimableBalanceSponsorshipRemovedEffectResponse
        {
            BalanceId = effect.BalanceId,
            FormerSponsor = effect.FormerSponsor
        };
        Assert.IsNotNull(back);
    }

    // Claimable Balance Sponsorship Updated
    [TestMethod]
    public void TestSerializationClaimableBalanceSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

        var back = new ClaimableBalanceSponsorshipUpdatedEffectResponse
        {
            BalanceId = effect.BalanceId, FormerSponsor = effect.FormerSponsor, NewSponsor = effect.NewSponsor
        };
        Assert.IsNotNull(back);
    }

    //Signer Sponsorship Created
    [TestMethod]
    public void TestSerializationSignerSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSignerSponsorshipCreatedEffect(back);
    }

    private static void AssertSignerSponsorshipCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerSponsorshipCreatedEffectResponse);
        var effect = (SignerSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("XAMF7DNTEJY74JPVMGTPZE4LFYTEGBXMGBHNUUMAA7IXMSBGHAMWSND6", effect.Signer);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.Sponsor);

        var back = new SignerSponsorshipCreatedEffectResponse { Signer = effect.Signer, Sponsor = effect.Sponsor };
        Assert.IsNotNull(back);
    }

    //Signer Sponsorship Removed
    [TestMethod]
    public void TestSerializationSignerSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertSignerSponsorshipRemovedEffect(back);
    }

    private static void AssertSignerSponsorshipRemovedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is SignerSponsorshipRemovedEffectResponse);
        var effect = (SignerSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("XAMF7DNTEJY74JPVMGTPZE4LFYTEGBXMGBHNUUMAA7IXMSBGHAMWSND6", effect.Signer);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.FormerSponsor);

        var back = new SignerSponsorshipRemovedEffectResponse
            { Signer = effect.Signer, FormerSponsor = effect.FormerSponsor };
        Assert.IsNotNull(back);
    }

    //Signer Sponsorship Updated
    [TestMethod]
    public void TestSerializationSignerSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

        var back = new SignerSponsorshipUpdatedEffectResponse
            { Signer = effect.Signer, FormerSponsor = effect.FormerSponsor, NewSponsor = effect.NewSponsor };
        Assert.IsNotNull(back);
    }

    //Trustline Sponsorship Created
    [TestMethod]
    public void TestSerializationTrustlineSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTrustlineSponsorshipCreatedEffect(back);
    }

    private static void AssertTrustlineSponsorshipCreatedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineSponsorshipCreatedEffectResponse);
        var effect = (TrustlineSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("ABC:GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.Asset);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.Sponsor);

        var back = new TrustlineSponsorshipCreatedEffectResponse { Asset = effect.Asset, Sponsor = effect.Sponsor };
        Assert.IsNotNull(back);
    }

    //Trustline Sponsorship Removed
    [TestMethod]
    public void TestSerializationTrustlineSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertTrustlineSponsorshipRemovedEffect(back);
    }

    private static void AssertTrustlineSponsorshipRemovedEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineSponsorshipRemovedEffectResponse);
        var effect = (TrustlineSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("ABC:GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", effect.Asset);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", effect.FormerSponsor);

        var back = new TrustlineSponsorshipRemovedEffectResponse
            { Asset = effect.Asset, FormerSponsor = effect.FormerSponsor };
        Assert.IsNotNull(back);
    }

    //Trustline Sponsorship Updated
    [TestMethod]
    public void TestSerializationTrustlineSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

        var back = new TrustlineSponsorshipUpdatedEffectResponse
        {
            Asset = effect.Asset,
            FormerSponsor = effect.FormerSponsor,
            NewSponsor = effect.NewSponsor
        };
        Assert.IsNotNull(back);
    }

    //Data Sponsorship Created
    [TestMethod]
    public void TestSerializationDataSponsorshipCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertDataSponsorshipCreatedData(back);
    }

    private static void AssertDataSponsorshipCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataSponsorshipCreatedEffectResponse);
        var effect = (DataSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.Sponsor);
        Assert.AreEqual("welcome-friend", effect.DataName);

        var back = new DataSponsorshipCreatedEffectResponse { Sponsor = effect.Sponsor, DataName = effect.DataName };
        Assert.IsNotNull(back);
    }

    //Data Sponsorship Removed
    [TestMethod]
    public void TestSerializationDataSponsorshipRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertDataSponsorshipRemovedData(back);
    }

    private static void AssertDataSponsorshipRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is DataSponsorshipRemovedEffectResponse);
        var effect = (DataSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
        Assert.AreEqual("welcome-friend", effect.DataName);

        var back = new DataSponsorshipRemovedEffectResponse
            { FormerSponsor = effect.FormerSponsor, DataName = effect.DataName };
        Assert.IsNotNull(back);
    }


    //Data Sponsorship Updated
    [TestMethod]
    public void TestSerializationDataSponsorshipUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

        var back = new DataSponsorshipUpdatedEffectResponse
            { FormerSponsor = effect.FormerSponsor, NewSponsor = effect.NewSponsor, DataName = effect.DataName };
        Assert.IsNotNull(back);
    }


    //Trustline Flags Updated
    [TestMethod]
    public void TestSerializationTrustlineFlagsUpdatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("trustlineFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
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

        var back = new TrustlineFlagsUpdatedEffectResponse
        {
            AssetType = effect.AssetType,
            AssetCode = effect.AssetCode,
            AssetIssuer = effect.AssetIssuer,
            Trustor = effect.Trustor,
            AuthorizedFlag = effect.AuthorizedFlag,
            AuthorizedToMaintainLiabilities = effect.AuthorizedToMaintainLiabilities,
            ClawbackEnabledFlag = effect.ClawbackEnabledFlag
        };
        Assert.IsNotNull(back);
    }

    //Claimable Balance Clawed Back
    [TestMethod]
    public void TestSerializationClaimableBalanceClawedBackEffect()
    {
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClawedBack.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertClaimableBalanceClawedBackEffect(back);
    }

    private static void AssertClaimableBalanceClawedBackEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceClawedBackEffectResponse);
        var effect = (ClaimableBalanceClawedBackEffectResponse)instance;

        Assert.AreEqual(
            new ClaimableBalanceClawedBackEffectResponse
                { BalanceId = "00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7" }.BalanceId,
            effect.BalanceId);

        var back = new ClaimableBalanceClawedBackEffectResponse();
        Assert.IsNotNull(back);
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolCreatedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolCreatedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);

        Assert.IsTrue(back is LiquidityPoolCreatedEffectResponse);
        var effect = (LiquidityPoolCreatedEffectResponse)back;

        Assert.AreEqual(effect.AccountMuxedId, 1278881UL);

        Assert.AreEqual(effect.LiquidityPool.ID.ToString(),
            "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385");

        Assert.AreEqual(effect.LiquidityPool.FeeBP, 30);
        Assert.AreEqual(effect.LiquidityPool.TotalTrustlines, 1);
        Assert.AreEqual(effect.LiquidityPool.TotalShares, "0.0000000");

        Assert.AreEqual(effect.LiquidityPool.Reserves[0].Asset.CanonicalName(), "native");
        Assert.AreEqual(effect.LiquidityPool.Reserves[0].Amount, "0.0000000");

        Assert.AreEqual(effect.LiquidityPool.Reserves[1].Asset.CanonicalName(),
            "TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25");
        Assert.AreEqual(effect.LiquidityPool.Reserves[1].Amount, "0.0000000");
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolDepositedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolDepositedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolDepositedEffectResponse);
        var effect = (LiquidityPoolDepositedEffectResponse)back;

        Assert.AreEqual(effect.AccountMuxedId, 1278881UL);

        Assert.AreEqual(effect.LiquidityPool.ID.ToString(),
            "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385");

        Assert.AreEqual(effect.LiquidityPool.FeeBP, 30);
        Assert.AreEqual(effect.LiquidityPool.TotalTrustlines, 1);
        Assert.AreEqual(effect.LiquidityPool.TotalShares, "1500.0000000");

        Assert.AreEqual(effect.ReservesDeposited[0].Asset.CanonicalName(), "native");
        Assert.AreEqual(effect.ReservesDeposited[0].Amount, "123.456789");

        Assert.AreEqual(effect.ReservesDeposited[1].Asset.CanonicalName(),
            "TEST:GD5Y3PMKI46MPILDG4OQP4SGFMRNKYEPJVDAPR3P3I2BMZ3O7IX6DB2Y");
        Assert.AreEqual(effect.ReservesDeposited[1].Amount, "478.7867966");

        Assert.AreEqual(effect.SharesReceived, "250.0000000");
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolRemovedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolRemovedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolRemovedEffectResponse);
        var effect = (LiquidityPoolRemovedEffectResponse)back;

        Assert.AreEqual(effect.AccountMuxedId, 1278881UL);
        Assert.AreEqual(effect.LiquidityPoolId.ToString(),
            "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385");
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolRevokedEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolRevokedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolRevokedEffectResponse);
        var effect = (LiquidityPoolRevokedEffectResponse)back;

        Assert.AreEqual(effect.AccountMuxedId, 1278881UL);

        Assert.AreEqual(effect.LiquidityPool.ID.ToString(),
            "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385");

        Assert.AreEqual(effect.LiquidityPool.FeeBP, 30);
        Assert.AreEqual(effect.LiquidityPool.TotalTrustlines, 1);
        Assert.AreEqual(effect.LiquidityPool.TotalShares, "0.0000000");

        Assert.AreEqual(effect.LiquidityPool.Reserves[0].Asset.CanonicalName(), "native");
        Assert.AreEqual(effect.LiquidityPool.Reserves[0].Amount, "0.0000000");

        Assert.AreEqual(effect.LiquidityPool.Reserves[1].Asset.CanonicalName(),
            "TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25");
        Assert.AreEqual(effect.LiquidityPool.Reserves[1].Amount, "0.0000000");

        Assert.AreEqual(effect.ReservesRevoked[0].Asset.CanonicalName(),
            "TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25");
        Assert.AreEqual(effect.ReservesRevoked[0].Amount, "1500.0000000");
        Assert.AreEqual(effect.ReservesRevoked[0].ClaimableBalanceId,
            "00000000836f572dd43b76853df6c88ca1b89394b547d74de0c87334ce7f9270cb342203");

        Assert.AreEqual(effect.SharesRevoked, "100.0000000");
    }

    [TestMethod]
    public void TestSerializeDeserializeLiquidityPoolTradeEffect()
    {
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolTradeEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolTradeEffectResponse);
        var effect = (LiquidityPoolTradeEffectResponse)back;

        Assert.AreEqual(effect.LiquidityPool.ID.ToString(),
            "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385");

        Assert.AreEqual(effect.Sold.Asset.CanonicalName(),
            "TEST:GC2262FQJAHVJSYWI6XEVQEH5CLPYCVSOLQHCDHNSKVWHTKYEZNAQS25");
        Assert.AreEqual(effect.Sold.Amount, "93.1375850");

        Assert.AreEqual(effect.Bought.Asset.CanonicalName(),
            "TEST2:GDQ4273UBKSHIE73RJB5KLBBM7W3ESHWA74YG7ZBXKZLKT5KZGPKKB7E");
        Assert.AreEqual(effect.Bought.Amount, "100.0000000");
    }
}