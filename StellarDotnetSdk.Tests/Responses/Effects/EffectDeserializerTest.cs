using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Responses.Effects;

/// <summary>
///     Unit tests for deserializing various effect response types from JSON.
/// </summary>
[TestClass]
public class EffectDeserializerTest
{
    /// <summary>
    ///     Verifies that AccountCreatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountCreatedEffectJson_ReturnsAccountCreatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountCreated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountCreatedData(instance);
    }

    /// <summary>
    ///     Verifies that AccountCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AccountRemovedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountRemovedEffectJson_ReturnsAccountRemovedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountRemoved.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountRemovedData(instance);
    }

    /// <summary>
    ///     Verifies that AccountRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AccountCreditedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountCreditedEffectJson_ReturnsAccountCreditedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountCredited.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountCreditedData(instance);
    }

    /// <summary>
    ///     Verifies that AccountCreditedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountCreditedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountCredited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AccountDebitedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountDebitedEffectJson_ReturnsAccountDebitedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountDebited.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountDebitedData(instance);
    }

    /// <summary>
    ///     Verifies that AccountDebitedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountDebitedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountDebited.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AccountThresholdsUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountThresholdsUpdatedEffectJson_ReturnsAccountThresholdsUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountThresholdsUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountThresholdsUpdatedData(instance);
    }

    /// <summary>
    ///     Verifies that AccountThresholdsUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountThresholdsUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountThresholdsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AccountHomeDomainUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountHomeDomainUpdatedEffectJson_ReturnsAccountHomeDomainUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountHomeDomainUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountHomeDomainUpdatedData(instance);
    }

    /// <summary>
    ///     Verifies that AccountHomeDomainUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountHomeDomainUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountHomeDomainUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AccountFlagsUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountFlagsUpdatedEffectJson_ReturnsAccountFlagsUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountFlagsUpdatedData(instance);
    }

    /// <summary>
    ///     Verifies that AccountFlagsUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountFlagsUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that SignerCreatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSignerCreatedEffectJson_ReturnsSignerCreatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSignerCreated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertSignerCreatedData(instance);
    }

    /// <summary>
    ///     Verifies that SignerCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithSignerCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSignerCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that SignerRemovedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSignerRemovedEffectJson_ReturnsSignerRemovedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSignerRemoved.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertSignerRemoveData(instance);
    }

    /// <summary>
    ///     Verifies that SignerRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithSignerRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSignerRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that SignerUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSignerUpdatedEffectJson_ReturnsSignerUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSignerUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertSignerUpdatedData(instance);
    }

    /// <summary>
    ///     Verifies that SignerUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithSignerUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSignerUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineCreatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTrustlineCreatedEffectJson_ReturnsTrustlineCreatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineCreated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertTrustlineCreatedData(instance);
    }

    /// <summary>
    ///     Verifies that TrustlineCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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
        Assert.IsNull(effect.LiquidityPoolId);

        Assert.AreEqual("1000.0", effect.Limit);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("http://horizon-testnet.stellar.org/operations/33788507721730", effect.Links.Operation.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=desc&cursor=33788507721730-2",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("http://horizon-testnet.stellar.org/effects?order=asc&cursor=33788507721730-2",
            effect.Links.Precedes.Href);
    }

    /// <summary>
    ///     Verifies that TrustlineCreatedLiquidityPoolSharesEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTrustlineCreatedLiquidityPoolSharesEffectJson_ReturnsTrustlineCreatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineCreatedLiquidityPoolShares.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertTrustlineCreatedLiquidityPoolSharesData(instance);
    }

    /// <summary>
    ///     Verifies that TrustlineCreatedLiquidityPoolSharesEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineCreatedLiquidityPoolSharesEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineCreatedLiquidityPoolShares.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertTrustlineCreatedLiquidityPoolSharesData(back);
    }

    private static void AssertTrustlineCreatedLiquidityPoolSharesData(EffectResponse instance)
    {
        Assert.IsTrue(instance is TrustlineCreatedEffectResponse);
        var effect = (TrustlineCreatedEffectResponse)instance;

        Assert.AreEqual("GB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVVYMB", effect.Account);
        Assert.AreEqual("liquidity_pool_shares", effect.AssetType);
        Assert.IsNull(effect.Asset);
        Assert.IsNull(effect.AssetCode);
        Assert.IsNull(effect.AssetIssuer);
        Assert.AreEqual("3cdf19b3d5d41f753e0f33ebf039f2733851732ab8fe679dcc5d6adafb4700e3", effect.LiquidityPoolId);

        Assert.AreEqual("922337203685.4775807", effect.Limit);
        Assert.IsNotNull(effect.Links);
        Assert.AreEqual("https://horizon-testnet.stellar.org/operations/8622194091364353", effect.Links.Operation.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/effects?order=desc&cursor=8622194091364353-1",
            effect.Links.Succeeds.Href);
        Assert.AreEqual("https://horizon-testnet.stellar.org/effects?order=asc&cursor=8622194091364353-1",
            effect.Links.Precedes.Href);
    }

    /// <summary>
    ///     Verifies that TrustlineRemovedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTrustlineRemovedEffectJson_ReturnsTrustlineRemovedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineRemoved.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertTrustlineRemovedData(instance);
    }

    /// <summary>
    ///     Verifies that TrustlineRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTrustlineUpdatedEffectJson_ReturnsTrustlineUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertTrustlineUpdatedData(instance);
    }

    /// <summary>
    ///     Verifies that TrustlineUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineAuthorizedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void Deserialize_WithTrustlineAuthorizedEffectJson_ReturnsTrustlineAuthorizedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorized.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertTrustlineAuthorizedData(instance);
    }

    /// <summary>
    ///     Verifies that TrustlineAuthorizedToMaintainLiabilitiesEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void
        Deserialize_WithTrustlineAuthorizedToMaintainLiabilitiesEffectJson_ReturnsTrustlineAuthorizedToMaintainLiabilitiesEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorizedToMaintainLiabilities.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertTrustlineAuthorizedToMaintainLiabilitiesEffect(instance);
    }

    /// <summary>
    ///     Verifies that TrustlineAuthorizedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void SerializeDeserialize_WithTrustlineAuthorizedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineDeauthorizedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void Deserialize_WithTrustlineDeauthorizedEffectJson_ReturnsTrustlineDeauthorizedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineDeAuthorized.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertTrustlineDeauthorizedData(instance);
    }

    /// <summary>
    ///     Verifies that TrustlineDeauthorizedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    [Obsolete]
    public void SerializeDeserialize_WithTrustlineDeauthorizedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineDeAuthorized.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AlphaNum12NativeTradeEffect can be deserialized and serialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAlphaNum12NativeTradeEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectTradeAlphaNum12Native.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAlphaNum12NativeTradeData(instance);
        Assert.IsNotNull(back);
        AssertAlphaNum12NativeTradeData(back);
    }

    /// <summary>
    ///     Verifies that NativeAlphaNum4TradeEffect can be deserialized and serialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithNativeAlphaNum4TradeEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectTradeNativeAphaNum4.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertNativeAphaNum4TradeData(instance);
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

    /// <summary>
    ///     Verifies that AccountInflationUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithAccountInflationUpdatedEffectJson_ReturnsAccountInflationDestinationUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountInflationUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertAccountInflationUpdated(instance);
    }

    /// <summary>
    ///     Verifies that AccountInflationUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountInflationUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectAccountInflationUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that DataCreatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithDataCreatedEffectJson_ReturnsDataCreatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectDataCreated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertDataCreatedData(instance);
    }

    /// <summary>
    ///     Verifies that DataCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithDataCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectDataCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that DataRemovedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithDataRemovedEffectJson_ReturnsDataRemovedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectDataRemoved.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertDataRemovedData(instance);
    }

    /// <summary>
    ///     Verifies that DataRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithDataRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectDataRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that DataUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithDataUpdatedEffectJson_ReturnsDataUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectDataUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertDataUpdatedData(instance);
    }

    /// <summary>
    ///     Verifies that DataUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithDataUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectDataUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that deserializing an unknown effect type throws JsonException.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithUnknownEffectJson_ThrowsJsonException()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectUnknown.json");
        var json = File.ReadAllText(jsonPath);

        // Act & Assert
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that SequenceBumpedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSequenceBumpedEffectJson_ReturnsSequenceBumpedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSequenceBumped.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertSequenceBumpedData(instance);
    }

    /// <summary>
    ///     Verifies that SequenceBumpedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithSequenceBumpedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectSequenceBumped.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that OfferCreatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOfferCreatedEffectJson_ReturnsOfferCreatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectOfferCreated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertOfferCreatedData(instance);
    }

    /// <summary>
    ///     Verifies that OfferCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithOfferCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectOfferCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that OfferRemovedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOfferRemovedEffectJson_ReturnsOfferRemovedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectOfferRemoved.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertOfferRemovedData(instance);
    }

    /// <summary>
    ///     Verifies that OfferRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithOfferRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectOfferRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that OfferUpdatedEffect can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOfferUpdatedEffectJson_ReturnsOfferUpdatedEffectResponse()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectOfferUpdated.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertOfferUpdatedData(instance);
    }

    /// <summary>
    ///     Verifies that OfferUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithOfferUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("effectOfferUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that AccountSponsorshipCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountSponsorshipCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertAccountSponsorshipCreatedData(back);
    }

    private static void AssertAccountSponsorshipCreatedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipCreatedEffectResponse);
        var effect = (AccountSponsorshipCreatedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.Sponsor);
    }

    /// <summary>
    ///     Verifies that AccountSponsorshipRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountSponsorshipRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertAccountSponsorshipRemovedData(back);
    }

    private static void AssertAccountSponsorshipRemovedData(EffectResponse instance)
    {
        Assert.IsTrue(instance is AccountSponsorshipRemovedEffectResponse);
        var effect = (AccountSponsorshipRemovedEffectResponse)instance;

        Assert.AreEqual("GCBQ6JRBPF3SXQBQ6SO5MRBE7WVV4UCHYOSHQGXSZNPZLFRYVYOWBZRQ", effect.FormerSponsor);
    }

    /// <summary>
    ///     Verifies that AccountSponsorshipUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithAccountSponsorshipUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("accountSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that ClaimableBalanceClaimantCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceClaimantCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClaimantCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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


    /// <summary>
    ///     Verifies that ClaimableBalanceClaimedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceClaimedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClaimed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that ClaimableBalanceCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalanceCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that ClaimableBalanceSponsorshipCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceSponsorshipCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that ClaimableBalanceSponsorshipRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceSponsorshipRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that ClaimableBalanceSponsorshipUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceSponsorshipUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalanceSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that SignerSponsorshipCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithSignerSponsorshipCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that SignerSponsorshipRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithSignerSponsorshipRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that SignerSponsorshipUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithSignerSponsorshipUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("signerSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineSponsorshipCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineSponsorshipCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineSponsorshipRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineSponsorshipRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineSponsorshipUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineSponsorshipUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that DataSponsorshipCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithDataSponsorshipCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipCreated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that DataSponsorshipRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithDataSponsorshipRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipRemoved.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that DataSponsorshipUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithDataSponsorshipUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("dataSponsorshipUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that TrustlineFlagsUpdatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithTrustlineFlagsUpdatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("trustlineFlagsUpdated.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that ClaimableBalanceClawedBackEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimableBalanceClawedBackEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimableBalanceClawedBack.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertClaimableBalanceClawedBackEffect(back);
    }

    private static void AssertClaimableBalanceClawedBackEffect(EffectResponse instance)
    {
        Assert.IsTrue(instance is ClaimableBalanceClawedBackEffectResponse);
        var effect = (ClaimableBalanceClawedBackEffectResponse)instance;

        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7", effect.BalanceId);
    }

    /// <summary>
    ///     Verifies that LiquidityPoolCreatedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolCreatedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolCreatedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that LiquidityPoolDepositedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolDepositedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolDepositedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that LiquidityPoolRemovedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolRemovedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolRemovedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        Assert.IsTrue(back is LiquidityPoolRemovedEffectResponse);
        var effect = (LiquidityPoolRemovedEffectResponse)back;

        Assert.AreEqual(1278881UL, effect.AccountMuxedId);
        Assert.IsNotNull(effect.LiquidityPoolId);
        Assert.AreEqual("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385",
            effect.LiquidityPoolId.ToString());
    }

    /// <summary>
    ///     Verifies that LiquidityPoolRevokedEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolRevokedEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolRevokedEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that LiquidityPoolTradeEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolTradeEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolTradeEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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

    /// <summary>
    ///     Verifies that LiquidityPoolWithdrewEffect can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithLiquidityPoolWithdrewEffect_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("LiquidityPoolWithdrewEffectResponse.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<EffectResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<EffectResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
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