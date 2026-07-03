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
/// <remarks>
///     Every case asserts through <see cref="AssertRejectsDuplicate{T}" />, which requires both a
///     <see cref="JsonException" /> and that its message names a duplicate property. That message check is
///     load-bearing: a bare <c>ExpectedException(typeof(JsonException))</c> would also pass if the payload
///     failed for an unrelated reason (missing field, bad asset format), so it could stay green even if the
///     duplicate guard were removed. Asserting the message ties each test to the guard it is meant to cover.
/// </remarks>
[TestClass]
public class DuplicatePropertyRejectionTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Asserts that deserializing <paramref name="json" /> as <typeparamref name="T" /> fails with a
    ///     <see cref="JsonException" /> whose message names a duplicate property — i.e. the rejection comes
    ///     from the duplicate-property guard (converter-level or the object mapper's), not from some other
    ///     validation failure that merely happens to throw <see cref="JsonException" />.
    /// </summary>
    private void AssertRejectsDuplicate<T>(string json)
    {
        var exception = Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<T>(json, _options));
        StringAssert.Contains(exception.Message, "Duplicate property",
            $"Expected a duplicate-property rejection, but got a different JsonException: {exception.Message}");
    }

    #region AssetAmount

    /// <summary>
    ///     Verifies that a duplicated amount property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void AssetAmount_WithDuplicateAmount_ThrowsJsonException()
    {
        var json = @"{""asset"":""native"",""amount"":""1.0"",""amount"":""999999.0""}";

        AssertRejectsDuplicate<AssetAmount>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated asset property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void AssetAmount_WithDuplicateAsset_ThrowsJsonException()
    {
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""native"",""asset"":""USD:{issuer.AccountId}"",""amount"":""1.0""}}";

        AssertRejectsDuplicate<AssetAmount>(json);
    }

    /// <summary>
    ///     Verifies that duplicates are detected case-insensitively, matching the serializer-level
    ///     guard under PropertyNameCaseInsensitive = true.
    /// </summary>
    [TestMethod]
    public void AssetAmount_WithCaseInsensitiveDuplicateAmount_ThrowsJsonException()
    {
        var json = @"{""asset"":""native"",""amount"":""1.0"",""Amount"":""999999.0""}";

        AssertRejectsDuplicate<AssetAmount>(json);
    }

    #endregion

    #region Reserve

    /// <summary>
    ///     Verifies that a duplicated amount property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void Reserve_WithDuplicateAmount_ThrowsJsonException()
    {
        var json = @"{""asset"":""native"",""amount"":""1.0"",""amount"":""999999.0""}";

        AssertRejectsDuplicate<Reserve>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated asset property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void Reserve_WithDuplicateAsset_ThrowsJsonException()
    {
        var issuer = KeyPair.Random();
        var json = $@"{{""asset"":""native"",""asset"":""USD:{issuer.AccountId}"",""amount"":""1.0""}}";

        AssertRejectsDuplicate<Reserve>(json);
    }

    /// <summary>
    ///     Verifies that duplicates are detected case-insensitively, matching the serializer-level
    ///     guard under PropertyNameCaseInsensitive = true.
    /// </summary>
    [TestMethod]
    public void Reserve_WithCaseInsensitiveDuplicateAsset_ThrowsJsonException()
    {
        var json = @"{""asset"":""native"",""Asset"":""native"",""amount"":""1.0""}";

        AssertRejectsDuplicate<Reserve>(json);
    }

    #endregion

    #region LiquidityPoolClaimableAssetAmount

    /// <summary>
    ///     Verifies that a duplicated amount property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void LiquidityPoolClaimableAssetAmount_WithDuplicateAmount_ThrowsJsonException()
    {
        var json = @"{""asset"":""native"",""amount"":""1.0"",""amount"":""999999.0""}";

        AssertRejectsDuplicate<LiquidityPoolClaimableAssetAmount>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated asset property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void LiquidityPoolClaimableAssetAmount_WithDuplicateAsset_ThrowsJsonException()
    {
        var issuer = KeyPair.Random();
        var json =
            $@"{{""asset"":""native"",""asset"":""USD:{issuer.AccountId}"",""amount"":""1.0"",""claimable_balance_id"":""00000000""}}";

        AssertRejectsDuplicate<LiquidityPoolClaimableAssetAmount>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated claimable_balance_id property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void LiquidityPoolClaimableAssetAmount_WithDuplicateClaimableBalanceId_ThrowsJsonException()
    {
        var json =
            @"{""asset"":""native"",""amount"":""1.0"",""claimable_balance_id"":""00000000"",""claimable_balance_id"":""ffffffff""}";

        AssertRejectsDuplicate<LiquidityPoolClaimableAssetAmount>(json);
    }

    #endregion

    #region Asset

    /// <summary>
    ///     Verifies that a duplicated asset_code property is rejected instead of the last value winning
    ///     (asset-code substitution).
    /// </summary>
    [TestMethod]
    public void Asset_WithDuplicateAssetCode_ThrowsJsonException()
    {
        var issuer = KeyPair.Random();
        var json =
            $@"{{""asset_type"":""credit_alphanum4"",""asset_code"":""USD"",""asset_code"":""EVL"",""asset_issuer"":""{issuer.AccountId}""}}";

        AssertRejectsDuplicate<Asset>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated asset_issuer property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void Asset_WithDuplicateAssetIssuer_ThrowsJsonException()
    {
        var issuer = KeyPair.Random();
        var attacker = KeyPair.Random();
        var json =
            $@"{{""asset_type"":""credit_alphanum4"",""asset_code"":""USD"",""asset_issuer"":""{issuer.AccountId}"",""asset_issuer"":""{attacker.AccountId}""}}";

        AssertRejectsDuplicate<Asset>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated asset_type property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void Asset_WithDuplicateAssetType_ThrowsJsonException()
    {
        var json = @"{""asset_type"":""credit_alphanum4"",""asset_type"":""native""}";

        AssertRejectsDuplicate<Asset>(json);
    }

    #endregion

    #region Predicate

    /// <summary>
    ///     Verifies that a duplicated abs_before property is rejected instead of the last value winning
    ///     (claimable-balance spend-window shift).
    /// </summary>
    [TestMethod]
    public void Predicate_WithDuplicateAbsBefore_ThrowsJsonException()
    {
        var json = @"{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before"":""2999-12-31T23:59:59Z""}";

        AssertRejectsDuplicate<Predicate>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated abs_before_epoch property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void Predicate_WithDuplicateAbsBeforeEpoch_ThrowsJsonException()
    {
        var json =
            @"{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before_epoch"":1577836800,""abs_before_epoch"":32503680000}";

        AssertRejectsDuplicate<Predicate>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated rel_before property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void Predicate_WithDuplicateRelBefore_ThrowsJsonException()
    {
        var json = @"{""rel_before"":100,""rel_before"":999999999}";

        AssertRejectsDuplicate<Predicate>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated not property is rejected instead of one of the values winning.
    /// </summary>
    [TestMethod]
    public void Predicate_WithDuplicateNot_ThrowsJsonException()
    {
        var json = @"{""not"":{""unconditional"":true},""not"":{""rel_before"":100}}";

        AssertRejectsDuplicate<Predicate>(json);
    }

    /// <summary>
    ///     Verifies that duplicates nested inside a composite predicate are rejected too
    ///     (the guard applies at every recursion level).
    /// </summary>
    [TestMethod]
    public void Predicate_WithNestedDuplicateAbsBefore_ThrowsJsonException()
    {
        var json =
            @"{""not"":{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before"":""2999-12-31T23:59:59Z""}}";

        AssertRejectsDuplicate<Predicate>(json);
    }

    /// <summary>
    ///     Verifies that duplicates nested inside an element of an and array are rejected too
    ///     (elements re-enter the converter, so the guard applies inside composite arrays as well).
    /// </summary>
    [TestMethod]
    public void Predicate_WithDuplicateInsideAndArrayElement_ThrowsJsonException()
    {
        var json =
            @"{""and"":[{""abs_before"":""2020-01-01T00:00:00Z"",""abs_before"":""2999-12-31T23:59:59Z""},{""unconditional"":true}]}";

        AssertRejectsDuplicate<Predicate>(json);
    }

    /// <summary>
    ///     Verifies that duplicates nested inside an element of an or array are rejected too.
    /// </summary>
    [TestMethod]
    public void Predicate_WithDuplicateInsideOrArrayElement_ThrowsJsonException()
    {
        var json =
            @"{""or"":[{""unconditional"":true},{""rel_before"":100,""rel_before"":999999999}]}";

        AssertRejectsDuplicate<Predicate>(json);
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
    public void OperationResponse_WithDuplicateAmount_ThrowsJsonException()
    {
        // type_i 1 = payment
        var json = @"{""type_i"":1,""amount"":""1.0"",""amount"":""999999.0""}";

        AssertRejectsDuplicate<OperationResponse>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated type_i discriminator on the polymorphic operation path is rejected
    ///     by the mapper re-parse (no type-confusion via a repeated discriminator).
    /// </summary>
    [TestMethod]
    public void OperationResponse_WithDuplicateTypeI_ThrowsJsonException()
    {
        var json = @"{""type_i"":1,""type_i"":0,""amount"":""1.0""}";

        AssertRejectsDuplicate<OperationResponse>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated financial field on the polymorphic effect path is rejected
    ///     (same transitive mapper-level protection as the operation path).
    /// </summary>
    [TestMethod]
    public void EffectResponse_WithDuplicateAmount_ThrowsJsonException()
    {
        // type_i 2 = account_credited
        var json = @"{""type_i"":2,""amount"":""1.0"",""amount"":""999999.0""}";

        AssertRejectsDuplicate<EffectResponse>(json);
    }

    #endregion

    #region Link

    /// <summary>
    ///     Verifies that a duplicated href property is rejected instead of the last value winning
    ///     (pagination URL substitution via _links.next).
    /// </summary>
    [TestMethod]
    public void Link_WithDuplicateHref_ThrowsJsonException()
    {
        var json = @"{""href"":""https://horizon.stellar.org/accounts"",""href"":""https://evil.example.com/accounts""}";

        AssertRejectsDuplicate<Link<Response>>(json);
    }

    /// <summary>
    ///     Verifies that a duplicated templated property is rejected instead of the last value winning.
    /// </summary>
    [TestMethod]
    public void Link_WithDuplicateTemplated_ThrowsJsonException()
    {
        var json = @"{""href"":""https://horizon.stellar.org/accounts"",""templated"":false,""templated"":true}";

        AssertRejectsDuplicate<Link<Response>>(json);
    }

    #endregion
}
