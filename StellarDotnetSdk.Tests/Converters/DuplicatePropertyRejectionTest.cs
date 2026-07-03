using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses.Predicates;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Regression tests: converters that hand-parse JSON must reject duplicate properties.
///     The SDK-wide <c>AllowDuplicateProperties = false</c> guard on <see cref="JsonOptions.DefaultOptions" />
///     is enforced by the built-in object mapper only; converters that read fields manually (via
///     <see cref="JsonDocument" /> or a raw <see cref="Utf8JsonReader" /> loop) are last-wins by default,
///     which would let a duplicated financial field (amount, asset code, time-lock predicate, pagination
///     href) be silently overridden by the last — potentially attacker-appended — value.
/// </summary>
[TestClass]
public class DuplicatePropertyRejectionTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    #region AssetAmount

    /// <summary>
    ///     Verifies that a duplicated amount property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void AssetAmount_WithDuplicateAmount_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""1.0"",""amount"":""999999.0""}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated asset property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void AssetAmount_WithDuplicateAsset_ThrowsJsonException()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""native"",""asset"":""USD:{issuer.AccountId}"",""amount"":""1.0""}}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that duplicates are detected case-insensitively, matching the serializer-level
    ///     guard under PropertyNameCaseInsensitive = true.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void AssetAmount_WithCaseInsensitiveDuplicateAmount_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""1.0"",""Amount"":""999999.0""}";

        // Act
        JsonSerializer.Deserialize<AssetAmount>(json, _options);
    }

    #endregion

    #region Reserve

    /// <summary>
    ///     Verifies that a duplicated amount property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Reserve_WithDuplicateAmount_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""1.0"",""amount"":""999999.0""}";

        // Act
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated asset property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Reserve_WithDuplicateAsset_ThrowsJsonException()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""native"",""asset"":""USD:{issuer.AccountId}"",""amount"":""1.0""}}";

        // Act
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    /// <summary>
    ///     Verifies that duplicates are detected case-insensitively, matching the serializer-level
    ///     guard under PropertyNameCaseInsensitive = true.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Reserve_WithCaseInsensitiveDuplicateAsset_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""asset"":""native"",""Asset"":""native"",""amount"":""1.0""}";

        // Act
        JsonSerializer.Deserialize<Reserve>(json, _options);
    }

    #endregion

    #region LiquidityPoolClaimableAssetAmount

    /// <summary>
    ///     Verifies that a duplicated amount property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void LiquidityPoolClaimableAssetAmount_WithDuplicateAmount_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""asset"":""native"",""amount"":""1.0"",""amount"":""999999.0""}";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated asset property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void LiquidityPoolClaimableAssetAmount_WithDuplicateAsset_ThrowsJsonException()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json =
            $@"{{""asset"":""native"",""asset"":""USD:{issuer.AccountId}"",""amount"":""1.0"",""claimable_balance_id"":""00000000""}}";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated claimable_balance_id property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void LiquidityPoolClaimableAssetAmount_WithDuplicateClaimableBalanceId_ThrowsJsonException()
    {
        // Arrange
        var json =
            @"{""asset"":""native"",""amount"":""1.0"",""claimable_balance_id"":""00000000"",""claimable_balance_id"":""ffffffff""}";

        // Act
        JsonSerializer.Deserialize<LiquidityPoolClaimableAssetAmount>(json, _options);
    }

    #endregion

    #region Asset

    /// <summary>
    ///     Verifies that a duplicated asset_code property is rejected instead of the last value winning
    ///     (asset-code substitution).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Asset_WithDuplicateAssetCode_ThrowsJsonException()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var json =
            $@"{{""asset_type"":""credit_alphanum4"",""asset_code"":""USD"",""asset_code"":""EVL"",""asset_issuer"":""{issuer.AccountId}""}}";

        // Act
        JsonSerializer.Deserialize<Asset>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated asset_issuer property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Asset_WithDuplicateAssetIssuer_ThrowsJsonException()
    {
        // Arrange
        var issuer = KeyPair.Random();
        var attacker = KeyPair.Random();
        var json =
            $@"{{""asset_type"":""credit_alphanum4"",""asset_code"":""USD"",""asset_issuer"":""{issuer.AccountId}"",""asset_issuer"":""{attacker.AccountId}""}}";

        // Act
        JsonSerializer.Deserialize<Asset>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated asset_type property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Asset_WithDuplicateAssetType_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""asset_type"":""credit_alphanum4"",""asset_type"":""native""}";

        // Act
        JsonSerializer.Deserialize<Asset>(json, _options);
    }

    #endregion

    #region Predicate

    /// <summary>
    ///     Verifies that a duplicated abs_before property is rejected instead of the last value winning
    ///     (claimable-balance spend-window shift).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Predicate_WithDuplicateAbsBefore_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before"":""2999-12-31T23:59:59Z""}";

        // Act
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated abs_before_epoch property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Predicate_WithDuplicateAbsBeforeEpoch_ThrowsJsonException()
    {
        // Arrange
        var json =
            @"{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before_epoch"":1577836800,""abs_before_epoch"":32503680000}";

        // Act
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated rel_before property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Predicate_WithDuplicateRelBefore_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""rel_before"":100,""rel_before"":999999999}";

        // Act
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated not property is rejected instead of one of the values winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Predicate_WithDuplicateNot_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""not"":{""unconditional"":true},""not"":{""rel_before"":100}}";

        // Act
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Verifies that duplicates nested inside a composite predicate are rejected too
    ///     (the guard applies at every recursion level).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Predicate_WithNestedDuplicateAbsBefore_ThrowsJsonException()
    {
        // Arrange
        var json =
            @"{""not"":{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before"":""2999-12-31T23:59:59Z""}}";

        // Act
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Verifies that duplicates nested inside an element of an and array are rejected too
    ///     (elements re-enter the converter, so the guard applies inside composite arrays as well).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Predicate_WithDuplicateInsideAndArrayElement_ThrowsJsonException()
    {
        // Arrange
        var json =
            @"{""and"":[{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before"":""2999-12-31T23:59:59Z""},{""unconditional"":true}]}";

        // Act
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    /// <summary>
    ///     Verifies that duplicates nested inside an element of an or array are rejected too.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Predicate_WithDuplicateInsideOrArrayElement_ThrowsJsonException()
    {
        // Arrange
        var json =
            @"{""or"":[{""unconditional"":true},{""rel_before"":100,""rel_before"":999999999}]}";

        // Act
        JsonSerializer.Deserialize<Predicate>(json, _options);
    }

    #endregion

    #region Polymorphic OperationResponse / EffectResponse

    /// <summary>
    ///     Verifies that a duplicated financial field on the polymorphic operation path is rejected.
    ///     OperationResponseJsonConverter reads only the type_i discriminator by hand and re-deserializes
    ///     the payload through the object mapper, whose AllowDuplicateProperties = false guard must reject
    ///     the duplicate — this is the load-bearing assumption for not giving these converters a manual guard.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void OperationResponse_WithDuplicateAmount_ThrowsJsonException()
    {
        // Arrange - type_i 1 = payment
        var json = @"{""type_i"":1,""amount"":""1.0"",""amount"":""999999.0""}";

        // Act
        JsonSerializer.Deserialize<OperationResponse>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated type_i discriminator on the polymorphic operation path is rejected
    ///     by the mapper re-parse (no type-confusion via a repeated discriminator).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void OperationResponse_WithDuplicateTypeI_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""type_i"":1,""type_i"":0,""amount"":""1.0""}";

        // Act
        JsonSerializer.Deserialize<OperationResponse>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated financial field on the polymorphic effect path is rejected
    ///     (same transitive mapper-level protection as the operation path).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void EffectResponse_WithDuplicateAmount_ThrowsJsonException()
    {
        // Arrange - type_i 2 = account_credited
        var json = @"{""type_i"":2,""amount"":""1.0"",""amount"":""999999.0""}";

        // Act
        JsonSerializer.Deserialize<EffectResponse>(json, _options);
    }

    #endregion

    #region Link

    /// <summary>
    ///     Verifies that a duplicated href property is rejected instead of the last value winning
    ///     (pagination URL substitution via _links.next).
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Link_WithDuplicateHref_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""href"":""https://horizon.stellar.org/accounts"",""href"":""https://evil.example.com/accounts""}";

        // Act
        JsonSerializer.Deserialize<Link<Response>>(json, _options);
    }

    /// <summary>
    ///     Verifies that a duplicated templated property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Link_WithDuplicateTemplated_ThrowsJsonException()
    {
        // Arrange
        var json = @"{""href"":""https://horizon.stellar.org/accounts"",""templated"":false,""templated"":true}";

        // Act
        JsonSerializer.Deserialize<Link<Response>>(json, _options);
    }

    #endregion
}
