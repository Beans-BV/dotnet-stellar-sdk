using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for EffectResponseJsonConverter.
///     Focus: polymorphic type selection based on type_i discriminator.
/// </summary>
[TestClass]
public class EffectResponseJsonConverterTest
{
    private const string BaseJson = """
                                    {
                                        "id": "0065571265847297-0000000001",
                                        "paging_token": "65571265847297-1",
                                        "account": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
                                        "created_at": "2023-01-01T00:00:00Z",
                                        "_links": {
                                            "operation": {"href": "https://horizon.stellar.org/operations/12345"},
                                            "succeeds": {"href": "https://horizon.stellar.org/effects?order=desc&cursor=12345-1"},
                                            "precedes": {"href": "https://horizon.stellar.org/effects?order=asc&cursor=12345-1"}
                                        }
                                    """;

    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Test representative effect types to verify the converter's type_i switch works.
    ///     We test a few from each range to ensure coverage.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountCreatedEffectTypeI0_ReturnsAccountCreatedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 0,
                              "type": "account_created",
                              "starting_balance": "10000.0"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AccountCreatedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of account_removed effect with type_i=1.
    ///     Verifies that type_i=1 deserializes to AccountRemovedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountRemovedEffectTypeI1_ReturnsAccountRemovedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 1,
                              "type": "account_removed"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AccountRemovedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of signer_created effect with type_i=10.
    ///     Verifies that type_i=10 deserializes to SignerCreatedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSignerCreatedEffectTypeI10_ReturnsSignerCreatedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 10,
                              "type": "signer_created",
                              "public_key": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
                              "weight": 1
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(SignerCreatedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of trustline_created effect with type_i=20.
    ///     Verifies that type_i=20 deserializes to TrustlineCreatedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithTrustlineCreatedEffectTypeI20_ReturnsTrustlineCreatedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 20,
                              "type": "trustline_created",
                              "asset_type": "credit_alphanum4",
                              "asset_code": "USD",
                              "asset_issuer": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
                              "limit": "1000.0"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(TrustlineCreatedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of offer_created effect with type_i=30.
    ///     Verifies that type_i=30 deserializes to OfferCreatedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithOfferCreatedEffectTypeI30_ReturnsOfferCreatedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 30,
                              "type": "offer_created"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OfferCreatedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of data_created effect with type_i=40.
    ///     Verifies that type_i=40 deserializes to DataCreatedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithDataCreatedEffectTypeI40_ReturnsDataCreatedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 40,
                              "type": "data_created",
                              "name": "test_data",
                              "value": "dGVzdA=="
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(DataCreatedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of claimable_balance_created effect with type_i=50.
    ///     Verifies that type_i=50 deserializes to ClaimableBalanceCreatedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithClaimableBalanceCreatedEffectTypeI50_ReturnsClaimableBalanceCreatedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 50,
                              "type": "claimable_balance_created",
                              "balance_id": "00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0",
                              "asset": "native",
                              "amount": "100.0"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ClaimableBalanceCreatedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of account_sponsorship_created effect with type_i=60.
    ///     Verifies that type_i=60 deserializes to AccountSponsorshipCreatedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithAccountSponsorshipCreatedEffectTypeI60_ReturnsAccountSponsorshipCreatedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 60,
                              "type": "account_sponsorship_created",
                              "sponsor": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AccountSponsorshipCreatedEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of claimable_balance_clawed_back effect with type_i=80.
    ///     Verifies that type_i=80 deserializes to ClaimableBalanceClawedBackEffectResponse.
    /// </summary>
    [TestMethod]
    public void
        Deserialize_WithClaimableBalanceClawedBackEffectTypeI80_ReturnsClaimableBalanceClawedBackEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 80,
                              "type": "claimable_balance_clawed_back",
                              "balance_id": "00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ClaimableBalanceClawedBackEffectResponse));
    }

    /// <summary>
    ///     Tests deserialization of liquidity_pool_removed effect with type_i=94.
    ///     Verifies that type_i=94 deserializes to LiquidityPoolRemovedEffectResponse.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithLiquidityPoolRemovedEffectTypeI94_ReturnsLiquidityPoolRemovedEffectResponse()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 94,
                              "type": "liquidity_pool_removed",
                              "liquidity_pool_id": "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385"
                              }
                              """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(LiquidityPoolRemovedEffectResponse));
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for unknown type_i values.
    ///     Verifies proper error handling when type_i does not match any known effect type.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithUnknownTypeI_ThrowsJsonException()
    {
        // Arrange
        var json = BaseJson + """
                              ,"type_i": 999,
                              "type": "unknown"
                              }
                              """;

        // Act & Assert
        JsonSerializer.Deserialize<EffectResponse>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for gap type_i values.
    ///     Verifies proper error handling when type_i falls in a gap between effect ranges.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithGapTypeI_ThrowsJsonException()
    {
        // Arrange - Type 8 is in a gap between account effects (0-7) and signer effects (10-12)
        var json = BaseJson + """
                              ,"type_i": 8,
                              "type": "unknown"
                              }
                              """;

        // Act & Assert
        JsonSerializer.Deserialize<EffectResponse>(json, _options);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException when type_i property is missing.
    ///     Verifies validation for required type_i property.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithMissingTypeI_ThrowsJsonException()
    {
        // Arrange
        var json = """
                   {
                       "id": "1",
                       "type": "unknown"
                   }
                   """;

        // Act & Assert
        JsonSerializer.Deserialize<EffectResponse>(json, _options);
    }

    /// <summary>
    ///     Tests deserialization of a contract_credited effect with type_i=96.
    ///     Payload shape taken verbatim from a live Horizon mainnet response emitted by a
    ///     Soroban SAC transfer. Verifies that type_i=96 dispatches to
    ///     <see cref="ContractCreditedEffectResponse" /> and that every wire-format field is
    ///     bound to the expected property.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithContractCreditedEffectTypeI96_ReturnsContractCreditedEffectResponse()
    {
        // Arrange - real Horizon mainnet payload (https://horizon.stellar.org/effects, operation 266896217031864321-13).
        const string json =
            """
            {
                "_links": {
                    "operation": {"href": "https://horizon.stellar.org/operations/266896217031864321"},
                    "succeeds": {"href": "https://horizon.stellar.org/effects?order=desc&cursor=266896217031864321-13"},
                    "precedes": {"href": "https://horizon.stellar.org/effects?order=asc&cursor=266896217031864321-13"}
                },
                "id": "0266896217031864321-0000000013",
                "paging_token": "266896217031864321-13",
                "account": "GBKDPWX3BY5PWVWY6CJVY57CFD3I54VXJZPEZTUGTBFDNBGMCSWNXTEA",
                "type": "contract_credited",
                "type_i": 96,
                "created_at": "2026-04-16T06:43:24Z",
                "asset_type": "native",
                "contract": "CB2TCA23Z3CLTXSVGJB2PAC3MTRNX6NPFETRRDDJEAVBJMAZIUPIKBZL",
                "amount": "1.0152349"
            }
            """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert - type dispatch + full field binding (every wire-format field, including base-class)
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ContractCreditedEffectResponse));

        var credited = (ContractCreditedEffectResponse)result;

        // Base-class fields (EffectResponse)
        Assert.AreEqual("0266896217031864321-0000000013", credited.Id);
        Assert.AreEqual("266896217031864321-13", credited.PagingToken);
        Assert.AreEqual("GBKDPWX3BY5PWVWY6CJVY57CFD3I54VXJZPEZTUGTBFDNBGMCSWNXTEA", credited.Account);
        Assert.AreEqual(
            new DateTimeOffset(2026, 4, 16, 6, 43, 24, TimeSpan.Zero),
            credited.CreatedAt);
        Assert.AreEqual(96, credited.TypeId);
        Assert.AreEqual("contract_credited", credited.Type);
        Assert.IsNotNull(credited.Links);
        Assert.AreEqual("https://horizon.stellar.org/operations/266896217031864321", credited.Links.Operation.Href);
        Assert.AreEqual(
            "https://horizon.stellar.org/effects?order=desc&cursor=266896217031864321-13",
            credited.Links.Succeeds.Href);
        Assert.AreEqual(
            "https://horizon.stellar.org/effects?order=asc&cursor=266896217031864321-13",
            credited.Links.Precedes.Href);

        // Subtype-specific fields (ContractCreditedEffectResponse)
        Assert.AreEqual("CB2TCA23Z3CLTXSVGJB2PAC3MTRNX6NPFETRRDDJEAVBJMAZIUPIKBZL", credited.Contract);
        Assert.AreEqual("1.0152349", credited.Amount);
        Assert.AreEqual("native", credited.AssetType);
        Assert.IsNull(credited.AssetCode);
        Assert.IsNull(credited.AssetIssuer);

        // Computed Asset property - exercises the native branch of Asset.Create(type, code, issuer)
        Assert.IsInstanceOfType(credited.Asset, typeof(AssetTypeNative));
    }

    /// <summary>
    ///     Tests deserialization of a contract_debited effect with type_i=97.
    ///     Payload shape constructed from the same wire format Horizon emits for the symmetric
    ///     SAC transfer debit, with a credit_alphanum4 asset to exercise the non-native branch
    ///     of <see cref="ContractDebitedEffectResponse.Asset" />.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithContractDebitedEffectTypeI97_ReturnsContractDebitedEffectResponse()
    {
        // Arrange - wire shape matches Horizon's emitted contract_debited effect.
        const string json =
            """
            {
                "_links": {
                    "operation": {"href": "https://horizon.stellar.org/operations/266896217031864321"},
                    "succeeds": {"href": "https://horizon.stellar.org/effects?order=desc&cursor=266896217031864321-14"},
                    "precedes": {"href": "https://horizon.stellar.org/effects?order=asc&cursor=266896217031864321-14"}
                },
                "id": "0266896217031864321-0000000014",
                "paging_token": "266896217031864321-14",
                "account": "GBKDPWX3BY5PWVWY6CJVY57CFD3I54VXJZPEZTUGTBFDNBGMCSWNXTEA",
                "type": "contract_debited",
                "type_i": 97,
                "created_at": "2026-04-16T06:43:24Z",
                "asset_type": "credit_alphanum4",
                "asset_code": "USDC",
                "asset_issuer": "GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN",
                "contract": "CB2TCA23Z3CLTXSVGJB2PAC3MTRNX6NPFETRRDDJEAVBJMAZIUPIKBZL",
                "amount": "42.0000000"
            }
            """;

        // Act
        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        // Assert - type dispatch + full field binding (every wire-format field, including base-class)
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ContractDebitedEffectResponse));

        var debited = (ContractDebitedEffectResponse)result;

        // Base-class fields (EffectResponse)
        Assert.AreEqual("0266896217031864321-0000000014", debited.Id);
        Assert.AreEqual("266896217031864321-14", debited.PagingToken);
        Assert.AreEqual("GBKDPWX3BY5PWVWY6CJVY57CFD3I54VXJZPEZTUGTBFDNBGMCSWNXTEA", debited.Account);
        Assert.AreEqual(
            new DateTimeOffset(2026, 4, 16, 6, 43, 24, TimeSpan.Zero),
            debited.CreatedAt);
        Assert.AreEqual(97, debited.TypeId);
        Assert.AreEqual("contract_debited", debited.Type);
        Assert.IsNotNull(debited.Links);
        Assert.AreEqual("https://horizon.stellar.org/operations/266896217031864321", debited.Links.Operation.Href);
        Assert.AreEqual(
            "https://horizon.stellar.org/effects?order=desc&cursor=266896217031864321-14",
            debited.Links.Succeeds.Href);
        Assert.AreEqual(
            "https://horizon.stellar.org/effects?order=asc&cursor=266896217031864321-14",
            debited.Links.Precedes.Href);

        // Subtype-specific fields (ContractDebitedEffectResponse)
        Assert.AreEqual("CB2TCA23Z3CLTXSVGJB2PAC3MTRNX6NPFETRRDDJEAVBJMAZIUPIKBZL", debited.Contract);
        Assert.AreEqual("42.0000000", debited.Amount);
        Assert.AreEqual("credit_alphanum4", debited.AssetType);
        Assert.AreEqual("USDC", debited.AssetCode);
        Assert.AreEqual("GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN", debited.AssetIssuer);

        // Computed Asset property - exercises the non-native branch of Asset.Create(type, code, issuer)
        Assert.IsInstanceOfType(debited.Asset, typeof(AssetTypeCreditAlphaNum4));
        var creditAsset = (AssetTypeCreditAlphaNum4)debited.Asset;
        Assert.AreEqual("USDC", creditAsset.Code);
        Assert.AreEqual("GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN", creditAsset.Issuer);
    }
}