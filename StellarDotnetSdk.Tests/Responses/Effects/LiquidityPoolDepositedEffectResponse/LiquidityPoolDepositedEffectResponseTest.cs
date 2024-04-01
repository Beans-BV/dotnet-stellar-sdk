﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Effects;

namespace StellarDotnetSdk.Tests.Responses.Effects;

[TestClass]
public class LiquidityPoolDepositedEffectResponseTest
{
    [TestMethod]
    public void TestCreation()
    {
        var json = File.ReadAllText(
            Path.Combine("responses/effects/", "LiquidityPoolDepositedEffectResponse/Data.json"));
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var response = (LiquidityPoolDepositedEffectResponse)instance;
        var clone = new LiquidityPoolDepositedEffectResponse(
            response.LiquidityPool, response.ReservesDeposited, response.SharesReceived);

        Assert.AreEqual(response.LiquidityPool, clone.LiquidityPool);
        Assert.AreEqual(response.ReservesDeposited, clone.ReservesDeposited);
        Assert.AreEqual(response.SharesReceived, clone.SharesReceived);
    }

    [TestMethod]
    public void TestDeserialize()
    {
        var json = File.ReadAllText(
            Path.Combine("responses/effects/", "LiquidityPoolDepositedEffectResponse/Data.json"));
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);

        AssertData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var json = File.ReadAllText(
            Path.Combine("responses/effects/", "LiquidityPoolDepositedEffectResponse/Data.json"));
        var instance = JsonSingleton.GetInstance<EffectResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<EffectResponse>(serialized);
        Assert.IsNotNull(back);
        AssertData(back);
    }

    private static void AssertData(EffectResponse instance)
    {
        Assert.IsTrue(instance is LiquidityPoolDepositedEffectResponse);
        var effect = (LiquidityPoolDepositedEffectResponse)instance;

        Assert.AreEqual(effect.AccountMuxedID, 1278881UL);

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
}