using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for KeyPairJsonConverter.
///     Focus: serialization and deserialization of KeyPair objects as account ID strings.
/// </summary>
[TestClass]
public class KeyPairJsonConverterTest
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new KeyPairJsonConverter() }
    };

    /// <summary>
    ///     Tests deserialization of valid account ID string to KeyPair.
    ///     Verifies that account ID strings deserialize to KeyPair instances with matching account ID.
    /// </summary>
    [TestMethod]
    public void TestRead_ValidAccountId()
    {
        var accountId = KeyPair.Random().AccountId;
        var json = $@"""{accountId}""";
        var keyPair = JsonSerializer.Deserialize<KeyPair>(json, _options);
        Assert.IsNotNull(keyPair);
        Assert.AreEqual(accountId, keyPair.AccountId);
    }

    /// <summary>
    ///     Tests deserialization of null JSON value.
    ///     Verifies that null JSON deserializes to null KeyPair.
    /// </summary>
    [TestMethod]
    public void TestRead_Null()
    {
        var json = "null";
        var keyPair = JsonSerializer.Deserialize<KeyPair>(json, _options);
        Assert.IsNull(keyPair);
    }

    /// <summary>
    ///     Tests serialization of KeyPair to account ID string.
    ///     Verifies that KeyPair instances serialize to JSON string containing the account ID.
    /// </summary>
    [TestMethod]
    public void TestWrite_AccountId()
    {
        var keyPair = KeyPair.Random();
        var json = JsonSerializer.Serialize(keyPair, _options);
        Assert.AreEqual($"\"{keyPair.AccountId}\"", json);
    }

    /// <summary>
    ///     Tests serialization of null KeyPair.
    ///     Verifies that null KeyPair serializes to null JSON value.
    /// </summary>
    [TestMethod]
    public void TestWrite_Null()
    {
        KeyPair? keyPair = null;
        var json = JsonSerializer.Serialize(keyPair, _options);
        Assert.AreEqual("null", json);
    }

    /// <summary>
    ///     Tests round-trip serialization and deserialization of KeyPair.
    ///     Verifies that serialized KeyPair can be deserialized back with matching account ID.
    /// </summary>
    [TestMethod]
    public void TestRoundTrip()
    {
        var original = KeyPair.Random();
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<KeyPair>(json, _options);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.AccountId, deserialized.AccountId);
    }
}


