using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

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

    /// <summary>
    ///     Test representative effect types to verify the converter's type_i switch works.
    ///     We test a few from each range to ensure coverage.
    /// </summary>
    [TestMethod]
    public void TestRead_AccountCreatedEffect_TypeI_0()
    {
        var json = BaseJson + """
            ,"type_i": 0,
            "type": "account_created",
            "starting_balance": "10000.0"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AccountCreatedEffectResponse));
    }

    [TestMethod]
    public void TestRead_AccountRemovedEffect_TypeI_1()
    {
        var json = BaseJson + """
            ,"type_i": 1,
            "type": "account_removed"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AccountRemovedEffectResponse));
    }

    [TestMethod]
    public void TestRead_SignerCreatedEffect_TypeI_10()
    {
        var json = BaseJson + """
            ,"type_i": 10,
            "type": "signer_created",
            "public_key": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
            "weight": 1
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(SignerCreatedEffectResponse));
    }

    [TestMethod]
    public void TestRead_TrustlineCreatedEffect_TypeI_20()
    {
        var json = BaseJson + """
            ,"type_i": 20,
            "type": "trustline_created",
            "asset_type": "credit_alphanum4",
            "asset_code": "USD",
            "asset_issuer": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2",
            "limit": "1000.0"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(TrustlineCreatedEffectResponse));
    }

    [TestMethod]
    public void TestRead_OfferCreatedEffect_TypeI_30()
    {
        var json = BaseJson + """
            ,"type_i": 30,
            "type": "offer_created"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OfferCreatedEffectResponse));
    }

    [TestMethod]
    public void TestRead_DataCreatedEffect_TypeI_40()
    {
        var json = BaseJson + """
            ,"type_i": 40,
            "type": "data_created",
            "name": "test_data",
            "value": "dGVzdA=="
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(DataCreatedEffectResponse));
    }

    [TestMethod]
    public void TestRead_ClaimableBalanceCreatedEffect_TypeI_50()
    {
        var json = BaseJson + """
            ,"type_i": 50,
            "type": "claimable_balance_created",
            "balance_id": "00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0",
            "asset": "native",
            "amount": "100.0"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ClaimableBalanceCreatedEffectResponse));
    }

    [TestMethod]
    public void TestRead_AccountSponsorshipCreatedEffect_TypeI_60()
    {
        var json = BaseJson + """
            ,"type_i": 60,
            "type": "account_sponsorship_created",
            "sponsor": "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(AccountSponsorshipCreatedEffectResponse));
    }

    [TestMethod]
    public void TestRead_ClaimableBalanceClawedBackEffect_TypeI_80()
    {
        var json = BaseJson + """
            ,"type_i": 80,
            "type": "claimable_balance_clawed_back",
            "balance_id": "00000000be7e37b24927c095e2292d5d0e6db8b0f2dbeb1355847c7fccb458cbdd61bfd0"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ClaimableBalanceClawedBackEffectResponse));
    }

    [TestMethod]
    public void TestRead_LiquidityPoolRemovedEffect_TypeI_94()
    {
        var json = BaseJson + """
            ,"type_i": 94,
            "type": "liquidity_pool_removed",
            "liquidity_pool_id": "4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385"
            }
            """;

        var result = JsonSerializer.Deserialize<EffectResponse>(json, _options);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(LiquidityPoolRemovedEffectResponse));
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForUnknownTypeI()
    {
        var json = BaseJson + """
            ,"type_i": 999,
            "type": "unknown"
            }
            """;

        JsonSerializer.Deserialize<EffectResponse>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForGapTypeI()
    {
        // Type 8 is in a gap between account effects (0-7) and signer effects (10-12)
        var json = BaseJson + """
            ,"type_i": 8,
            "type": "unknown"
            }
            """;

        JsonSerializer.Deserialize<EffectResponse>(json, _options);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void TestRead_ThrowsForMissingTypeI()
    {
        var json = """
            {
                "id": "1",
                "type": "unknown"
            }
            """;

        JsonSerializer.Deserialize<EffectResponse>(json, _options);
    }
}
